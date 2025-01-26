using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class JintProgramStatus
        : IProgramStatus
    {
        readonly Jint.Engine engine;

        public JintProgramStatus(
            Jint.Engine engine
        )
        {
            this.engine = engine;
        }

        public string GetLineInfo()
        {
            if (!engine.Debugger.CurrentLocation.HasValue) return "";
            var location = engine.Debugger.CurrentLocation.Value;
            return String.Format(
                "{1}.{2}-{3}.{4}",
                location.SourceFile,
                location.Start.Line,
                location.Start.Column,
                location.End.Line,
                location.End.Column
            );

        }
    }
}
