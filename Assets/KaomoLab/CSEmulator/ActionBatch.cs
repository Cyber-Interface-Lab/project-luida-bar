using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public class ActionBatch
    {
        readonly List<Action> actions = new List<Action>();

        public ActionBatch()
        {
        }

        public void Add(Action action)
        {
            actions.Add(action);
        }

        public void Do()
        {
            foreach (var Action in actions)
            {
                Action();
            }
            actions.Clear();
        }
    }
}
