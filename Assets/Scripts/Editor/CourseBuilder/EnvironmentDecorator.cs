using System.Collections.Generic;
using MetalPod.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MetalPod.EditorTools.CourseBuilder
{
    public static class EnvironmentDecorator
    {
        private const string MaterialFolder = "Assets/Materials/Generated/CourseBuilder";

        public static void ApplyEnvironment(EnvironmentType type, bool useGreyboxStyle, IReadOnlyList<Renderer> groundRenderers)
        {
            if (useGreyboxStyle)
            {
                ApplyGreyboxEnvironment();
                ApplyGroundMaterial(GetOrCreateGroundMaterial(type, true), groundRenderers);
                return;
            }

            switch (type)
            {
                case EnvironmentType.Lava:
                    ApplyLavaEnvironment();
                    break;
                case EnvironmentType.Ice:
                    ApplyIceEnvironment();
                    break;
                case EnvironmentType.Toxic:
                    ApplyToxicEnvironment();
                    break;
                default:
                    ApplyGreyboxEnvironment();
                    break;
            }

            ApplyGroundMaterial(GetOrCreateGroundMaterial(type, false), groundRenderers);
        }

        public static Color GetGroundColor(EnvironmentType type, bool greybox)
        {
            if (greybox)
            {
                return new Color(0.5f, 0.5f, 0.5f);
            }

            return type switch
            {
                EnvironmentType.Lava => new Color(0.1f, 0.08f, 0.06f),
                EnvironmentType.Ice => new Color(0.7f, 0.8f, 0.9f),
                EnvironmentType.Toxic => new Color(0.3f, 0.25f, 0.2f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }

        private static void ApplyLavaEnvironment()
        {
            Light light = Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.color = new Color(1f, 0.6f, 0.3f);
                light.intensity = 1.2f;
                light.transform.rotation = Quaternion.Euler(35f, -45f, 0f);
            }

            RenderSettings.ambientLight = new Color(0.3f, 0.1f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.2f, 0.08f, 0.02f);
            RenderSettings.fogDensity = 0.01f;
        }

        private static void ApplyIceEnvironment()
        {
            Light light = Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.color = new Color(0.7f, 0.8f, 1f);
                light.intensity = 0.85f;
                light.transform.rotation = Quaternion.Euler(45f, -20f, 0f);
            }

            RenderSettings.ambientLight = new Color(0.15f, 0.2f, 0.3f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);
            RenderSettings.fogDensity = 0.015f;
        }

        private static void ApplyToxicEnvironment()
        {
            Light light = Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.color = new Color(0.8f, 0.9f, 0.7f);
                light.intensity = 0.6f;
                light.transform.rotation = Quaternion.Euler(55f, -10f, 0f);
            }

            RenderSettings.ambientLight = new Color(0.1f, 0.15f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.15f, 0.2f, 0.05f);
            RenderSettings.fogDensity = 0.02f;
        }

        private static void ApplyGreyboxEnvironment()
        {
            Light light = Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.color = Color.white;
                light.intensity = 1f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.35f);
            RenderSettings.fog = false;
            RenderSettings.fogDensity = 0f;
        }

        private static void ApplyGroundMaterial(Material material, IReadOnlyList<Renderer> renderers)
        {
            if (material == null || renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sharedMaterial = material;
                }
            }
        }

        private static Material GetOrCreateGroundMaterial(EnvironmentType type, bool greybox)
        {
            EnsureDirectory(MaterialFolder);

            string prefix = greybox ? "Greybox" : type.ToString();
            string path = $"{MaterialFolder}/{prefix}_Ground.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Standard");
                material = new Material(shader)
                {
                    name = $"{prefix}_Ground"
                };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = GetGroundColor(type, greybox);
            material.SetFloat("_Glossiness", greybox ? 0.05f : 0.2f);
            EditorUtility.SetDirty(material);
            return material;
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
    }
}
