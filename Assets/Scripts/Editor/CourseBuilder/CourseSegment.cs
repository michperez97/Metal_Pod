using System;
using MetalPod.Course;
using UnityEngine;

namespace MetalPod.EditorTools.CourseBuilder
{
    public enum SegmentType
    {
        Straight,
        TurnLeft,
        TurnRight,
        SCurve,
        Ramp,
        Gap,
        NarrowCorridor,
        WideOpen,
        Bridge,
        Tunnel
    }

    public enum HazardPlacement
    {
        Left,
        Center,
        Right,
        Random,
        BothSides,
        Overhead,
        Behind
    }

    [Serializable]
    public class CourseSegmentData
    {
        public SegmentType type;
        public float length = 30f;
        public float width = 15f;
        public float elevation;
        public bool hasCheckpoint;
        public int checkpointIndex;
        public HazardEntry[] hazards;
        public CollectibleEntry[] collectibles;
        public bool hasLeftWall = true;
        public bool hasRightWall = true;
        public float wallHeight = 5f;

        public static CourseSegmentData Create(
            SegmentType type,
            float length,
            float width,
            bool hasCheckpoint = false,
            int checkpointIndex = 0,
            float elevation = 0f,
            bool hasLeftWall = true,
            bool hasRightWall = true,
            float wallHeight = 5f,
            HazardEntry[] hazards = null,
            CollectibleEntry[] collectibles = null)
        {
            return new CourseSegmentData
            {
                type = type,
                length = length,
                width = width,
                hasCheckpoint = hasCheckpoint,
                checkpointIndex = checkpointIndex,
                elevation = elevation,
                hasLeftWall = hasLeftWall,
                hasRightWall = hasRightWall,
                wallHeight = wallHeight,
                hazards = hazards ?? Array.Empty<HazardEntry>(),
                collectibles = collectibles ?? Array.Empty<CollectibleEntry>()
            };
        }
    }

    [Serializable]
    public class HazardEntry
    {
        public string hazardType;
        public HazardPlacement placement;
        public Vector3 localOffset;
        public float scale = 1f;
        public float customParam1;

        public static HazardEntry Create(
            string hazardType,
            HazardPlacement placement,
            Vector3 localOffset,
            float scale = 1f,
            float customParam1 = 0f)
        {
            return new HazardEntry
            {
                hazardType = hazardType,
                placement = placement,
                localOffset = localOffset,
                scale = scale,
                customParam1 = customParam1
            };
        }
    }

    [Serializable]
    public class CollectibleEntry
    {
        public Vector3 localOffset;
        public CollectibleType type;

        public static CollectibleEntry Create(Vector3 localOffset, CollectibleType type = CollectibleType.Currency)
        {
            return new CollectibleEntry
            {
                localOffset = localOffset,
                type = type
            };
        }
    }
}
