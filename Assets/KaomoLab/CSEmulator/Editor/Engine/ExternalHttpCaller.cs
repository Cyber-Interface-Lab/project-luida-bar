using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class ExternalHttpCaller
        : EmulateClasses.IExternalCaller
    {
        public event Handler OnChangeLimit = delegate { };

        readonly IExternalCallerOptions options;
        readonly ILogger logger;

        IJintCallback<string, string, string> Callback = new JintCallback<string, string, string>();

        public CallExternalRateLimit rateLimit => options.rateLimit;

        [System.Serializable]
        public class Request
        {
            public string request;
        }

        [System.Serializable]
        public class Response
        {
            public string verify;
            public string response;
        }

        public ExternalHttpCaller(
            IExternalCallerOptions options,
            ILogger logger
        )
        {
            this.options = options;
            this.logger = logger;
            this.options.OnChangeLimit += () =>
            {
                OnChangeLimit.Invoke();
            };
        }

        public async void CallExternal(string request, string meta)
        {
            using (var client = new HttpClient())
            {
                var requestJson = GetRequestJson(request);
                var postData = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(options.url, postData);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    //CSETODO notfoundでどういうメッセージが帰ってくる？
                    var reason = string.Format("{0}:{1}:{2}", result.StatusCode, result.ReasonPhrase, options.url);
                    Callback.Execute(null, meta, reason);
                    logger.Error(reason);
                    return;
                }

                var json = await result.Content.ReadAsStringAsync();
                Response response = null;
                try
                {
                    response = UnityEngine.JsonUtility.FromJson<Response>(json);
                }
                catch(ArgumentException e)
                {
                    logger.Error("response is not json format.");
                    logger.Error(json);
                    return;
                }
                Callback.Execute(response.response, meta, null);

            }
        }

        string GetRequestJson(string request)
        {
            var r = new Request() { request = request };
            var ret = UnityEngine.JsonUtility.ToJson(r);
            return ret;
        }

        public void SetCallEndCallback(IJintCallback<string, string, string> Callback)
        {
            this.Callback = Callback;
        }
    }
}
