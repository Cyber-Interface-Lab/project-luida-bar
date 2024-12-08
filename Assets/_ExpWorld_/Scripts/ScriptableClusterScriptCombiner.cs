#if UNITY_EDITOR
using Assets.KaomoLab.CSCombiner;
using ClusterVR.CreatorKit.Item.Implements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptableClusterScriptCombiner : CSCombiner
{
    public List<JavaScriptAsset> ClusterScripts => clusterScripts;
    public List<JavaScriptAsset> PlayerScripts => playerScripts;
    
    public void PrependScript(JavaScriptAsset clusterScript, JavaScriptAsset playerScript, bool combineNow = false)
    {
        if (clusterScript != null) clusterScripts.Insert(0, clusterScript);
        if (playerScript != null) playerScripts.Insert(0, playerScript);
        if (combineNow) Combine();
    }
    
    public void AppendScript(JavaScriptAsset clusterScript, JavaScriptAsset playerScript, bool combineNow = false)
    {
        if (clusterScript != null) clusterScripts.Add(clusterScript);
        if (playerScript != null) playerScripts.Add(playerScript);
        if (combineNow) Combine();
    }
    
    public void ReplaceScript(JavaScriptAsset clusterScript, int clusterScriptIndex, JavaScriptAsset playerScript, int playerScriptIndex, bool combineNow = false)
    {
        if (clusterScript != null) clusterScripts[clusterScriptIndex] = clusterScript;
        if (playerScript != null) playerScripts[playerScriptIndex] = playerScript;
        if (combineNow) Combine();
    }
}
#endif
