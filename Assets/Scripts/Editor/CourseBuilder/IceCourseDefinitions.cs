using MetalPod.ScriptableObjects;

namespace MetalPod.EditorTools.CourseBuilder
{
    public static class IceCourseDefinitions
    {
        public static CourseDefinitionData GetCourse1()
        {
            return CourseDefinitionHelpers.Build(
                "Frozen Lake",
                "Ice_Course_01",
                EnvironmentType.Ice,
                CourseDefinitionHelpers.S(SegmentType.Straight, 50f, 15f),
                CourseDefinitionHelpers.S(
                    SegmentType.WideOpen,
                    80f,
                    25f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Left, z: 20f, scale: 1.8f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Right, z: 45f, scale: 1.8f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Center, z: 62f, scale: 1.6f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-7f, 1.25f, 18f),
                        CourseDefinitionHelpers.C(6f, 1.25f, 58f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    40f,
                    12f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Left, y: 8f, z: 12f),
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Right, y: 8f, z: 28f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnLeft,
                    30f,
                    12f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Center, z: 15f, scale: 1.5f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BlizzardZone", HazardPlacement.Center, z: 24f, scale: 1.3f, p1: 3f),
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Left, y: 8f, z: 38f))),
                CourseDefinitionHelpers.S(
                    SegmentType.WideOpen,
                    60f,
                    20f,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Left, z: 18f, scale: 1.6f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Right, z: 36f, scale: 1.6f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-5f, 1.25f, 16f),
                        CourseDefinitionHelpers.C(5f, 1.25f, 44f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    40f,
                    15f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 20f),
                        CourseDefinitionHelpers.C(3f, 1.25f, 30f))));
        }

        public static CourseDefinitionData GetCourse2()
        {
            return CourseDefinitionHelpers.Build(
                "Crystal Caverns",
                "Ice_Course_02",
                EnvironmentType.Ice,
                CourseDefinitionHelpers.S(SegmentType.Tunnel, 40f, 10f),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    30f,
                    6f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Center, z: 14f, scale: 1.3f),
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Center, y: 8f, z: 24f))),
                CourseDefinitionHelpers.S(
                    SegmentType.WideOpen,
                    40f,
                    15f,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IceWall", HazardPlacement.Left, z: 16f),
                        CourseDefinitionHelpers.H("IceWall", HazardPlacement.Right, z: 28f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-4f, 1.25f, 16f),
                        CourseDefinitionHelpers.C(4f, 1.25f, 28f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Tunnel,
                    50f,
                    8f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BlizzardZone", HazardPlacement.Center, z: 26f, scale: 1.4f, p1: 6f))),
                CourseDefinitionHelpers.S(
                    SegmentType.SCurve,
                    40f,
                    8f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Left, z: 12f, scale: 1.2f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Right, z: 26f, scale: 1.2f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Center, z: 34f, scale: 1.2f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 18f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    30f,
                    6f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Left, y: 8f, z: 8f),
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Center, y: 8f, z: 16f),
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Right, y: 8f, z: 24f),
                        CourseDefinitionHelpers.H("IceWall", HazardPlacement.Center, z: 26f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 22f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 3,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BlizzardZone", HazardPlacement.Center, z: 25f, scale: 1.3f, p1: 5f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Center, z: 35f, scale: 1.2f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-3f, 1.25f, 18f),
                        CourseDefinitionHelpers.C(3f, 1.25f, 36f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    30f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 16f),
                        CourseDefinitionHelpers.C(5f, 1.25f, 22f))));
        }

        public static CourseDefinitionData GetCourse3()
        {
            return CourseDefinitionHelpers.Build(
                "Avalanche Pass",
                "Ice_Course_03",
                EnvironmentType.Ice,
                CourseDefinitionHelpers.S(SegmentType.Straight, 40f, 15f),
                CourseDefinitionHelpers.S(
                    SegmentType.WideOpen,
                    60f,
                    20f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BlizzardZone", HazardPlacement.Center, z: 30f, scale: 1.5f, p1: 6f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Left, z: 18f, scale: 1.5f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Right, z: 42f, scale: 1.5f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnRight,
                    30f,
                    10f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("FallingIcicle", HazardPlacement.Right, y: 8f, z: 16f),
                        CourseDefinitionHelpers.H("IcePatch", HazardPlacement.Center, z: 18f, scale: 1.2f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    40f,
                    8f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("IceWall", HazardPlacement.Left, z: 10f),
                        CourseDefinitionHelpers.H("IceWall", HazardPlacement.Center, z: 22f),
                        CourseDefinitionHelpers.H("IceWall", HazardPlacement.Right, z: 34f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 20f),
                        CourseDefinitionHelpers.C(-4f, 1.25f, 36f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    80f,
                    15f,
                    checkpoint: true,
                    checkpointIndex: 3,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("Avalanche", HazardPlacement.Behind, z: 4f))),
                CourseDefinitionHelpers.S(
                    SegmentType.SCurve,
                    50f,
                    10f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("BlizzardZone", HazardPlacement.Center, z: 20f, scale: 1.2f, p1: 5f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(3f, 1.25f, 18f),
                        CourseDefinitionHelpers.C(-3f, 1.25f, 34f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    40f,
                    8f,
                    checkpoint: true,
                    checkpointIndex: 4,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("Avalanche", HazardPlacement.Behind, z: 3f),
                        CourseDefinitionHelpers.H("IceWall", HazardPlacement.Center, z: 30f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Ramp,
                    20f,
                    8f,
                    elevation: 5f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 2f, 12f),
                        CourseDefinitionHelpers.C(2f, 2f, 16f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    30f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-4f, 1.25f, 10f),
                        CourseDefinitionHelpers.C(4f, 1.25f, 20f),
                        CourseDefinitionHelpers.C(0f, 1.25f, 26f),
                        CourseDefinitionHelpers.C(0f, 1.25f, 14f))));
        }
    }
}
