using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class IconAsset
    {
        public Texture2D texture2D { get; private set; }

        public IconAsset(
            Texture2D texture2D
        )
        {
            this.texture2D = texture2D;
        }
        public IconAsset()
        {
#if UNITY_EDITOR
            this.texture2D = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/KaomoLab/CSEmulator/UIs/NoIcon.png");
#endif
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return String.Format("[IconAsset]");
        }
    }
}
