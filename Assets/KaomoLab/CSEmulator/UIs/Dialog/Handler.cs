using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.KaomoLab.CSEmulator.UIs.Dialog
{
    [DisallowMultipleComponent]
    public class Handler
        : MonoBehaviour
    {
        public bool isInputting { get; private set; } = false;

        UIDocument ui;
        VisualElement _buttonArea;
        Label _caption;
        Button[] _buttons;

        Action<int> Callback;
        string caption;
        string[] buttons;

        void Awake()
        {
            ui = GetComponent<UIDocument>();
            _caption = ui.rootVisualElement.Q<Label>("caption");
            _buttonArea = ui.rootVisualElement.Q<VisualElement>("ButtonArea");

            //enabled = falseにするとclickedのcallbackがうまく動かなくなる？
            //enabled = true後に+=しても動かないのでvisibleで対応。
            ui.rootVisualElement.visible = false;
            isInputting = false;
        }

        public void StartInput(
            Action<int> Callback,
            string caption,
            string[] buttons
        )
        {
            if(this.Callback != null)
            {
                //ここに来ないように制御すること。
                throw new Exception("プログラムミス。開発者に連絡してください。");
            }

            this.Callback = Callback;
            _caption.text = caption;
            var buttonStep = 100 / buttons.Length;
            for(var i = 0; i < buttons.Length; i++)
            {
                var button = new Button(() =>
                {
                    Callback(i);
                    EndInput();
                });
                button.style.position = new StyleEnum<Position>(Position.Absolute);
                button.style.top = new StyleLength(new Length(0, LengthUnit.Percent));
                button.style.bottom = new StyleLength(new Length(0, LengthUnit.Percent));
                button.style.left = new StyleLength(new Length(buttonStep * i, LengthUnit.Percent));
                button.style.right = new StyleLength(new Length(buttonStep * (buttons.Length - i - 1), LengthUnit.Percent));
                button.text = buttons[i];
                _buttonArea.Add(button);
            }
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
