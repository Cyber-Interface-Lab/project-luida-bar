using ClusterVR.CreatorKit.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent]
    public class CSEmulatorPlayerLocalUIEventBridge
        : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointEventEmitter
    {
        public event Action<Vector2> OnDown = delegate {};
        public event Action<Vector2> OnUp = delegate { };

        public void OnPointerDown(PointerEventData eventData)
        {
            var p = eventData.position;
            OnDown.Invoke(p);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var p = eventData.position;
            OnUp.Invoke(p);
        }
    }

    [DisallowMultipleComponent]
    public class CSEmulatorPlayerLocalUI : MonoBehaviour
    {
        IPointEventEmitter pointEventEmitter;
        PointOfViewManager pointOfViewManager;

        //この辺からの流れめちゃくちゃ筋が悪いけど、今は考えるほどリソース割けないのでこのまま行く
        public void Construct(
            IPointEventEmitter pointEventEmitter,
            PointOfViewManager pointOfViewManager
        )
        {
            this.pointEventEmitter = pointEventEmitter;
            this.pointOfViewManager = pointOfViewManager;
        }

        void Start()
        {
            SetupInteractable();
            SetupComponent<Button, CSEmulatorLocalUIButton>(pointEventEmitter, pointOfViewManager);
        }

        void SetupInteractable()
        {
            var playerLocalUI = GetComponent<IPlayerLocalUI>();
            if (playerLocalUI.SortingOrder != PlayerLocalUISortingOrder.Interactable) return;

            var canvas = GetComponent<Canvas>();
            if(canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                //PreviewOnlyのMouse操作用Canvasより手前に持ってくる
                canvas.sortingOrder += 1001;
            }
        }

        void SetupComponent<C, T>(
            IPointEventEmitter pointEventEmitter,
            PointOfViewManager pointOfViewManager
        )
            where C : Component
            where T : CSEmulatorLocalUIComponent
        {
            foreach (var b in GetComponentsInChildren<C>(true))
            {
                var c = b.gameObject.AddComponent<T>();
                c.Construct(pointEventEmitter, pointOfViewManager);
            }
        }

        public static IEnumerable<IPlayerLocalUI> GetAllPlayerLocalUIs()
        {
            var ret = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(
                    //inactiveも含めておく
                    o => o.GetComponentsInChildren<IPlayerLocalUI>(true)
                );
            return ret;
        }
    }

    public abstract class CSEmulatorLocalUIComponent : MonoBehaviour
    {
        protected IPointEventEmitter pointEventEmitter;
        protected PointOfViewManager pointOfViewManager;
        public void Construct(
            IPointEventEmitter pointEventEmitter,
            PointOfViewManager pointOfViewManager
        )
        {
            this.pointEventEmitter = pointEventEmitter;
            this.pointOfViewManager = pointOfViewManager;
        }
        public abstract void Shutdown();
    }

    [DisallowMultipleComponent]
    public class CSEmulatorLocalUIButton
        : CSEmulatorLocalUIComponent,
        IPointerDownHandler, IPointerUpHandler
    {
        Action<bool> Callback = isDown => { };
        bool needInitialize = true;

        //CCKのコピペ
        int raycastLayerMask = ~(ClusterVR.CreatorKit.Constants.LayerName.PostProcessingMask | ClusterVR.CreatorKit.Constants.LayerName.CameraOnlyMask);

        void Start()
        {
            var b = GetComponent<Button>();
        }

        private void PointEventEmitter_OnDown(Vector2 p)
        {
            if (!CheckRay(p)) return;
            Callback(true);
        }
        private void PointEventEmitter_OnUp(Vector2 p)
        {
            if (!CheckRay(p)) return;
            Callback(false);
        }
        bool CheckRay(Vector2 p)
        {
            var ray = pointOfViewManager.targetCamera.ScreenPointToRay(p);
            if (!Physics.Raycast(ray, out var hitInfo, 10, raycastLayerMask)) //rayの長さは10m(CCK2.26.0実測)
                return false;
            if (hitInfo.collider.gameObject != gameObject)
                return false;

            return true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Callback(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //ドキュメントではボタン外でUpした場合に不発とのことだけど、実際はキャプチャされてる？ようなのでこのまま
            Callback(false);
        }

        public void SetOnClickCallback(Action<bool> Callback)
        {
            if (needInitialize)
            {
                pointEventEmitter.OnDown += PointEventEmitter_OnDown;
                pointEventEmitter.OnUp += PointEventEmitter_OnUp;
                needInitialize = false;
            }
            this.Callback = Callback;
        }

        public override void Shutdown()
        {
            Callback = _ => { };
            pointEventEmitter.OnDown -= PointEventEmitter_OnDown;
            pointEventEmitter.OnUp -= PointEventEmitter_OnUp;
            needInitialize = true;
        }
    }
}
