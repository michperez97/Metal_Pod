using System;
using System.Collections.Generic;
using MetalPod.Course;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using MetalPod.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetalPod.EditorTools.CourseBuilder
{
    public static class CourseBuilder
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string GeneratedDataFolder = "Assets/ScriptableObjects/Data/GeneratedCourses";
        private const string GeneratedStatsFolder = "Assets/ScriptableObjects/Data/Generated";

        [MenuItem("Metal Pod/Courses/Build All Courses")]
        public static void BuildAllCourses()
        {
            BuildTestCourse();
            BuildLavaCourses();
            BuildIceCourses();
            BuildToxicCourses();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CourseBuilder] Built all 10 courses.");
        }

        [MenuItem("Metal Pod/Courses/Build Test Course")]
        public static void BuildTestCourse()
        {
            BuildCourse(TestCourseDefinition.Get(), true);
        }

        [MenuItem("Metal Pod/Courses/Build Lava Courses")]
        public static void BuildLavaCourses()
        {
            BuildCourse(LavaCourseDefinitions.GetCourse1(), false);
            BuildCourse(LavaCourseDefinitions.GetCourse2(), false);
            BuildCourse(LavaCourseDefinitions.GetCourse3(), false);
        }

        [MenuItem("Metal Pod/Courses/Build Ice Courses")]
        public static void BuildIceCourses()
        {
            BuildCourse(IceCourseDefinitions.GetCourse1(), false);
            BuildCourse(IceCourseDefinitions.GetCourse2(), false);
            BuildCourse(IceCourseDefinitions.GetCourse3(), false);
        }

        [MenuItem("Metal Pod/Courses/Build Toxic Courses")]
        public static void BuildToxicCourses()
        {
            BuildCourse(ToxicCourseDefinitions.GetCourse1(), false);
            BuildCourse(ToxicCourseDefinitions.GetCourse2(), false);
            BuildCourse(ToxicCourseDefinitions.GetCourse3(), false);
        }

        private static void BuildCourse(CourseDefinitionData definition, bool forceGreybox)
        {
            if (definition == null || definition.segments == null || definition.segments.Length == 0)
            {
                Debug.LogError("[CourseBuilder] Invalid course definition.");
                return;
            }

            EnsureDirectory(ScenesFolder);
            EnsureDirectory(GeneratedDataFolder);
            EnsureDirectory(GeneratedStatsFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            GameObject environmentRoot = new GameObject("Environment");
            GameObject courseRoot = new GameObject("Course");
            GameObject hazardsRoot = new GameObject("Hazards");
            GameObject collectiblesRoot = new GameObject("Collectibles");

            var segmentObjects = new List<GameObject>(definition.segments.Length);
            var groundRenderers = new List<Renderer>(definition.segments.Length * 2);

            Vector3 cursorPosition = definition.startPosition;
            float cursorAngle = definition.startRotation;

            for (int i = 0; i < definition.segments.Length; i++)
            {
                CourseSegmentData segment = definition.segments[i];
                GameObject segmentObject = GenerateSegment(
                    segment,
                    cursorPosition,
                    cursorAngle,
                    definition.environmentType,
                    environmentRoot.transform,
                    i,
                    groundRenderers);

                segmentObjects.Add(segmentObject);
                (cursorPosition, cursorAngle) = AdvanceCursor(segment, cursorPosition, cursorAngle);
            }

            Transform spawnPoint = CreateSpawnPoint(definition.startPosition, definition.startRotation, courseRoot.transform);
            PlaceStartLine(definition.startPosition, definition.startRotation, definition.trackWidth, courseRoot.transform);
            FinishLine finishLine = PlaceFinishLine(cursorPosition, cursorAngle, definition.trackWidth, courseRoot.transform);
            PlaceCheckpoints(definition, segmentObjects, courseRoot.transform);
            PlaceCollectibles(definition, segmentObjects, collectiblesRoot.transform);
            HazardPlacer.PlaceAllHazards(definition, segmentObjects, hazardsRoot.transform);

            HovercraftController hovercraft = PlaceHovercraft(spawnPoint.position + Vector3.up * 0.75f, spawnPoint.rotation);
            ConfigureMainCamera(hovercraft.transform);
            AddHud();

            CourseManager courseManager = CreateCourseManager(definition, courseRoot.transform, finishLine, spawnPoint);

            bool useGreybox = forceGreybox || string.Equals(definition.sceneName, "TestCourse", StringComparison.OrdinalIgnoreCase);
            EnvironmentDecorator.ApplyEnvironment(definition.environmentType, useGreybox, groundRenderers);

            EditorUtility.SetDirty(courseManager);
            EditorSceneManager.MarkSceneDirty(scene);

            string scenePath = $"{ScenesFolder}/{definition.sceneName}.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[CourseBuilder] Built {definition.sceneName} at {scenePath}");
        }

        private static GameObject GenerateSegment(
            CourseSegmentData segment,
            Vector3 position,
            float angle,
            EnvironmentType environment,
            Transform parent,
            int index,
            List<Renderer> groundRenderers)
        {
            GameObject root = new GameObject($"Segment_{index + 1:00}_{segment.type}");
            root.transform.SetParent(parent);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            switch (segment.type)
            {
                case SegmentType.Straight:
                    CreateGroundQuad(root.transform, segment.length, segment.width, environment, groundRenderers);
                    CreateStraightWalls(root.transform, segment, segment.width, segment.length, 0f);
                    break;
                case SegmentType.TurnLeft:
                    CreateTurnGeometry(root.transform, segment, true, environment, groundRenderers);
                    break;
                case SegmentType.TurnRight:
                    CreateTurnGeometry(root.transform, segment, false, environment, groundRenderers);
                    break;
                case SegmentType.SCurve:
                    CreateSCurveGeometry(root.transform, segment, environment, groundRenderers);
                    break;
                case SegmentType.Ramp:
                    CreateRampGeometry(root.transform, segment, environment, groundRenderers);
                    CreateStraightWalls(root.transform, segment, segment.width, segment.length, segment.elevation * 0.5f);
                    break;
                case SegmentType.Gap:
                    CreateGapGeometry(root.transform, segment, environment, groundRenderers);
                    CreateStraightWalls(root.transform, segment, segment.width, segment.length, 0f);
                    break;
                case SegmentType.NarrowCorridor:
                    float corridorWidth = Mathf.Max(4f, segment.width);
                    CreateGroundQuad(root.transform, segment.length, corridorWidth, environment, groundRenderers);
                    CreateStraightWalls(root.transform, segment, corridorWidth, segment.length, 0f);
                    break;
                case SegmentType.WideOpen:
                    CreateGroundQuad(root.transform, segment.length, segment.width * 1.5f, environment, groundRenderers);
                    break;
                case SegmentType.Bridge:
                    float bridgeWidth = Mathf.Max(2f, segment.width);
                    CreateGroundQuad(root.transform, segment.length, bridgeWidth, environment, groundRenderers);
                    break;
                case SegmentType.Tunnel:
                    CreateGroundQuad(root.transform, segment.length, segment.width, environment, groundRenderers);
                    CreateStraightWalls(root.transform, segment, segment.width, segment.length, 0f);
                    CreateCeiling(root.transform, segment.length, segment.width, segment.wallHeight, segment.elevation * 0.5f);
                    break;
            }

            return root;
        }

        private static void CreateGroundQuad(Transform parent, float length, float width, EnvironmentType _, List<Renderer> groundRenderers)
        {
            Renderer renderer = CreateBox(
                parent,
                "Ground",
                new Vector3(0f, -0.1f, length * 0.5f),
                new Vector3(Mathf.Max(2f, width), 0.2f, Mathf.Max(2f, length)),
                Quaternion.identity,
                true,
                true);

            if (renderer != null)
            {
                groundRenderers.Add(renderer);
            }
        }

        private static void CreateRampGeometry(Transform parent, CourseSegmentData segment, EnvironmentType _, List<Renderer> groundRenderers)
        {
            float slopeAngle = Mathf.Atan2(segment.elevation, Mathf.Max(0.1f, segment.length)) * Mathf.Rad2Deg;
            Renderer renderer = CreateBox(
                parent,
                "Ramp",
                new Vector3(0f, (segment.elevation * 0.5f) - 0.1f, segment.length * 0.5f),
                new Vector3(Mathf.Max(2f, segment.width), 0.25f, Mathf.Max(2f, segment.length)),
                Quaternion.Euler(-slopeAngle, 0f, 0f),
                true,
                true);

            if (renderer != null)
            {
                groundRenderers.Add(renderer);
            }
        }

        private static void CreateGapGeometry(Transform parent, CourseSegmentData segment, EnvironmentType _, List<Renderer> groundRenderers)
        {
            float preLength = segment.length * 0.3f;
            float postLength = segment.length * 0.3f;

            Renderer before = CreateBox(
                parent,
                "Gap_Before",
                new Vector3(0f, -0.1f, preLength * 0.5f),
                new Vector3(Mathf.Max(2f, segment.width), 0.2f, Mathf.Max(2f, preLength)),
                Quaternion.identity,
                true,
                true);

            Renderer after = CreateBox(
                parent,
                "Gap_After",
                new Vector3(0f, -0.1f, segment.length - (postLength * 0.5f)),
                new Vector3(Mathf.Max(2f, segment.width), 0.2f, Mathf.Max(2f, postLength)),
                Quaternion.identity,
                true,
                true);

            if (before != null)
            {
                groundRenderers.Add(before);
            }

            if (after != null)
            {
                groundRenderers.Add(after);
            }
        }

        private static void CreateTurnGeometry(Transform parent, CourseSegmentData segment, bool isLeft, EnvironmentType _, List<Renderer> groundRenderers)
        {
            int slices = 8;
            float sign = isLeft ? -1f : 1f;
            float radius = Mathf.Max(5f, segment.length / (Mathf.PI * 0.5f));
            Vector3 center = new Vector3(sign * radius, 0f, 0f);
            Vector3 startRadial = new Vector3(-sign * radius, 0f, 0f);

            for (int i = 0; i < slices; i++)
            {
                float t0 = i / (float)slices;
                float t1 = (i + 1f) / slices;

                Vector3 p0 = center + (Quaternion.Euler(0f, sign * 90f * t0, 0f) * startRadial);
                Vector3 p1 = center + (Quaternion.Euler(0f, sign * 90f * t1, 0f) * startRadial);
                Vector3 mid = (p0 + p1) * 0.5f;
                Vector3 forward = (p1 - p0).normalized;
                float sliceLength = Vector3.Distance(p0, p1);

                Renderer ground = CreateBox(
                    parent,
                    $"TurnSlice_{i:00}",
                    new Vector3(mid.x, -0.1f, mid.z),
                    new Vector3(Mathf.Max(2f, segment.width), 0.2f, Mathf.Max(1f, sliceLength + 0.1f)),
                    Quaternion.LookRotation(forward, Vector3.up),
                    true,
                    true);

                if (ground != null)
                {
                    groundRenderers.Add(ground);
                }

                if (segment.hasLeftWall || segment.hasRightWall)
                {
                    Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                    float wallOffset = (segment.width * 0.5f) + 0.3f;
                    Vector3 wallScale = new Vector3(0.6f, Mathf.Max(2f, segment.wallHeight), Mathf.Max(1f, sliceLength + 0.2f));

                    if (segment.hasLeftWall)
                    {
                        CreateBox(
                            parent,
                            $"TurnWallLeft_{i:00}",
                            new Vector3(mid.x - (right.x * wallOffset), (segment.wallHeight * 0.5f), mid.z - (right.z * wallOffset)),
                            wallScale,
                            Quaternion.LookRotation(forward, Vector3.up),
                            false,
                            true);
                    }

                    if (segment.hasRightWall)
                    {
                        CreateBox(
                            parent,
                            $"TurnWallRight_{i:00}",
                            new Vector3(mid.x + (right.x * wallOffset), (segment.wallHeight * 0.5f), mid.z + (right.z * wallOffset)),
                            wallScale,
                            Quaternion.LookRotation(forward, Vector3.up),
                            false,
                            true);
                    }
                }
            }
        }

        private static void CreateSCurveGeometry(Transform parent, CourseSegmentData segment, EnvironmentType _, List<Renderer> groundRenderers)
        {
            int slices = 10;
            float amplitude = Mathf.Max(1f, segment.width * 0.28f);
            var points = new Vector3[slices + 1];

            for (int i = 0; i <= slices; i++)
            {
                float t = i / (float)slices;
                float x = Mathf.Sin(t * Mathf.PI * 2f) * amplitude;
                float z = t * segment.length;
                points[i] = new Vector3(x, -0.1f, z);
            }

            for (int i = 0; i < slices; i++)
            {
                Vector3 p0 = points[i];
                Vector3 p1 = points[i + 1];
                Vector3 mid = (p0 + p1) * 0.5f;
                Vector3 direction = (p1 - p0).normalized;
                float sliceLength = Vector3.Distance(p0, p1);

                Renderer ground = CreateBox(
                    parent,
                    $"SCurveSlice_{i:00}",
                    mid,
                    new Vector3(Mathf.Max(2f, segment.width), 0.2f, Mathf.Max(1f, sliceLength + 0.1f)),
                    Quaternion.LookRotation(direction, Vector3.up),
                    true,
                    true);

                if (ground != null)
                {
                    groundRenderers.Add(ground);
                }

                if (segment.hasLeftWall || segment.hasRightWall)
                {
                    Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
                    float wallOffset = (segment.width * 0.5f) + 0.3f;
                    Vector3 wallScale = new Vector3(0.6f, Mathf.Max(2f, segment.wallHeight), Mathf.Max(1f, sliceLength + 0.2f));

                    if (segment.hasLeftWall)
                    {
                        CreateBox(
                            parent,
                            $"SCurveWallLeft_{i:00}",
                            mid - (right * wallOffset) + (Vector3.up * (segment.wallHeight * 0.5f + 0.1f)),
                            wallScale,
                            Quaternion.LookRotation(direction, Vector3.up),
                            false,
                            true);
                    }

                    if (segment.hasRightWall)
                    {
                        CreateBox(
                            parent,
                            $"SCurveWallRight_{i:00}",
                            mid + (right * wallOffset) + (Vector3.up * (segment.wallHeight * 0.5f + 0.1f)),
                            wallScale,
                            Quaternion.LookRotation(direction, Vector3.up),
                            false,
                            true);
                    }
                }
            }
        }

        private static void CreateStraightWalls(Transform parent, CourseSegmentData segment, float width, float length, float yOffset)
        {
            if (segment.hasLeftWall)
            {
                CreateWall(parent, true, length, segment.wallHeight, width, yOffset);
            }

            if (segment.hasRightWall)
            {
                CreateWall(parent, false, length, segment.wallHeight, width, yOffset);
            }
        }

        private static void CreateWall(Transform parent, bool left, float length, float wallHeight, float trackWidth, float yOffset)
        {
            float thickness = 0.6f;
            float x = (trackWidth * 0.5f + thickness * 0.5f) * (left ? -1f : 1f);
            CreateBox(
                parent,
                left ? "Wall_Left" : "Wall_Right",
                new Vector3(x, (wallHeight * 0.5f) + yOffset, length * 0.5f),
                new Vector3(thickness, Mathf.Max(2f, wallHeight), Mathf.Max(2f, length)),
                Quaternion.identity,
                false,
                true);
        }

        private static void CreateCeiling(Transform parent, float length, float width, float wallHeight, float yOffset)
        {
            CreateBox(
                parent,
                "Ceiling",
                new Vector3(0f, Mathf.Max(2.5f, wallHeight) + yOffset, length * 0.5f),
                new Vector3(Mathf.Max(2f, width), 0.4f, Mathf.Max(2f, length)),
                Quaternion.identity,
                false,
                true);
        }

        private static Renderer CreateBox(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            bool setGroundLayer,
            bool includeCollider)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = localRotation;
            cube.transform.localScale = localScale;

            if (!includeCollider)
            {
                Collider col = cube.GetComponent<Collider>();
                if (col != null)
                {
                    UnityEngine.Object.DestroyImmediate(col);
                }
            }

            if (setGroundLayer)
            {
                TrySetLayer(cube, GameConstants.LAYER_GROUND);
            }

            return cube.GetComponent<Renderer>();
        }

        private static (Vector3 position, float angle) AdvanceCursor(CourseSegmentData segment, Vector3 currentPosition, float currentAngle)
        {
            Vector3 forward = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.right;
            float elevation = segment.elevation;

            return segment.type switch
            {
                SegmentType.TurnLeft => AdvanceTurn(currentPosition, currentAngle, segment.length, segment.elevation, true),
                SegmentType.TurnRight => AdvanceTurn(currentPosition, currentAngle, segment.length, segment.elevation, false),
                _ => (currentPosition + (forward * segment.length) + (Vector3.up * elevation), currentAngle)
            };
        }

        private static (Vector3 position, float angle) AdvanceTurn(Vector3 currentPosition, float currentAngle, float arcLength, float elevation, bool left)
        {
            float sign = left ? -1f : 1f;
            float radius = Mathf.Max(5f, arcLength / (Mathf.PI * 0.5f));

            Vector3 right = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.right;
            Vector3 center = currentPosition + (right * sign * radius);
            Vector3 startRadial = -right * sign * radius;
            Vector3 endRadial = Quaternion.AngleAxis(sign * 90f, Vector3.up) * startRadial;

            Vector3 endPosition = center + endRadial + (Vector3.up * elevation);
            float endAngle = currentAngle + (sign * 90f);
            return (endPosition, endAngle);
        }

        private static Transform CreateSpawnPoint(Vector3 position, float angle, Transform parent)
        {
            GameObject spawn = new GameObject("SpawnPoint");
            spawn.transform.SetParent(parent);
            spawn.transform.position = position + Vector3.up * 0.5f;
            spawn.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            return spawn.transform;
        }

        private static void PlaceStartLine(Vector3 position, float angle, float width, Transform parent)
        {
            GameObject startLine = new GameObject("StartLine");
            startLine.transform.SetParent(parent);
            startLine.transform.position = position + Vector3.up * 0.02f;
            startLine.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            BoxCollider trigger = startLine.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(Mathf.Max(4f, width), 2f, 1.5f);

            CreateBox(startLine.transform, "StartLineMesh", Vector3.zero, new Vector3(Mathf.Max(4f, width), 0.05f, 1.2f), Quaternion.identity, false, false);
        }

        private static FinishLine PlaceFinishLine(Vector3 position, float angle, float width, Transform parent)
        {
            GameObject finishObject = new GameObject("FinishLine");
            finishObject.transform.SetParent(parent);
            finishObject.transform.position = position + Vector3.up * 0.5f;
            finishObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            BoxCollider trigger = finishObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(Mathf.Max(4f, width), 2.5f, 2f);

            FinishLine finishLine = finishObject.AddComponent<FinishLine>();
            TrySetTag(finishObject, GameConstants.TAG_FINISH);

            CreateBox(
                finishObject.transform,
                "FinishBanner",
                new Vector3(0f, 2.2f, 0f),
                new Vector3(Mathf.Max(3f, width * 0.9f), 0.5f, 0.3f),
                Quaternion.identity,
                false,
                false);

            return finishLine;
        }

        private static void PlaceCheckpoints(CourseDefinitionData definition, IReadOnlyList<GameObject> segments, Transform parent)
        {
            for (int i = 0; i < definition.segments.Length && i < segments.Count; i++)
            {
                CourseSegmentData segment = definition.segments[i];
                if (!segment.hasCheckpoint)
                {
                    continue;
                }

                Transform segmentTransform = segments[i].transform;
                float width = GetEffectiveSegmentWidth(segment);
                float zOffset = Mathf.Max(2f, segment.length * 0.85f);

                GameObject checkpointObject = new GameObject($"Checkpoint_{segment.checkpointIndex + 1}");
                checkpointObject.transform.SetParent(parent);
                checkpointObject.transform.position = segmentTransform.TransformPoint(new Vector3(0f, 1f, zOffset));
                checkpointObject.transform.rotation = segmentTransform.rotation;
                TrySetTag(checkpointObject, GameConstants.TAG_CHECKPOINT);

                BoxCollider trigger = checkpointObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(Mathf.Max(3f, width * 0.75f), 2.5f, 1.4f);

                Checkpoint checkpoint = checkpointObject.AddComponent<Checkpoint>();

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "Marker";
                marker.transform.SetParent(checkpointObject.transform, false);
                marker.transform.localPosition = new Vector3(0f, -0.85f, 0f);
                marker.transform.localScale = new Vector3(Mathf.Max(1.25f, width * 0.2f), 0.05f, Mathf.Max(1.25f, width * 0.2f));
                Collider markerCollider = marker.GetComponent<Collider>();
                if (markerCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(markerCollider);
                }

                GameObject label = new GameObject("CheckpointLabel");
                label.transform.SetParent(checkpointObject.transform, false);
                label.transform.localPosition = new Vector3(0f, 1.4f, 0f);
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.alignment = TextAlignment.Center;
                textMesh.characterSize = 0.2f;
                textMesh.fontSize = 48;

                SerializedObject serialized = new SerializedObject(checkpoint);
                SerializedProperty indexProperty = serialized.FindProperty("checkpointIndex");
                if (indexProperty != null)
                {
                    indexProperty.intValue = segment.checkpointIndex;
                }

                SerializedProperty rendererProperty = serialized.FindProperty("indicatorRenderer");
                if (rendererProperty != null)
                {
                    rendererProperty.objectReferenceValue = marker.GetComponent<Renderer>();
                }

                SerializedProperty labelProperty = serialized.FindProperty("checkpointNumberLabel");
                if (labelProperty != null)
                {
                    labelProperty.objectReferenceValue = textMesh;
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void PlaceCollectibles(CourseDefinitionData definition, IReadOnlyList<GameObject> segments, Transform parent)
        {
            for (int i = 0; i < definition.segments.Length && i < segments.Count; i++)
            {
                CourseSegmentData segment = definition.segments[i];
                if (segment.collectibles == null || segment.collectibles.Length == 0)
                {
                    continue;
                }

                for (int j = 0; j < segment.collectibles.Length; j++)
                {
                    CollectibleEntry entry = segment.collectibles[j];
                    Vector3 worldPosition = segments[i].transform.TransformPoint(entry.localOffset);

                    GameObject collectibleObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    collectibleObject.name = $"Collectible_{i + 1:00}_{j + 1:00}";
                    collectibleObject.transform.SetParent(parent);
                    collectibleObject.transform.position = worldPosition;
                    collectibleObject.transform.localScale = Vector3.one * 1f;

                    Collider meshCollider = collectibleObject.GetComponent<Collider>();
                    if (meshCollider != null)
                    {
                        UnityEngine.Object.DestroyImmediate(meshCollider);
                    }

                    SphereCollider trigger = collectibleObject.AddComponent<SphereCollider>();
                    trigger.isTrigger = true;
                    trigger.radius = 0.6f;

                    Collectible collectible = collectibleObject.AddComponent<Collectible>();
                    SerializedObject serialized = new SerializedObject(collectible);
                    SerializedProperty typeProperty = serialized.FindProperty("type");
                    if (typeProperty != null)
                    {
                        typeProperty.enumValueIndex = (int)entry.type;
                    }

                    serialized.ApplyModifiedPropertiesWithoutUndo();

                    TrySetTag(collectibleObject, GameConstants.TAG_COLLECTIBLE);
                    TrySetLayer(collectibleObject, GameConstants.LAYER_COLLECTIBLE);
                }
            }
        }

        private static CourseManager CreateCourseManager(CourseDefinitionData definition, Transform parent, FinishLine finishLine, Transform spawnPoint)
        {
            GameObject courseManagerObject = new GameObject("CourseManager");
            courseManagerObject.transform.SetParent(parent);

            CourseTimer timer = courseManagerObject.AddComponent<CourseTimer>();
            CourseManager manager = courseManagerObject.AddComponent<CourseManager>();

            CourseDataSO courseData = GetOrCreateCourseDataAsset(definition);

            SerializedObject serialized = new SerializedObject(manager);
            SetObjectReference(serialized, "courseData", courseData);
            SetObjectReference(serialized, "courseTimer", timer);
            SetObjectReference(serialized, "finishLine", finishLine);
            SetObjectReference(serialized, "defaultSpawnPoint", spawnPoint);
            SetFloat(serialized, "countdownSeconds", 3f);
            SetFloat(serialized, "respawnDelaySeconds", 1.5f);
            SetFloat(serialized, "introDurationSeconds", 0f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return manager;
        }

        private static HovercraftController PlaceHovercraft(Vector3 position, Quaternion rotation)
        {
            GameObject hovercraftObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hovercraftObject.name = "Hovercraft";
            hovercraftObject.transform.position = position;
            hovercraftObject.transform.rotation = rotation;
            hovercraftObject.transform.localScale = new Vector3(1.3f, 0.6f, 2f);

            TrySetTag(hovercraftObject, GameConstants.TAG_PLAYER);
            TrySetLayer(hovercraftObject, GameConstants.LAYER_HOVERCRAFT);

            Rigidbody rigidbody = hovercraftObject.AddComponent<Rigidbody>();
            rigidbody.mass = 2f;
            rigidbody.drag = 0.5f;
            rigidbody.angularDrag = 1f;

            HovercraftPhysics physics = hovercraftObject.AddComponent<HovercraftPhysics>();
            HovercraftInput input = hovercraftObject.AddComponent<HovercraftInput>();
            HovercraftHealth health = hovercraftObject.AddComponent<HovercraftHealth>();
            HovercraftController controller = hovercraftObject.AddComponent<HovercraftController>();
            hovercraftObject.AddComponent<HovercraftVisuals>();

            HovercraftStatsSO stats = GetOrCreateDefaultHovercraftStats();
            AssignHovercraftStats(physics, health, controller, stats);

            _ = input;
            return controller;
        }

        private static void ConfigureMainCamera(Transform hovercraftTransform)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }

            camera.transform.SetParent(hovercraftTransform);
            camera.transform.localPosition = new Vector3(0f, 5.5f, -10f);
            camera.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
        }

        private static void AddHud()
        {
            GameObject canvasObject = new GameObject("HUDCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            HUD hud = canvasObject.AddComponent<HUD>();

            GameObject timerObject = new GameObject("TimerText");
            timerObject.transform.SetParent(canvasObject.transform, false);
            RectTransform timerRect = timerObject.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0f, -24f);
            timerRect.sizeDelta = new Vector2(260f, 60f);

            TextMeshProUGUI timerText = timerObject.AddComponent<TextMeshProUGUI>();
            timerText.fontSize = 36f;
            timerText.text = "00:00.00";
            timerText.alignment = TextAlignmentOptions.Center;

            SerializedObject serialized = new SerializedObject(hud);
            SetObjectReference(serialized, "timerText", timerText);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static HovercraftStatsSO GetOrCreateDefaultHovercraftStats()
        {
            string path = $"{GeneratedStatsFolder}/HovercraftStats_Generated.asset";
            HovercraftStatsSO stats = AssetDatabase.LoadAssetAtPath<HovercraftStatsSO>(path);
            if (stats == null)
            {
                stats = ScriptableObject.CreateInstance<HovercraftStatsSO>();
                stats.baseSpeed = 20f;
                stats.maxSpeed = 40f;
                stats.boostMultiplier = 1.4f;
                stats.boostDuration = 2.5f;
                stats.boostCooldown = 4f;
                stats.brakeForce = 15f;
                stats.turnSpeed = 2.6f;
                stats.hoverHeight = 2f;
                stats.hoverForce = 65f;
                stats.hoverDamping = 5f;
                stats.maxHealth = 100f;
                stats.maxShield = 45f;
                stats.shieldRegenRate = 5f;
                stats.shieldRegenDelay = 3f;
                stats.driftFactor = 0.95f;
                stats.tiltSensitivity = 1f;
                stats.stabilizationForce = 10f;
                AssetDatabase.CreateAsset(stats, path);
            }

            return stats;
        }

        private static void AssignHovercraftStats(HovercraftPhysics physics, HovercraftHealth health, HovercraftController controller, HovercraftStatsSO stats)
        {
            SerializedObject physicsSerialized = new SerializedObject(physics);
            SetObjectReference(physicsSerialized, "stats", stats);
            physicsSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject healthSerialized = new SerializedObject(health);
            SetObjectReference(healthSerialized, "stats", stats);
            healthSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject controllerSerialized = new SerializedObject(controller);
            SetObjectReference(controllerSerialized, "stats", stats);
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CourseDataSO GetOrCreateCourseDataAsset(CourseDefinitionData definition)
        {
            string safeName = string.IsNullOrWhiteSpace(definition.sceneName) ? "Course" : definition.sceneName;
            string path = $"{GeneratedDataFolder}/{safeName}.asset";

            CourseDataSO data = AssetDatabase.LoadAssetAtPath<CourseDataSO>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<CourseDataSO>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.courseId = safeName.ToLowerInvariant();
            data.courseName = definition.courseName;
            data.sceneName = safeName;
            data.environmentType = definition.environmentType;
            data.courseIndex = GetCourseIndex(safeName);
            data.difficulty = GetDifficulty(safeName);
            data.hazardDescriptions = GetHazardDescriptions(definition);

            (float gold, float silver, float bronze) = GetMedalTimes(safeName);
            data.goldTime = gold;
            data.silverTime = silver;
            data.bronzeTime = bronze;

            EditorUtility.SetDirty(data);
            return data;
        }

        private static int GetCourseIndex(string sceneName)
        {
            if (sceneName.EndsWith("_01", StringComparison.Ordinal)) return 0;
            if (sceneName.EndsWith("_02", StringComparison.Ordinal)) return 1;
            if (sceneName.EndsWith("_03", StringComparison.Ordinal)) return 2;
            return 0;
        }

        private static DifficultyLevel GetDifficulty(string sceneName)
        {
            return sceneName switch
            {
                "Lava_Course_01" => DifficultyLevel.Easy,
                "Lava_Course_02" => DifficultyLevel.Medium,
                "Lava_Course_03" => DifficultyLevel.Hard,
                "Ice_Course_01" => DifficultyLevel.Medium,
                "Ice_Course_02" => DifficultyLevel.Hard,
                "Ice_Course_03" => DifficultyLevel.Extreme,
                "Toxic_Course_01" => DifficultyLevel.Medium,
                "Toxic_Course_02" => DifficultyLevel.Hard,
                "Toxic_Course_03" => DifficultyLevel.Extreme,
                _ => DifficultyLevel.Easy
            };
        }

        private static (float gold, float silver, float bronze) GetMedalTimes(string sceneName)
        {
            return sceneName switch
            {
                "Lava_Course_01" => (50f, 65f, 80f),
                "Lava_Course_02" => (70f, 90f, 110f),
                "Lava_Course_03" => (90f, 120f, 150f),
                "Ice_Course_01" => (60f, 80f, 100f),
                "Ice_Course_02" => (80f, 105f, 130f),
                "Ice_Course_03" => (100f, 130f, 160f),
                "Toxic_Course_01" => (65f, 85f, 105f),
                "Toxic_Course_02" => (85f, 110f, 140f),
                "Toxic_Course_03" => (110f, 145f, 180f),
                _ => (55f, 75f, 95f)
            };
        }

        private static string[] GetHazardDescriptions(CourseDefinitionData definition)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < definition.segments.Length; i++)
            {
                HazardEntry[] hazards = definition.segments[i].hazards;
                if (hazards == null)
                {
                    continue;
                }

                for (int j = 0; j < hazards.Length; j++)
                {
                    string type = hazards[j]?.hazardType;
                    if (!string.IsNullOrWhiteSpace(type))
                    {
                        set.Add(type);
                    }
                }
            }

            var results = new List<string>(set.Count);
            foreach (string item in set)
            {
                results.Add(item);
            }

            return results.ToArray();
        }

        private static float GetEffectiveSegmentWidth(CourseSegmentData segment)
        {
            return segment.type switch
            {
                SegmentType.NarrowCorridor => Mathf.Max(4f, segment.width),
                SegmentType.Bridge => Mathf.Max(2f, segment.width),
                SegmentType.WideOpen => segment.width * 1.5f,
                _ => segment.width
            };
        }

        private static void TrySetTag(GameObject gameObject, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    gameObject.tag = tag;
                    return;
                }
            }
        }

        private static void TrySetLayer(GameObject gameObject, string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                return;
            }

            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                gameObject.layer = layer;
            }
        }

        private static void EnsureDirectory(string fullPath)
        {
            string[] parts = fullPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            if (serializedObject == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            if (serializedObject == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
