using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.KaomoLab.CSEmulator.UIs.TextInput
{
    [DisallowMultipleComponent]
    public class Handler
        : MonoBehaviour
    {
        public string caption
        {
            get => _caption.text;
            set => _caption.text = value;
        }
        public string text
        {
            get => _text.text;
            set => _text.value = value;
        }
        public bool isInputting { get; private set; } = false;

        UIDocument ui;
        Label _caption;
        TextField _text;
        Button _cancel;
        Button _send;
        Button _busy;

        Action<string> SendCallback;
        Action CancelCallback;
        Action BusyCallback;

        void Awake()
        {
            ui = GetComponent<UIDocument>();
            _caption = ui.rootVisualElement.Q<Label>("caption");
            _text = ui.rootVisualElement.Q<TextField>("text");
            _cancel = ui.rootVisualElement.Q<Button>("Cancel");
            _send = ui.rootVisualElement.Q<Button>("Send");
            _busy = ui.rootVisualElement.Q<Button>("Busy");

            text = "";
            caption = "";

            _cancel.clicked += Cancel_clicked;
            _send.clicked += Send_clicked;
            _busy.clicked += Busy_clicked;

            //enabled = falseにするとclickedのcallbackがうまく動かなくなる？
            //enabled = true後に+=しても動かないのでvisibleで対応。
            ui.rootVisualElement.visible = false;
            isInputting = false;
        }

        private void Send_clicked()
        {
            if (SendCallback == null) return;

            SendCallback(text);
            EndInput();
        }

        private void Cancel_clicked()
        {
            CancelCallback();
            EndInput();
        }

        private void Busy_clicked()
        {
            BusyCallback();
            EndInput();
        }

        public void StartInput(
            Action<string> SendCallback,
            Action CancelCallback,
            Action BusyCallback
        )
        {
            if(this.SendCallback != null)
            {
                //ここに来ないように制御すること。
                throw new Exception("プログラムミス。開発者に連絡してください。");
            }

            this.SendCallback = SendCallback;
            this.CancelCallback = CancelCallback;
            this.BusyCallback = BusyCallback;
            isInputting = true;
            ui.rootVisualElement.visible = true;
        }

        void EndInput()
        {
            //Destroyされている可能性があるので
            if(ui != null)
                ui.rootVisualElement.visible = false;
            SendCallback = null;
            isInputting = false;
        }
    }
}
