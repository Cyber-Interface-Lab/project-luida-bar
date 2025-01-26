using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class OnUpdateBridge
        : IDisposable, EmulateClasses.IUpdateListenerBinder
    {
        long prevTicks;

        Dictionary<string, (EmulateClasses.IJintCallback<double>, UnityEngine.GameObject)> UpdateCallbacks = new();
        Dictionary<string, (Action<double>, UnityEngine.GameObject)> LateUpdateCallbacks = new Dictionary<string, (Action<double>, UnityEngine.GameObject)>();

        public Action InvokeUpdate { get; private set; }

        ILogger logger;

        public OnUpdateBridge(
            ILogger logger
        )
        {
            this.logger = logger;
            InvokeUpdate = () =>
            {
                //prevTicksを初期化したらUpdateを通常処理に切り替える
                prevTicks = DateTime.Now.Ticks;
                InvokeUpdate = () =>
                {
                    var nowTicks = DateTime.Now.Ticks;
                    var deltaTime = (double)(nowTicks - prevTicks) / 10_000_000d;
                    //ループ中に登録が解除されるので、念のためにToArrayしておく
                    foreach(var (Callback, source) in UpdateCallbacks.Values.ToArray())
                    {
                        try
                        {
                            Callback.Execute(deltaTime);
                        }
                        catch(Exception e)
                        {
                            this.logger.Exception(e, source);
                        }
                    }
                    foreach (var (Callback, source) in LateUpdateCallbacks.Values.ToArray())
                    {
                        try
                        {
                            Callback(deltaTime);
                        }
                        catch (Exception e)
                        {
                            this.logger.Exception(e, source);
                        }
                    }
                    prevTicks = nowTicks;
                };
            };
        }

        public void ChangeLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void SetUpdateCallback(string key, UnityEngine.GameObject source, EmulateClasses.IJintCallback<double> Callback)
        {
            UpdateCallbacks[key] = (Callback, source);
        }

        public void DeleteUpdateCallback(string key)
        {
            if (!UpdateCallbacks.ContainsKey(key)) return;
            UpdateCallbacks.Remove(key);
        }

        public void SetLateUpdateCallback(string key, UnityEngine.GameObject source, Action<double> Callback)
        {
            LateUpdateCallbacks[key] = (Callback, source);
        }

        public void DeleteLateUpdateCallback(string key)
        {
            if (!LateUpdateCallbacks.ContainsKey(key)) return;
            LateUpdateCallbacks.Remove(key);
        }

        public void Dispose()
        {
            UpdateCallbacks.Clear();
            LateUpdateCallbacks.Clear();
        }
    }
}
