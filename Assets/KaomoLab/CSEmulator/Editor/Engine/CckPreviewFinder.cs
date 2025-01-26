using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class CckPreviewFinder
    {
        public GameObject previewRoot
        {
            get
            {
                if (_previewRoot == null) Find();
                return _previewRoot;
            }
        }
        public GameObject controller
        {
            get
            {
                if (_previewRoot == null) Find();
                return _controller;
            }
        }
        public GameObject controllerRoot
        {
            get
            {
                if (_previewRoot == null) Find();
                return _controllerRoot;
            }
        }
        public GameObject camera
        {
            get
            {
                if (_previewRoot == null) Find();
                return _camera;
            }
        }
        public GameObject grabPoint
        {
            get
            {
                if (_previewRoot == null) Find();
                return _grabPoint;
            }
        }
        public GameObject canvas
        {
            get
            {
                if (_previewRoot == null) Find();
                return _canvas;
            }
        }
        public GameObject panel
        {
            get
            {
                if (_previewRoot == null) Find();
                return _panel;
            }
        }

        GameObject _previewRoot = null;

        GameObject _controller = null;
        GameObject _controllerRoot = null;
        GameObject _camera = null;
        GameObject _grabPoint = null;
        GameObject _canvas = null;
        GameObject _panel = null;

        public CckPreviewFinder()
        {
        }

        void Find()
        {
            if (_previewRoot != null) return;

            var controllers = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(
                    //PreviewOnlyの中にあるキャラを操作するアレ。
                    o => o.GetComponentsInChildren<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>(true)
                );
            if (controllers.Count() > 1)
            {
                UnityEngine.Debug.LogWarning("[PreviewOnly] exists multiple.");
            }

            //とりあえずLastで。
            _controller = controllers.Last().gameObject;
            _controllerRoot = _controller.transform.Find("Root").gameObject;
            _camera = _controller.transform.Find("Root/MainCamera").gameObject;
            _grabPoint = _controller.transform.Find("Root/MainCamera/GrabPoint").gameObject;

            _previewRoot = _controller.transform.parent.gameObject;
            _canvas = _previewRoot.transform.Find("CanvasAndEventHandler/Canvas").gameObject;
            _panel = _previewRoot.transform.Find("CanvasAndEventHandler/Canvas/Panel").gameObject;
        }
    }
}
