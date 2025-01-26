using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterVR.CreatorKit.World.Implements.WorldRuntimeSetting;

namespace Assets.KaomoLab.CSEmulator
{
    public static class Commons
    {
        public static float STANDARD_GRAVITY = -9.81f;

        static System.Text.RegularExpressions.Regex uuidPattern = new System.Text.RegularExpressions.Regex(
            "^([0-9a-fA-F]{8})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{12})$"
        );
        public static bool IsUUID(string str)
        {
            return uuidPattern.IsMatch(str);
        }

        public static int BuildLayerMask(params int[] maskBits)
        {
            int mask = 0;

            foreach (var bit in maskBits)
            {
                mask = mask | (1 << bit);
            }

            return mask;
        }

        public static string GetFullPath(this UnityEngine.GameObject gameObject)
        {
            return GetFullPath(gameObject.transform);
        }
        static string GetFullPath(UnityEngine.Transform transform)
        {
            string path = transform.name;
            var parent = transform.parent;
            while (parent)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            return path;
        }

        public static T AddComponent<T>(UnityEngine.GameObject gameObject)
            where T : UnityEngine.MonoBehaviour
        {
            var c = gameObject.GetComponent<T>();
            if (c == null)
            {
                return gameObject.AddComponent<T>();
            }
            else
            {
                return c;
            }
        }

        public static string ObjectArrayToString(
            object[] objectArray
        )
        {
            return "[" + String.Join(",", objectArray.Select(o =>
            {
                if (o is string str)
                    return "\"" + str + "\"";
                else if (o is System.Dynamic.ExpandoObject _eo)
                    return ExpandoObjectToString(_eo, openb: "{", closeb: "}", indent: "", separator: ",");
                return o?.ToString();
            })) + "]";
        }

        public static string ExpandoObjectToString(
            System.Dynamic.ExpandoObject eo,
            string openb = "{\n", string closeb = "}\n",
            string indent = " ", string separator = ",\n",
            int depth = 0, string _pref = "", string _suff = ""
        )
        {
            var pref = _pref + indent + openb;
            var suff = indent + closeb + _suff;
            depth++;
            var ind = String.Concat(Enumerable.Repeat(indent, depth).ToArray());
            var body = pref;
            foreach (var kv in eo)
            {
                body += ind + kv.Key + ":";
                if (kv.Value is System.Dynamic.ExpandoObject _eo)
                {
                    body += ExpandoObjectToString(_eo, openb, closeb, indent, separator, depth, _pref, _suff);
                }
                else if (kv.Value is object[] oa)
                {
                    body += ObjectArrayToString(oa);
                }
                else if (kv.Value is string str)
                {
                    body += "\"" + str + "\"";
                }
                else
                {
                    body += kv.Value?.ToString();
                }
                body += separator;
            }
            body += suff;

            return body;
        }

        public static UnityEngine.Vector2 Clone(this UnityEngine.Vector2 v)
        {
            return new UnityEngine.Vector2(v.x, v.y);
        }

        public static UnityEngine.Vector3 Clone(this UnityEngine.Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }

        public static UnityEngine.Quaternion Clone(this UnityEngine.Quaternion q)
        {
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static bool IsChild(this UnityEngine.GameObject child, UnityEngine.GameObject parent)
        {
            var target = child;
            while(target != null)
            {
                if (target == parent) return true;
                if (target.transform.parent == null) return false;
                target = target.transform.parent.gameObject;
            }
            return false;
        }

    }

    //CSETODO この辺のSettingsの扱いもいまいちな気がする。V3でなんとかしたい
    public class MovingPlatformSettings
    {
        public readonly bool UseMovingPlatform;
        public readonly bool MovingPlatformHorizontalInertia;
        public readonly bool MovingPlatformVerticalInertia;

        public MovingPlatformSettings(WorldRuntimeSetting worldRuntimeSetting)
        {
            UseMovingPlatform = worldRuntimeSetting?.UseMovingPlatform ?? WorldRuntimeSetting.DefaultValues.UseMovingPlatform;
            MovingPlatformHorizontalInertia = worldRuntimeSetting?.MovingPlatformHorizontalInertia ?? WorldRuntimeSetting.DefaultValues.MovingPlatformHorizontalInertia;
            MovingPlatformVerticalInertia = worldRuntimeSetting?.MovingPlatformVerticalInertia ?? WorldRuntimeSetting.DefaultValues.MovingPlatformVerticalInertia;
        }
    }
    public class MantlingSettings
    {
        public readonly bool UseMantling;

        public MantlingSettings(WorldRuntimeSetting worldRuntimeSetting)
        {
            UseMantling = worldRuntimeSetting?.UseMantling ?? WorldRuntimeSetting.DefaultValues.UseMantling;
        }
    }
    public class HudSettings
    {
        public readonly bool useClusterHudV2;

        public HudSettings(WorldRuntimeSetting worldRuntimeSetting)
        {
            var hudType = worldRuntimeSetting?.UseHUDType ?? WorldRuntimeSetting.DefaultValues.HUDType;
            useClusterHudV2 = hudType == ClusterVR.CreatorKit.Proto.WorldRuntimeSetting.Types.HUDType.ClusterHudV2;
        }
    }
    public class ClippingPlanesSettings
    {
        public readonly bool useCustomClippingPlanes;
        public readonly float nearPlane;
        public readonly float farPlane;

        public ClippingPlanesSettings(WorldRuntimeSetting worldRuntimeSetting)
        {
            useCustomClippingPlanes = worldRuntimeSetting?.UseCustomClippingPlanes ?? WorldRuntimeSetting.DefaultValues.UseCustomClippingPlanes;
            nearPlane = worldRuntimeSetting?.NearPlane ?? WorldRuntimeSetting.DefaultValues.NearPlane;
            farPlane = worldRuntimeSetting?.FarPlane ?? WorldRuntimeSetting.DefaultValues.FarPlane;
        }
    }
    public class CrouchSettings
    {
        public readonly bool enableCrouchWalk;

        public CrouchSettings(WorldRuntimeSetting worldRuntimeSetting)
        {
            var hudSettings = new HudSettings(worldRuntimeSetting);
            if(!hudSettings.useClusterHudV2)
            {
                enableCrouchWalk = false;
                return;
            }
            enableCrouchWalk = worldRuntimeSetting?.EnableCrouchWalk ?? WorldRuntimeSetting.DefaultValues.EnableCrouchWalk;
        }
    }
}
