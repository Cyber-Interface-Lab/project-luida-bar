using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ApiAudio
    {
        //structなのでnullではなく、各値のデフォルト値が入っている。
        readonly ClusterVR.CreatorKit.Item.ItemAudioSet itemAudioSet;
        readonly IRunningContext runningContext;

        readonly UnityEngine.GameObject root;
        readonly UnityEngine.GameObject gameObject;
        readonly UnityEngine.AudioSource audioSource;

        //1を超えて値を保持できるように
        float _volume;

        public ApiAudio(
            ClusterVR.CreatorKit.Item.ItemAudioSet itemAudioSet,
            IRunningContext runningContext,
            UnityEngine.GameObject root
        )
        {
            this.itemAudioSet = itemAudioSet;
            this.runningContext = runningContext;
            this.root = root;

            //ScriptableItemの子のような挙動をする。
            gameObject = new UnityEngine.GameObject();
            gameObject.transform.parent = root.transform;
            gameObject.transform.localPosition = UnityEngine.Vector3.zero;
            gameObject.transform.localRotation = UnityEngine.Quaternion.identity;
            gameObject.transform.localScale = UnityEngine.Vector3.one;

            this.audioSource = gameObject.AddComponent<UnityEngine.AudioSource>();
            audioSource.clip = itemAudioSet.AudioClip;
            audioSource.loop = itemAudioSet.Loop;
            audioSource.dopplerLevel = 0;
            audioSource.spatialBlend = 1.0f;
            _volume = audioSource.volume;
        }

        public float volume
        {
            get
            {
                if (runningContext.CheckTopLevel("ApiAudio.volume")) return 1;
                return _volume;
            }
            set
            {
                if (runningContext.CheckTopLevel("ApiAudio.volume")) return;
                //CSETODO audioSourceは1を超えて設定できないので、別の変数に入れおく。
                //なにかいい方法を見つける？
                _volume = Math.Clamp(value, 0f, 2.5f);
                audioSource.volume = value;
            }
        }

        public void attach(SubNode subNode)
        {
            if (runningContext.CheckTopLevel("ApiAudio.attach()")) return;
            gameObject.transform.localPosition = subNode.getPosition()._ToUnityEngine();
        }

        public void attachToRoot()
        {
            if (runningContext.CheckTopLevel("ApiAudio.attachToRoot()")) return;
            gameObject.transform.localPosition = UnityEngine.Vector3.zero;
        }

        public void play()
        {
            if (runningContext.CheckTopLevel("ApiAudio.play()")) return;
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.Play();
        }

        public void stop()
        {
            if (runningContext.CheckTopLevel("ApiAudio.stop()")) return;
            audioSource.Stop();
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            var clip = itemAudioSet.AudioClip;
            return String.Format("[ApiAudio][{0}:{1}]", itemAudioSet.Id, clip == null ? null : clip.name);
        }
    }
}
