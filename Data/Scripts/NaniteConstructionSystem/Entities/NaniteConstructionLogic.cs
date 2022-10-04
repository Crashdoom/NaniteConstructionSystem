using System;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.Game.ModAPI;
using VRage.Utils;
using Sandbox.Game.Entities;

namespace NaniteConstructionSystem.Entities
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ShipWelder), false, "LargeNaniteControlFacility", "SmallNaniteControlFacility")]
    public class LargeControlFacilityLogic : MyGameLogicComponent
    {
        private NaniteConstructionBlock m_block = null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            Logging.Instance.WriteLine($"[Nanite] Creating LargeControlFacility...", 2);
            try {
                base.UpdateOnceBeforeFrame();
                m_block = NaniteConstructionManager.CreateNaniteFactory(Entity);
            } catch(Exception exc) {
                MyLog.Default.WriteLineAndConsole($"##MOD: Nanites UpdateOnceBeforeFrame, ERROR: {exc}");
            }
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                m_block.Update();
            }
            catch (System.Exception e)
                { Logging.Instance.WriteLine($"LargeControlFacilityLogic.UpdateBeforeSimulation Exception: {e}"); }
        }

        public override void Close()
        {
            if (NaniteConstructionManager.NaniteBlocks != null && Entity != null)
            {
                NaniteConstructionManager.NaniteBlocks.Remove(Entity.EntityId);
                Logging.Instance.WriteLine(string.Format("REMOVING Nanite Factory: {0}", Entity.EntityId), 1);
            }

            if (m_block != null)
                m_block.Unload();
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Projector), false)]
    public class NaniteProjectorLogic : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!NaniteConstructionManager.ProjectorBlocks.ContainsKey(Entity.EntityId))
                NaniteConstructionManager.ProjectorBlocks.Add(Entity.EntityId, (IMyCubeBlock)Entity);
        }

        /// <summary>
        /// GetObjectBuilder on a block is always null
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return null;
        }

        public override void Close()
        {
            if (NaniteConstructionManager.ProjectorBlocks == null)
                return;

            if (NaniteConstructionManager.ProjectorBlocks.ContainsKey(Entity.EntityId))
                NaniteConstructionManager.ProjectorBlocks.Remove(Entity.EntityId);
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), false)]
    public class NaniteAssemblerLogic : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!NaniteConstructionManager.AssemblerBlocks.ContainsKey(Entity.EntityId))
            {
                NaniteConstructionManager.AssemblerBlocks.Add(Entity.EntityId, (IMyCubeBlock)Entity);
                if (NaniteConstructionManager.NaniteSync != null)
                    NaniteConstructionManager.NaniteSync.SendNeedAssemblerSettings(Entity.EntityId);
            }
        }

        /// <summary>
        /// GetObjectBuilder on a block is always null
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return null;
        }

        public override void Close()
        {
            if (NaniteConstructionManager.AssemblerBlocks == null)
                return;

            if (NaniteConstructionManager.AssemblerBlocks.ContainsKey(Entity.EntityId))
                NaniteConstructionManager.AssemblerBlocks.Remove(Entity.EntityId);
        }
    }
}
