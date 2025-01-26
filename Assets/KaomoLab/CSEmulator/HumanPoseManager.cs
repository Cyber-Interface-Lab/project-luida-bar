using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterVR.CreatorKit.Item.Implements;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{
    public class HumanPoseManager
    {

        //そのうちHumanPoseManagerに統合させる。別ファイルにすると統合後にunitypackageを上書きした時に残るのが厄介なため。
        public class RidingPoseManager
        {
            readonly IHumanoidAnimationCreator humanoidAnimationCreator;
            readonly Animator humanoidAnimator;
            readonly AnimationClip defaultAnimationClip;
            readonly HumanPoseHandler poseHandler;

            readonly Quaternion hipRotationOffset;
            readonly Quaternion spineRotationOffset;
            readonly Quaternion chestRotationOffset;

            HumanoidAnimation humanoidAnimation = null; //CCK依存にしたくなかったけども仕方なし
            Vector3 hipPosition;
            Quaternion hipRotation;
            public bool isRiding { get; private set; } = false;
            long lastTick = -1;
            float nowSampleSeconds = 0;

            public RidingPoseManager(
                IHumanoidAnimationCreator humanoidAnimationCreator,
                Animator humanoidAnimator,
                AnimationClip defaultAnimationClip
            )
            {
                this.humanoidAnimationCreator = humanoidAnimationCreator;
                this.humanoidAnimator = humanoidAnimator;
                this.defaultAnimationClip = defaultAnimationClip;
                poseHandler = new HumanPoseHandler(
                    humanoidAnimator.avatar,
                    humanoidAnimator.transform
                );
                //PositionOffsetも一応あるけども3cm相当＋身長によって変化なので無視
                hipRotationOffset = Quaternion.Euler(340, 0, 0); //CCK2.25.0実測値
                spineRotationOffset = Quaternion.Euler(8, 0, 0); //CCK2.25.0実測値
                chestRotationOffset = Quaternion.Euler(22, 0, 0); //CCK2.25.0実測値
            }

            public void GetOn(
                AnimationClip overrideAnimationClip
            )
            {
                isRiding = true;
                lastTick = DateTime.Now.Ticks;
                nowSampleSeconds = 0;
                humanoidAnimation = humanoidAnimationCreator.Create(
                    overrideAnimationClip == null ? defaultAnimationClip : overrideAnimationClip
                );
            }

            public void GetOff()
            {
                isRiding = false;
            }

            public void SetHip(
                Vector3 position,
                Quaternion rotation
            )
            {
                this.hipPosition = position;
                this.hipRotation = rotation;
            }

            public void Apply()
            {
                if (!isRiding) return;

                var nowTicks = DateTime.Now.Ticks;
                var deltaTicks = nowTicks - lastTick;
                lastTick = nowTicks;
                var deltaSec = (float)((double)deltaTicks / TimeSpan.TicksPerSecond);
                nowSampleSeconds += deltaSec;
                if (humanoidAnimation.Length < nowSampleSeconds) nowSampleSeconds = 0;

                var partialPose = humanoidAnimation.Sample(nowSampleSeconds);
                var pose = new HumanPose();
                if (partialPose.CenterRotation.HasValue)
                    pose.bodyPosition = partialPose.CenterPosition.Value;
                if (partialPose.CenterRotation.HasValue)
                    pose.bodyRotation = partialPose.CenterRotation.Value;
                pose.muscles = partialPose.Muscles.Select(v => v.HasValue ? v.Value : 0).ToArray();

                poseHandler.SetHumanPose(ref pose);

                //headのIKの近似。hipからheadまで固定しておく。
                var hip = humanoidAnimator.GetBoneTransform(HumanBodyBones.Hips);
                hip.transform.position = hipPosition;
                hip.transform.rotation = hipRotation * hipRotationOffset;
                var spine = humanoidAnimator.GetBoneTransform(HumanBodyBones.Spine);
                spine.transform.rotation = hip.transform.rotation * spineRotationOffset;
                var chest = humanoidAnimator.GetBoneTransform(HumanBodyBones.Chest);
                chest.transform.rotation = spine.transform.rotation * chestRotationOffset;
                var neck = humanoidAnimator.GetBoneTransform(HumanBodyBones.Neck);
                neck.transform.rotation = chest.transform.rotation;
                var head = humanoidAnimator.GetBoneTransform(HumanBodyBones.Head);
                head.transform.rotation = neck.transform.rotation;
            }
        }

        public class PoseMerging
        {
            public readonly Vector3? position = null;
            public readonly Quaternion? rotation = null;
            public readonly float?[] muscles = null;
            public readonly float weight = 0f;
            public PoseMerging(
                Vector3? position, Quaternion? rotation, float?[] muscles, float weight
            )
            {
                this.position = position;
                this.rotation = rotation;
                this.muscles = muscles;
                this.weight = weight;
            }
        }

        public class BoneRotating
        {
            public readonly Transform bone;
            public readonly Quaternion rotation;
            public BoneRotating(Transform bone, Quaternion rotation)
            {
                this.bone = bone;
                this.rotation = rotation;
            }
        }

        const int NUMBER_OF_MUSCLES = 95;

        readonly HumanPoseHandler poseHandler;

        Vector3? position = null;
        Quaternion? rotation = null;
        bool invalidMuscles = true;
        float?[] muscles = new float[NUMBER_OF_MUSCLES].Select(_ => (float?)null).ToArray();
        double timeoutSeconds = Double.PositiveInfinity;
        double timeoutTransitionSeconds = 0;
        double transitionSeconds = 0;
        long lastTransitionTicks = -1;
        long timeoutTransitionRestTicks = 0;
        HumanPose? lastPose = null;

        readonly List<PoseMerging> mergingPoses = new List<PoseMerging>(10);
        readonly List<BoneRotating> rotatingBones = new List<BoneRotating>(10);

        public HumanPoseManager(
            Animator animator
        )
        {
            poseHandler = new HumanPoseHandler(
                animator.avatar,
                animator.transform
            );
        }

        public void SetPosition(
            Vector3? position
        )
        {
            this.position = position;
        }

        public void SetRotation(
            Quaternion? rotation
        )
        {
            this.rotation = rotation;
        }

        public void SetMuscles(
            float[] muscles,
            bool[] hasMascles
        )
        {
            for (var i = 0; i < NUMBER_OF_MUSCLES; i++)
            {
                //hasMasclesではない時は、前回のは残らないようなのでnull
                this.muscles[i] = hasMascles[i] ? muscles[i] : null;
            }
            invalidMuscles = false;
        }
        public void InvalidateMuscles()
        {
            invalidMuscles = true;
            muscles = new float[NUMBER_OF_MUSCLES].Select(_ => (float?)null).ToArray();
        }

        public void SetHumanTransition(
            double timeoutSeconds,
            double timeoutTransitionSeconds,
            double transitionSeconds
        )
        {
            this.timeoutSeconds = timeoutSeconds;
            this.timeoutTransitionSeconds = timeoutTransitionSeconds;
            this.transitionSeconds = transitionSeconds;
            this.timeoutTransitionRestTicks = (long)(timeoutTransitionSeconds * TimeSpan.TicksPerSecond);
        }

        public void InvalidateHumanTransition()
        {
            timeoutSeconds = Double.PositiveInfinity;
            timeoutTransitionSeconds = 0;
            transitionSeconds = 0;
            timeoutTransitionRestTicks = 0;
        }

        public void MergeHumanPoseOnFrame(
            Vector3? position, Quaternion? rotation, float[] muscles, bool[] hasMascles, float weight
        )
        {
            var mergingPose = new PoseMerging(
                position, rotation, muscles.Select((m, i) => hasMascles[i] ? (float?)m : null).ToArray(), weight
            );
            mergingPoses.Add(mergingPose);
        }

        public void OverwriteHumanoidBoneRotation(Transform bone, Quaternion rotation)
        {
            var rotatingBone = new BoneRotating(bone, rotation);
            rotatingBones.Add(rotatingBone);
        }

        public void Apply()
        {
            var nowTicks = DateTime.Now.Ticks;

            if (lastTransitionTicks == -1) lastTransitionTicks = nowTicks;
            var deltaTicks = nowTicks - lastTransitionTicks;

            //初期状態ではこの段階でhumanPoseにはT-Poseのmusclesが入っている。
            //musclesを全て0にすると、手を前に出した中腰ポーズになる。
            var humanPose = GetHumanPose();
            if (lastPose == null) lastPose = humanPose;

            if (!invalidMuscles)
            {
                for (var i = 0; i < NUMBER_OF_MUSCLES; i++)
                {
                    if (!muscles[i].HasValue) continue;
                    var m = Lerp(lastPose.Value.muscles[i], muscles[i].Value, deltaTicks, transitionSeconds);
                    //timeoutTransitionSeconds=0の時でもif文なしで動くようにしたかった
                    //そのため、Lerpは1->0になっている。
                    //この辺をif文まみれにすると変なバグを踏みそうなので
                    m = Lerp(humanPose.muscles[i], m, timeoutTransitionRestTicks, timeoutTransitionSeconds);
                    humanPose.muscles[i] = m;
                }
            }

            if (position != null)
            {
                var p = Lerp(lastPose.Value.bodyPosition, position.Value.Clone(), deltaTicks, transitionSeconds);
                p = Lerp(humanPose.bodyPosition.Clone(), p.Clone(), timeoutTransitionRestTicks, timeoutTransitionSeconds);
                humanPose.bodyPosition = p;
            }
            if (rotation != null)
            {
                var r = Lerp(lastPose.Value.bodyRotation, rotation.Value.Clone(), deltaTicks, transitionSeconds);
                r = Lerp(humanPose.bodyRotation.Clone(), r.Clone(), timeoutTransitionRestTicks, timeoutTransitionSeconds);
                humanPose.bodyRotation = r;
            }

            lastPose = humanPose;

            foreach(var mergingPose in mergingPoses)
            {
                if(mergingPose.position.HasValue)
                {
                    var p = Vector3.Lerp(humanPose.bodyPosition, mergingPose.position.Value, mergingPose.weight);
                    humanPose.bodyPosition = p;
                }
                if (mergingPose.rotation.HasValue)
                {
                    var r = Quaternion.Lerp(humanPose.bodyRotation, mergingPose.rotation.Value, mergingPose.weight);
                    humanPose.bodyRotation = r;
                }
                for (var i = 0; i < NUMBER_OF_MUSCLES; i++)
                {
                    if (!mergingPose.muscles[i].HasValue) continue;
                    var m = Mathf.Lerp(humanPose.muscles[i], mergingPose.muscles[i].Value, mergingPose.weight);
                    humanPose.muscles[i] = m;
                }
            }
            mergingPoses.Clear();
            poseHandler.SetHumanPose(ref humanPose);

            foreach(var rotatingBone in rotatingBones)
            {
                rotatingBone.bone.rotation = rotatingBone.rotation;
            }
            rotatingBones.Clear();

            lastTransitionTicks = nowTicks;

            var deltaSec = (double)deltaTicks / (double)TimeSpan.TicksPerSecond;
            transitionSeconds -= deltaSec;
            if (transitionSeconds < 0) transitionSeconds = 0;

            //timeoutSecondsが終了したら、timeoutTransitionRestTicksの減算が始まり
            //減算が終了したら、各種値を未設定のものにする。
            if (timeoutSeconds != double.PositiveInfinity) timeoutSeconds -= deltaSec;
            if (timeoutSeconds < 0) timeoutSeconds = 0;
            if (timeoutSeconds == 0) timeoutTransitionRestTicks -= deltaTicks;
            if (timeoutTransitionRestTicks < 0)
            {
                timeoutTransitionRestTicks = 0;
                timeoutSeconds = double.PositiveInfinity;
                timeoutTransitionSeconds = 0;
                SetPosition(null);
                SetRotation(null);
                InvalidateMuscles();
            }
        }

        public HumanPose GetHumanPose()
        {
            var humanPose = new HumanPose();
            poseHandler.GetHumanPose(ref humanPose);
            return humanPose;
        }

        float Lerp(float start, float end, long deltaTicks, double leftSec)
        {
            if (leftSec == 0) return end;
            //deltaTicksはdoubleの有効桁数に入るはずなのでOKとする
            var deltaSec = (double)deltaTicks / (double)TimeSpan.TicksPerSecond;
            if (leftSec <= deltaSec) return end;

            var ret = Mathf.Lerp(start, end, (float)(deltaSec / leftSec));
            return ret;
        }

        Vector3 Lerp(Vector3 start, Vector3 end, long deltaTicks, double leftSec)
        {
            if (leftSec == 0) return end;
            var deltaSec = (double)deltaTicks / (double)TimeSpan.TicksPerSecond;
            if (leftSec <= deltaSec) return end;

            var ret = Vector3.Lerp(start, end, (float)(deltaSec / leftSec));
            return ret;
        }

        Quaternion Lerp(Quaternion start, Quaternion end, long deltaTicks, double leftSec)
        {
            if (leftSec == 0) return end;
            var deltaSec = (double)deltaTicks / (double)TimeSpan.TicksPerSecond;
            if (leftSec <= deltaSec) return end;

            var ret = Quaternion.Lerp(start, end, (float)(deltaSec / leftSec));
            return ret;
        }
    }
}
