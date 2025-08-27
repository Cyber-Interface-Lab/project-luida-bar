#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RenameGrandchildren
{
    private const string MENU_PATH = "Tools/Rename Grandchildren from Parent Name";

    /// <summary>
    /// Creates a menu item that renames the first child of specifically named children
    /// of the selected GameObject.
    /// </summary>
    [MenuItem(MENU_PATH)]
    private static void RenameFirstGrandchild()
    {
        // Get the currently selected GameObject in the Hierarchy.
        GameObject selectedObject = Selection.activeGameObject;

        // --- Error Handling and Validation ---
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a parent GameObject in the Hierarchy.", "OK");
            return;
        }

        if (selectedObject.transform.childCount == 0)
        {
            Debug.LogWarning($"The selected object '{selectedObject.name}' has no children to process.", selectedObject);
            return;
        }

        int renameCount = 0;
        const string CHILD_PREFIX = "other_";
        const string NEW_NAME_PREFIX = "Avatar_";

        // Group all changes into a single Undo action.
        Undo.SetCurrentGroupName("Rename Grandchildren");
        int group = Undo.GetCurrentGroup();

        // --- Main Logic: Iterate through all direct children ---
        foreach (Transform childTransform in selectedObject.transform)
        {
            // Check if the child's name matches the pattern "other_{i}"
            if (childTransform.name.StartsWith(CHILD_PREFIX))
            {
                // Extract the number part of the name.
                string numberPart = childTransform.name.Substring(CHILD_PREFIX.Length);
                
                // Try to parse the extracted part as an integer.
                if (int.TryParse(numberPart, out int index))
                {
                    // Check if this child has any children of its own (grandchildren).
                    if (childTransform.childCount > 0)
                    {
                        // Get the first grandchild.
                        Transform grandchildTransform = childTransform.GetChild(0);
                        GameObject grandchildObject = grandchildTransform.gameObject;

                        // Construct the new name.
                        string newName = $"{NEW_NAME_PREFIX}{index}";

                        // Record the object state before changing it, for Undo functionality.
                        Undo.RecordObject(grandchildObject, "Rename Grandchild");

                        // Apply the new name.
                        grandchildObject.name = newName;
                        renameCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"Child '{childTransform.name}' has no children to rename.", childTransform.gameObject);
                    }
                }
                else
                {
                     Debug.LogWarning($"Child '{childTransform.name}' starts with prefix but does not have a valid number suffix.", childTransform.gameObject);
                }
            }
        }
        
        // Collapse all recorded changes into one undo step in the history.
        Undo.CollapseUndoOperations(group);

        // --- Final Feedback ---
        if (renameCount > 0)
        {
            Debug.Log($"Successfully renamed {renameCount} grandchildren under '{selectedObject.name}'.");
        }
        else
        {
            Debug.Log($"Process complete. No matching children found to rename under '{selectedObject.name}'.");
        }
    }
}
#endif
