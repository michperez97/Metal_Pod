#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Localization
{
    /// <summary>
    /// Shared editor-side localization helpers.
    /// </summary>
    public static class LocalizationEditor
    {
        public const string LocalizationFolderPath = "Assets/Resources/Localization";

        public static string[] GetAvailableLanguageCodes()
        {
            if (!Directory.Exists(LocalizationFolderPath))
            {
                return Array.Empty<string>();
            }

            string[] files = Directory.GetFiles(LocalizationFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            var codes = new List<string>(files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(files[i]);
                if (string.Equals(fileName, "language_manifest", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                codes.Add(fileName);
            }

            codes.Sort(StringComparer.OrdinalIgnoreCase);
            return codes.ToArray();
        }

        public static Dictionary<string, string> LoadLanguageFromDisk(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }

            string filePath = Path.Combine(LocalizationFolderPath, languageCode + ".json");
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return LocalizationManager.ParseLanguageJson(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Localization] Failed to read '{filePath}': {exception.Message}");
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }
        }

        public static List<string> FindMissingKeys(Dictionary<string, string> referenceStrings, Dictionary<string, string> targetStrings)
        {
            var missing = new List<string>();
            if (referenceStrings == null || targetStrings == null)
            {
                return missing;
            }

            foreach (KeyValuePair<string, string> pair in referenceStrings)
            {
                if (!targetStrings.ContainsKey(pair.Key))
                {
                    missing.Add(pair.Key);
                }
            }

            missing.Sort(StringComparer.Ordinal);
            return missing;
        }

        [MenuItem("Metal Pod/Localization/Validate Language Files")]
        private static void ValidateLanguageFiles()
        {
            Dictionary<string, string> english = LoadLanguageFromDisk("en");
            string[] languages = GetAvailableLanguageCodes();

            if (english.Count == 0)
            {
                Debug.LogWarning("[Localization] Could not load Assets/Resources/Localization/en.json");
                return;
            }

            for (int i = 0; i < languages.Length; i++)
            {
                string code = languages[i];
                Dictionary<string, string> lang = LoadLanguageFromDisk(code);
                List<string> missing = FindMissingKeys(english, lang);
                if (missing.Count > 0)
                {
                    Debug.LogWarning($"[Localization] {code}.json is missing {missing.Count} keys.");
                }
                else
                {
                    Debug.Log($"[Localization] {code}.json is complete ({lang.Count} keys).");
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif
