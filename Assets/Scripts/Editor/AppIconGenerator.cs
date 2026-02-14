#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Editor
{
    /// <summary>
    /// Generates required iOS App Icon variants from a single 1024x1024 source PNG.
    /// Menu path: Metal Pod/Generate App Icons
    /// </summary>
    public static class AppIconGenerator
    {
        private struct IconSpec
        {
            public readonly int Size;
            public readonly string Suffix;

            public IconSpec(int size, string suffix)
            {
                Size = size;
                Suffix = suffix;
            }
        }

        // 18 slot variants (17 unique pixel sizes) including App Store icon.
        private static readonly IconSpec[] IconSizes =
        {
            new IconSpec(40, "iphone-notification@2x"),
            new IconSpec(60, "iphone-notification@3x"),
            new IconSpec(58, "iphone-settings@2x"),
            new IconSpec(87, "iphone-settings@3x"),
            new IconSpec(80, "iphone-spotlight@2x"),
            new IconSpec(120, "iphone-spotlight@3x"),
            new IconSpec(120, "iphone-app@2x"),
            new IconSpec(180, "iphone-app@3x"),

            new IconSpec(20, "ipad-notification@1x"),
            new IconSpec(40, "ipad-notification@2x"),
            new IconSpec(29, "ipad-settings@1x"),
            new IconSpec(58, "ipad-settings@2x"),
            new IconSpec(40, "ipad-spotlight@1x"),
            new IconSpec(80, "ipad-spotlight@2x"),
            new IconSpec(76, "ipad-app@1x"),
            new IconSpec(152, "ipad-app@2x"),
            new IconSpec(167, "ipad-pro-app@2x"),

            new IconSpec(1024, "appstore")
        };

        [MenuItem("Metal Pod/Generate App Icons")]
        public static void GenerateIcons()
        {
            string sourcePath = EditorUtility.OpenFilePanel("Select 1024x1024 App Icon Source", "Assets", "png");
            if (string.IsNullOrEmpty(sourcePath))
            {
                return;
            }

            byte[] sourceBytes = File.ReadAllBytes(sourcePath);
            Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(sourceBytes);

            if (source.width != 1024 || source.height != 1024)
            {
                EditorUtility.DisplayDialog("Invalid Source", $"Source image must be 1024x1024. Got {source.width}x{source.height}.", "OK");
                Object.DestroyImmediate(source);
                return;
            }

            string outputDir = "Assets/AppIcons";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            int uniqueSizeCount = GetUniqueSizeCount();

            for (int i = 0; i < IconSizes.Length; i++)
            {
                IconSpec spec = IconSizes[i];
                EditorUtility.DisplayProgressBar(
                    "Generating App Icons",
                    $"Creating icon-{spec.Suffix}.png ({spec.Size}x{spec.Size})",
                    (float)i / IconSizes.Length);

                Texture2D resized = ResizeTexture(source, spec.Size, spec.Size);
                string outPath = Path.Combine(outputDir, $"icon-{spec.Suffix}.png");
                File.WriteAllBytes(outPath, resized.EncodeToPNG());
                Object.DestroyImmediate(resized);
            }

            Object.DestroyImmediate(source);
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "App Icons Generated",
                $"Created {IconSizes.Length} icon variants ({uniqueSizeCount} unique sizes) in {outputDir}/",
                "OK");
            Debug.Log($"[AppIconGenerator] Generated {IconSizes.Length} variants ({uniqueSizeCount} unique sizes) in {outputDir}");
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        private static int GetUniqueSizeCount()
        {
            System.Collections.Generic.HashSet<int> sizes = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < IconSizes.Length; i++)
            {
                sizes.Add(IconSizes[i].Size);
            }

            return sizes.Count;
        }
    }
}
#endif
