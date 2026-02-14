using System.Collections.Generic;
using MetalPod.Hazards;
using MetalPod.Shared;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MetalPod.EditorTools.CourseBuilder
{
    public static class HazardPlacer
    {
        public static void PlaceAllHazards(CourseDefinitionData def, IReadOnlyList<GameObject> segments, Transform hazardsRoot)
        {
            if (def == null || def.segments == null || segments == null || hazardsRoot == null)
            {
                return;
            }

            int segmentCount = Mathf.Min(def.segments.Length, segments.Count);
            for (int i = 0; i < segmentCount; i++)
            {
                CourseSegmentData segment = def.segments[i];
                if (segment == null || segment.hazards == null || segment.hazards.Length == 0)
                {
                    continue;
                }

                for (int j = 0; j < segment.hazards.Length; j++)
                {
                    HazardEntry entry = segment.hazards[j];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.hazardType))
                    {
                        continue;
                    }

                    PlaceHazardEntry(entry, segments[i].transform, segment, hazardsRoot, i, j);
                }
            }
        }

        private static void PlaceHazardEntry(
            HazardEntry entry,
            Transform segmentRoot,
            CourseSegmentData segment,
            Transform hazardsRoot,
            int segmentIndex,
            int hazardIndex)
        {
            if (entry.placement == HazardPlacement.BothSides)
            {
                HazardEntry left = CloneWithPlacement(entry, HazardPlacement.Left);
                HazardEntry right = CloneWithPlacement(entry, HazardPlacement.Right);
                PlaceHazardEntry(left, segmentRoot, segment, hazardsRoot, segmentIndex, hazardIndex);
                PlaceHazardEntry(right, segmentRoot, segment, hazardsRoot, segmentIndex, hazardIndex + 1000);
                return;
            }

            Vector3 localPosition = CalculateHazardPosition(entry.placement, segment, segmentIndex, hazardIndex) + entry.localOffset;
            Vector3 worldPosition = segmentRoot.TransformPoint(localPosition);

            GameObject hazardObj = new GameObject($"Hazard_{entry.hazardType}_{segmentIndex + 1}_{hazardIndex + 1}");
            hazardObj.transform.SetParent(hazardsRoot);
            hazardObj.transform.position = worldPosition;
            hazardObj.transform.rotation = segmentRoot.rotation;
            hazardObj.transform.localScale = Vector3.one * Mathf.Max(0.1f, entry.scale);

            TrySetTag(hazardObj, GameConstants.TAG_HAZARD);
            TrySetLayer(hazardObj, GameConstants.LAYER_HAZARD);

            ConfigureHazardObject(hazardObj, entry, segment);
        }

        private static void ConfigureHazardObject(GameObject obj, HazardEntry entry, CourseSegmentData segment)
        {
            switch (entry.hazardType)
            {
                case "IndustrialPress":
                    SetupIndustrialPress(obj, segment);
                    return;
                case "Avalanche":
                    SetupAvalanche(obj, segment);
                    return;
                case "IceWall":
                    SetupIceWall(obj, segment, entry.scale);
                    return;
                case "BarrelExplosion":
                    SetupBarrelExplosion(obj, entry.scale);
                    return;
                case "FallingDebris":
                    SetupFallingDebris(obj, entry.scale);
                    return;
            }

            BoxCollider trigger = obj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(3f, 2f, 3f) * Mathf.Max(0.2f, entry.scale);

            Component hazardComponent = AddHazardComponent(obj, entry.hazardType);
            CreateMarker(obj.transform, entry.hazardType, trigger.size, trigger.isTrigger);
            ApplyCustomParam(hazardComponent, entry.customParam1);
        }

        private static void SetupFallingDebris(GameObject obj, float scale)
        {
            BoxCollider collider = obj.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            collider.size = new Vector3(2f, 2f, 2f) * Mathf.Max(0.3f, scale);

            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.mass = 1.5f;
            rb.useGravity = true;

            obj.AddComponent<FallingDebris>();
            obj.transform.position += Vector3.up * 8f;

            CreateMarker(obj.transform, "FallingDebris", collider.size, false);
        }

        private static void SetupIceWall(GameObject obj, CourseSegmentData segment, float scale)
        {
            BoxCollider collider = obj.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            collider.size = new Vector3(Mathf.Max(3f, segment.width * 0.55f), 4.5f, 1.2f) * Mathf.Max(0.4f, scale);

            obj.AddComponent<IceWall>();
            CreateMarker(obj.transform, "IceWall", collider.size, false);
        }

        private static void SetupBarrelExplosion(GameObject obj, float scale)
        {
            CapsuleCollider collider = obj.AddComponent<CapsuleCollider>();
            collider.isTrigger = false;
            collider.radius = Mathf.Max(0.4f, scale * 0.6f);
            collider.height = Mathf.Max(1f, scale * 1.6f);

            obj.AddComponent<BarrelExplosion>();
            CreateMarker(obj.transform, "BarrelExplosion", new Vector3(collider.radius * 2f, collider.height, collider.radius * 2f), false);
        }

        private static void SetupIndustrialPress(GameObject obj, CourseSegmentData segment)
        {
            IndustrialPress press = obj.AddComponent<IndustrialPress>();

            BoxCollider blocker = obj.AddComponent<BoxCollider>();
            blocker.isTrigger = false;
            blocker.size = new Vector3(Mathf.Max(3f, segment.width * 0.75f), 0.5f, 2f);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "PressHead";
            head.transform.SetParent(obj.transform, false);
            head.transform.localScale = new Vector3(Mathf.Max(3f, segment.width * 0.75f), 1.2f, 2.5f);
            head.transform.localPosition = new Vector3(0f, 3.25f, 0f);
            Object.DestroyImmediate(head.GetComponent<Collider>());

            GameObject crushZoneObject = new GameObject("CrushZone");
            crushZoneObject.transform.SetParent(obj.transform, false);
            crushZoneObject.transform.localPosition = new Vector3(0f, 1f, 0f);
            BoxCollider crushZone = crushZoneObject.AddComponent<BoxCollider>();
            crushZone.isTrigger = true;
            crushZone.size = new Vector3(Mathf.Max(3f, segment.width * 0.75f), 1.8f, 2.8f);

            SerializedObject serialized = new SerializedObject(press);
            serialized.FindProperty("pressHead")?.SetObjectReferenceValue(head.transform);
            serialized.FindProperty("openPosition")?.vector3Value = head.transform.localPosition;
            serialized.FindProperty("closedPosition")?.vector3Value = new Vector3(0f, 0.65f, 0f);
            serialized.FindProperty("crushZone")?.SetObjectReferenceValue(crushZone);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            CreateMarker(obj.transform, "IndustrialPress", blocker.size, false);
        }

        private static void SetupAvalanche(GameObject obj, CourseSegmentData segment)
        {
            Avalanche avalanche = obj.AddComponent<Avalanche>();

            BoxCollider startTrigger = obj.AddComponent<BoxCollider>();
            startTrigger.isTrigger = true;
            startTrigger.size = new Vector3(Mathf.Max(6f, segment.width), 4f, 10f);

            GameObject killZoneObject = new GameObject("KillZone");
            killZoneObject.transform.SetParent(obj.transform, false);
            killZoneObject.transform.localPosition = new Vector3(0f, 1.5f, 1f);
            BoxCollider killZone = killZoneObject.AddComponent<BoxCollider>();
            killZone.isTrigger = true;
            killZone.size = new Vector3(Mathf.Max(6f, segment.width), 3f, 7f);

            SerializedObject serialized = new SerializedObject(avalanche);
            serialized.FindProperty("killZone")?.SetObjectReferenceValue(killZone);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            CreateMarker(obj.transform, "Avalanche", startTrigger.size, true);
        }

        private static Component AddHazardComponent(GameObject obj, string type)
        {
            return type switch
            {
                "LavaFlow" => obj.AddComponent<LavaFlow>(),
                "VolcanicEruption" => obj.AddComponent<VolcanicEruption>(),
                "LavaGeyser" => obj.AddComponent<LavaGeyser>(),
                "HeatZone" => obj.AddComponent<HeatZone>(),
                "FallingDebris" => obj.AddComponent<FallingDebris>(),
                "IcePatch" => obj.AddComponent<IcePatch>(),
                "FallingIcicle" => obj.AddComponent<FallingIcicle>(),
                "BlizzardZone" => obj.AddComponent<BlizzardZone>(),
                "IceWall" => obj.AddComponent<IceWall>(),
                "Avalanche" => obj.AddComponent<Avalanche>(),
                "ToxicGas" => obj.AddComponent<ToxicGas>(),
                "AcidPool" => obj.AddComponent<AcidPool>(),
                "IndustrialPress" => obj.AddComponent<IndustrialPress>(),
                "ElectricFence" => obj.AddComponent<ElectricFence>(),
                "BarrelExplosion" => obj.AddComponent<BarrelExplosion>(),
                _ => null
            };
        }

        private static void ApplyCustomParam(Component hazardComponent, float value)
        {
            if (hazardComponent == null || value <= 0f)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(hazardComponent);

            if (hazardComponent is LavaFlow)
            {
                serialized.FindProperty("useIntermittentFlow")?.SetBoolValue(true);
                serialized.FindProperty("activeDuration")?.SetFloatValue(value);
                serialized.FindProperty("inactiveDuration")?.SetFloatValue(value);
            }
            else if (hazardComponent is VolcanicEruption)
            {
                serialized.FindProperty("eruptionInterval")?.SetFloatValue(value);
            }
            else if (hazardComponent is ToxicGas)
            {
                serialized.FindProperty("isPeriodic")?.SetBoolValue(true);
                serialized.FindProperty("ventInterval")?.SetFloatValue(value);
            }
            else if (hazardComponent is BlizzardZone)
            {
                serialized.FindProperty("windForce")?.SetFloatValue(value);
            }
            else if (hazardComponent is AcidPool)
            {
                serialized.FindProperty("hasRisingLevel")?.SetBoolValue(true);
                serialized.FindProperty("riseSpeed")?.SetFloatValue(value);
            }
            else if (hazardComponent is ElectricFence)
            {
                serialized.FindProperty("onDuration")?.SetFloatValue(value);
                serialized.FindProperty("offDuration")?.SetFloatValue(Mathf.Max(0.5f, value + 0.75f));
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Vector3 CalculateHazardPosition(HazardPlacement placement, CourseSegmentData seg, int segmentIndex, int hazardIndex)
        {
            float width = Mathf.Max(2f, seg.width);
            float length = Mathf.Max(4f, seg.length);

            float random01 = Mathf.Abs(Mathf.Sin((segmentIndex + 1f) * 12.9898f + (hazardIndex + 1f) * 78.233f));
            random01 -= Mathf.Floor(random01);

            float x = 0f;
            float y = 0.6f;
            float z = length * 0.5f;

            switch (placement)
            {
                case HazardPlacement.Left:
                    x = -width * 0.38f;
                    break;
                case HazardPlacement.Right:
                    x = width * 0.38f;
                    break;
                case HazardPlacement.Random:
                    x = Mathf.Lerp(-width * 0.4f, width * 0.4f, random01);
                    z = Mathf.Lerp(length * 0.2f, length * 0.8f, 1f - random01);
                    break;
                case HazardPlacement.Overhead:
                    y = 8f;
                    break;
                case HazardPlacement.Behind:
                    z = 2.5f;
                    break;
            }

            return new Vector3(x, y, z);
        }

        private static HazardEntry CloneWithPlacement(HazardEntry entry, HazardPlacement placement)
        {
            return new HazardEntry
            {
                hazardType = entry.hazardType,
                placement = placement,
                localOffset = entry.localOffset,
                scale = entry.scale,
                customParam1 = entry.customParam1
            };
        }

        private static void CreateMarker(Transform parent, string hazardType, Vector3 size, bool transparent)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Marker";
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = size;

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Object.DestroyImmediate(markerCollider);
            }

            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Material material = new Material(Shader.Find("Standard"));
            material.color = GetHazardColor(hazardType, transparent);
            material.SetFloat("_Glossiness", 0.1f);
            renderer.sharedMaterial = material;
        }

        private static Color GetHazardColor(string hazardType, bool transparent)
        {
            Color baseColor = hazardType switch
            {
                "LavaFlow" => new Color(1f, 0.35f, 0.15f),
                "VolcanicEruption" => new Color(1f, 0.25f, 0.1f),
                "LavaGeyser" => new Color(1f, 0.4f, 0.15f),
                "HeatZone" => new Color(1f, 0.55f, 0.2f),
                "IcePatch" => new Color(0.55f, 0.8f, 1f),
                "FallingIcicle" => new Color(0.7f, 0.9f, 1f),
                "BlizzardZone" => new Color(0.75f, 0.9f, 1f),
                "IceWall" => new Color(0.5f, 0.75f, 1f),
                "Avalanche" => new Color(0.85f, 0.9f, 1f),
                "ToxicGas" => new Color(0.35f, 0.9f, 0.3f),
                "AcidPool" => new Color(0.5f, 1f, 0.2f),
                "IndustrialPress" => new Color(0.7f, 0.7f, 0.7f),
                "ElectricFence" => new Color(0.3f, 0.9f, 1f),
                "BarrelExplosion" => new Color(1f, 0.55f, 0.2f),
                _ => new Color(1f, 0.2f, 0.2f)
            };

            if (!transparent)
            {
                return baseColor;
            }

            baseColor.a = 0.65f;
            return baseColor;
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

        private static void SetFloatValue(this SerializedProperty property, float value)
        {
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetBoolValue(this SerializedProperty property, bool value)
        {
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetObjectReferenceValue(this SerializedProperty property, Object value)
        {
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
