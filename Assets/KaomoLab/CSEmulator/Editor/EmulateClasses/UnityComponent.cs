using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class UnityComponent
    {
        public interface IValueReflection
        {
            Type type { get; }
            object GetValue(object value);
            void SetValue(object target, object value);
        }
        public class FieldReflector : IValueReflection
        {
            public Type type { get; private set; }
            public readonly FieldInfo field;
            public FieldReflector(FieldInfo field)
            {
                this.field = field;
                this.type = field.FieldType;
            }
            public object GetValue(object value) => field.GetValue(value);
            public void SetValue(object target, object value) => field.SetValue(target, value);
        }
        public class PropertyReflector : IValueReflection
        {
            public Type type { get; private set; }
            public readonly PropertyInfo property;
            public PropertyReflector(PropertyInfo property)
            {
                this.property = property;
                this.type = property.PropertyType;
            }
            public object GetValue(object value) => property.GetValue(value);
            public void SetValue(object target, object value) => property.SetValue(target, value);
        }

        public interface IValueConverter
        {
            bool IsTarget(Type type);
            object Convert(object target);
        }
        public class TypedPassConverter
            : IValueConverter
        {
            readonly Type type;
            public TypedPassConverter(Type type)
            {
                this.type = type;
            }
            public bool IsTarget(Type type)
            {
                return this.type == type;
            }
            public object Convert(object target)
            {
                return target;
            }
        }


        public class TypedValueConverter
            : IValueConverter
        {
            readonly Type type;
            readonly Func<object, object> Converter;

            public TypedValueConverter(Type type, Func<object, object> Converter)
            {
                this.type = type;
                this.Converter = Converter;
            }
            public bool IsTarget(Type type)
            {
                return this.type == type;
            }
            public object Convert(object target)
            {
                var ret = Converter(target);
                return ret;
            }
        }

        public class EnumConverter
            : IValueConverter
        {
            readonly Func<object, object> Converter;

            public EnumConverter(Func<object, object> Converter)
            {
                this.Converter = Converter;
            }

            public bool IsTarget(Type type)
            {
                if (type.BaseType == typeof(Enum))
                {
                    return true;
                }
                return false;
            }

            public object Convert(object target)
            {
                var ret = Converter(target);
                return ret;
            }

        }

        public class UnityProp
            : DynamicObject
        {
            readonly Dictionary<string, IValueReflection> reflections = new();
            readonly IItemExceptionFactory itemExceptionFactory;
            readonly Component component;
            readonly List<string> supports; //nullあり
            readonly IValueConverter[] getConverters;
            readonly IValueConverter[] setConverters;

            public UnityProp(
                IItemExceptionFactory itemExceptionFactory,
                Component component,
                List<string> supports,
                IValueConverter[] getConverters,
                IValueConverter[] setConverters
            )
            {
                this.itemExceptionFactory = itemExceptionFactory;
                this.component = component;
                this.supports = supports;
                this.getConverters = getConverters;
                this.setConverters = setConverters;
                BuildReflections(component.GetType());
            }
            void BuildReflections(Type type)
            {
                var flags = BindingFlags.Instance | BindingFlags.Public;
                foreach (var f in type.GetFields(flags))
                {
                    if (reflections.ContainsKey(f.Name)) continue;
                    reflections.Add(f.Name, new FieldReflector(f));
                }
                foreach (var p in type.GetProperties(flags))
                {
                    if (reflections.ContainsKey(p.Name)) continue;
                    reflections.Add(p.Name, new PropertyReflector(p));
                }
                foreach(var i in type.GetInterfaces())
                {
                    BuildReflections(i);
                }
                if(type.BaseType != null) BuildReflections(type.BaseType);
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (supports != null && !supports.Contains(binder.Name))
                {
                    throw itemExceptionFactory.CreateGeneral(String.Format("{0}.{1}はサポート外です。", component.GetType().Name, binder.Name));
                }
                if (!reflections.ContainsKey(binder.Name))
                {
                    Debug.LogWarning(String.Format("{0}.{1}はありません。", component.GetType().Name, binder.Name));
                    result = null;
                    return true;
                }
                var reflection = reflections[binder.Name];
                var ret = reflection.GetValue(component);

                foreach(var conveter in getConverters)
                {
                    if (!conveter.IsTarget(reflection.type)) continue;
                    ret = conveter.Convert(ret);
                    result = ret;
                    return true;
                }
                //converterにないものはnull
                result = null;
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                if (supports != null && !supports.Contains(binder.Name))
                {
                    throw itemExceptionFactory.CreateGeneral(String.Format("{0}.{1}はサポート外です。", component.GetType().Name, binder.Name));
                }
                if (!reflections.ContainsKey(binder.Name))
                {
                    throw itemExceptionFactory.CreateGeneral(String.Format("{0}.{1}はありません。", component.GetType().Name, binder.Name));
                }

                var reflection = reflections[binder.Name];
                var ret = value;
                foreach (var conveter in setConverters)
                {
                    if (!conveter.IsTarget(reflection.type)) continue;
                    ret = conveter.Convert(ret);
                    reflection.SetValue(component, ret);
                    return true;
                }
                //converterにないものは無視
                UnityEngine.Debug.LogWarning(String.Format("扱えない型のプロパティです。{0}", binder.Name));
                return false;
            }
        }

        public interface IMethodWrapper
        {
            public void OnClick(Action<bool> Callback);
            public void Play();
            public void Stop();

            public void SetBool(string id, bool value);
            public void SetFloat(string id, float value);
            public void SetInteger(string id, int value);
            public void SetTrigger(string id);

            public static void NotSupport(IItemExceptionFactory itemExceptionFactory, Component target, string name)
            {
                throw itemExceptionFactory.CreateGeneral(String.Format("{0}は{1}に対応していません。", target.GetType().Name, name));
            }
        }
        public class NotSupportedWrapper<T>
            : IMethodWrapper where T : Component
        {
            protected readonly IItemExceptionFactory itemExceptionFactory;
            protected readonly T component;

            public NotSupportedWrapper(
                IItemExceptionFactory itemExceptionFactory,
                T component
            )
            {
                this.itemExceptionFactory = itemExceptionFactory;
                this.component = component;
            }

            public virtual void OnClick(Action<bool> Callback) => IMethodWrapper.NotSupport(itemExceptionFactory, component, "onClick");
            public virtual void Play() => IMethodWrapper.NotSupport(itemExceptionFactory, component, "play");
            public virtual void Stop() => IMethodWrapper.NotSupport(itemExceptionFactory, component, "stop");
            public virtual void SetBool(string id, bool value) => IMethodWrapper.NotSupport(itemExceptionFactory, component, "setBool");
            public virtual void SetFloat(string id, float value) => IMethodWrapper.NotSupport(itemExceptionFactory, component, "setFloat");
            public virtual void SetInteger(string id, int value) => IMethodWrapper.NotSupport(itemExceptionFactory, component, "setInteger");
            public virtual void SetTrigger(string id) => IMethodWrapper.NotSupport(itemExceptionFactory, component, "setTrigger");
        }
        public class AnimatorWrapper
            : NotSupportedWrapper<Animator>
        {
            public AnimatorWrapper(
                IItemExceptionFactory itemExceptionFactory,
                Animator animator
            ) : base(itemExceptionFactory, animator)
            {
            }

            public override void SetBool(string id, bool value) => component.SetBool(id, value);
            public override void SetFloat(string id, float value) => component.SetFloat(id, value);
            public override void SetInteger(string id, int value) => component.SetInteger(id, value);
            public override void SetTrigger(string id) => component.SetTrigger(id);
        }
        public abstract class PlayableWrapper<T>
            : NotSupportedWrapper<T> where T : Component
        {
            public PlayableWrapper(
                IItemExceptionFactory itemExceptionFactory,
                T component
            ) : base(itemExceptionFactory, component)
            {
            }

            public abstract override void Play();
            public abstract override void Stop();
        }
        public class PlayableDirectorWrapper
            : PlayableWrapper<PlayableDirector>
        {
            public PlayableDirectorWrapper(
                IItemExceptionFactory itemExceptionFactory,
                PlayableDirector playableDirector
            ) : base(itemExceptionFactory, playableDirector) { }
            public override void Play()
            {
                component.Stop();
                component.Play();
            }
            public override void Stop() => component.Stop();
        }
        public class AudioSourceWrapper
            : PlayableWrapper<AudioSource>
        {
            public AudioSourceWrapper(
                IItemExceptionFactory itemExceptionFactory,
                AudioSource audioSource
            ) : base(itemExceptionFactory, audioSource) { }
            public override void Play()
            {
                component.Stop();
                component.Play();
            }
            public override void Stop() => component.Stop();
        }
        public class ParticleSystemWrapper
            : PlayableWrapper<ParticleSystem>
        {
            public ParticleSystemWrapper(
                IItemExceptionFactory itemExceptionFactory,
                ParticleSystem particleSystem
            ) : base(itemExceptionFactory, particleSystem) { }
            public override void Play()
            {
                component.Stop();
                component.Play();
            }
            public override void Stop() => component.Stop();
        }
        public class VideoPlayerWrapper
            : PlayableWrapper<VideoPlayer>
        {
            public VideoPlayerWrapper(
                IItemExceptionFactory itemExceptionFactory,
                VideoPlayer videoPlayer
            ) : base(itemExceptionFactory, videoPlayer) { }
            public override void Play()
            {
                component.Stop();
                component.Play();
            }
            public override void Stop() => component.Stop();
        }
        public class ButtonWrapper
            : NotSupportedWrapper<Button>
        {
            public ButtonWrapper(
                IItemExceptionFactory itemExceptionFactory, Button component
            ) : base(itemExceptionFactory, component)
            {
            }
            public override void OnClick(Action<bool> Callback)
            {
                var c = component.gameObject.GetComponent<Components.CSEmulatorLocalUIButton>();
                if (c == null) return;
                c.SetOnClickCallback(Callback);
            }
        }

        static Dictionary<string, List<string>> scriptableItemSupports = new()
        {
            { "Animator", new List<string>(){
            }},
            { "AudioSource", new List<string>(){
                "bypassEffects",
                "bypassListenerEffects",
                "bypassReverbZones",
                "dopplerLevel",
                "loop",
                "maxDistance",
                "minDistance",
                "mute",
                "panStereo",
                "pitch",
                "playOnAwake",
                "priority",
                "spatialize",
                "spatializePostEffects",
            }},
            { "Button", new List<string>(){
                "interactable",
                "transition",
            }},
            { "Camera", new List<string>(){
                "allowMSAA",
                "backgroundColor",
                "depth",
                "farClipPlane",
                "fieldOfView",
                "focalLength",
                "forceIntoRenderTexture",
                "allowHDR",
                "lensShift",
                "nearClipPlane",
                "useOcclusionCulling",
                "orthographic",
                "orthographicSize",
                "stereoConvergence",
                "stereoSeparation",
            }},
            { "Canvas", new List<string>(){
                "overridePixelPerfect",
                "overrideSorting",
                "pixelPerfect",
                "planeDistance",
                "normalizedSortingGridSize",
            }},
            { "CanvasGroup", new List<string>(){
                "alpha",
                "blocksRaycasts",
                "ignoreParentGroups",
                "interactable",
            }},
            { "BoxCollider", new List<string>(){
                "center",
                "isTrigger",
                "size",
            }},
            { "CapsuleCollider", new List<string>(){
                "center",
                "height",
                "isTrigger",
                "radius",
            }},
            { "GridLayoutGroup", new List<string>(){
                "cellSize",
                "childAlignment",
                "constraint",
                "constraintCount",
                "spacing",
                "startAxis",
                "startCorner",
            }},
            { "HorizontalLayoutGroup", new List<string>(){
                "childAlignment",
                "childControlHeight",
                "childControlWidth",
                "childForceExpandHeight",
                "childForceExpandWidth",
                "childScaleHeight",
                "childScaleWidth",
                "reverseArrangement",
                "spacing",
            }},
            { "Image", new List<string>(){
                "color",
                "fillAmount",
                "fillCenter",
                "fillClockwise",
                "fillMethod",
                "fillOrigin",
                "maskable",
                "pixelsPerUnitMultiplier",
                "preserveAspect",
                "raycastPadding",
                "raycastTarget",
                "type",
                "useSpriteMesh",
            }},
            { "MeshCollider", new List<string>(){
                "convex",
                "isTrigger",
            }},
            { "MeshRenderer", new List<string>(){
                "receiveShadows",
                "rendererPriority",
                "sortingOrder",
            }},
            { "ParticleSystem", new List<string>(){
            }},
            { "PlayableDirector", new List<string>(){
            }},
            { "PositionConstraint", new List<string>(){
                "constraintActive",
                "translationAxis",
                "translationAtRest",
                "translationOffset",
                "weight",
            }},
            { "PostProcessVolume", new List<string>(){
                "blendDistance",
                "isGlobal",
                "priority",
                "weight",
            }},
            { "RawImage", new List<string>(){
                "color",
                "maskable",
                "raycastPadding",
                "raycastTarget",
            }},
            { "RectTransform", new List<string>(){
                "anchorMax",
                "anchorMin",
                "anchoredPosition",
                "pivot",
                "localPosition",
                "localScale",
                "sizeDelta",
            }},
            { "Rigidbody", new List<string>(){
                "angularDrag",
                "drag",
                "isKinematic",
                "mass",
                "useGravity",
            }},
            { "RotationConstraint", new List<string>(){
                "constraintActive",
                "rotationAxis",
                "rotationAtRest",
                "rotationOffset",
                "weight",
            }},
            { "ScaleConstraint", new List<string>(){
                "constraintActive",
                "scalingAxis",
                "scaleAtRest",
                "scaleOffset",
                "weight",
            }},
            { "SkinnedMeshRenderer", new List<string>(){
                "receiveShadows",
                "rendererPriority",
                "skinnedMotionVectors",
                "sortingOrder",
                "updateWhenOffscreen",
            }},
            { "SphereCollider", new List<string>(){
                "center",
                "isTrigger",
                "radius",
            }},
            { "Text", new List<string>(){
                "text",
                "color",
                "maskable",
                "raycastPadding",
                "raycastTarget",
            }},
            { "Transform", new List<string>(){
                "localPosition",
                "localRotation",
                "localScale",
            }},
            { "VerticalLayoutGroup", new List<string>(){
                "childAlignment",
                "childControlHeight",
                "childControlWidth",
                "childForceExpandHeight",
                "childForceExpandWidth",
                "childScaleHeight",
                "childScaleWidth",
                "reverseArrangement",
                "spacing",
            }},
            { "VideoPlayer", new List<string>(){
                "sendFrameReadyEvents",
                "isLooping",
                "playOnAwake",
                "playbackSpeed",
                "skipOnDrop",
                "targetCameraAlpha",
                "waitForFirstFrame",
            }},
        };

        public static UnityComponent GetScriptableItemUnityComponent(
            GameObject gameObject, string type, IItemExceptionFactory itemExceptionFactory
        )
        {
            if (!scriptableItemSupports.ContainsKey(type))
                throw itemExceptionFactory.CreateGeneral(String.Format("Component:{0}には対応していません。", type));

            var component = gameObject.GetComponent(type);
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(String.Format("Component:{0}は{1}にありません。", type, gameObject.name));
                return null;
            }

            var members = scriptableItemSupports[type];
            var ret = new UnityComponent(component, members, itemExceptionFactory);
            return ret;
        }

        public static UnityComponent GetPlayerLocalUnityComponent(
            GameObject gameObject, string type, IItemExceptionFactory itemExceptionFactory
        )
        {
            if (!scriptableItemSupports.ContainsKey(type))
                throw itemExceptionFactory.CreateGeneral(String.Format("Component:{0}には対応していません。", type));

            var component = gameObject.GetComponent(type);
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(String.Format("Component:{0}は{1}にありません。", type, gameObject.name));
                return null;
            }

            var ret = new UnityComponent(component, null, itemExceptionFactory);
            return ret;
        }

        public readonly UnityProp unityProp;

        readonly Component component;
        readonly List<string> supportMembers;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly IMethodWrapper methodWrapper;

        UnityComponent(
            Component component,
            List<string> supportMembers,
            IItemExceptionFactory itemExceptionFactory
        )
        {
            this.component = component;
            this.supportMembers = supportMembers;
            this.itemExceptionFactory = itemExceptionFactory;
            unityProp = new UnityProp(itemExceptionFactory, component, supportMembers, new IValueConverter[] {
                new TypedPassConverter(typeof(bool)),
                new TypedValueConverter(typeof(int), v => (int)v),
                new TypedValueConverter(typeof(double), v => (float)(double)v),
                new TypedValueConverter(typeof(float), v => (float)v),
                new TypedPassConverter(typeof(string)),
                new EnumConverter(v => (int)v),
                new TypedValueConverter(typeof(Vector2), v => new EmulateVector2((Vector2)v)),
                new TypedValueConverter(typeof(Vector3), v => new EmulateVector3((Vector3)v)),
                new TypedValueConverter(typeof(Quaternion), v => new EmulateQuaternion((Quaternion)v)),
                new TypedValueConverter(typeof(Vector4), v => new EmulateVector4((Vector4)v)),
                new TypedValueConverter(typeof(Color), v => new EmulateColor((Color)v)),
            }, new IValueConverter[] {
                new TypedPassConverter(typeof(bool)),
                new TypedValueConverter(typeof(int), v => (int)(double)v),
                new TypedValueConverter(typeof(double), v => (float)(double)v),
                new TypedValueConverter(typeof(float), v => (float)(double)v),
                new TypedPassConverter(typeof(string)),
                new EnumConverter(v => (int)(double)v),
                new TypedValueConverter(typeof(Vector2), v => ((EmulateVector2)v)._ToUnityEngine()),
                new TypedValueConverter(typeof(Vector3), v => ((EmulateVector3)v)._ToUnityEngine()),
                new TypedValueConverter(typeof(Quaternion), v => ((EmulateQuaternion)v)._ToUnityEngine()),
                new TypedValueConverter(typeof(Vector4), v => ((EmulateVector4)v)._ToUnityEngine()),
                new TypedValueConverter(typeof(Color), v => ((EmulateColor)v)._ToUnityEngine()),
            });
            this.methodWrapper = CreateMethodWrapper(itemExceptionFactory, component);
        }
        IMethodWrapper CreateMethodWrapper(
            IItemExceptionFactory itemExceptionFactory, Component component
        )
        {
            if (component is Animator animator)
            {
                return new AnimatorWrapper(itemExceptionFactory, animator);
            }
            if (component is PlayableDirector playableDirector)
            {
                return new PlayableDirectorWrapper(itemExceptionFactory, playableDirector);
            }
            if (component is AudioSource audioSource)
            {
                return new AudioSourceWrapper(itemExceptionFactory, audioSource);
            }
            if (component is ParticleSystem particleSystem)
            {
                return new ParticleSystemWrapper(itemExceptionFactory, particleSystem);
            }
            if (component is VideoPlayer videoPlayer)
            {
                return new VideoPlayerWrapper(itemExceptionFactory, videoPlayer);
            }
            if (component is Button button)
            {
                var local = component.GetComponentInParent<ClusterVR.CreatorKit.World.Implements.PlayerLocalUI.PlayerLocalUI>();
                if (local == null)
                {
                    //onClickに反応するのはPlayerLocalUIのみ
                    return new NotSupportedWrapper<Button>(itemExceptionFactory, button);
                }

                return new ButtonWrapper(itemExceptionFactory, button);
            }
            return new NotSupportedWrapper<Component>(itemExceptionFactory, component);
        }

        public void onClick(Action<bool> Callback)
        {
            methodWrapper.OnClick(Callback);
        }

        public void play()
        {
            methodWrapper.Play();
        }

        public void setBool(string id, bool value)
        {
            methodWrapper.SetBool(id, value);
        }

        public void setFloat(string id, float value)
        {
            methodWrapper.SetFloat(id, value);
        }

        public void setInteger(string id, int value)
        {
            methodWrapper.SetInteger(id, value);
        }

        public void setTrigger(string id)
        {
            methodWrapper.SetTrigger(id);
        }

        public void stop()
        {
            methodWrapper.Stop();
        }

        public object toJSON(string key)
        {
            //return this;
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return String.Format("[UnityComponent][{0}][{1}]", component.GetType().Name, component.name);
        }
    }
}
