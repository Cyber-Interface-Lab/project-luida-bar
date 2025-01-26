using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class CameraHandle
    {
        readonly IPlayerController playerController;

        public CameraHandle(
            IPlayerController playerController
        )
        {
            this.playerController = playerController;
        }

        public EmulateVector3 getPosition()
        {
            return new EmulateVector3(playerController.GetCameraPosition());
        }

        public EmulateQuaternion getRotation()
        {
            return new EmulateQuaternion(playerController.GetCameraRotation());
        }

        public bool isFirstPersonView()
        {
            //内部的にはoptionの値を見ているので、タイムラグなし
            return playerController.isFirstPersonView;
        }

        //null | float
        public void setFieldOfView(object value)
        {
            setFieldOfView(value, false);
        }
        public void setFieldOfView(object value, bool immediate)
        {
            if(value == null)
            {
                var fov = playerController.GetCameraFieldOfView();
                if (immediate)
                {
                    playerController.SetCameraFieldOfView(fov);
                }
                else
                {
                    var start = playerController.GetCameraFieldOfViewNow();
                    playerController.RunCoroutine(
                        () => EasingFloatAction(start, fov, playerController.SetCameraFieldOfView)
                    );
                }
            }
            if(value is double doubleValue)
            {
                var floatValue = Mathf.Clamp((float)doubleValue, 10, 80);
                if (immediate)
                {
                    playerController.SetCameraFieldOfViewTemporary(floatValue);
                }
                else
                {
                    var start = playerController.GetCameraFieldOfViewNow();
                    playerController.RunCoroutine(
                        () => EasingFloatAction(start, floatValue, playerController.SetCameraFieldOfViewTemporary)
                    );
                }
            }
        }
        System.Collections.IEnumerator EasingFloatAction(float start, float end, Action<float> Action)
        {
            var elapse = 0f;
            var duration = 1f;
            while (elapse < duration)
            {
                var f = (end - start) * elapse / duration + start;
                Action(f);
                elapse += Time.deltaTime;
                yield return null;
            }
        }
        System.Collections.IEnumerator EasingVector2Action(float duration, Vector2 start, Vector2 end, Action<Vector2> Action)
        {
            var elapse = 0f;
            while (elapse < duration)
            {
                var f = (end - start) * elapse / duration + start;
                Action(f);
                elapse += Time.deltaTime;
                yield return null;
            }
        }

        public void setPosition(EmulateVector3 position)
        {
            playerController.SetCameraPosition(position?._ToUnityEngine());
        }

        public void setRotation(EmulateQuaternion rotation)
        {
            playerController.SetCameraRotation(rotation?._ToUnityEngine());
        }

        //null | boolean
        public void setThirdPersonAvatarForwardLock(object value)
        {
            if (value == null)
            {
                playerController.OverwriteFaceConstraint(null);
            }
            if (value is bool boolValue)
            {
                playerController.OverwriteFaceConstraint(boolValue);
            }
        }

        //nullあり
        public void setThirdPersonAvatarScreenPosition(EmulateVector2 pos)
        {
            setThirdPersonAvatarScreenPosition(pos, false);
        }
        public void setThirdPersonAvatarScreenPosition(EmulateVector2 pos, bool immediate)
        {
            if (pos == null)
            {
                var d = new Vector2(0.5f, 0.5f);
                if (immediate)
                {
                    playerController.SetThirdPersonCameraScreenPosition(d);
                }
                else
                {
                    var start = playerController.GetThirdPersonCameraScreenPositionNow();
                    playerController.RunCoroutine(
                        () => EasingVector2Action(0.5f, start, d, playerController.SetThirdPersonCameraScreenPosition)
                    );
                }
            }
            else
            {
                if (immediate)
                {
                    playerController.SetThirdPersonCameraScreenPosition(pos._ToUnityEngine());
                }
                else
                {
                    var start = playerController.GetThirdPersonCameraScreenPositionNow();
                    playerController.RunCoroutine(
                        () => EasingVector2Action(0.5f, start, pos._ToUnityEngine(), playerController.SetThirdPersonCameraScreenPosition)
                    );
                }
            }
        }

        //null | float
        public void setThirdPersonDistance(object distance)
        {
            setThirdPersonDistance(distance, false);
        }
        public void setThirdPersonDistance(object distance, bool immediate)
        {
            if (distance == null)
            {
                var d = playerController.GetThirdPersonCameraDistanceDefault();
                if (immediate)
                {
                    playerController.SetThirdPersonCameraDistanceTemporary(d);
                }
                else
                {
                    var start = playerController.GetThirdPersonCameraDistanceNow();
                    playerController.RunCoroutine(
                        () => EasingFloatAction(start, d, playerController.SetThirdPersonCameraDistanceTemporary)
                    );
                }
            }
            if (distance is double doubleDistance)
            {
                var floatDistance = Mathf.Max((float)doubleDistance, 0.2f);
                if (immediate)
                {
                    playerController.SetThirdPersonCameraDistanceTemporary(floatDistance);
                }
                else
                {
                    //大きい値を指定すると、easing中は壁を突き抜けるけど面倒なのでこのまま
                    var start = playerController.GetThirdPersonCameraDistanceNow();
                    playerController.RunCoroutine(
                        () => EasingFloatAction(start, floatDistance, playerController.SetThirdPersonCameraDistanceTemporary)
                    );
                }
            }
        }

        public void _Shutdown()
        {
            playerController.SetCameraFieldOfView(
                playerController.GetCameraFieldOfView()
            );
            playerController.SetThirdPersonCameraDistanceTemporary(
                playerController.GetThirdPersonCameraDistanceDefault()
            );
            playerController.OverwriteFaceConstraint(null);
            playerController.SetThirdPersonCameraScreenPosition(
                new Vector2(0.5f, 0.5f)
            );
            playerController.SetCameraPosition(null);
            playerController.SetCameraRotation(null);
            //CSETODO カメラ系の機能が追加されたらここでリセットさせる。
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return string.Format("[CameraHandle]");
        }
    }
}
