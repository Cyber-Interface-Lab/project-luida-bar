using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class SubNode
    {
        class JintCallbackUpdate : IJintCallback<double>
        {
            readonly Action<double> Callback;
            public JintCallbackUpdate(Action<double> Callback)
            {
                this.Callback = Callback;
            }
            public void Execute(double arg)
            {
                Callback(arg);
            }
        }

        static float TRANSFORM_LIMIT = 10;
        static float ROTATION_LIMIT = 45;
        static double INTERVAL_LIMIT = 0.2;


        readonly GameObject gameObject;
        readonly ClusterVR.CreatorKit.Item.IItem parent;
        readonly ClusterVR.CreatorKit.World.ITextView textView;
        readonly IRunningContext runningContext;
        readonly IUpdateListenerBinder updateListenerBinder;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly string positionCallbackKey;
        readonly string rotationCallbackKey;


        double positionChangeInterval = INTERVAL_LIMIT;
        double rotationChangeInterval = INTERVAL_LIMIT;

        Vector3 prevSetPosition;


        public SubNode(
            Transform transform,
            ClusterVR.CreatorKit.Item.IItem parent,
            ClusterVR.CreatorKit.World.ITextView textView,
            IRunningContext runningContext,
            IUpdateListenerBinder updateListenerBinder,
            IItemExceptionFactory itemExceptionFactory
        )
        {
            this.gameObject = null;
            if(transform != null)
            {
                gameObject = transform.gameObject;
                //SubNode名は重複しないという前提なのでkeyとして使用
                this.positionCallbackKey = gameObject.name + "_p";
                this.rotationCallbackKey = gameObject.name + "_r";
                this.prevSetPosition = gameObject.transform.localPosition;
            }
            this.parent = parent;
            this.textView = textView;
            this.runningContext = runningContext;
            this.updateListenerBinder = updateListenerBinder;
            this.itemExceptionFactory = itemExceptionFactory;
        }

        public bool getEnabled()
        {
            if (runningContext.CheckTopLevel("SubNode.getEnabled()")) { }; //メッセージだけの模様？
            if (gameObject == null) return false;
            if (parent.IsDestroyed) return false;
            return gameObject.activeSelf;
        }

        public EmulateVector3 getGlobalPosition()
        {
            if (runningContext.CheckTopLevel("SubNode.getGlobalPosition()")) { }; //メッセージだけの模様？
            if (gameObject == null) return null;
            if (parent.IsDestroyed) return null;
            return new EmulateVector3(gameObject.transform.position);
        }

        public EmulateQuaternion getGlobalRotation()
        {
            if (runningContext.CheckTopLevel("SubNode.getGlobalRotation()")) { }; //メッセージだけの模様？
            if (gameObject == null) return null;
            if (parent.IsDestroyed) return null;
            return new EmulateQuaternion(gameObject.transform.rotation);
        }

        public EmulateVector3 getPosition()
        {
            if (runningContext.CheckTopLevel("SubNode.getPosition()")) { }; //メッセージだけの模様？
            //孫ノードの場合、子ノードベースのPositionになる
            if (gameObject == null) return null;
            if (parent.IsDestroyed) return null;
            return new EmulateVector3(gameObject.transform.localPosition);
        }

        public EmulateQuaternion getRotation()
        {
            if (runningContext.CheckTopLevel("SubNode.getRotation()")) { }; //メッセージだけの模様？
            if (gameObject == null) return null;
            if (parent.IsDestroyed) return null;
            return new EmulateQuaternion(gameObject.transform.localRotation);
        }

        public bool getTotalEnabled()
        {
            if (runningContext.CheckTopLevel("SubNode.getTotalEnabled()")) { }; //メッセージだけの模様？
            if (gameObject == null) return false;
            if (parent.IsDestroyed) return false;
            return gameObject.activeInHierarchy;
        }

        public UnityComponent getUnityComponent(string type)
        {
            var ret = UnityComponent.GetScriptableItemUnityComponent(
                gameObject, type, itemExceptionFactory
            );
            return ret;
        }

        public string name
        {
            get => gameObject.name;
        }

        public void setEnabled(bool v)
        {
            if (runningContext.CheckTopLevel("SubNode.setEnabled()")) return;
            if (gameObject == null) return;
            if (parent.IsDestroyed) return;

            //あえて時間差つくる？
            gameObject.SetActive(v);
        }

        public void setPosition(EmulateVector3 v)
        {
            if (runningContext.CheckTopLevel("SubNode.setPosition()")) return;
            if (gameObject == null) return;
            if (parent.IsDestroyed) return;

            var toPosition = v._ToUnityEngine();
            if (prevSetPosition == toPosition)
            {
                //移動先を連打されているような場合は、後述のleap処理に影響を出させないようにここで離脱。
                gameObject.transform.localPosition = toPosition;
                prevSetPosition = toPosition;
                return;
            }
            prevSetPosition = toPosition;

            var max = MaxDistance(gameObject.transform.localPosition, toPosition);
            if(max > TRANSFORM_LIMIT)
            {
                //一つでも10より大きいならワープ。
                gameObject.transform.localPosition = toPosition;
                return;
            }


            //現在位置を元に上書きという挙動でOK
            var _from = gameObject.transform.localPosition;
            var _to = toPosition;
            //直前のintervalの速度でlerpさせる＆interval計測開始
            var _remain = positionChangeInterval;
            positionChangeInterval = 0;
            updateListenerBinder.SetUpdateCallback(
                positionCallbackKey,
                gameObject,
                new JintCallbackUpdate((dt) =>
                {
                    var lerped = Vector3.Lerp(_from, _to, (float)Math.Min(dt / _remain, 1));
                    gameObject.transform.localPosition = lerped;
                    _from = lerped;
                    _remain -= dt;

                    positionChangeInterval = Math.Min(positionChangeInterval + dt, INTERVAL_LIMIT);

                    if (_remain > 0)
                        return;

                    //intervalをLIMITまで計測させる
                    updateListenerBinder.SetUpdateCallback(
                        positionCallbackKey,
                        gameObject,
                        new JintCallbackUpdate((dt) =>
                        {
                            positionChangeInterval += dt;
                            if (positionChangeInterval < INTERVAL_LIMIT)
                                return;

                            positionChangeInterval = INTERVAL_LIMIT;
                            updateListenerBinder.DeleteUpdateCallback(positionCallbackKey);
                        })
                    );

                })
            );


        }

        public void setRotation(EmulateQuaternion v)
        {
            if (runningContext.CheckTopLevel("SubNode.setRotation()")) return;
            if (gameObject == null) return;
            if (parent.IsDestroyed) return;

            var toRotation = v._ToUnityEngine();
            var max = MaxDistance(gameObject.transform.localRotation, toRotation);

            if(max >= ROTATION_LIMIT)
            {
                //45deg以上ならワープ
                gameObject.transform.localRotation = toRotation;
                return;
            }

            //現在位置を元に上書きという挙動でOK
            var _from = gameObject.transform.localRotation;
            var _to = toRotation;
            //直前のintervalの時間でlerpさせるという挙動でOK
            var _remain = rotationChangeInterval;
            rotationChangeInterval = 0;
            updateListenerBinder.SetUpdateCallback(
                rotationCallbackKey,
                gameObject,
                new JintCallbackUpdate((dt) =>
                {
                    var lerped = Quaternion.Slerp(_from, _to, (float)Math.Min(dt / _remain, 1));
                    gameObject.transform.localRotation = lerped;
                    _from = lerped;
                    _remain -= dt;

                    rotationChangeInterval = Math.Min(rotationChangeInterval + dt, INTERVAL_LIMIT);

                    if (_remain > 0)
                        return;

                    //intervalをLIMITまで計測させる
                    updateListenerBinder.SetUpdateCallback(
                        rotationCallbackKey,
                        gameObject,
                        new JintCallbackUpdate((dt) =>
                        {
                            rotationChangeInterval += dt;
                            if (rotationChangeInterval < INTERVAL_LIMIT)
                                return;

                            rotationChangeInterval = INTERVAL_LIMIT;
                            updateListenerBinder.DeleteUpdateCallback(rotationCallbackKey);
                        })
                    );

                })
            );


        }

        public void setText(string text)
        {
            if (runningContext.CheckTopLevel("SubNode.setText()")) return;
            textView.SetText(text);
        }

        public void setTextAlignment(TextAlignment alignment)
        {
            if (runningContext.CheckTopLevel("SubNode.setTextAlignment()")) return;
            textView.SetTextAlignment((UnityEngine.TextAlignment)alignment);
        }

        public void setTextAnchor(TextAnchor anchor)
        {
            if (runningContext.CheckTopLevel("SubNode.setTextAnchor()")) return;
            textView.SetTextAnchor((UnityEngine.TextAnchor)anchor);
        }

        public void setTextColor(float r, float g, float b, float a)
        {
            if (runningContext.CheckTopLevel("SubNode.setTextColor()")) return;
            textView.SetColor(new Color(r, g, b, a));
        }

        public void setTextSize(float size)
        {
            if (runningContext.CheckTopLevel("SubNode.setTextSize()")) return;
            textView.SetSize(size);
        }

        float MaxDistance(Vector3 v1, Vector3 v2)
        {
            var delta = v1 - v2;
            var max = new float[] {
                delta.x, delta.y, delta.z
            }.Select(d => Math.Abs(d))
            .Max();
            return max;
        }

        float MaxDistance(Quaternion q1, Quaternion q2)
        {
            var delta = q1.eulerAngles - q2.eulerAngles;
            var max = new float[] {
                delta.x, delta.y, delta.z
            }.Select(d => Math.Abs(d))
            .Select(d => d % 360)
            .Select(d => d > 180 ? 360 - d : d)
            .Max();
            return max;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format(
                "[SubNode][{1}/{0}]",
                gameObject == null ? null : gameObject.name,
                parent == null ? null : parent.gameObject.name
            );
        }


    }
}
