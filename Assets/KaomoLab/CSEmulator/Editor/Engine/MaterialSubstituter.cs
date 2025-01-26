using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class MaterialSubstituter
        : EmulateClasses.IMaterialSubstituter
    {
        readonly Dictionary<int, Material> dict = new Dictionary<int, Material>();

        public MaterialSubstituter(

        )
        {
        }

        public Material Prepare(Material material)
        {
            if (!dict.ContainsKey(material.GetInstanceID()))
            {
                var clone = new Material(material);
                dict[material.GetInstanceID()] = clone;
            }
            return dict[material.GetInstanceID()];
        }

        public void Destroy()
        {
            foreach(var m in dict.Values)
            {
                UnityEngine.Object.Destroy(m);
            }
        }
    }
}
