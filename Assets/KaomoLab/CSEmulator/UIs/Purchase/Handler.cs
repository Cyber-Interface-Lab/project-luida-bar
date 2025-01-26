using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.KaomoLab.CSEmulator.UIs.Purchase
{
    [DisallowMultipleComponent]
    public class Handler
        : MonoBehaviour
    {
        //CSETODO enum系はEditor以下じゃなくていいかも？
        public class Statuses
        {
            public int Unknown;
            public int Purchased;
            public int Busy;
            public int UserCanceled;
            public int NotAvailable;
            public int Failed;
        }

        public string caption
        {
            get => _caption.text;
            set => _caption.text = value;
        }
        public bool isInputting { get; private set; } = false;

        UIDocument ui;
        Label _caption;
        TextField _text;
        Button _userCanceled;
        Button _purchased;
        Button _busy;
        Button _unknown;
        Button _notAvailable;
        Button _failed;

        Action<int> Callback; //PurchaseRequestStatus
        Statuses statuses;

        void Awake()
        {
            ui = GetComponent<UIDocument>();
            _caption = ui.rootVisualElement.Q<Label>("caption");
            _text = ui.rootVisualElement.Q<TextField>("text");
            _userCanceled = ui.rootVisualElement.Q<Button>("UserCanceled");
            _purchased = ui.rootVisualElement.Q<Button>("Purchased");
            _busy = ui.rootVisualElement.Q<Button>("Busy");
            _unknown = ui.rootVisualElement.Q<Button>("Unknown");
            _notAvailable = ui.rootVisualElement.Q<Button>("NotAvailable");
            _failed = ui.rootVisualElement.Q<Button>("Failed");

            caption = "";

            _userCanceled.clicked += UserCanceled_clicked;
            _purchased.clicked += Purchased_clicked;
            _busy.clicked += Busy_clicked;
            _unknown.clicked += Unknown_clicked;
            _notAvailable.clicked += NotAvailable_clicked;
            _failed.clicked += Failed_clicked;

            //enabled = falseにするとclickedのcallbackがうまく動かなくなる？
            //enabled = true後に+=しても動かないのでvisibleで対応。
            ui.rootVisualElement.visible = false;
            isInputting = false;
        }

        private void Failed_clicked()
        {
            Callback(statuses.Failed);
            EndInput();
        }

        private void NotAvailable_clicked()
        {
            Callback(statuses.NotAvailable);
            EndInput();
        }

        private void Unknown_clicked()
        {
            Callback(statuses.Unknown);
            EndInput();
        }

        private void Purchased_clicked()
        {
            Callback(statuses.Purchased);
            EndInput();
        }

        private void UserCanceled_clicked()
        {
            Callback(statuses.UserCanceled);
            EndInput();
        }

        private void Busy_clicked()
        {
            Callback(statuses.Busy);
            EndInput();
        }

        public void StartInput(
            Action<int> Callback,
            Statuses statuses
        )
        {
            if(this.Callback != null)
            {
                //ここに来ないように制御すること。
                throw new Exception("プログラムミス。開発者に連絡してください。");
            }

            this.Callback = Callback;
            this.statuses = statuses;
            isInputting = true;
            ui.rootVisualElement.visible = true;
        }

        void EndInput()
        {
            //Destroyされている可能性があるので
            if(ui != null)
                ui.rootVisualElement.visible = false;
            Callback = null;
            isInputting = false;
        }
    }
}
