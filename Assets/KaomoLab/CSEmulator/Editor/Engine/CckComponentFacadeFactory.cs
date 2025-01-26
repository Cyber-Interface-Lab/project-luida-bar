using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class CckComponentFacadeFactory
        : EmulateClasses.ICckComponentFacadeFactory
    {
        readonly ClusterVR.CreatorKit.Editor.Preview.RoomState.RoomStateRepository roomStateRepository;
        readonly ClusterVR.CreatorKit.Trigger.ISignalGenerator signalGenerator;
        readonly ClusterVR.CreatorKit.Gimmick.IGimmickUpdater gimmickUpdater;

        public CckComponentFacadeFactory(
            ClusterVR.CreatorKit.Editor.Preview.RoomState.RoomStateRepository roomStateRepository,
            ClusterVR.CreatorKit.Trigger.ISignalGenerator signalGenerator,
            ClusterVR.CreatorKit.Gimmick.IGimmickUpdater gimmickUpdater
        )
        {
            this.roomStateRepository = roomStateRepository;
            this.signalGenerator = signalGenerator;
            this.gimmickUpdater = gimmickUpdater;
        }

        public ICckComponentFacade Create(GameObject gameObject)
        {
            var ret = new CckComponentFacade(
                gameObject,
                roomStateRepository, signalGenerator, gimmickUpdater
            );
            return ret;
        }
    }
}
