using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.VersionControl;

namespace Assets.KaomoLab.CSEmulator.Editor
{
    public class HeaderedLogger
        : ILogger
    {
        readonly string header;
        readonly ILogger logger;

        public HeaderedLogger(string header, ILogger logger)
        {
            this.header = header;
            this.logger = logger;
        }

        public void Error(string message)
        {
            logger.Error(String.Format("{0}{1}", header, message));
        }

        public void Exception(JsError e)
        {
            logger.Error(String.Format("{0}", header));
            logger.Exception(e);
        }

        public void Exception(Exception e)
        {
            var ne = new Exception(String.Format("{0}{1}", header, e.Message), e);
            logger.Exception(ne);
        }

        public void Exception(Exception e, UnityEngine.GameObject source)
        {
            var ne = new Exception(String.Format("{0}{1}", header, e.Message), e);
            logger.Exception(ne, source);
        }

        public void Info(string message)
        {
            logger.Info(String.Format("{0}{1}", header, message));
        }

        public void Warning(string message)
        {
            logger.Warning(String.Format("{0}{1}", header, message));
        }
    }
}
