using MetalPod.ScriptableObjects;

namespace MetalPod.EditorTools.CourseBuilder
{
    public static class LavaCourseDefinitions
    {
        public static CourseDefinitionData GetCourse1()
        {
            return CourseDefinitionHelpers.Build(
                "Inferno Gate",
                "Lava_Course_01",
                EnvironmentType.Lava,
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    60f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 28f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Left, z: 24f, scale: 1.2f),
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Right, z: 24f, scale: 1.2f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnRight,
                    40f,
                    12f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Right, z: 18f, scale: 1.4f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    60f,
                    15f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, z: 18f, scale: 1f),
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, x: -2f, z: 42f, scale: 1f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-4f, 1.25f, 16f))),
                CourseDefinitionHelpers.S(
                    SegmentType.WideOpen,
                    50f,
                    20f,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Left, z: 16f, scale: 1.4f),
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Right, z: 31f, scale: 1.4f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 28f),
                        CourseDefinitionHelpers.C(6f, 1.25f, 40f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    40f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, z: 20f, scale: 1f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 32f))));
        }

        public static CourseDefinitionData GetCourse2()
        {
            return CourseDefinitionHelpers.Build(
                "Magma Run",
                "Lava_Course_02",
                EnvironmentType.Lava,
                CourseDefinitionHelpers.S(SegmentType.Straight, 40f, 15f),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    60f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Center, z: 12f, p1: 3f),
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Center, z: 30f, p1: 4f),
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Center, z: 48f, p1: 5f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-3f, 1.25f, 22f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnLeft,
                    30f,
                    12f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Left, z: 16f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Ramp,
                    30f,
                    12f,
                    elevation: 5f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 2f, 17f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Bridge,
                    40f,
                    4f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.BothSides, z: 20f, scale: 1.6f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.3f, 22f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    12f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Left, z: 10f),
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Center, z: 20f),
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Right, z: 30f),
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Center, z: 40f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-4f, 1.25f, 15f))),
                CourseDefinitionHelpers.S(
                    SegmentType.SCurve,
                    40f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, z: 14f),
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, x: 2f, z: 30f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.4f, 20f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    30f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 3,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Center, z: 14f, p1: 2.5f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    40f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-3f, 1.25f, 14f),
                        CourseDefinitionHelpers.C(3f, 1.25f, 28f),
                        CourseDefinitionHelpers.C(0f, 1.25f, 34f))));
        }

        public static CourseDefinitionData GetCourse3()
        {
            return CourseDefinitionHelpers.Build(
                "Eruption",
                "Lava_Course_03",
                EnvironmentType.Lava,
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    40f,
                    8f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.2f, 20f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("HeatZone", HazardPlacement.Center, z: 22f, scale: 1.2f),
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Left, z: 34f),
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Right, z: 40f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-3f, 1.25f, 11f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Bridge,
                    30f,
                    4f,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaGeyser", HazardPlacement.Center, y: -1f, z: 14f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnRight,
                    30f,
                    10f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("VolcanicEruption", HazardPlacement.Center, z: 14f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    60f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, x: -2f, z: 12f),
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, x: 2f, z: 26f),
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, x: 0f, z: 40f),
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, x: -1f, z: 52f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 36f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    30f,
                    6f,
                    checkpoint: true,
                    checkpointIndex: 2,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.BothSides, z: 15f, scale: 1.2f))),
                CourseDefinitionHelpers.S(
                    SegmentType.SCurve,
                    50f,
                    8f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.BothSides, z: 22f, scale: 1.2f),
                        CourseDefinitionHelpers.H("FallingDebris", HazardPlacement.Overhead, x: 0f, z: 32f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-3f, 1.25f, 20f),
                        CourseDefinitionHelpers.C(3f, 1.25f, 40f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Bridge,
                    20f,
                    3f,
                    checkpoint: true,
                    checkpointIndex: 3,
                    hasLeftWall: false,
                    hasRightWall: false,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.BothSides, z: 9f, scale: 1.4f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 11f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Ramp,
                    30f,
                    10f,
                    checkpoint: true,
                    checkpointIndex: 4,
                    elevation: 8f,
                    hazards: CourseDefinitionHelpers.Hazards(
                        CourseDefinitionHelpers.H("LavaFlow", HazardPlacement.Center, y: -2f, z: 18f)),
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 2.3f, 15f),
                        CourseDefinitionHelpers.C(3f, 2.4f, 24f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    30f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 12f))));
        }
    }
}
