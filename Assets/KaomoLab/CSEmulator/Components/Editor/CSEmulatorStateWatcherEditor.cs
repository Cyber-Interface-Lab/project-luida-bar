using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

namespace Assets.KaomoLab.CSEmulator.Components.Editor
{
    [CustomEditor(typeof(CSEmulatorStateWatcher))]
    public class CSEmulatorStateWatcherEditor
         : UnityEditor.Editor
    {
        private void OnEnable()
        {
            var watcher = (CSEmulatorStateWatcher)target;
            watcher.OnVariablesUpdated -= UpdateVariables;
            watcher.OnVariablesUpdated += UpdateVariables;
        }
        private void OnDisable()
        {
            var watcher = (CSEmulatorStateWatcher)target;
            watcher.OnVariablesUpdated -= UpdateVariables;
        }
        void UpdateVariables()
        {
            this.Repaint();
        }

        public override void OnInspectorGUI()
        {
            var watcher = (CSEmulatorStateWatcher)target;
            ShowVariables(watcher.GetVariables(), "/", watcher.foldouts, new Regex(".+"));
        }
        public static void ShowVariables(
            IEnumerable<IVariable> variables, string path, Dictionary<string, bool> folds, Regex showPattern
        )
        {
            bool hasVariables = false;
            foreach(var variable in variables)
            {
                hasVariables = true;
                if(variable.hasChild)
                {
                    if (showPattern.IsMatch(variable.name)) ShowParentVariable(variable, path, folds, showPattern);
                }
                else
                {
                    if(showPattern.IsMatch(variable.name)) ShowVariable(variable);
                }
            }
            if (!hasVariables)
            {
                EditorGUILayout.LabelField(
                    "表示できる値はありません"
                );
            }
        }

        public static void ShowParentVariable(IVariable variable, string path, Dictionary<string, bool> folds, Regex showPattern)
        {
            var newPath = String.Format("{0}{1}/", path, variable.name);
            if (!folds.ContainsKey(newPath)) folds[newPath] = false;

            folds[newPath] = EditorGUILayout.Foldout(
                folds[newPath], String.Format("{0}({1})", variable.name, variable.type)
            );
            if (!folds[newPath]) return;

            using (new EditorGUI.IndentLevelScope())
            {
                ShowVariables(variable.children, newPath, folds, showPattern);
            }

        }

        public static void ShowVariable(IVariable variable)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    String.Format("{0}({1})", variable.name, variable.type),
                    new GUIStyle(GUI.skin.label) { wordWrap = false },
                    GUILayout.MinWidth(150)
                );
                EditorGUILayout.SelectableLabel(
                    variable.value,
                    new GUIStyle(GUI.skin.textField) { wordWrap = false, padding = new RectOffset(0, 0, 0, 0) },
                    GUILayout.ExpandWidth(true), GUILayout.Height(15)
                );
            }
        }
    }
}
