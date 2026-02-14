#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using MetalPod.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MetalPod.Editor
{
    /// <summary>
    /// Editor-only toolbox for frequent playtest and build flows.
    /// </summary>
    public class DebugEditorWindow : EditorWindow
    {
        private const string SaveFileName = "metalpod_save.json";
        private const string BackupSaveFileName = "metalpod_save_backup.json";

        private static readonly string[] CourseSceneNames =
        {
            "TestCourse",
            "InfernoGate",
            "MoltenRidge",
            "MagmaCanyon",
            "FrozenPass",
            "GlacialRavine",
            "ArcticStorm",
            "RustValley",
            "ChemicalPlant",
            "BiohazardCore"
        };

        private static readonly string[] CourseDisplayNames =
        {
            "Test Course",
            "Inferno Gate (Lava 1)",
            "Molten Ridge (Lava 2)",
            "Magma Canyon (Lava 3)",
            "Frozen Pass (Ice 1)",
            "Glacial Ravine (Ice 2)",
            "Arctic Storm (Ice 3)",
            "Rust Valley (Toxic 1)",
            "Chemical Plant (Toxic 2)",
            "Biohazard Core (Toxic 3)"
        };

        private int _selectedCourse;
        private int _currencyAmount = 5000;
        private int _upgradeLevel = 5;
        private Vector2 _scroll;

        [MenuItem("Metal Pod/Debug Tools")]
        public static void ShowWindow()
        {
            DebugEditorWindow window = GetWindow<DebugEditorWindow>("Metal Pod Debug");
            window.minSize = new Vector2(480f, 420f);
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("Metal Pod Debug Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space(8f);

            DrawQuickPlaySection();
            DrawSceneShortcutsSection();
            DrawSaveDataSection();
            DrawBuildSection();
            DrawProjectInfoSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawQuickPlaySection()
        {
            EditorGUILayout.LabelField("Quick Play", EditorStyles.boldLabel);
            _selectedCourse = EditorGUILayout.Popup("Course", _selectedCourse, CourseDisplayNames);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Course"))
            {
                PlaySelectedCourse();
            }

            if (GUILayout.Button("Open Scene"))
            {
                OpenSelectedCourse();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8f);
        }

        private void DrawSceneShortcutsSection()
        {
            EditorGUILayout.LabelField("Scene Shortcuts", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Main Menu"))
            {
                OpenSceneByName("MainMenu", OpenSceneMode.Single);
            }

            if (GUILayout.Button("Workshop"))
            {
                OpenSceneByName("Workshop", OpenSceneMode.Single);
            }

            if (GUILayout.Button("_Persistent"))
            {
                OpenSceneByName("_Persistent", OpenSceneMode.Single);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8f);
        }

        private void DrawSaveDataSection()
        {
            EditorGUILayout.LabelField("Save Data", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"Save Path: {GetSavePath()}", MessageType.None);

            _currencyAmount = EditorGUILayout.IntField("Currency Amount", _currencyAmount);
            _upgradeLevel = EditorGUILayout.IntSlider("Upgrade Level", _upgradeLevel, 0, 10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Save Location"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }

            if (GUILayout.Button("View Save JSON (Log)"))
            {
                string savePath = GetSavePath();
                if (File.Exists(savePath))
                {
                    Debug.Log(File.ReadAllText(savePath));
                }
                else
                {
                    Debug.Log("No save file found.");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Currency"))
            {
                ModifySaveData(data => data.currency = Mathf.Max(0, data.currency + _currencyAmount));
            }

            if (GUILayout.Button("Set Currency"))
            {
                ModifySaveData(data => data.currency = Mathf.Max(0, _currencyAmount));
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Set Core Upgrades"))
            {
                ModifySaveData(data =>
                {
                    int clamped = Mathf.Max(0, _upgradeLevel);
                    data.upgradeLevels.Set("speed", clamped);
                    data.upgradeLevels.Set("handling", clamped);
                    data.upgradeLevels.Set("shield", clamped);
                    data.upgradeLevels.Set("boost", clamped);
                });
            }

            if (GUILayout.Button("Delete Save Data"))
            {
                if (EditorUtility.DisplayDialog("Delete Save Data",
                        "This permanently deletes the current save and backup files.",
                        "Delete",
                        "Cancel"))
                {
                    DeleteSaveFiles();
                }
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawBuildSection()
        {
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build iOS (Development)"))
            {
                BuildIOS(BuildOptions.Development | BuildOptions.AllowDebugging);
            }

            if (GUILayout.Button("Build iOS (Release)"))
            {
                BuildIOS(BuildOptions.None);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8f);
        }

        private static void DrawProjectInfoSection()
        {
            EditorGUILayout.LabelField("Project Info", EditorStyles.boldLabel);

            int scriptCount = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" }).Length;
            int shaderCount = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/Shaders" }).Length;
            int soCount = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Length;
            int testCount = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Tests" }).Length;

            EditorGUILayout.LabelField($"Scripts: {scriptCount}");
            EditorGUILayout.LabelField($"Shaders: {shaderCount}");
            EditorGUILayout.LabelField($"ScriptableObjects: {soCount}");
            EditorGUILayout.LabelField($"Test Scripts: {testCount}");
            EditorGUILayout.LabelField($"Build Target: {EditorUserBuildSettings.activeBuildTarget}");
            EditorGUILayout.LabelField($"Color Space: {PlayerSettings.colorSpace}");
        }

        private void PlaySelectedCourse()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string courseName = GetSelectedCourseName();
            string persistentPath = FindBuildScenePath("_Persistent");
            string coursePath = FindBuildScenePath(courseName);

            if (string.IsNullOrWhiteSpace(coursePath))
            {
                EditorUtility.DisplayDialog("Scene Missing",
                    $"Scene '{courseName}' was not found in Build Settings.", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(persistentPath))
            {
                EditorSceneManager.OpenScene(persistentPath, OpenSceneMode.Single);
                EditorSceneManager.OpenScene(coursePath, OpenSceneMode.Additive);
            }
            else
            {
                EditorSceneManager.OpenScene(coursePath, OpenSceneMode.Single);
            }

            EditorApplication.isPlaying = true;
        }

        private void OpenSelectedCourse()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            OpenSceneByName(GetSelectedCourseName(), OpenSceneMode.Single);
        }

        private void OpenSceneByName(string sceneName, OpenSceneMode mode)
        {
            string scenePath = FindBuildScenePath(sceneName);
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                EditorUtility.DisplayDialog("Scene Missing",
                    $"Scene '{sceneName}' was not found in Build Settings.", "OK");
                return;
            }

            EditorSceneManager.OpenScene(scenePath, mode);
        }

        private string GetSelectedCourseName()
        {
            int safeIndex = Mathf.Clamp(_selectedCourse, 0, CourseSceneNames.Length - 1);
            return CourseSceneNames[safeIndex];
        }

        private static string FindBuildScenePath(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (scene == null || !scene.enabled || string.IsNullOrWhiteSpace(scene.path))
                {
                    continue;
                }

                string pathName = Path.GetFileNameWithoutExtension(scene.path);
                if (string.Equals(pathName, sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    return scene.path;
                }
            }

            return null;
        }

        private static void BuildIOS(BuildOptions options)
        {
            string[] scenes = GetEnabledBuildScenes();
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("No Build Scenes",
                    "No enabled scenes were found in Build Settings.", "OK");
                return;
            }

            string outputPath = "Builds/iOS";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.iOS, options);
            Debug.Log($"Build started: {outputPath} ({options})");
        }

        private static string[] GetEnabledBuildScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene != null && scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
                .Select(scene => scene.path)
                .ToArray();
        }

        private static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        private static string GetBackupSavePath()
        {
            return Path.Combine(Application.persistentDataPath, BackupSaveFileName);
        }

        private static void ModifySaveData(Action<SaveData> mutate)
        {
            if (mutate == null)
            {
                return;
            }

            SaveData data = LoadSaveData();
            mutate(data);
            EnsureSaveDataConsistency(data);

            data.version = 1;
            data.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string savePath = GetSavePath();
            string directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            AssetDatabase.Refresh();
            Debug.Log("Save data updated.");
        }

        private static SaveData LoadSaveData()
        {
            string path = GetSavePath();
            if (!File.Exists(path))
            {
                return SaveData.CreateDefault();
            }

            try
            {
                string json = File.ReadAllText(path);
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                return loaded ?? SaveData.CreateDefault();
            }
            catch
            {
                return SaveData.CreateDefault();
            }
        }

        private static void EnsureSaveDataConsistency(SaveData data)
        {
            if (data == null)
            {
                return;
            }

            if (data.upgradeLevels == null)
            {
                data.upgradeLevels = new SerializableIntDict();
            }

            if (data.bestTimes == null)
            {
                data.bestTimes = new SerializableFloatDict();
            }

            if (data.bestMedals == null)
            {
                data.bestMedals = new SerializableIntDict();
            }

            if (data.completedCourses == null)
            {
                data.completedCourses = new SerializableBoolDict();
            }

            if (data.unlockedCourses == null)
            {
                data.unlockedCourses = new SerializableBoolDict();
            }

            if (data.ownedCosmetics == null)
            {
                data.ownedCosmetics = new System.Collections.Generic.List<string>();
            }
        }

        private static void DeleteSaveFiles()
        {
            TryDelete(GetSavePath());
            TryDelete(GetBackupSavePath());
            AssetDatabase.Refresh();
            Debug.Log("Save data deleted.");
        }

        private static void TryDelete(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to delete '{path}': {ex.Message}");
            }
        }
    }
}
#endif
