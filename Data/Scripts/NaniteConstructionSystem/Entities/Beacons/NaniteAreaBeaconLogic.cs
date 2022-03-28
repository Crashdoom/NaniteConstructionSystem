using System;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.Utils;
using Sandbox.ModAPI;
using NaniteConstructionSystem.Extensions;

namespace NaniteConstructionSystem.Entities.Beacons
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Projector), false, "LargeNaniteAreaBeacon")]
    public class NaniteAreaBeaconLogic : MyGameLogicComponent
    {
        private NaniteBeacon m_beacon = null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            if (Sync.IsClient)
                NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            try {
                base.UpdateOnceBeforeFrame();

                Logging.Instance.WriteLine($"ADDING Area Beacon: {Entity.EntityId}", 1);
                m_beacon = new NaniteAreaBeacon((IMyFunctionalBlock)Entity);

                if (Sync.IsClient)
                    NaniteConstructionManager.NaniteSync.SendNeedBeaconTerminalSettings(Entity.EntityId);

            } catch(Exception exc) {
                MyLog.Default.WriteLineAndConsole($"##MOD: nanites UpdateOnceBeforeFrame, ERROR: {exc}");
            }
        }

        public override void Close()
        {
            try
            {
                if (m_beacon != null)
                    m_beacon.Close();

                base.Close();
            }
            catch (System.Exception e)
                { Logging.Instance.WriteLine($"NaniteAreaBeaconLogic.Close exception:\n{e.ToString()}"); }
        }

        public override void UpdateBeforeSimulation()
        {
            try {
                base.UpdateBeforeSimulation();
                m_beacon.Update();
            } catch(Exception exc) {
                MyLog.Default.WriteLineAndConsole($"##MOD: nanites UpdateBeforeSimulation, ERROR: {exc}");
            }
        }
    }
}
