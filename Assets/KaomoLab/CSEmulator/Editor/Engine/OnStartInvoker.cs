using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class OnStartInvoker
        : EmulateClasses.IStartListenerBinder
    {
        public OnStartInvoker() { }

        public Action InvokeStart = () => { };

        public void DeleteStartCallback()
        {
            InvokeStart = () => { };
        }

        public void SetUpdateCallback(EmulateClasses.IJintCallback Callback)
        {
            InvokeStart = () =>
            {
                //OnUpdateBridgeが1フレーム使って初期化しているのでそれに合わせている。
                //OnUpdateBridgeの初期化、考え直したほうがいい気がする
                InvokeStart = () =>
                {
                    InvokeStart = () => { }; //エラーが起きても再度実行しない
                    Callback.Execute();
                };
            };
        }
    }
}
