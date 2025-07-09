#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class FanArrangeEqualGapWindow : EditorWindow
{
    GameObject target;
    float innerRadius = 1f;
    float outerRadius = 5f;
    float angleRange  = 180f;
    int   firstRingCount      = 2;
    int   firstFourRingsTotal = 25;

    [MenuItem("Tools/Fan Arrange Children")]
    static void OpenWindow()
    {
        GetWindow<FanArrangeEqualGapWindow>("Fan Arrange (Ring Gaps)").Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Fan-Arrange Children (Per-Ring Gaps)", EditorStyles.boldLabel);
        target                 = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
        innerRadius            = EditorGUILayout.FloatField("Inner Radius", innerRadius);
        outerRadius            = EditorGUILayout.FloatField("Outer Radius", outerRadius);
        angleRange             = EditorGUILayout.FloatField("Fan Angle (°)", angleRange);
        firstRingCount         = EditorGUILayout.IntField("Count on Ring 0", firstRingCount);
        firstFourRingsTotal    = EditorGUILayout.IntField("Total on Rings 0–3", firstFourRingsTotal);

        if (GUILayout.Button("Arrange Children"))
            ArrangeWithPerRingGaps();
    }

    void ArrangeWithPerRingGaps()
    {
        if (target == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Target GameObject.", "OK");
            return;
        }

        int totalChildren = target.transform.childCount;
        if (totalChildren == 0)
        {
            EditorUtility.DisplayDialog("Info", "Target has no children to arrange.", "OK");
            return;
        }
        if (innerRadius < 0 || outerRadius <= innerRadius)
        {
            EditorUtility.DisplayDialog("Error", "Require 0 ≤ Inner < Outer radius.", "OK");
            return;
        }
        if (firstRingCount < 0 || firstFourRingsTotal < firstRingCount || firstFourRingsTotal > totalChildren)
        {
            EditorUtility.DisplayDialog("Error", "Invalid override values.", "OK");
            return;
        }

        // compute global sector‐area gap for ring count estimation
        float thetaRad = angleRange * Mathf.Deg2Rad;
        float area     = 0.5f * (outerRadius*outerRadius - innerRadius*innerRadius) * thetaRad;
        float gap      = Mathf.Sqrt(area / totalChildren);

        // determine number of rings
        int ringCount    = Mathf.Max(1, Mathf.FloorToInt((outerRadius - innerRadius) / gap) + 1);
        float radialStep = (ringCount > 1) 
            ? (outerRadius - innerRadius) / (ringCount - 1) 
            : 0f;

        Vector3 origin = target.transform.position;

        // generate candidate ring‐index list
        var ringIndices = new List<int>();
        for (int ri = 0; ri < ringCount; ri++)
        {
            float r = innerRadius + radialStep*ri;
            int segs = Mathf.Max(1, Mathf.FloorToInt(r * thetaRad / gap));
            for (int ai = 0; ai < segs; ai++)
                ringIndices.Add(ri);
        }
        // pad if too few slots
        if (ringIndices.Count < totalChildren)
        {
            int deficit = totalChildren - ringIndices.Count;
            for (int i = 0; i < deficit; i++)
                ringIndices.Add(ringCount - 1);
        }

        // bucket indices by ring
        var buckets = ringIndices
            .Select((ri, idx) => new { ri, idx })
            .GroupBy(x => x.ri)
            .ToDictionary(g => g.Key, g => g.Select(x => x.idx).ToList());

        // select exact override slots
        var selected = new List<int>();

        // ring 0
        if (buckets.ContainsKey(0))
            selected.AddRange(buckets[0].Take(firstRingCount));

        // rings 1–3
        int need1to3 = firstFourRingsTotal - selected.Count;
        for (int ri = 1; ri <= 3 && need1to3 > 0; ri++)
        {
            if (!buckets.ContainsKey(ri)) continue;
            var take = buckets[ri].Take(need1to3).ToList();
            selected.AddRange(take);
            need1to3 -= take.Count;
        }

        // rings ≥4
        for (int ri = 4; ri < ringCount && selected.Count < totalChildren; ri++)
        {
            if (!buckets.ContainsKey(ri)) continue;
            foreach (var idx in buckets[ri])
            {
                if (selected.Count == totalChildren) break;
                selected.Add(idx);
            }
        }

        // backfill rings 1–3 if still needed
        if (selected.Count < totalChildren)
        {
            for (int ri = 1; ri <= 3 && selected.Count < totalChildren; ri++)
            {
                if (!buckets.ContainsKey(ri)) continue;
                foreach (var idx in buckets[ri])
                {
                    if (selected.Count == totalChildren) break;
                    if (!selected.Contains(idx))
                        selected.Add(idx);
                }
            }
        }

        // sort selected by ring ascending
        selected.Sort((a, b) => ringIndices[a] != ringIndices[b]
            ? ringIndices[a] - ringIndices[b]
            : a - b);

        // count per ring
        var ringCounts = selected
            .GroupBy(i => ringIndices[i])
            .ToDictionary(g => g.Key, g => g.Count());

        // build final positions with per‐ring angular spacing
        var finalPositions = new List<Vector3>();
        foreach (var ri in ringCounts.Keys.OrderBy(r => r))
        {
            int n = ringCounts[ri];
            float r = innerRadius + radialStep * ri;
            for (int j = 0; j < n; j++)
            {
                float angle = -angleRange/2f + angleRange * (j + 0.5f) / n;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                finalPositions.Add(origin + dir * r);
            }
        }
        // pad outer ring if needed
        if (finalPositions.Count < totalChildren)
        {
            int need = totalChildren - finalPositions.Count;
            float r = innerRadius + radialStep * (ringCount - 1);
            for (int j = 0; j < need; j++)
            {
                float angle = -angleRange/2f + angleRange * (j + 0.5f) / need;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                finalPositions.Add(origin + dir * r);
            }
        }

        // apply to children
        Undo.RegisterCompleteObjectUndo(target.transform, "Fan-Arrange Per-Ring Gaps");
        for (int i = 0; i < totalChildren; i++)
        {
            Transform child = target.transform.GetChild(i);
            Vector3 pos = finalPositions[i];
            Undo.RecordObject(child, "Move Child");
            child.position = pos;
            // face inward
            child.rotation = Quaternion.LookRotation((origin - pos).normalized);
            EditorUtility.SetDirty(child);
        }

        EditorUtility.DisplayDialog(
            "Done",
            $"Placed {totalChildren} children:\n" +
            $"- {firstRingCount} on ring 0\n" +
            $"- {firstFourRingsTotal} on rings 0–3 total\n" +
            $"- per-ring gaps allowed",
            "OK"
        );
    }
}
#endif
