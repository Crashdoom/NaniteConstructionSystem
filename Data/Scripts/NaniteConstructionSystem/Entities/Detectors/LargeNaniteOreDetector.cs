using Sandbox.ModAPI;
using VRage.Game;
using VRageMath;

namespace NaniteConstructionSystem.Entities.Detectors
{
    public class LargeNaniteOreDetector : NaniteOreDetector
    {
        public LargeNaniteOreDetector(IMyFunctionalBlock block) : base(block)
        {
            supportFilter = true;
            maxScanningLevel = 2;
            minRange = 50f;
            basePower = 0.5f;
        }
    }

    public class NaniteMiningItem
    {
        public byte VoxelMaterial { get; set; }
        public Vector3D Position { get; set; }
        public Vector3I VoxelPosition { get; set; }
        public MyVoxelMaterialDefinition Definition { get; set; }
        public long VoxelId { get; set; }
        public long OreDetectorId { get; set; }
        public float Amount { get; set; }
    }
}
