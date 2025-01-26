using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class GroupStateProxyMapper
        : IGroupStateProxyMapper
    {
        readonly Dictionary<string, GroupStateProxy.GroupStateProxySet> states = new();

        public GroupStateProxyMapper(
        )
        {
        }

        public void ApplyState(GroupStateProxy groupStateProxy, string hostId)
        {
            if (states.ContainsKey(hostId))
            {
                groupStateProxy.OverwriteState(states[hostId]);
            }
            else
            {
                //Hostより前にRunしてしまった場合、Memberのstateを使うことにする。
                //onStartは最初のフレームで動くため問題ないはず
                states[hostId] = groupStateProxy.GetState();
            }
        }
    }
}
