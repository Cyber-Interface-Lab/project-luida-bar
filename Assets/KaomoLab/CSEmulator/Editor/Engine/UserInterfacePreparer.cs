using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class UserInterfacePreparer
        : EmulateClasses.IUserInputInterfaceHandler, EmulateClasses.IButtonInterfaceHandler
    {
        readonly CckPreviewFinder previewFinder;
        UIs.TextInput.Handler uiTextInput = null;
        UIs.Purchase.Handler uiPurchaseInput = null;
        UIs.Dialog.Handler uiDialog = null;
        UIs.IconArea.Handler iconArea = null;

        readonly Dictionary<int, Action<bool>> buttonCallbacks = new();
        readonly Dictionary<int, bool> buttonEnables = new();

        public UserInterfacePreparer(
            CckPreviewFinder previewFinder
        )
        {
            this.previewFinder = previewFinder;
            buttonCallbacks[0] = _ => { };
            buttonCallbacks[1] = _ => { };
            buttonCallbacks[2] = _ => { };
            buttonCallbacks[3] = _ => { };
            buttonEnables[0] = false;
            buttonEnables[1] = false;
            buttonEnables[2] = false;
            buttonEnables[3] = false;
        }

        T LoadHandler<T>(string path)
        {
            var ret = GameObject.Instantiate(
                AssetDatabase.LoadAssetAtPath<GameObject>(path),
                previewFinder.previewRoot.transform
            ).GetComponent<T>();
            return ret;
        }

        public bool isUserInputting => isInputting();
        bool isInputting()
        {
            return (uiTextInput != null ? uiTextInput.isInputting : false)
                || (uiPurchaseInput != null ? uiPurchaseInput.isInputting : false)
                || (uiDialog != null ? uiDialog.isInputting : false)
                ;
        }

        public void StartTextInput(string caption, Action<string> SendCallback, Action CancelCallback, Action BusyCallback)
        {
            //都度Instantiateする方法に変更した。
            //インスタンスを残していると「Destroy may not be called from edit mode!」が出るため。
            //なお、ExitingPlayModeでDestroyImmidiateしてもタイミングのよってはエラーが出てしまう。
            uiTextInput = LoadHandler<UIs.TextInput.Handler>(
                "Assets/KaomoLab/CSEmulator/UIs/TextInput/View.prefab"
            );

            var image = previewFinder.panel.GetComponent<UnityEngine.UI.Image>();
            var controller = previewFinder.controller.GetComponent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>();
            //ボタンクリックを拾わなくなるので
            image.raycastTarget = false;
            //WASDで移動してしまうので
            controller.enabled = false;

            uiTextInput.caption = caption;
            uiTextInput.text = "";
            uiTextInput.StartInput(
                text => {
                    image.raycastTarget = true;
                    controller.enabled = true;
                    GameObject.DestroyImmediate(uiTextInput.gameObject);
                    uiTextInput = null;
                    SendCallback(text);
                },
                () => {
                    image.raycastTarget = true;
                    controller.enabled = true;
                    GameObject.DestroyImmediate(uiTextInput.gameObject);
                    uiTextInput = null;
                    CancelCallback();
                },
                () => {
                    image.raycastTarget = true;
                    controller.enabled = true;
                    GameObject.DestroyImmediate(uiTextInput.gameObject);
                    uiTextInput = null;
                    BusyCallback();
                }
            );
        }

        void PrepareIconArea()
        {
            if (iconArea != null) return;

            iconArea = LoadHandler<UIs.IconArea.Handler>(
                "Assets/KaomoLab/CSEmulator/UIs/IconArea/IconArea.prefab"
            );
            var parent = previewFinder.panel;
            iconArea.transform.SetParent(parent.transform, false);
            iconArea.OnButton += (index, isDown) =>
            {
                buttonCallbacks[index](isDown);
            };
        }

        public void ShowButton(int index, EmulateClasses.IconAsset icon)
        {
            PrepareIconArea();
            iconArea.Show(index, icon.texture2D);
            buttonEnables[index] = true;
        }

        public void HideButton(int index)
        {
            PrepareIconArea();
            iconArea.Hide(index);
            buttonEnables[index] = false;
        }

        public void HideAllButtons()
        {
            PrepareIconArea();
            iconArea.HideAll();
            foreach(var index in buttonEnables.Keys.ToArray())
            {
                buttonEnables[index] = false;
            }
        }

        public void SetButtonCallback(int index, IJintCallback<bool> Callback)
        {
            buttonCallbacks[index] = isDown =>
            {
                if (!buttonEnables[index]) return;
                Callback.Execute(isDown);
            };
        }

        public void DeleteAllButtonCallbacks()
        {
            foreach (var index in buttonCallbacks.Keys.ToArray())
            {
                buttonCallbacks[index] = _ => { };
            }
        }

        public void StartPurchase(string productName, string productId, string meta, Action<PurchaseRequestStatus> Callback)
        {
            //都度Instantiateする方法に変更した。
            //インスタンスを残していると「Destroy may not be called from edit mode!」が出るため。
            //なお、ExitingPlayModeでDestroyImmidiateしてもタイミングのよってはエラーが出てしまう。
            uiPurchaseInput = LoadHandler<UIs.Purchase.Handler>(
                "Assets/KaomoLab/CSEmulator/UIs/Purchase/View.prefab"
            );

            var image = previewFinder.panel.GetComponent<UnityEngine.UI.Image>();
            var controller = previewFinder.controller.GetComponent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>();
            //ボタンクリックを拾わなくなるので
            image.raycastTarget = false;
            //WASDで移動してしまうので
            controller.enabled = false;

            uiPurchaseInput.caption = productName + "\n" + productId;
            uiPurchaseInput.StartInput(
                status => {
                    image.raycastTarget = true;
                    controller.enabled = true;
                    GameObject.DestroyImmediate(uiPurchaseInput.gameObject);
                    uiPurchaseInput = null;
                    Callback((PurchaseRequestStatus)status);
                },
                new UIs.Purchase.Handler.Statuses()
                {
                    Purchased = (int)PurchaseRequestStatus.Purchased,
                    UserCanceled = (int)PurchaseRequestStatus.UserCanceled,
                    Busy = (int)PurchaseRequestStatus.Busy,
                    Failed = (int)PurchaseRequestStatus.Failed,
                    NotAvailable = (int)PurchaseRequestStatus.NotAvailable,
                    Unknown = (int)PurchaseRequestStatus.Unknown,
                }
            );
        }

        public void StartDialog(string caption, string[] buttons, Action<int> Callback)
        {
            //都度Instantiateする方法に変更した。
            //インスタンスを残していると「Destroy may not be called from edit mode!」が出るため。
            //なお、ExitingPlayModeでDestroyImmidiateしてもタイミングのよってはエラーが出てしまう。
            uiDialog = LoadHandler<UIs.Dialog.Handler>(
                "Assets/KaomoLab/CSEmulator/UIs/Dialog/View.prefab"
            );

            var image = previewFinder.panel.GetComponent<UnityEngine.UI.Image>();
            var controller = previewFinder.controller.GetComponent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>();
            //ボタンクリックを拾わなくなるので
            image.raycastTarget = false;
            //WASDで移動してしまうので
            controller.enabled = false;

            uiDialog.StartInput(_ =>
            {
                image.raycastTarget = true;
                controller.enabled = true;
                GameObject.DestroyImmediate(uiDialog.gameObject);
                uiDialog = null;
            }, caption, buttons);
        }
    }
}
