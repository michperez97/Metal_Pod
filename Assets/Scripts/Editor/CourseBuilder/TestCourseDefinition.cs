using MetalPod.ScriptableObjects;

namespace MetalPod.EditorTools.CourseBuilder
{
    public static class TestCourseDefinition
    {
        public static CourseDefinitionData Get()
        {
            return CourseDefinitionHelpers.Build(
                "Test Course",
                "TestCourse",
                EnvironmentType.Lava,
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    50f,
                    15f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 1.25f, 22f))),
                CourseDefinitionHelpers.S(
                    SegmentType.TurnLeft,
                    30f,
                    15f,
                    checkpoint: true,
                    checkpointIndex: 0,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-5.5f, 1.25f, 16f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Ramp,
                    40f,
                    15f,
                    elevation: 4f,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(0f, 2.1f, 20f),
                        CourseDefinitionHelpers.C(5.5f, 2.1f, 27f))),
                CourseDefinitionHelpers.S(
                    SegmentType.Gap,
                    20f,
                    15f,
                    checkpoint: true,
                    checkpointIndex: 1,
                    collectibles: CourseDefinitionHelpers.Collectibles(
                        CourseDefinitionHelpers.C(-5.5f, 1.25f, 16f))),
                CourseDefinitionHelpers.S(
                    SegmentType.NarrowCorridor,
                    30f,
                    6f,
                    hasLeftWall: true,
                    hasRightWall: true),
                CourseDefinitionHelpers.S(
                    SegmentType.SCurve,
                    40f,
                    12f,
                    checkpoint: true,
                    checkpointIndex: 2),
                CourseDefinitionHelpers.S(
                    SegmentType.Straight,
                    40f,
                    15f,
                    hasLeftWall: true,
                    hasRightWall: true));
        }
    }
}
