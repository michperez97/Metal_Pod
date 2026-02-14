using System;
using System.Collections.Generic;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.EditorTools.CourseBuilder
{
    [Serializable]
    public class CourseDefinitionData
    {
        public string courseName;
        public string sceneName;
        public EnvironmentType environmentType;
        public float trackWidth = 15f;
        public CourseSegmentData[] segments;
        public Vector3 startPosition = Vector3.zero;
        public float startRotation;
    }

    public static class CourseDefinitionHelpers
    {
        public static HazardEntry H(string type, HazardPlacement placement, float x = 0f, float y = 0f, float z = 0f, float scale = 1f, float p1 = 0f)
        {
            return HazardEntry.Create(type, placement, new Vector3(x, y, z), scale, p1);
        }

        public static CollectibleEntry C(float x, float y, float z, MetalPod.Course.CollectibleType type = MetalPod.Course.CollectibleType.Currency)
        {
            return CollectibleEntry.Create(new Vector3(x, y, z), type);
        }

        public static HazardEntry[] Hazards(params HazardEntry[] entries)
        {
            return entries ?? Array.Empty<HazardEntry>();
        }

        public static CollectibleEntry[] Collectibles(params CollectibleEntry[] entries)
        {
            return entries ?? Array.Empty<CollectibleEntry>();
        }

        public static CourseSegmentData S(
            SegmentType type,
            float length,
            float width,
            bool checkpoint = false,
            int checkpointIndex = 0,
            float elevation = 0f,
            bool hasLeftWall = true,
            bool hasRightWall = true,
            float wallHeight = 5f,
            HazardEntry[] hazards = null,
            CollectibleEntry[] collectibles = null)
        {
            return CourseSegmentData.Create(
                type,
                length,
                width,
                checkpoint,
                checkpointIndex,
                elevation,
                hasLeftWall,
                hasRightWall,
                wallHeight,
                hazards,
                collectibles);
        }

        public static CourseDefinitionData Build(string courseName, string sceneName, EnvironmentType environmentType, params CourseSegmentData[] segments)
        {
            return new CourseDefinitionData
            {
                courseName = courseName,
                sceneName = sceneName,
                environmentType = environmentType,
                trackWidth = 15f,
                startPosition = Vector3.zero,
                startRotation = 0f,
                segments = segments ?? Array.Empty<CourseSegmentData>()
            };
        }

        public static List<CollectibleEntry> ScatterCollectibles(int count, float width, float length, float y = 1.25f)
        {
            var results = new List<CollectibleEntry>(count);
            if (count <= 0)
            {
                return results;
            }

            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / (count + 1f);
                float x = Mathf.Lerp(-width * 0.35f, width * 0.35f, (i % 2 == 0) ? 0.25f : 0.75f);
                float z = Mathf.Lerp(length * 0.15f, length * 0.85f, t);
                results.Add(C(x, y, z));
            }

            return results;
        }
    }
}
