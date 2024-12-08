#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

public class StateListEditor : EditorWindow
{
    private StateList stateList;
    private SerializedObject serializedStateList;
    private SerializedProperty statesProperty;
    private string prefabPath = "Assets/_ExpWorld_/Prefabs/State/State/State.prefab";

    private readonly string[] FixedStateNames = new string[3] {"Trial - Task", "Trial - Rest", "Trial - Questionnaire"};

    [MenuItem("Window/State List Editor")]
    public static void ShowWindow()
    {
        GetWindow<StateListEditor>("State List Editor");
    }

    private void OnGUI()
    {
        string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        string stateListPath = scenePath.Replace("Scenes", "ExpSettings/StateList").Replace(".unity", ".asset");

        stateList = AssetDatabase.LoadAssetAtPath<StateList>(stateListPath);

        if (stateList == null)
        {
            EditorGUILayout.HelpBox($"StateList not found at {stateListPath}. Please ensure it exists.", MessageType.Warning);
            return;
        }

        if (serializedStateList == null || serializedStateList.targetObject != stateList)
        {
            serializedStateList = new SerializedObject(stateList);
            statesProperty = serializedStateList.FindProperty("States");
        }

        EditorGUILayout.LabelField("Edit States", EditorStyles.boldLabel);

        serializedStateList.Update();

        for (int i = 0; i < statesProperty.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("State ID", GUILayout.Width(60));
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(60));

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();

            SerializedProperty state = statesProperty.GetArrayElementAtIndex(i);
            SerializedProperty stateName = state.FindPropertyRelative("StateName");
            SerializedProperty destStateName = state.FindPropertyRelative("DestStateName");

            EditorGUILayout.LabelField("State name:");
            EditorGUI.BeginDisabledGroup(Array.IndexOf(FixedStateNames, stateName.stringValue) > -1);
            EditorGUILayout.PropertyField(stateName, GUIContent.none, GUILayout.Width(150));
            if (string.IsNullOrEmpty(stateName.stringValue))
            {
                stateName.stringValue = "State";
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            #region Transition Destination State
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Transit destination state");
            int destStateIndex = Array.FindIndex(stateList.States, s => s.StateName == destStateName.stringValue);
            string[] stateNames = Array.ConvertAll(stateList.States, s => s.StateName);
            EditorGUI.BeginDisabledGroup(Array.IndexOf(FixedStateNames, stateName.stringValue) > -1);
            destStateIndex = EditorGUILayout.Popup(destStateIndex, stateNames, GUILayout.Width(150));
            EditorGUI.EndDisabledGroup();

            if (destStateIndex >= 0)
            {
                destStateName.stringValue = stateList.States[destStateIndex].StateName;
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Move state
            EditorGUILayout.BeginVertical();

            // Label and buttons for moving state
            EditorGUILayout.LabelField("Move state to:");

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(Array.IndexOf(FixedStateNames, stateName.stringValue) > -1);
            if (GUILayout.Button("Up", GUILayout.Width(50)))
            {
                GUI.FocusControl(null); // Unfocus any field
                if (i > 0)
                {
                    statesProperty.MoveArrayElement(i, i - 1);
                }
            }

            if (GUILayout.Button("Down", GUILayout.Width(50)))
            {
                GUI.FocusControl(null); // Unfocus any field
                if (i < statesProperty.arraySize - 1)
                {
                    statesProperty.MoveArrayElement(i, i + 1);
                }
            }

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                GUI.FocusControl(null); // Unfocus any field
                statesProperty.DeleteArrayElementAtIndex(i);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            #endregion

            #region Exit time
            EditorGUILayout.BeginVertical();

            SerializedProperty hasExitTime = state.FindPropertyRelative("HasExitTime");
            SerializedProperty exitTime = state.FindPropertyRelative("ExitTime");

            EditorGUILayout.LabelField("Has Exit Time", GUILayout.Width(100));
            // EditorGUI.BeginDisabledGroup(Array.IndexOf(FixedStateNames, stateName.stringValue) > -1);
            hasExitTime.boolValue = EditorGUILayout.Toggle(hasExitTime.boolValue, GUILayout.Width(100));

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();

            if (hasExitTime.boolValue)
            {
                EditorGUILayout.LabelField("Exit Time", GUILayout.Width(100));
                exitTime.floatValue = EditorGUILayout.FloatField(exitTime.floatValue, GUILayout.Width(60));
            }
            // EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            #endregion

            #region Repeating
            EditorGUILayout.BeginVertical();

            SerializedProperty isRepeated = state.FindPropertyRelative("IsRepeated");
            SerializedProperty repeatDestStateName = state.FindPropertyRelative("RepeatDestStateName");
            SerializedProperty repeatCount = state.FindPropertyRelative("RepeatCount");

            EditorGUILayout.LabelField("Is Repeated", GUILayout.Width(80));

            EditorGUI.BeginDisabledGroup(Array.IndexOf(FixedStateNames, stateName.stringValue) > -1);
            isRepeated.boolValue = EditorGUILayout.Toggle(isRepeated.boolValue, GUILayout.Width(100));

            if (isRepeated.boolValue)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField("Repeat destination state");
                int repeatStateIndex = Array.FindIndex(stateList.States, s => s.StateName == repeatDestStateName.stringValue);
                repeatStateIndex = EditorGUILayout.Popup(repeatStateIndex, stateNames, GUILayout.Width(150));
                if (repeatStateIndex >= 0)
                {
                    repeatDestStateName.stringValue = stateList.States[repeatStateIndex].StateName;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField("Repeat Count", GUILayout.Width(80));
                repeatCount.intValue = EditorGUILayout.IntField(Math.Max(repeatCount.intValue, 1), GUILayout.Width(60));
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                if (!EditorGUIUtility.editingTextField) GUI.FocusControl(null); // Unfocus any field
                GameObject transition = GameObject.Find(stateName.stringValue)?.transform.Find("Transition")?.gameObject;
                if (transition != null)
                {
                    int destStateId = Array.FindIndex(stateList.States, s => s.StateName == destStateName.stringValue);
                    UpdateTransitionDestStateId(transition, destStateId, stateName.stringValue == "Trial - Rest");

                    UpdateTransitionExitTime(transition, hasExitTime.boolValue, exitTime.floatValue);

                    int repeatDestStateId = Array.FindIndex(stateList.States, s => s.StateName == repeatDestStateName.stringValue);
                    UpdateRepeatedTransition(transition, Math.Max(0, repeatDestStateId), isRepeated.boolValue ? repeatCount.intValue : 1);
                }
            }
        }

        if (GUILayout.Button("Add State"))
        {
            GUI.FocusControl(null); // Unfocus any field
            statesProperty.InsertArrayElementAtIndex(statesProperty.arraySize);
        }

        serializedStateList.ApplyModifiedProperties();

        UpdateSceneObjects();
    }

    private void UpdateSceneObjects()
    {
        GameObject statesObject = GameObject.Find("States");
        if (statesObject == null)
        {
            statesObject = new GameObject("States");
        }

        for (int i = 0; i < stateList.States.Length; i++)
        {
            string stateName = stateList.States[i].StateName;
            if (string.IsNullOrEmpty(stateName))
            {
                stateName = "State";
            }
            Transform child = statesObject.transform.Find(stateName);

            if (child == null)
            {
                child = FindChildByStateID(statesObject.transform, i);
                if (child != null)
                {
                    child.name = stateName;
                }
                else
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        GameObject newChild = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        newChild.name = stateName;
                        newChild.transform.SetParent(statesObject.transform);
                        child = newChild.transform;
                    }
                }
            }
            else if (child.name != stateName)
            {
                child.name = stateName;
            }

            if (child.GetSiblingIndex() != i)
            {
                child.SetSiblingIndex(i);
            }

            GameObject transition = child.Find("Transition")?.gameObject;
            if (transition != null)
            {
                UpdateTransitionCurrentStateId(transition, i);
                int destStateId = Array.FindIndex(stateList.States, s => s.StateName == stateList.States[i].DestStateName);
                UpdateTransitionDestStateId(transition, destStateId, stateList.States[i].StateName == "Trial - Rest");
            }
        }

        for (int i = statesObject.transform.childCount - 1; i >= stateList.States.Length; i--)
        {
            DestroyImmediate(statesObject.transform.GetChild(i).gameObject);
        }
    }


    private Transform FindChildByStateID(Transform parent, int stateID)
    {
        foreach (Transform child in parent)
        {
            GameObject transition = child.Find("Transition")?.gameObject;
            if (transition != null)
            {
                Component stateIdSettingComp = transition.GetComponent<ClusterVR.CreatorKit.Operation.Implements.ItemLogic>();
                if (stateIdSettingComp != null)
                {
                    SerializedObject serializedComp = new SerializedObject(stateIdSettingComp);
                    SerializedProperty specificProperty = serializedComp.FindProperty("logic.statements");

                    if (specificProperty != null && specificProperty.isArray && specificProperty.arraySize > 0)
                    {
                        for (int i = 0; i < specificProperty.arraySize; i++)
                        {
                            SerializedProperty targetKey = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.targetState.key");
                            if (targetKey != null && targetKey.stringValue == "state_id")
                            {
                                SerializedProperty stateIdProp = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.expression.value.constant.integerValue");
                                if (stateIdProp != null && stateIdProp.intValue == stateID)
                                {
                                    return child;
                                }
                            }
                        }
                    }
                }
            }
        }
        return null;
    }

    private void UpdateTransitionCurrentStateId(GameObject transition, int stateId)
    {
        Component stateIdSettingComp = transition.GetComponent<ClusterVR.CreatorKit.Operation.Implements.ItemLogic>();
        if (stateIdSettingComp != null)
        {
            SerializedObject serializedComp = new SerializedObject(stateIdSettingComp);
            SerializedProperty specificProperty = serializedComp.FindProperty("logic.statements");

            if (specificProperty != null && specificProperty.isArray && specificProperty.arraySize > 0)
            {
                for (int i = 0; i < specificProperty.arraySize; i++)
                {
                    SerializedProperty targetKey = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.targetState.key");
                    if (targetKey != null && targetKey.stringValue == "state_id")
                    {
                        SerializedProperty stateIdProp = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.expression.value.constant.integerValue");
                        if (stateIdProp != null)
                        {
                            stateIdProp.intValue = stateId;
                            serializedComp.ApplyModifiedProperties();
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Property not found: logic.statements.singleStatement.expression.value.constant.integerValue");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Property not found: logic.statements.singleStatement.targetState.key");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Property not found: logic.statements");
            }
        }
    }

    private void UpdateTransitionDestStateId(GameObject transition, int destStateId, bool isTrialRestState = false)
    {
        var globalLogics = transition.GetComponents<ClusterVR.CreatorKit.Operation.Implements.GlobalLogic>();
        Component transitionSettingLogic = null;
        foreach (var globalLogic in globalLogics)
        {
            SerializedObject serializedComp = new SerializedObject(globalLogic);
            var keyProp = serializedComp.FindProperty("globalGimmickKey.key.key");
            if (keyProp != null && keyProp.stringValue == "state_triggerTransition")
            {
                transitionSettingLogic = globalLogic;
                break;
            }
        }
        if (transitionSettingLogic != null)
        {
            SerializedObject serializedTransitionSettingLogic = new SerializedObject(transitionSettingLogic);
            SerializedProperty specificProperty = serializedTransitionSettingLogic.FindProperty("logic.statements");

            if (specificProperty != null && specificProperty.isArray && specificProperty.arraySize > 0)
            {
                for (int i = 0; i < specificProperty.arraySize; i++)
                {
                    SerializedProperty targetKey = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.targetState.key");
                    if (targetKey != null && targetKey.stringValue == "state_currentID")
                    {
                        SerializedProperty transitDestStateIdProp = isTrialRestState
                            ? specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.expression.operatorExpression.operands.Array.data[1].value.constant.integerValue")
                            : specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.expression.value.constant.integerValue");

                        if (transitDestStateIdProp != null)
                        {
                            transitDestStateIdProp.intValue = destStateId;
                            serializedTransitionSettingLogic.ApplyModifiedProperties();
                        }
                        else
                        {
                            Debug.LogWarning("Property not found: logic.statements.singleStatement.expression.operatorExpression.operands.Array.data[1].value.constant.integerValue or logic.statements.singleStatement.expression.value.constant.integerValue");
                        }

                        if (isTrialRestState)
                        {
                            SerializedProperty trialTaskStateIdProp = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.expression.operatorExpression.operands.Array.data[2].value.constant.integerValue");
                            if (trialTaskStateIdProp != null)
                            {
                                trialTaskStateIdProp.intValue = Array.FindIndex(stateList.States, s => s.StateName == "Trial - Task");
                                serializedTransitionSettingLogic.ApplyModifiedProperties();
                            }
                            else
                            {
                                Debug.LogWarning("Property not found: logic.statements.singleStatement.expression.operatorExpression.operands.Array.data[2].value.constant.integerValue");
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Property not found: logic.statements");
            }
        }
    }

    private void UpdateTransitionExitTime(GameObject transition, bool hasExitTime, float exitTime)
    {
        var itemTimers = transition.GetComponents<ClusterVR.CreatorKit.Operation.Implements.ItemTimer>();
        foreach (var itemTimer in itemTimers)
        {
            SerializedObject serializedComp = new SerializedObject(itemTimer);
            var keyProp = serializedComp.FindProperty("key.key");
            if (keyProp != null && (keyProp.stringValue == "state_enter" || keyProp.stringValue == "state_enter(disabled)"))
            {
                keyProp.stringValue = hasExitTime ? "state_enter" : "state_enter(disabled)";
                var delayTimeProp = serializedComp.FindProperty("delayTimeSeconds");
                delayTimeProp.floatValue = exitTime;
                serializedComp.ApplyModifiedProperties();
                break;
            }
        }
    }

    private void UpdateRepeatedTransition(GameObject transition, int repeatDestStateId = 0, int repeatCount = 1)
    {
        var globalLogics = transition.GetComponents<ClusterVR.CreatorKit.Operation.Implements.GlobalLogic>();
        Component transitionSettingLogic = null;
        foreach (var globalLogic in globalLogics)
        {
            SerializedObject serializedComp = new SerializedObject(globalLogic);
            var keyProp = serializedComp.FindProperty("globalGimmickKey.key.key");
            if (keyProp != null && keyProp.stringValue == "state_triggerTransitionToRepeat")
            {
                transitionSettingLogic = globalLogic;
                break;
            }
        }
        if (transitionSettingLogic != null)
        {
            SerializedObject serializedTransitionSettingLogic = new SerializedObject(transitionSettingLogic);
            SerializedProperty specificProperty = serializedTransitionSettingLogic.FindProperty("logic.statements");

            if (specificProperty != null && specificProperty.isArray && specificProperty.arraySize > 0)
            {
                for (int i = 0; i < specificProperty.arraySize; i++)
                {
                    SerializedProperty targetKey = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.targetState.key");
                    if (targetKey != null && targetKey.stringValue == "state_currentID")
                    {
                        SerializedProperty transitDestStateIdProp = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.expression.value.constant.integerValue");
                        if (transitDestStateIdProp != null)
                        {
                            transitDestStateIdProp.intValue = repeatDestStateId;
                            serializedTransitionSettingLogic.ApplyModifiedProperties();
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Property not found: logic.statements.singleStatement.expression.value.constant.integerValue");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Property not found: logic.statements.singleStatement.targetState.key");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Property not found: logic.statements");
            }
        }

        var stateIdSettingComp = transition.GetComponent<ClusterVR.CreatorKit.Operation.Implements.ItemLogic>();
        if (stateIdSettingComp != null)
        {
            SerializedObject serializedComp = new SerializedObject(stateIdSettingComp);
            SerializedProperty specificProperty = serializedComp.FindProperty("logic.statements");

            if (specificProperty != null && specificProperty.isArray && specificProperty.arraySize > 0)
            {
                for (int i = 0; i < specificProperty.arraySize; i++)
                {
                    SerializedProperty targetKey = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.targetState.key");
                    if (targetKey != null && targetKey.stringValue == "state_repeatCountMax")
                    {
                        SerializedProperty repeatCountMaxProp = specificProperty.GetArrayElementAtIndex(i).FindPropertyRelative("singleStatement.expression.value.constant.integerValue");
                        if (repeatCountMaxProp != null)
                        {
                            repeatCountMaxProp.intValue = repeatCount;
                            serializedComp.ApplyModifiedProperties();
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Property not found: logic.statements.singleStatement.expression.value.constant.integerValue");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Property not found: logic.statements.singleStatement.targetState.key");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Property not found: logic.statements");
            }
        }
    }
}
#endif
