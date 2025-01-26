using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.KaomoLab.CSEmulator.Components.Editor
{
    [CustomEditor(typeof(CSEmulatorPrefabItem))]
    public class CSEmulatorPrefabItemEditor
         : UnityEditor.Editor
    {
        const string UUID_CAPTION = @"ItemTemplateIdとは「7865f52c-1305-4489-b780-c3562109e5e8」というような文字列です。
クラフトアイテムの情報取得Windowで取得できます。";


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var id = serializedObject.FindProperty("_id");
            EditorGUILayout.PropertyField(
                id, new GUIContent("ItemTemplateId")
            );
            if (id.stringValue == "")
            {
                EditorGUILayout.HelpBox(
                    "ItemTemplateIdを入力してください。\n\n" + UUID_CAPTION,
                    MessageType.Warning
                );
            }
            else if (!KaomoLab.CSEmulator.Commons.IsUUID(id.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "ItemTemplateIdの形式ではありません。\n\n" + UUID_CAPTION,
                    MessageType.Error
                );
            }
            EditorGUILayout.LabelField(
                "ItemTemplateIdを入力すると、シーン上に配置した場合でも$.destroyできるようになります。",
                new GUIStyle(GUI.skin.label) { wordWrap = true }
            );

            serializedObject.ApplyModifiedProperties();

        }
    }
}
