#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class StateDependentObjectEditor : EditorWindow
{
    private string prefabPath = "Assets/_ExpWorld_/Prefabs/State/StateDependent/StateDependentObject.prefab";
    private GameObject selectedGameObject;
    private GameObject instantiatedObject;
    private Component secondComponent;
    private Component thirdComponent;
    private SerializedObject serializedSecondComponent;
    private SerializedObject serializedThirdComponent;
    private string specificPropertyPath = "logic.statements";
    private int selectedStateIndex;
    private StateList stateList;

    [MenuItem("Window/Create State Dependent Object")]
    public static void ShowWindow()
    {
        GetWindow<StateDependentObjectEditor>("Create State Dependent Object");
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

        // Button to instantiate the prefab
        if (GUILayout.Button("Create new state dependent GameObject"))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            { 
                instantiatedObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instantiatedObject.name = prefab.name + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss");
                instantiatedObject.transform.position = Vector3.zero; // Optional: Set position to zero or any default position
                selectedGameObject = instantiatedObject; // Assign to the top GameObject field
                UpdateEditorWindowFields(selectedGameObject);
            }
        }

        // Field for selecting the GameObject
        selectedGameObject = (GameObject)EditorGUILayout.ObjectField("GameObject", selectedGameObject, typeof(GameObject), true);

        // Check if selectedGameObject is changed
        if (selectedGameObject != null && (serializedSecondComponent == null || serializedSecondComponent.targetObject != selectedGameObject))
        {
            UpdateEditorWindowFields(selectedGameObject);
        }

        // Hide other fields if the selectedGameObject is empty
        if (selectedGameObject == null)
        {
            return;
        }

        // Dropdown to select the state ID
        if (stateList.States.Length > 0)
        {
            string[] stateNames = new string[stateList.States.Length];
            for (int i = 0; i < stateList.States.Length; i++)
            {
                stateNames[i] = stateList.States[i].StateName;
            }
            selectedStateIndex = EditorGUILayout.Popup("State ID", selectedStateIndex, stateNames);
        }

        // Show and update the second component's properties
        if (secondComponent != null && serializedSecondComponent != null)
        {
            // Retrieve and display the specific property
            SerializedProperty specificProperty = serializedSecondComponent.FindProperty(specificPropertyPath);

            if (specificProperty != null && specificProperty.isArray && specificProperty.arraySize > 0)
            {
                SerializedProperty firstElement = specificProperty.GetArrayElementAtIndex(0).FindPropertyRelative("singleStatement.expression.value.constant.integerValue");
                if (firstElement != null)
                {
                    firstElement.intValue = selectedStateIndex;
                    serializedSecondComponent.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Property not found: {specificPropertyPath}");
            }
        }

        // Button to duplicate the asset in the third component's sourceCodeAsset field
        if (GUILayout.Button("Duplicate Asset in Third Component"))
        {
            DuplicateAssetInThirdComponent();
        }
    }

    private void UpdateEditorWindowFields(GameObject gameObject)
    {
        if (gameObject != null)
        {
            secondComponent = GetComponentByIndex(gameObject, 2);
            thirdComponent = GetComponentByIndex(gameObject, 3);

            if (secondComponent != null)
            {
                serializedSecondComponent = new SerializedObject(secondComponent);
                SerializedProperty specificProperty = serializedSecondComponent.FindProperty(specificPropertyPath);

                if (specificProperty != null && specificProperty.isArray && specificProperty.arraySize > 0)
                {
                    SerializedProperty firstElement = specificProperty.GetArrayElementAtIndex(0).FindPropertyRelative("singleStatement.expression.value.constant.integerValue");
                    if (firstElement != null)
                    {
                        selectedStateIndex = firstElement.intValue;
                    }
                }
            }

            if (thirdComponent != null)
            {
                serializedThirdComponent = new SerializedObject(thirdComponent);
            }
        }
    }

    private Component GetComponentByIndex(GameObject gameObject, int index)
    {
        Component[] components = gameObject.GetComponents<Component>();
        if (index >= 0 && index < components.Length)
        {
            return components[index];
        }
        return null;
    }

    private void DuplicateAssetInThirdComponent()
    {
        if (thirdComponent != null && serializedThirdComponent != null)
        {
            SerializedProperty sourceCodeAssetProp = serializedThirdComponent.FindProperty("sourceCodeAsset");
            if (sourceCodeAssetProp != null && sourceCodeAssetProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object originalAsset = sourceCodeAssetProp.objectReferenceValue;
                if (originalAsset != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(originalAsset);
                    string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CopyAsset(assetPath, newAssetPath);
                    AssetDatabase.SaveAssets();

                    Object newAsset = AssetDatabase.LoadAssetAtPath<Object>(newAssetPath);
                    sourceCodeAssetProp.objectReferenceValue = newAsset;
                    serializedThirdComponent.ApplyModifiedProperties();
                }
            }
        }
    }

    private void ShowNestedProperties(SerializedProperty property)
    {
        SerializedProperty nestedProp = property.Copy();
        int depth = nestedProp.depth;
        nestedProp.NextVisible(true);

        EditorGUI.indentLevel++;
        while (nestedProp.depth > depth)
        {
            EditorGUILayout.PropertyField(nestedProp, true);
            nestedProp.NextVisible(false);
        }
        EditorGUI.indentLevel--;
    }

    private static void DebugProperty(SerializedProperty property, string path = "")
    {
        // Build the property path
        string currentPath = string.IsNullOrEmpty(path) ? property.name : $"{path}.{property.name}";

        // Log the property details
        Debug.Log($"{currentPath} ({property.propertyType}): {GetPropertyValue(property)}");

        // Recursively debug child properties if the current property has children
        if (property.hasVisibleChildren)
        {
            SerializedProperty copy = property.Copy();
            if (copy.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(property, copy))
                        break;

                    DebugProperty(copy, currentPath);
                }
                while (copy.NextVisible(false));
            }
        }
    }

    private static string GetPropertyValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                return property.intValue.ToString();
            case SerializedPropertyType.Boolean:
                return property.boolValue.ToString();
            case SerializedPropertyType.Float:
                return property.floatValue.ToString();
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Color:
                return property.colorValue.ToString();
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue != null ? property.objectReferenceValue.name : "null";
            case SerializedPropertyType.LayerMask:
                return property.intValue.ToString();
            case SerializedPropertyType.Enum:
                return property.enumNames[property.enumValueIndex];
            case SerializedPropertyType.Vector2:
                return property.vector2Value.ToString();
            case SerializedPropertyType.Vector3:
                return property.vector3Value.ToString();
            case SerializedPropertyType.Vector4:
                return property.vector4Value.ToString();
            case SerializedPropertyType.Rect:
                return property.rectValue.ToString();
            case SerializedPropertyType.ArraySize:
                return property.arraySize.ToString();
            case SerializedPropertyType.Character:
                return ((char)property.intValue).ToString();
            case SerializedPropertyType.AnimationCurve:
                return property.animationCurveValue.ToString();
            case SerializedPropertyType.Bounds:
                return property.boundsValue.ToString();
            case SerializedPropertyType.Gradient:
                // Gradient is not directly accessible, you may need to handle this separately
                return "Gradient";
            case SerializedPropertyType.Quaternion:
                return property.quaternionValue.eulerAngles.ToString();
            default:
                return "Unknown Type";
        }
    }
}
#endif
