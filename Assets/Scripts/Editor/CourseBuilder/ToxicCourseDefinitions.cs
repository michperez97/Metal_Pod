using MetalPod.ScriptableObjects;

namespace MetalPod.EditorTools.CourseBuilder
{
    public static class ToxicCourseDefinitions
    {
        public static CourseDefinitionData GetCourse1()
        {
            return CourseDefinitionHelpers.Build(
                "Waste Disposal",
                "Toxic_Course_01",
                EnvironmentType.Toxic,
                CourseDefinitionHelpers.S(SegmentType.Straight, 40f, 12f),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Left, z: 12f, p1: 3f),
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Center, z: 26f, p1: 3.5f),
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Right, z: 40f, p1: 4f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 18f))),
                CourseDefinitionHelpers.S(
                    SegmentType.WideOpen,
                    40f,
                    18f,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Left, z: 10f, scale: 1.4f),
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Right, z: 24f, scale: 1.4f),
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Left, z: 34f, scale: 1.3f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnLeft,
                    30f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Left, z: 6f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Left, z: 12f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Left, z: 18f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Left, z: 24f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    40f,
                    8f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Center, z: 20f, scale: 1.6f, p1: 0f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-2.5f, 1.25f, 18f),
                        CourseDefinitionHelpers.C(2.5f, 1.25f, 30f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Left, z: 16f, scale: 1.2f),
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Right, z: 30f, scale: 1.2f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Center, z: 40f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Center, z: 44f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 22f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    30f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 14f),
                        CourseDefinitionHelpers.C(4f, 1.25f, 22f))));
        }

        public static CourseDefinitionData GetCourse2()
        {
            return CourseDefinitionHelpers.Build(
                "The Foundry",
                "Toxic_Course_02",
                EnvironmentType.Toxic,
                CourseDefinitionHelpers.S(SegmentType.Straight, 40f, 12f),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    40f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 10f),
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 20f),
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 30f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    30f,
                    8f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 10f),
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 22f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Center, z: 18f, p1: 1f),
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Right, z: 34f, p1: 4f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-3f, 1.25f, 14f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnRight,
                    30f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Right, z: 16f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Bridge,
                    30f,
                    5f,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.BothSides, z: 16f, p1: 1f),
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 20f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 14f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 3,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 10f),
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 24f),
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Left, z: 34f, p1: 3f),
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Right, z: 42f, p1: 1f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-3f, 1.25f, 22f),
                        CourseDefinitionHelpers.C(3f, 1.25f, 40f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    30f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 10f),
                        CourseDefinitionHelpers.C(4f, 1.25f, 20f),
                        CourseDefinitionHelpers.C(-4f, 1.25f, 25f),
                        CourseDefinitionHelpers.C(0f, 1.25f, 16f))));
        }

        public static CourseDefinitionData GetCourse3()
        {
            return CourseDefinitionHelpers.Build(
                "Meltdown",
                "Toxic_Course_03",
                EnvironmentType.Toxic,
                CourseDefinitionHelpers.S(SegmentType.NarrowCorridor, 30f, 8f),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    40f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Left, z: 6f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Right, z: 12f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Left, z: 18f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Right, z: 24f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Left, z: 30f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Right, z: 36f),
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Center, z: 22f, p1: 3f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 14f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Bridge,
                    20f,
                    4f,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.BothSides, z: 10f, p1: 2f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 8f),
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 16f),
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 24f),
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 32f),
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 40f))),
                CourseDefinitionHelpers.S(
                    SegmentType.WideOpen,
                    40f,
                    15f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Random, z: 10f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Random, z: 16f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Random, z: 22f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Random, z: 28f),
                        CourseDefinitionHelpers.H("BarrelExplosion", HazardPlacement.Random, z: 34f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-4f, 1.25f, 10f),
                        CourseDefinitionHelpers.C(4f, 1.25f, 20f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Tunnel,
                    40f,
                    8f,
                    checkpoint: true,
                    checkpointIndex: 3,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 8f),
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 18f),
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 30f),
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Center, z: 24f, p1: 3f))),
                CourseDefinitionHelpers.S(
                    SegmentType.SCurve,
                    40f,
                    8f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Left, z: 12f, p1: 1.5f),
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Right, z: 28f, p1: 1.5f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-2.5f, 1.25f, 14f),
                        CourseDefinitionHelpers.C(2.5f, 1.25f, 30f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    30f,
                    6f,
                    checkpoint: true,
                    checkpointIndex: 4,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IndustrialPress", HazardPlacement.Center, z: 8f),
                        CourseDefinitionHelpers.H("ElectricFence", HazardPlacement.Center, z: 16f),
                        CourseDefinitionHelpers.H("ToxicGas", HazardPlacement.Center, z: 20f, p1: 3f),
                        CourseDefinitionHelpers.H("AcidPool", HazardPlacement.Center, z: 24f, p1: 1.5f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Ramp,
                    20f,
                    6f,
                    elevation: 5f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 2f, 10f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    30f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 10f),
                        CourseDefinitionHelpers.C(-4f, 1.25f, 16f),
                        CourseDefinitionHelpers.C(4f, 1.25f, 22f),
                        CourseDefinitionHelpers.C(0f, 1.25f, 26f))));
        }
    }
}
