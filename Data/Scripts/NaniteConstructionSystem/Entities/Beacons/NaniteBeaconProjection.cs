﻿using Sandbox.ModAPI;
using VRageMath;
using Sandbox.Game.Entities;

using NaniteConstructionSystem.Entities.Effects;
using NaniteConstructionSystem.Extensions;

namespace NaniteConstructionSystem.Entities.Beacons
{
    public class NaniteBeaconProjection : NaniteBeacon
    {
        public NaniteBeaconProjection(IMyFunctionalBlock beaconBlock) : base(beaconBlock)
        {
            if (Sync.IsClient)
                m_effects.Add(new NaniteBeaconEffect((MyCubeBlock)m_beaconBlock, Vector3.Zero, new Vector4(0.95f, 0.0f, 0.95f, 0.75f)));
        }
    }
}
