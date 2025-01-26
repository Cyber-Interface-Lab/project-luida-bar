using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.KaomoLab.CSEmulator.Obsoletes.v1.Editor
{
    [CustomEditor(typeof(KaomoLabCSEmulator))]
    public class KaomoLabCSEmulatorEditor
         : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            var text = new TextElement();
            text.text = "v2よりコンポーネントの追加は不要になりました。\nこのコンポーネントは削除してください。";
            container.Add(text);

            return container;
        }
    }
}
