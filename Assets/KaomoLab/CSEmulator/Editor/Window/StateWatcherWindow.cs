using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ClusterVR.CreatorKit.Item;
using ClusterVR.CreatorKit.Item.Implements;
using Assets.KaomoLab.CSEmulator.Components.Editor;
using System.Text.RegularExpressions;

namespace Assets.KaomoLab.CSEmulator.Editor.Window
{
    public class StateWatcherWindow : EditorWindow
    {
        enum WatchState
        {
            Stop, WaitInitialize, Watch
        }

        [SerializeField] GameObject item;
        [SerializeField] string showRegex = ".+";
        bool openSettings = true;
        Dictionary<string, bool> folds = new();
        Vector2 scrollPosition;

        StateWatcherWindow window;

        [MenuItem("Window/かおもラボ/CSEmulatorStateWatcher")]
        public static StateWatcherWindow ShowWindow()
        {
            var w = CreateInstance<StateWatcherWindow>();
            w.titleContent = new GUIContent("$.state");
            w.Show();
            w.window = w;
            return w;
        }

        void OnGUI()
        {
            var serialized = new SerializedObject(this);
            serialized.Update();

            openSettings = EditorGUILayout.Foldout( openSettings, "設定" );
            window.titleContent = new GUIContent(item == null ? "$.state" : String.Format("{0} $.state", item.name));

            if (openSettings)
            {
                if (GUILayout.Button("別のウィンドウを出す"))
                {
                    var window = ShowWindow();
                    var p = this.position;
                    p.xMin += 10;
                    p.yMin += 10;
                    p.xMax += 10;
                    p.yMax += 10;
                    window.position = p;
                    return;
                }

                EditorGUILayout.PropertyField(
                    serialized.FindProperty("item"), new GUIContent("対象アイテム")
                );

                EditorGUILayout.PropertyField(
                    serialized.FindProperty("showRegex"), new GUIContent("表示パターン", "この正規表現にマッチした変数名が表示されます")
                );
            }
            var showPattern = new Regex(".+");
            try { showPattern = new Regex(showRegex); }
            catch (Exception)
            {
                EditorGUILayout.HelpBox("正規表現として扱えません", MessageType.Error);
            }

            var watcher = GetComponent<Components.CSEmulatorStateWatcher>(item); //item?.だとnull判定されない？

            //この辺からのw != nullをなんとかきれいに書きたい
            if (watcher != null) watcher.OnVariablesUpdated -= OnVariablesUpdated;
            serialized.ApplyModifiedProperties(); //このタイミングでitemに反映される
            if (watcher != null) watcher.OnVariablesUpdated += OnVariablesUpdated;

            if(item == null)
            {
                EditorGUILayout.HelpBox("Itemを指定してください", MessageType.Info);
            }
            if (item != null && !item.GetComponent<Item>())
            {
                EditorGUILayout.HelpBox("Itemを指定してください", MessageType.Error);
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("値はプレビュー時に表示されます", MessageType.Info);
                return;
            }
            if (watcher != null)
            {
                using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = scroll.scrollPosition;
                    CSEmulatorStateWatcherEditor.ShowVariables(
                        watcher.GetVariables(), "/", folds, showPattern
                    );
                }
            }
        }

        T GetComponent<T>(GameObject gameObject)
        {
            if (gameObject == null) return default(T);
            return gameObject.GetComponent<T>();
        }

        private void OnVariablesUpdated()
        {
            Repaint();
        }
    }
}
