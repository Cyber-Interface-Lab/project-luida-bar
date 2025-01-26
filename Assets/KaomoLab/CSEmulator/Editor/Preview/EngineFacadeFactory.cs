using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class EngineFacadeFactory
    {
        readonly OptionBridge options;

        public EngineFacadeFactory(
            OptionBridge options
        )
        {
            this.options = options;
        }

        public EngineFacade CreateDefault(
        )
        {
            return new EngineFacade(
                options,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.ItemCreator,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.ItemDestroyer,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.SpawnPointManager
            );
        }

    }
}
