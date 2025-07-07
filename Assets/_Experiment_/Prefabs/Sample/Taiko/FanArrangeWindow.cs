using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FanArrangeEqualGapWindow : EditorWindow
{
    GameObject target;
    float innerRadius = 1f;
    float outerRadius = 5f;
    float angleRange  = 180f;

    [MenuItem("Tools/Fan Arrange Children (Equal Gap)")]
    static void OpenWindow()
    {
        GetWindow<FanArrangeEqualGapWindow>("Fan Equal Gap").Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Fan-Arrange Children (Equal Gap)", EditorStyles.boldLabel);
        target       = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
        innerRadius  = EditorGUILayout.FloatField("Inner Radius", innerRadius);
        outerRadius  = EditorGUILayout.FloatField("Outer Radius", outerRadius);
        angleRange   = EditorGUILayout.FloatField("Fan Angle (°)", angleRange);

        if (GUILayout.Button("Arrange with Equal Gap"))
            ArrangeWithEqualGap();
    }

    void ArrangeWithEqualGap()
    {
        if (target == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Target GameObject.", "OK");
            return;
        }

        int count = target.transform.childCount;
        if (count == 0)
        {
            EditorUtility.DisplayDialog("Info", "Target has no children to arrange.", "OK");
            return;
        }

        if (innerRadius < 0 || outerRadius <= innerRadius)
        {
            EditorUtility.DisplayDialog("Error", "Require 0 ≤ Inner < Outer radius.", "OK");
            return;
        }

        // compute sector parameters
        float thetaRad = angleRange * Mathf.Deg2Rad;
        float area = 0.5f * (outerRadius*outerRadius - innerRadius*innerRadius) * thetaRad;
        // target gap = sqrt(area / count)
        float gap = Mathf.Sqrt(area / count);

        // compute number of rings
        int ringCount = Mathf.Max(1, Mathf.FloorToInt((outerRadius - innerRadius) / gap) + 1);
        float radialSpacing = (ringCount > 1) 
            ? (outerRadius - innerRadius) / (ringCount - 1) 
            : 0f;

        Vector3 origin = target.transform.position;
        var positions = new List<Vector3>();

        // generate candidate positions
        for (int ri = 0; ri < ringCount; ri++)
        {
            float r = innerRadius + radialSpacing * ri;
            float circumference = r * thetaRad;
            // number of items on this ring
            int segs = Mathf.Max(1, Mathf.FloorToInt(circumference / gap));
            float angStep = angleRange / segs;

            for (int ai = 0; ai < segs; ai++)
            {
                float angle = -angleRange/2f + angStep * (ai + 0.5f);
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                positions.Add(origin + dir * r);
            }
        }

        // if too few positions, pad along outer rim
        if (positions.Count < count)
        {
            int deficit = count - positions.Count;
            int segs = Mathf.Max(1, Mathf.FloorToInt(outerRadius * thetaRad / gap));
            float angStep = angleRange / segs;
            for (int i = 0; i < deficit; i++)
            {
                float angle = -angleRange/2f + angStep * ((i % segs) + 0.5f);
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                positions.Add(origin + dir * outerRadius);
            }
        }

        // assign only as many as needed
        Undo.RegisterCompleteObjectUndo(target.transform, "Fan-Arrange Equal Gap");
        for (int i = 0; i < count; i++)
        {
            Transform child = target.transform.GetChild(i);
            Vector3 pos = positions[i];
            Undo.RecordObject(child, "Move Child");
            child.position = pos;
            // face toward center/origin
            Vector3 lookDir = (origin - pos).normalized;
            child.rotation = Quaternion.LookRotation(lookDir);
            EditorUtility.SetDirty(child);
        }

        EditorUtility.DisplayDialog(
            "Done",
            $"Arranged {count} children\nwith approx. {gap:F2} units gap.",
            "OK"
        );
    }
}
