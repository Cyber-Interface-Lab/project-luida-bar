using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class DebugLogFactory
        : Engine.ILoggerFactory
    {
        readonly UnityEngine.GameObject gameObject;
        readonly EmulatorOptions options;

        public DebugLogFactory(
            UnityEngine.GameObject gameObject,
            EmulatorOptions options
        )
        {
            this.gameObject = gameObject;
            this.options = options;
        }

        public ILogger Create(IProgramStatus programStatus)
        {
            return new DebugLogger(
                gameObject,
                programStatus,
                options               
            );
        }
    }
}
