#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using MetalPod.Core;
using MetalPod.Course;
using MetalPod.Hovercraft;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using MetalPod.UI;
using MetalPod.Workshop;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetalPod.Editor
{
    public static class MetalPodSetupWizard
    {
        private const string DataRoot = "Assets/ScriptableObjects/Data";
        private const string CoursesRoot = DataRoot + "/Courses";
        private const string UpgradesRoot = DataRoot + "/Upgrades";
        private const string EnvironmentsRoot = DataRoot + "/Environments";
        private const string CosmeticsRoot = DataRoot + "/Cosmetics";
        private const string HazardsRoot = DataRoot + "/Hazards";

        private const string HovercraftPrefabPath = "Assets/Prefabs/Hovercraft/Hovercraft.prefab";

        private const string PersistentScenePath = "Assets/Scenes/_Persistent.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string WorkshopScenePath = "Assets/Scenes/Workshop.unity";
        private const string TestCourseScenePath = "Assets/Scenes/TestCourse.unity";

        private sealed class CourseSeed
        {
            public string AssetName;
            public string CourseId;
            public string CourseName;
            public EnvironmentType EnvironmentType;
            public int CourseIndex;
            public string SceneName;
            public float Gold;
            public float Silver;
            public float Bronze;
            public int RequiredMedals;
            public string PrereqId;
            public DifficultyLevel Difficulty;
        }

        private sealed class CourseSceneSeed
        {
            public string ScenePath;
            public string SceneName;
            public EnvironmentType EnvironmentType;
            public int CheckpointCount;
            public int HazardMarkerCount;
            public float TrackLength;
        }

        private sealed class UpgradeSeed
        {
            public string AssetName;
            public string UpgradeId;
            public string UpgradeName;
            public UpgradeCategory Category;
            public UpgradeLevel[] Levels;
            public StatModifier[] Modifiers;
        }

        private sealed class EnvironmentSeed
        {
            public string AssetName;
            public string EnvironmentId;
            public string EnvironmentName;
            public EnvironmentType Type;
            public int RequiredMedals;
            public string PrimaryHex;
            public string SecondaryHex;
            public string[] CourseIds;
        }

        private sealed class CosmeticSeed
        {
            public string AssetName;
            public string CosmeticId;
            public string CosmeticName;
            public CosmeticType Type;
            public int Cost;
            public int RequiredMedals;
            public string Description;
            public string PrimaryHex;
            public string SecondaryHex;
            public string AccentHex;
        }

        private sealed class HazardSeed
        {
            public string AssetName;
            public string HazardId;
            public string HazardName;
            public float DamagePerSecond;
            public float DamagePerHit;
            public float WarningRadius;
            public int ThreatLevel;
        }

        private static readonly CourseSeed[] CourseSeeds =
        {
            new CourseSeed
            {
                AssetName = "Lava_Course_01.asset",
                CourseId = "lava_01",
                CourseName = "Inferno Gate",
                EnvironmentType = EnvironmentType.Lava,
                CourseIndex = 0,
                SceneName = "Lava_Course_01",
                Gold = 50f,
                Silver = 65f,
                Bronze = 80f,
                RequiredMedals = 0,
                PrereqId = null,
                Difficulty = DifficultyLevel.Easy
            },
            new CourseSeed
            {
                AssetName = "Lava_Course_02.asset",
                CourseId = "lava_02",
                CourseName = "Magma Run",
                EnvironmentType = EnvironmentType.Lava,
                CourseIndex = 1,
                SceneName = "Lava_Course_02",
                Gold = 70f,
                Silver = 90f,
                Bronze = 110f,
                RequiredMedals = 0,
                PrereqId = "lava_01",
                Difficulty = DifficultyLevel.Medium
            },
            new CourseSeed
            {
                AssetName = "Lava_Course_03.asset",
                CourseId = "lava_03",
                CourseName = "Eruption",
                EnvironmentType = EnvironmentType.Lava,
                CourseIndex = 2,
                SceneName = "Lava_Course_03",
                Gold = 90f,
                Silver = 120f,
                Bronze = 150f,
                RequiredMedals = 3,
                PrereqId = "lava_02",
                Difficulty = DifficultyLevel.Hard
            },
            new CourseSeed
            {
                AssetName = "Ice_Course_01.asset",
                CourseId = "ice_01",
                CourseName = "Frozen Lake",
                EnvironmentType = EnvironmentType.Ice,
                CourseIndex = 0,
                SceneName = "Ice_Course_01",
                Gold = 60f,
                Silver = 80f,
                Bronze = 100f,
                RequiredMedals = 5,
                PrereqId = "lava_03",
                Difficulty = DifficultyLevel.Medium
            },
            new CourseSeed
            {
                AssetName = "Ice_Course_02.asset",
                CourseId = "ice_02",
                CourseName = "Crystal Caverns",
                EnvironmentType = EnvironmentType.Ice,
                CourseIndex = 1,
                SceneName = "Ice_Course_02",
                Gold = 80f,
                Silver = 105f,
                Bronze = 130f,
                RequiredMedals = 7,
                PrereqId = "ice_01",
                Difficulty = DifficultyLevel.Hard
            },
            new CourseSeed
            {
                AssetName = "Ice_Course_03.asset",
                CourseId = "ice_03",
                CourseName = "Avalanche Pass",
                EnvironmentType = EnvironmentType.Ice,
                CourseIndex = 2,
                SceneName = "Ice_Course_03",
                Gold = 100f,
                Silver = 130f,
                Bronze = 160f,
                RequiredMedals = 9,
                PrereqId = "ice_02",
                Difficulty = DifficultyLevel.Extreme
            },
            new CourseSeed
            {
                AssetName = "Toxic_Course_01.asset",
                CourseId = "toxic_01",
                CourseName = "Waste Disposal",
                EnvironmentType = EnvironmentType.Toxic,
                CourseIndex = 0,
                SceneName = "Toxic_Course_01",
                Gold = 65f,
                Silver = 85f,
                Bronze = 105f,
                RequiredMedals = 12,
                PrereqId = "ice_03",
                Difficulty = DifficultyLevel.Medium
            },
            new CourseSeed
            {
                AssetName = "Toxic_Course_02.asset",
                CourseId = "toxic_02",
                CourseName = "The Foundry",
                EnvironmentType = EnvironmentType.Toxic,
                CourseIndex = 1,
                SceneName = "Toxic_Course_02",
                Gold = 85f,
                Silver = 110f,
                Bronze = 140f,
                RequiredMedals = 15,
                PrereqId = "toxic_01",
                Difficulty = DifficultyLevel.Hard
            },
            new CourseSeed
            {
                AssetName = "Toxic_Course_03.asset",
                CourseId = "toxic_03",
                CourseName = "Meltdown",
                EnvironmentType = EnvironmentType.Toxic,
                CourseIndex = 2,
                SceneName = "Toxic_Course_03",
                Gold = 110f,
                Silver = 145f,
                Bronze = 180f,
                RequiredMedals = 18,
                PrereqId = "toxic_02",
                Difficulty = DifficultyLevel.Extreme
            }
        };

        private static readonly CourseSceneSeed[] CourseSceneSeeds =
        {
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Lava_Course_01.unity", SceneName = "Lava_Course_01", EnvironmentType = EnvironmentType.Lava, CheckpointCount = 3, HazardMarkerCount = 4, TrackLength = 260f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Lava_Course_02.unity", SceneName = "Lava_Course_02", EnvironmentType = EnvironmentType.Lava, CheckpointCount = 4, HazardMarkerCount = 5, TrackLength = 320f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Lava_Course_03.unity", SceneName = "Lava_Course_03", EnvironmentType = EnvironmentType.Lava, CheckpointCount = 5, HazardMarkerCount = 6, TrackLength = 380f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Ice_Course_01.unity", SceneName = "Ice_Course_01", EnvironmentType = EnvironmentType.Ice, CheckpointCount = 3, HazardMarkerCount = 4, TrackLength = 280f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Ice_Course_02.unity", SceneName = "Ice_Course_02", EnvironmentType = EnvironmentType.Ice, CheckpointCount = 4, HazardMarkerCount = 5, TrackLength = 340f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Ice_Course_03.unity", SceneName = "Ice_Course_03", EnvironmentType = EnvironmentType.Ice, CheckpointCount = 5, HazardMarkerCount = 6, TrackLength = 400f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Toxic_Course_01.unity", SceneName = "Toxic_Course_01", EnvironmentType = EnvironmentType.Toxic, CheckpointCount = 3, HazardMarkerCount = 5, TrackLength = 300f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Toxic_Course_02.unity", SceneName = "Toxic_Course_02", EnvironmentType = EnvironmentType.Toxic, CheckpointCount = 4, HazardMarkerCount = 6, TrackLength = 360f },
            new CourseSceneSeed { ScenePath = "Assets/Scenes/Toxic_Course_03.unity", SceneName = "Toxic_Course_03", EnvironmentType = EnvironmentType.Toxic, CheckpointCount = 5, HazardMarkerCount = 7, TrackLength = 430f }
        };

        private static readonly UpgradeSeed[] UpgradeSeeds =
        {
            new UpgradeSeed
            {
                AssetName = "Upgrade_Speed.asset",
                UpgradeId = "speed",
                UpgradeName = "Thruster Output",
                Category = UpgradeCategory.Speed,
                Levels = CreateLevels(
                    Level(100, 1.1f),
                    Level(250, 1.2f),
                    Level(500, 1.3f),
                    Level(1000, 1.4f),
                    Level(2000, 1.5f)),
                Modifiers = CreateModifiers(Modifier("maxSpeed", 4f))
            },
            new UpgradeSeed
            {
                AssetName = "Upgrade_Handling.asset",
                UpgradeId = "handling",
                UpgradeName = "Gyro Stabilizers",
                Category = UpgradeCategory.Handling,
                Levels = CreateLevels(
                    Level(100, 1.08f),
                    Level(250, 1.16f),
                    Level(500, 1.24f),
                    Level(1000, 1.32f),
                    Level(2000, 1.4f)),
                Modifiers = CreateModifiers(Modifier("turnSpeed", 0.24f))
            },
            new UpgradeSeed
            {
                AssetName = "Upgrade_Shield.asset",
                UpgradeId = "shield",
                UpgradeName = "Energy Barrier",
                Category = UpgradeCategory.Shield,
                Levels = CreateLevels(
                    Level(150, 1.12f),
                    Level(350, 1.24f),
                    Level(600, 1.36f),
                    Level(1200, 1.48f),
                    Level(2500, 1.6f)),
                Modifiers = CreateModifiers(Modifier("maxShield", 6f))
            },
            new UpgradeSeed
            {
                AssetName = "Upgrade_Boost.asset",
                UpgradeId = "boost",
                UpgradeName = "Nitro Injector",
                Category = UpgradeCategory.Boost,
                Levels = CreateLevels(
                    Level(100, 1.1f),
                    Level(250, 1.2f),
                    Level(500, 1.3f),
                    Level(1000, 1.4f),
                    Level(2000, 1.5f)),
                Modifiers = CreateModifiers(Modifier("boostMultiplier", 0.1f))
            }
        };

        private static readonly EnvironmentSeed[] EnvironmentSeeds =
        {
            new EnvironmentSeed
            {
                AssetName = "LavaEnvironment.asset",
                EnvironmentId = "lava",
                EnvironmentName = "Volcanic Wasteland",
                Type = EnvironmentType.Lava,
                RequiredMedals = 0,
                PrimaryHex = "#FF4400",
                SecondaryHex = "#1A0000",
                CourseIds = new[] { "lava_01", "lava_02", "lava_03" }
            },
            new EnvironmentSeed
            {
                AssetName = "IceEnvironment.asset",
                EnvironmentId = "ice",
                EnvironmentName = "Frozen Abyss",
                Type = EnvironmentType.Ice,
                RequiredMedals = 5,
                PrimaryHex = "#0088FF",
                SecondaryHex = "#001133",
                CourseIds = new[] { "ice_01", "ice_02", "ice_03" }
            },
            new EnvironmentSeed
            {
                AssetName = "ToxicEnvironment.asset",
                EnvironmentId = "toxic",
                EnvironmentName = "Industrial Wasteland",
                Type = EnvironmentType.Toxic,
                RequiredMedals = 12,
                PrimaryHex = "#44FF00",
                SecondaryHex = "#0A1A00",
                CourseIds = new[] { "toxic_01", "toxic_02", "toxic_03" }
            }
        };

        private static readonly CosmeticSeed[] CosmeticSeeds =
        {
            new CosmeticSeed { AssetName = "Color_Default.asset", CosmeticId = "default", CosmeticName = "Hazard Yellow", Type = CosmeticType.ColorScheme, Cost = 0, RequiredMedals = 0, Description = "Default paint scheme", PrimaryHex = "#D4A017", SecondaryHex = "#2A2A2A", AccentHex = "#FF8800" },
            new CosmeticSeed { AssetName = "Color_RedBlack.asset", CosmeticId = "red_black", CosmeticName = "Hellfire", Type = CosmeticType.ColorScheme, Cost = 200, RequiredMedals = 0, Description = "Aggressive red-black palette", PrimaryHex = "#CC0000", SecondaryHex = "#1A1A1A", AccentHex = "#FF4400" },
            new CosmeticSeed { AssetName = "Color_BlueSilver.asset", CosmeticId = "blue_silver", CosmeticName = "Frost Runner", Type = CosmeticType.ColorScheme, Cost = 200, RequiredMedals = 0, Description = "Cold blue and silver finish", PrimaryHex = "#2244AA", SecondaryHex = "#C0C0C0", AccentHex = "#00AAFF" },
            new CosmeticSeed { AssetName = "Color_GreenBlack.asset", CosmeticId = "green_black", CosmeticName = "Toxic Avenger", Type = CosmeticType.ColorScheme, Cost = 200, RequiredMedals = 0, Description = "Toxic industrial palette", PrimaryHex = "#228B22", SecondaryHex = "#1A1A1A", AccentHex = "#44FF00" },
            new CosmeticSeed { AssetName = "Color_Chrome.asset", CosmeticId = "chrome", CosmeticName = "Chrome Crusher", Type = CosmeticType.ColorScheme, Cost = 500, RequiredMedals = 0, Description = "Polished chrome finish", PrimaryHex = "#E0E0E0", SecondaryHex = "#808080", AccentHex = "#FFFFFF" },
            new CosmeticSeed { AssetName = "Decal_Skull.asset", CosmeticId = "decal_skull", CosmeticName = "Death's Head", Type = CosmeticType.Decal, Cost = 100, RequiredMedals = 0, Description = "Classic skull decal" },
            new CosmeticSeed { AssetName = "Decal_Flames.asset", CosmeticId = "decal_flames", CosmeticName = "Hellfire", Type = CosmeticType.Decal, Cost = 100, RequiredMedals = 0, Description = "Flame decal" },
            new CosmeticSeed { AssetName = "Decal_Lightning.asset", CosmeticId = "decal_lightning", CosmeticName = "Thunder Strike", Type = CosmeticType.Decal, Cost = 150, RequiredMedals = 0, Description = "Lightning decal" },
            new CosmeticSeed { AssetName = "Decal_RacingStripes.asset", CosmeticId = "decal_stripes", CosmeticName = "Speed Demon", Type = CosmeticType.Decal, Cost = 75, RequiredMedals = 0, Description = "Racing stripes decal" },
            new CosmeticSeed { AssetName = "Decal_Number73.asset", CosmeticId = "decal_73", CosmeticName = "Lucky 73", Type = CosmeticType.Decal, Cost = 0, RequiredMedals = 0, Description = "Lucky number decal" }
        };

        private static readonly HazardSeed[] HazardSeeds =
        {
            new HazardSeed { AssetName = "Hazard_LavaFlow.asset", HazardId = "lava_flow", HazardName = "Lava Flow", DamagePerSecond = 20f, DamagePerHit = 10f, WarningRadius = 18f, ThreatLevel = 2 },
            new HazardSeed { AssetName = "Hazard_Eruption.asset", HazardId = "eruption", HazardName = "Volcanic Eruption", DamagePerSecond = 0f, DamagePerHit = 30f, WarningRadius = 25f, ThreatLevel = 3 },
            new HazardSeed { AssetName = "Hazard_Geyser.asset", HazardId = "lava_geyser", HazardName = "Lava Geyser", DamagePerSecond = 0f, DamagePerHit = 25f, WarningRadius = 20f, ThreatLevel = 3 },
            new HazardSeed { AssetName = "Hazard_FallingDebris.asset", HazardId = "falling_debris", HazardName = "Falling Debris", DamagePerSecond = 0f, DamagePerHit = 20f, WarningRadius = 20f, ThreatLevel = 2 },
            new HazardSeed { AssetName = "Hazard_HeatZone.asset", HazardId = "heat_zone", HazardName = "Heat Zone", DamagePerSecond = 12f, DamagePerHit = 5f, WarningRadius = 16f, ThreatLevel = 2 },
            new HazardSeed { AssetName = "Hazard_IcePatch.asset", HazardId = "ice_patch", HazardName = "Ice Patch", DamagePerSecond = 0f, DamagePerHit = 0f, WarningRadius = 12f, ThreatLevel = 1 },
            new HazardSeed { AssetName = "Hazard_Icicle.asset", HazardId = "falling_icicle", HazardName = "Falling Icicle", DamagePerSecond = 0f, DamagePerHit = 18f, WarningRadius = 16f, ThreatLevel = 2 },
            new HazardSeed { AssetName = "Hazard_Blizzard.asset", HazardId = "blizzard", HazardName = "Blizzard", DamagePerSecond = 4f, DamagePerHit = 0f, WarningRadius = 24f, ThreatLevel = 2 },
            new HazardSeed { AssetName = "Hazard_ToxicGas.asset", HazardId = "toxic_gas", HazardName = "Toxic Gas", DamagePerSecond = 14f, DamagePerHit = 6f, WarningRadius = 18f, ThreatLevel = 2 },
            new HazardSeed { AssetName = "Hazard_Acid.asset", HazardId = "acid_pool", HazardName = "Acid Pool", DamagePerSecond = 22f, DamagePerHit = 12f, WarningRadius = 16f, ThreatLevel = 3 },
            new HazardSeed { AssetName = "Hazard_Press.asset", HazardId = "industrial_press", HazardName = "Industrial Press", DamagePerSecond = 0f, DamagePerHit = 999f, WarningRadius = 14f, ThreatLevel = 3 },
            new HazardSeed { AssetName = "Hazard_Electric.asset", HazardId = "electric_fence", HazardName = "Electric Fence", DamagePerSecond = 10f, DamagePerHit = 15f, WarningRadius = 15f, ThreatLevel = 2 },
            new HazardSeed { AssetName = "Hazard_Barrel.asset", HazardId = "barrel_explosion", HazardName = "Barrel Explosion", DamagePerSecond = 0f, DamagePerHit = 25f, WarningRadius = 14f, ThreatLevel = 2 }
        };

        [MenuItem("Metal Pod/Full Project Setup")]
        public static void RunFullSetup()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Configuring project settings...", 0.05f);
                ConfigureProjectSettings();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Generating ScriptableObject assets...", 0.2f);
                GenerateScriptableObjectAssets();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Creating hovercraft prefab...", 0.35f);
                CreateHovercraftPrefab();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Creating persistent scene...", 0.5f);
                CreatePersistentScene();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Creating main menu scene...", 0.62f);
                CreateMainMenuScene();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Creating workshop scene...", 0.72f);
                CreateWorkshopScene();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Creating test course scene...", 0.82f);
                CreateTestCourseScene();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Creating all course scenes...", 0.9f);
                CreateAllCourseScenes();

                EditorUtility.DisplayProgressBar("Metal Pod Setup", "Updating build settings...", 0.97f);
                UpdateBuildScenes();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Metal Pod Setup", "Full project setup completed successfully.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Metal Pod setup failed: {ex}");
                EditorUtility.DisplayDialog("Metal Pod Setup", "Setup failed. Check Console for details.", "OK");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Metal Pod/Setup/1. Configure Project Settings")]
        public static void MenuConfigureProjectSettings()
        {
            RunSingleStep("Configure Project Settings", ConfigureProjectSettings);
        }

        [MenuItem("Metal Pod/Setup/2. Generate ScriptableObject Assets")]
        public static void MenuGenerateScriptableObjectAssets()
        {
            RunSingleStep("Generate ScriptableObject Assets", GenerateScriptableObjectAssets);
        }

        [MenuItem("Metal Pod/Setup/3. Create Hovercraft Prefab")]
        public static void MenuCreateHovercraftPrefab()
        {
            RunSingleStep("Create Hovercraft Prefab", CreateHovercraftPrefab);
        }

        [MenuItem("Metal Pod/Setup/4. Create Persistent Scene")]
        public static void MenuCreatePersistentScene()
        {
            RunSingleStep("Create Persistent Scene", CreatePersistentScene);
        }

        [MenuItem("Metal Pod/Setup/5. Create Main Menu Scene")]
        public static void MenuCreateMainMenuScene()
        {
            RunSingleStep("Create Main Menu Scene", CreateMainMenuScene);
        }

        [MenuItem("Metal Pod/Setup/6. Create Workshop Scene")]
        public static void MenuCreateWorkshopScene()
        {
            RunSingleStep("Create Workshop Scene", CreateWorkshopScene);
        }

        [MenuItem("Metal Pod/Setup/7. Create Test Course Scene")]
        public static void MenuCreateTestCourseScene()
        {
            RunSingleStep("Create Test Course Scene", CreateTestCourseScene);
        }

        [MenuItem("Metal Pod/Setup/8. Create All Course Scenes")]
        public static void MenuCreateAllCourseScenes()
        {
            RunSingleStep("Create All Course Scenes", CreateAllCourseScenes);
        }

        private static void RunSingleStep(string title, Action step)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Metal Pod Setup", title, 0.5f);
                step?.Invoke();
                UpdateBuildScenes();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Metal Pod setup step completed: {title}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ConfigureProjectSettings()
        {
            AddTag(GameConstants.TAG_PLAYER);
            AddTag(GameConstants.TAG_CHECKPOINT);
            AddTag(GameConstants.TAG_HAZARD);
            AddTag(GameConstants.TAG_COLLECTIBLE);
            AddTag(GameConstants.TAG_FINISH);

            SetLayer(8, GameConstants.LAYER_HOVERCRAFT);
            SetLayer(9, GameConstants.LAYER_GROUND);
            SetLayer(10, GameConstants.LAYER_HAZARD);
            SetLayer(11, GameConstants.LAYER_COLLECTIBLE);

            int hovercraftLayer = LayerMask.NameToLayer(GameConstants.LAYER_HOVERCRAFT);
            int groundLayer = LayerMask.NameToLayer(GameConstants.LAYER_GROUND);
            int hazardLayer = LayerMask.NameToLayer(GameConstants.LAYER_HAZARD);
            int collectibleLayer = LayerMask.NameToLayer(GameConstants.LAYER_COLLECTIBLE);

            if (hovercraftLayer >= 0 && groundLayer >= 0)
            {
                Physics.IgnoreLayerCollision(hovercraftLayer, groundLayer, false);
            }

            if (hovercraftLayer >= 0 && hazardLayer >= 0)
            {
                Physics.IgnoreLayerCollision(hovercraftLayer, hazardLayer, false);
            }

            if (hovercraftLayer >= 0 && collectibleLayer >= 0)
            {
                Physics.IgnoreLayerCollision(hovercraftLayer, collectibleLayer, false);
            }

            if (hazardLayer >= 0 && collectibleLayer >= 0)
            {
                Physics.IgnoreLayerCollision(hazardLayer, collectibleLayer, true);
            }

            PlayerSettings.companyName = "Crocobyte";
            PlayerSettings.productName = "Metal Pod";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.targetOSVersionString = "15.0";

            ConfigureQualityLevels();
            ConfigureInputHandling();
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureQualityLevels()
        {
            string[] qualityNames = QualitySettings.names;
            if (qualityNames == null || qualityNames.Length == 0)
            {
                return;
            }

            int originalLevel = QualitySettings.GetQualityLevel();
            int maxConfigured = Mathf.Min(3, qualityNames.Length);

            for (int i = 0; i < maxConfigured; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.vSyncCount = 0;

                if (i == 0)
                {
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.shadowDistance = 20f;
                    QualitySettings.shadowCascades = 0;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.softParticles = false;
                    QualitySettings.pixelLightCount = 1;
                }
                else if (i == 1)
                {
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowProjection = ShadowProjection.StableFit;
                    QualitySettings.shadowDistance = 50f;
                    QualitySettings.shadowCascades = 2;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.softParticles = true;
                    QualitySettings.pixelLightCount = 2;
                }
                else
                {
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowProjection = ShadowProjection.StableFit;
                    QualitySettings.shadowDistance = 80f;
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.antiAliasing = 4;
                    QualitySettings.softParticles = true;
                    QualitySettings.pixelLightCount = 4;
                }
            }

            QualitySettings.SetQualityLevel(originalLevel, false);
        }

        private static void ConfigureInputHandling()
        {
            UnityEngine.Object[] settingsObjects = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settingsObjects == null || settingsObjects.Length == 0)
            {
                return;
            }

            SerializedObject playerSettingsObject = new SerializedObject(settingsObjects[0]);
            SerializedProperty activeInputHandler = playerSettingsObject.FindProperty("activeInputHandler");
            if (activeInputHandler != null)
            {
                // 0 = old, 1 = new, 2 = both.
                activeInputHandler.intValue = 1;
                playerSettingsObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void GenerateScriptableObjectAssets()
        {
            EnsureFolder(DataRoot);
            EnsureFolder(CoursesRoot);
            EnsureFolder(UpgradesRoot);
            EnsureFolder(EnvironmentsRoot);
            EnsureFolder(CosmeticsRoot);
            EnsureFolder(HazardsRoot);

            CreateHovercraftDefaults();
            CreateGameConfig();

            Dictionary<string, CourseDataSO> coursesById = CreateCourseAssets();
            CreateUpgradeAssets();
            CreateEnvironmentAssets(coursesById);
            CreateCosmeticAssets();
            CreateHazardAssets();
        }

        private static void CreateHovercraftDefaults()
        {
            HovercraftStatsSO stats = CreateOrLoadAsset<HovercraftStatsSO>(DataRoot + "/DefaultHovercraftStats.asset");
            stats.baseSpeed = 20f;
            stats.maxSpeed = 40f;
            stats.boostMultiplier = 1.5f;
            stats.boostDuration = 3f;
            stats.boostCooldown = 5f;
            stats.brakeForce = 15f;
            stats.turnSpeed = 3f;
            stats.hoverHeight = 2f;
            stats.hoverForce = 65f;
            stats.hoverDamping = 5f;
            stats.raycastCount = 4;
            stats.maxHealth = 100f;
            stats.maxShield = 50f;
            stats.shieldRegenRate = 5f;
            stats.shieldRegenDelay = 3f;
            stats.driftFactor = 0.95f;
            stats.tiltSensitivity = 1f;
            stats.stabilizationForce = 10f;
            EditorUtility.SetDirty(stats);
        }

        private static void CreateGameConfig()
        {
            GameConfigSO config = CreateOrLoadAsset<GameConfigSO>(DataRoot + "/GameConfig.asset");
            config.startingCurrency = 0;
            config.firstCourseId = "lava_01";
            config.respawnDelay = 2f;
            config.respawnInvincibilityDuration = 3f;
            config.countdownSeconds = 3;
            config.targetFrameRate = 60;
            config.maxParticleCount = 500;
            config.defaultTiltSensitivity = 1f;
            config.minTiltSensitivity = 0.5f;
            config.maxTiltSensitivity = 2f;
            config.tiltDeadzone = 0.05f;
            EditorUtility.SetDirty(config);
        }

        private static Dictionary<string, CourseDataSO> CreateCourseAssets()
        {
            Dictionary<string, CourseDataSO> coursesById = new Dictionary<string, CourseDataSO>();

            for (int i = 0; i < CourseSeeds.Length; i++)
            {
                CourseSeed seed = CourseSeeds[i];
                CourseDataSO course = CreateOrLoadAsset<CourseDataSO>($"{CoursesRoot}/{seed.AssetName}");
                course.courseId = seed.CourseId;
                course.courseName = seed.CourseName;
                course.description = seed.CourseName;
                course.environmentType = seed.EnvironmentType;
                course.courseIndex = seed.CourseIndex;
                course.sceneName = seed.SceneName;
                course.goldTime = seed.Gold;
                course.silverTime = seed.Silver;
                course.bronzeTime = seed.Bronze;
                course.requiredMedals = seed.RequiredMedals;
                course.difficulty = seed.Difficulty;
                course.prerequisiteCourse = null;
                EditorUtility.SetDirty(course);

                coursesById[seed.CourseId] = course;
            }

            for (int i = 0; i < CourseSeeds.Length; i++)
            {
                CourseSeed seed = CourseSeeds[i];
                if (string.IsNullOrWhiteSpace(seed.PrereqId))
                {
                    continue;
                }

                if (!coursesById.TryGetValue(seed.CourseId, out CourseDataSO course) || !coursesById.TryGetValue(seed.PrereqId, out CourseDataSO prereq))
                {
                    continue;
                }

                course.prerequisiteCourse = prereq;
                EditorUtility.SetDirty(course);
            }

            return coursesById;
        }

        private static void CreateUpgradeAssets()
        {
            for (int i = 0; i < UpgradeSeeds.Length; i++)
            {
                UpgradeSeed seed = UpgradeSeeds[i];
                UpgradeDataSO upgrade = CreateOrLoadAsset<UpgradeDataSO>($"{UpgradesRoot}/{seed.AssetName}");
                upgrade.upgradeId = seed.UpgradeId;
                upgrade.upgradeName = seed.UpgradeName;
                upgrade.description = seed.UpgradeName;
                upgrade.category = seed.Category;
                upgrade.levels = seed.Levels;
                upgrade.statModifiers = seed.Modifiers;
                EditorUtility.SetDirty(upgrade);
            }
        }

        private static void CreateEnvironmentAssets(Dictionary<string, CourseDataSO> coursesById)
        {
            for (int i = 0; i < EnvironmentSeeds.Length; i++)
            {
                EnvironmentSeed seed = EnvironmentSeeds[i];
                EnvironmentDataSO environment = CreateOrLoadAsset<EnvironmentDataSO>($"{EnvironmentsRoot}/{seed.AssetName}");

                List<CourseDataSO> courses = new List<CourseDataSO>();
                for (int j = 0; j < seed.CourseIds.Length; j++)
                {
                    string courseId = seed.CourseIds[j];
                    if (coursesById.TryGetValue(courseId, out CourseDataSO courseAsset) && courseAsset != null)
                    {
                        courses.Add(courseAsset);
                    }
                }

                environment.environmentId = seed.EnvironmentId;
                environment.environmentName = seed.EnvironmentName;
                environment.environmentType = seed.Type;
                environment.description = seed.EnvironmentName;
                environment.requiredMedalsToUnlock = seed.RequiredMedals;
                environment.primaryColor = ParseHex(seed.PrimaryHex, Color.white);
                environment.secondaryColor = ParseHex(seed.SecondaryHex, Color.gray);
                environment.courses = courses.ToArray();
                EditorUtility.SetDirty(environment);
            }
        }

        private static void CreateCosmeticAssets()
        {
            for (int i = 0; i < CosmeticSeeds.Length; i++)
            {
                CosmeticSeed seed = CosmeticSeeds[i];
                CosmeticDataSO cosmetic = CreateOrLoadAsset<CosmeticDataSO>($"{CosmeticsRoot}/{seed.AssetName}");
                cosmetic.cosmeticId = seed.CosmeticId;
                cosmetic.cosmeticName = seed.CosmeticName;
                cosmetic.cosmeticType = seed.Type;
                cosmetic.cost = seed.Cost;
                cosmetic.requiredMedals = seed.RequiredMedals;
                cosmetic.description = seed.Description;

                if (seed.Type == CosmeticType.ColorScheme)
                {
                    cosmetic.primaryColor = ParseHex(seed.PrimaryHex, Color.white);
                    cosmetic.secondaryColor = ParseHex(seed.SecondaryHex, Color.gray);
                    cosmetic.accentColor = ParseHex(seed.AccentHex, Color.black);
                }

                EditorUtility.SetDirty(cosmetic);
            }
        }

        private static void CreateHazardAssets()
        {
            for (int i = 0; i < HazardSeeds.Length; i++)
            {
                HazardSeed seed = HazardSeeds[i];
                HazardDataSO hazard = CreateOrLoadAsset<HazardDataSO>($"{HazardsRoot}/{seed.AssetName}");
                hazard.hazardId = seed.HazardId;
                hazard.hazardName = seed.HazardName;
                hazard.description = seed.HazardName;
                hazard.damagePerSecond = seed.DamagePerSecond;
                hazard.damagePerHit = seed.DamagePerHit;
                hazard.warningRadius = seed.WarningRadius;
                hazard.threatLevel = seed.ThreatLevel;
                EditorUtility.SetDirty(hazard);
            }
        }

        private static GameObject CreateHovercraftPrefab()
        {
            EnsureDirectoryExists(HovercraftPrefabPath);

            GameObject prefabRoot;
            bool loadedFromPrefab = false;

            if (File.Exists(HovercraftPrefabPath))
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(HovercraftPrefabPath);
                loadedFromPrefab = true;
            }
            else
            {
                prefabRoot = new GameObject("Hovercraft");
            }

            try
            {
                BuildHovercraftHierarchy(prefabRoot);

                if (loadedFromPrefab)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, HovercraftPrefabPath);
                }
                else
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, HovercraftPrefabPath);
                }
            }
            finally
            {
                if (loadedFromPrefab)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(prefabRoot);
                }
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(HovercraftPrefabPath);
        }

        private static void BuildHovercraftHierarchy(GameObject root)
        {
            root.name = "Hovercraft";
            TrySetTag(root, GameConstants.TAG_PLAYER);
            SetLayerRecursively(root, LayerMask.NameToLayer(GameConstants.LAYER_HOVERCRAFT));

            Rigidbody rb = EnsureComponent<Rigidbody>(root);
            rb.mass = 10f;
            rb.drag = 1f;
            rb.angularDrag = 3f;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            BoxCollider box = EnsureComponent<BoxCollider>(root);
            box.size = new Vector3(2f, 1f, 4f);
            box.center = new Vector3(0f, 0.5f, 0f);

            HovercraftController controller = EnsureComponent<HovercraftController>(root);
            HovercraftStats stats = EnsureComponent<HovercraftStats>(root);
            HovercraftInput input = EnsureComponent<HovercraftInput>(root);
            HovercraftHealth health = EnsureComponent<HovercraftHealth>(root);
            HovercraftPhysics physics = EnsureComponent<HovercraftPhysics>(root);
            HovercraftVisuals visuals = EnsureComponent<HovercraftVisuals>(root);
            HovercraftAudio audio = EnsureComponent<HovercraftAudio>(root);
            HovercraftCustomization customization = EnsureComponent<HovercraftCustomization>(root);

            HovercraftStatsSO defaultStats = AssetDatabase.LoadAssetAtPath<HovercraftStatsSO>(DataRoot + "/DefaultHovercraftStats.asset");
            if (defaultStats == null)
            {
                CreateHovercraftDefaults();
                defaultStats = AssetDatabase.LoadAssetAtPath<HovercraftStatsSO>(DataRoot + "/DefaultHovercraftStats.asset");
            }

            GameObject model = EnsurePrimitiveChild(root.transform, "Model", PrimitiveType.Cube);
            model.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = new Vector3(2f, 0.8f, 4f);
            Collider modelCollider = model.GetComponent<Collider>();
            if (modelCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(modelCollider);
            }

            GameObject physicsChild = EnsureChild(root.transform, "Physics");
            Transform[] hoverPoints =
            {
                EnsureChild(physicsChild.transform, "HoverPoint_FL").transform,
                EnsureChild(physicsChild.transform, "HoverPoint_FR").transform,
                EnsureChild(physicsChild.transform, "HoverPoint_BL").transform,
                EnsureChild(physicsChild.transform, "HoverPoint_BR").transform
            };

            hoverPoints[0].localPosition = new Vector3(-0.8f, 0f, 1.5f);
            hoverPoints[1].localPosition = new Vector3(0.8f, 0f, 1.5f);
            hoverPoints[2].localPosition = new Vector3(-0.8f, 0f, -1.5f);
            hoverPoints[3].localPosition = new Vector3(0.8f, 0f, -1.5f);

            EnsureChild(root.transform, "Input");
            EnsureChild(root.transform, "Health");

            GameObject visualsChild = EnsureChild(root.transform, "Visuals");
            ParticleSystem mainLeft = CreateParticleChild(visualsChild.transform, "Thruster_Main_L", new Vector3(-0.6f, 0f, -1.5f));
            ParticleSystem mainRight = CreateParticleChild(visualsChild.transform, "Thruster_Main_R", new Vector3(0.6f, 0f, -1.5f));
            ParticleSystem sideLeft = CreateParticleChild(visualsChild.transform, "Thruster_Side_L", new Vector3(-1.1f, 0f, 0.2f));
            ParticleSystem sideRight = CreateParticleChild(visualsChild.transform, "Thruster_Side_R", new Vector3(1.1f, 0f, 0.2f));
            ParticleSystem boost = CreateParticleChild(visualsChild.transform, "Boost_Effect", new Vector3(0f, 0f, -2f));
            ParticleSystem sparks = CreateParticleChild(visualsChild.transform, "Sparks", new Vector3(0f, 0.2f, 0f));
            ParticleSystem smoke = CreateParticleChild(visualsChild.transform, "Smoke", new Vector3(0f, 0.7f, -0.3f));
            ParticleSystem fire = CreateParticleChild(visualsChild.transform, "Fire", new Vector3(0f, 0.7f, 0.1f));

            GameObject audioChild = EnsureChild(root.transform, "Audio");
            AudioSource engineSource = EnsureComponent<AudioSource>(EnsureChild(audioChild.transform, "EngineSource"));
            engineSource.loop = true;
            engineSource.playOnAwake = true;
            engineSource.spatialBlend = 0f;

            AudioSource sfxSource = EnsureComponent<AudioSource>(EnsureChild(audioChild.transform, "SFXSource"));
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            GameObject customizationChild = EnsureChild(root.transform, "Customization");
            Transform attachTop = EnsureChild(customizationChild.transform, "TopMount").transform;
            Transform attachLeft = EnsureChild(customizationChild.transform, "SideMount_L").transform;
            Transform attachRight = EnsureChild(customizationChild.transform, "SideMount_R").transform;
            Transform attachRear = EnsureChild(customizationChild.transform, "RearMount").transform;

            attachTop.localPosition = new Vector3(0f, 0.75f, 0f);
            attachLeft.localPosition = new Vector3(-0.95f, 0.3f, -0.1f);
            attachRight.localPosition = new Vector3(0.95f, 0.3f, -0.1f);
            attachRear.localPosition = new Vector3(0f, 0.4f, -1.8f);

            SetSerializedReference(controller, "stats", defaultStats);
            SetSerializedReference(stats, "baseStats", defaultStats);
            SetSerializedReference(health, "stats", defaultStats);
            SetSerializedReference(physics, "stats", defaultStats);
            SetSerializedArrayReference(physics, "hoverPoints", hoverPoints);

            SetSerializedReference(visuals, "mainThrusters", new[] { mainLeft, mainRight });
            SetSerializedReference(visuals, "sideThrusters", new[] { sideLeft, sideRight });
            SetSerializedReference(visuals, "boostThruster", boost);
            SetSerializedReference(visuals, "sparksEffect", sparks);
            SetSerializedReference(visuals, "smokeEffect", smoke);
            SetSerializedReference(visuals, "fireEffect", fire);
            SetSerializedReference(visuals, "modelRenderers", new[] { model.GetComponent<Renderer>() });

            SetSerializedReference(audio, "engineSource", engineSource);
            SetSerializedReference(audio, "sfxSource", sfxSource);

            SetSerializedReference(customization, "bodyRenderer", model.GetComponent<Renderer>());
            SetSerializedReference(customization, "accentRenderer", model.GetComponent<Renderer>());
            SetSerializedReference(customization, "decalTargetRenderer", model.GetComponent<Renderer>());
            SetSerializedArrayReference(customization, "attachPoints", new[] { attachTop, attachLeft, attachRight, attachRear });

            EditorUtility.SetDirty(root);
        }

        private static void CreatePersistentScene()
        {
            EnsureDirectoryExists(PersistentScenePath);
            Scene scene = OpenOrCreateScene(PersistentScenePath, NewSceneSetup.EmptyScene);

            GameObject gmObject = EnsureRootObject(scene, "GameManager");
            GameManager gameManager = EnsureComponent<GameManager>(gmObject);
            GameStateManager stateManager = EnsureComponent<GameStateManager>(gmObject);
            SceneLoader sceneLoader = EnsureComponent<SceneLoader>(gmObject);
            AudioManager audioManager = EnsureComponent<AudioManager>(gmObject);

            GameObject audioSources = EnsureChild(gmObject.transform, "AudioSources");
            AudioSource musicA = EnsureComponent<AudioSource>(EnsureChild(audioSources.transform, "MusicSource_A"));
            AudioSource musicB = EnsureComponent<AudioSource>(EnsureChild(audioSources.transform, "MusicSource_B"));
            AudioSource ambient = EnsureComponent<AudioSource>(EnsureChild(audioSources.transform, "AmbientSource"));
            AudioSource sfx = EnsureComponent<AudioSource>(EnsureChild(audioSources.transform, "SFXSource_0"));

            GameConfigSO config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(DataRoot + "/GameConfig.asset");
            if (config == null)
            {
                CreateGameConfig();
                config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(DataRoot + "/GameConfig.asset");
            }

            SetSerializedReference(gameManager, "gameConfig", config);
            SetSerializedReference(gameManager, "gameStateManager", stateManager);
            SetSerializedReference(gameManager, "sceneLoader", sceneLoader);
            SetSerializedReference(gameManager, "audioManager", audioManager);
            SetSerializedBool(gameManager, "loadMainMenuOnStart", true);

            SetSerializedReference(audioManager, "musicSource", musicA);
            SetSerializedReference(audioManager, "secondaryMusicSource", musicB);
            SetSerializedReference(audioManager, "ambientSource", ambient);
            SetSerializedReference(audioManager, "sfxSource", sfx);
            SetSerializedInt(audioManager, "sfxPoolSize", 8);

            GameObject progressionObject = EnsureRootObject(scene, "ProgressionManager");
            EnsureComponent<ProgressionManager>(progressionObject);

            EditorSceneManager.SaveScene(scene, PersistentScenePath);
        }

        private static void CreateMainMenuScene()
        {
            EnsureDirectoryExists(MainMenuScenePath);
            Scene scene = OpenOrCreateScene(MainMenuScenePath, NewSceneSetup.DefaultGameObjects);

            GameObject canvas = CreateCanvas("MainMenuCanvas");

            RawImage background = EnsureComponent<RawImage>(EnsureUIChild(canvas.transform, "Background"));
            SetupRect(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            background.color = new Color(0.05f, 0.05f, 0.06f, 1f);

            TMP_Text titleText = CreateUIText(canvas.transform, "Title", "METAL POD", 72, new Color(1f, 0.53f, 0f), new Vector2(0f, 220f));
            titleText.fontStyle = FontStyles.Bold;

            CreateUIText(canvas.transform, "Subtitle", "A Crocobyte Game", 24, new Color(0.65f, 0.65f, 0.65f, 1f), new Vector2(0f, 150f));

            GameObject buttonGroup = CreateVerticalGroup(canvas.transform, "ButtonGroup", new Vector2(0f, -60f), 20f);
            Button continueButton = CreateButton(buttonGroup.transform, "ContinueButton", "CONTINUE");
            Button newGameButton = CreateButton(buttonGroup.transform, "NewGameButton", "NEW GAME");
            Button settingsButton = CreateButton(buttonGroup.transform, "SettingsButton", "SETTINGS");
            Button quitButton = CreateButton(buttonGroup.transform, "QuitButton", "QUIT");

            GameObject confirmPanel = EnsureUIChild(canvas.transform, "ConfirmNewGamePanel");
            Image confirmBg = EnsureComponent<Image>(confirmPanel);
            confirmBg.color = new Color(0f, 0f, 0f, 0.78f);
            RectTransform confirmRect = confirmPanel.GetComponent<RectTransform>();
            SetupRect(confirmRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 280f), Vector2.zero, Vector2.zero);

            CreateUIText(confirmPanel.transform, "Prompt", "Start a new game?", 34, Color.white, new Vector2(0f, 70f));
            Button confirmYes = CreateButton(confirmPanel.transform, "YesButton", "YES");
            Button confirmNo = CreateButton(confirmPanel.transform, "NoButton", "NO");
            confirmYes.GetComponent<RectTransform>().anchoredPosition = new Vector2(-110f, -70f);
            confirmNo.GetComponent<RectTransform>().anchoredPosition = new Vector2(110f, -70f);

            confirmPanel.SetActive(false);

            GameObject mainMenuObject = EnsureRootObject(scene, "MainMenuUI");
            MainMenuUI mainMenu = EnsureComponent<MainMenuUI>(mainMenuObject);
            SetSerializedReference(mainMenu, "continueButton", continueButton);
            SetSerializedReference(mainMenu, "newGameButton", newGameButton);
            SetSerializedReference(mainMenu, "settingsButton", settingsButton);
            SetSerializedReference(mainMenu, "quitButton", quitButton);
            SetSerializedReference(mainMenu, "titleText", titleText);
            SetSerializedReference(mainMenu, "backgroundImage", background);
            SetSerializedReference(mainMenu, "confirmNewGamePanel", confirmPanel);
            SetSerializedReference(mainMenu, "confirmNewGameYesButton", confirmYes);
            SetSerializedReference(mainMenu, "confirmNewGameNoButton", confirmNo);

            EnsureComponent<UIManager>(EnsureRootObject(scene, "UIManager"));
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void CreateWorkshopScene()
        {
            EnsureDirectoryExists(WorkshopScenePath);
            Scene scene = OpenOrCreateScene(WorkshopScenePath, NewSceneSetup.DefaultGameObjects);

            int groundLayer = LayerMask.NameToLayer(GameConstants.LAYER_GROUND);

            GameObject floor = EnsurePrimitiveRoot(scene, "WorkshopFloor", PrimitiveType.Plane);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(6f, 1f, 6f);
            if (groundLayer >= 0)
            {
                floor.layer = groundLayer;
            }

            GameObject wallBack = EnsurePrimitiveRoot(scene, "WorkshopWall_Back", PrimitiveType.Cube);
            wallBack.transform.position = new Vector3(0f, 4f, 30f);
            wallBack.transform.localScale = new Vector3(60f, 8f, 1f);

            GameObject wallLeft = EnsurePrimitiveRoot(scene, "WorkshopWall_Left", PrimitiveType.Cube);
            wallLeft.transform.position = new Vector3(-30f, 4f, 0f);
            wallLeft.transform.localScale = new Vector3(1f, 8f, 60f);

            GameObject wallRight = EnsurePrimitiveRoot(scene, "WorkshopWall_Right", PrimitiveType.Cube);
            wallRight.transform.position = new Vector3(30f, 4f, 0f);
            wallRight.transform.localScale = new Vector3(1f, 8f, 60f);

            GameObject platform = EnsurePrimitiveRoot(scene, "DisplayPlatform", PrimitiveType.Cylinder);
            platform.transform.position = new Vector3(0f, 0.5f, 0f);
            platform.transform.localScale = new Vector3(6f, 1f, 6f);

            GameObject hovercraft = EnsureSceneHovercraftInstance(new Vector3(0f, 2f, 0f));

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0f, 5.5f, -12f);
                mainCamera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

                WorkshopCameraController cameraController = EnsureComponent<WorkshopCameraController>(mainCamera.gameObject);
                GameObject cameraTargets = EnsureRootObject(scene, "WorkshopCameraTargets");
                Transform defaultView = EnsureChild(cameraTargets.transform, "DefaultView").transform;
                Transform craftView = EnsureChild(cameraTargets.transform, "HovercraftView").transform;
                Transform mapView = EnsureChild(cameraTargets.transform, "MapView").transform;

                defaultView.position = new Vector3(0f, 5.5f, -12f);
                defaultView.rotation = Quaternion.Euler(18f, 0f, 0f);
                craftView.position = new Vector3(0f, 3.5f, -6f);
                craftView.rotation = Quaternion.Euler(14f, 0f, 0f);
                mapView.position = new Vector3(0f, 11f, -18f);
                mapView.rotation = Quaternion.Euler(24f, 0f, 0f);

                SetSerializedReference(cameraController, "defaultView", defaultView);
                SetSerializedReference(cameraController, "hovercraftView", craftView);
                SetSerializedReference(cameraController, "mapView", mapView);
            }

            GameObject canvas = CreateCanvas("WorkshopCanvas");

            GameObject topBar = EnsureUIChild(canvas.transform, "TopBar");
            Image topBarBg = EnsureComponent<Image>(topBar);
            topBarBg.color = new Color(0f, 0f, 0f, 0.55f);
            SetupRect(topBar.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 100f), Vector2.zero, Vector2.zero);

            CreateUIText(topBar.transform, "CurrencyLabel", "Credits", 30, Color.white, new Vector2(-220f, -42f));
            TMP_Text currencyValue = CreateUIText(topBar.transform, "CurrencyValue", "0", 34, new Color(1f, 0.78f, 0.2f), new Vector2(-95f, -42f));
            CreateUIText(topBar.transform, "MedalsLabel", "Medals", 30, Color.white, new Vector2(120f, -42f));
            CreateUIText(topBar.transform, "MedalsValue", "0", 34, new Color(0.8f, 0.9f, 1f), new Vector2(230f, -42f));

            CurrencyDisplay currencyDisplay = EnsureComponent<CurrencyDisplay>(currencyValue.gameObject);
            SetSerializedReference(currencyDisplay, "currencyText", currencyValue);

            GameObject sidePanel = EnsureUIChild(canvas.transform, "SidePanels");
            SetupRect(sidePanel.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(420f, 620f), new Vector2(-220f, 0f), new Vector2(-220f, 0f));
            Image sidePanelBg = EnsureComponent<Image>(sidePanel);
            sidePanelBg.color = new Color(0f, 0f, 0f, 0.4f);

            GameObject upgradePanel = EnsureUIChild(sidePanel.transform, "UpgradePanel");
            EnsureComponent<Image>(upgradePanel).color = new Color(0.12f, 0.12f, 0.14f, 0.7f);
            SetupRect(upgradePanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.5f), new Vector2(380f, 180f), Vector2.zero, Vector2.zero);
            CreateUIText(upgradePanel.transform, "Title", "UPGRADES", 30, new Color(1f, 0.6f, 0.2f), new Vector2(0f, 0f));

            GameObject customizationPanel = EnsureUIChild(sidePanel.transform, "CustomizationPanel");
            EnsureComponent<Image>(customizationPanel).color = new Color(0.12f, 0.12f, 0.14f, 0.7f);
            SetupRect(customizationPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.5f), new Vector2(380f, 180f), Vector2.zero, Vector2.zero);
            CreateUIText(customizationPanel.transform, "Title", "CUSTOMIZATION", 30, new Color(1f, 0.6f, 0.2f), new Vector2(0f, 0f));

            GameObject courseSelectionPanel = EnsureUIChild(canvas.transform, "CourseSelectionPanel");
            Image coursePanelBg = EnsureComponent<Image>(courseSelectionPanel);
            coursePanelBg.color = new Color(0f, 0f, 0f, 0.42f);
            SetupRect(courseSelectionPanel.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(380f, 0f), new Vector2(0f, 0f), new Vector2(360f, 0f));
            CreateUIText(courseSelectionPanel.transform, "Title", "COURSES", 32, new Color(1f, 0.6f, 0.2f), new Vector2(0f, 250f));

            GameObject bottomNav = EnsureUIChild(canvas.transform, "BottomNav");
            Image navBg = EnsureComponent<Image>(bottomNav);
            navBg.color = new Color(0f, 0f, 0f, 0.55f);
            SetupRect(bottomNav.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), Vector2.zero, Vector2.zero);

            Button upgradesButton = CreateButton(bottomNav.transform, "UpgradesButton", "UPGRADES");
            Button customizationButton = CreateButton(bottomNav.transform, "CustomizationButton", "CUSTOMIZE");
            Button coursesButton = CreateButton(bottomNav.transform, "CoursesButton", "COURSES");

            upgradesButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260f, 56f);
            customizationButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 56f);
            coursesButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(260f, 56f);

            GameObject workshopManagerObject = EnsureRootObject(scene, "WorkshopManager");
            WorkshopManager workshopManager = EnsureComponent<WorkshopManager>(workshopManagerObject);
            WorkshopCameraController workshopCamera = mainCamera != null
                ? mainCamera.GetComponent<WorkshopCameraController>()
                : null;

            SetSerializedReference(workshopManager, "upgradePanel", upgradePanel);
            SetSerializedReference(workshopManager, "customizationPanel", customizationPanel);
            SetSerializedReference(workshopManager, "courseSelectionPanel", courseSelectionPanel);
            SetSerializedReference(workshopManager, "hovercraftDisplay", hovercraft != null ? hovercraft.transform : null);
            SetSerializedReference(workshopManager, "workshopCamera", workshopCamera);
            SetSerializedReference(workshopManager, "currencyDisplay", currencyDisplay);
            SetSerializedReference(workshopManager, "availableCourses", LoadAllCourseAssets());

            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, WorkshopScenePath);
        }

        private static void CreateTestCourseScene()
        {
            EnsureDirectoryExists(TestCourseScenePath);
            Scene scene = OpenOrCreateScene(TestCourseScenePath, NewSceneSetup.DefaultGameObjects);

            BuildCourseSceneStructure(
                scene,
                "TestCourse",
                EnvironmentType.Lava,
                trackLength: 300f,
                checkpointCount: 3,
                hazardMarkerCount: 4,
                includeCollectibles: true,
                includeSideNarrowSection: true);

            EditorSceneManager.SaveScene(scene, TestCourseScenePath);
        }

        private static void CreateAllCourseScenes()
        {
            for (int i = 0; i < CourseSceneSeeds.Length; i++)
            {
                CourseSceneSeed seed = CourseSceneSeeds[i];
                EnsureDirectoryExists(seed.ScenePath);
                Scene scene = OpenOrCreateScene(seed.ScenePath, NewSceneSetup.DefaultGameObjects);

                BuildCourseSceneStructure(
                    scene,
                    seed.SceneName,
                    seed.EnvironmentType,
                    seed.TrackLength,
                    seed.CheckpointCount,
                    seed.HazardMarkerCount,
                    includeCollectibles: false,
                    includeSideNarrowSection: false);

                EditorSceneManager.SaveScene(scene, seed.ScenePath);
            }
        }

        private static void BuildCourseSceneStructure(
            Scene scene,
            string sceneName,
            EnvironmentType environmentType,
            float trackLength,
            int checkpointCount,
            int hazardMarkerCount,
            bool includeCollectibles,
            bool includeSideNarrowSection)
        {
            int groundLayer = LayerMask.NameToLayer(GameConstants.LAYER_GROUND);

            GameObject ground = EnsurePrimitiveRoot(scene, "Ground", PrimitiveType.Plane);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, Mathf.Max(8f, trackLength / 10f));
            if (groundLayer >= 0)
            {
                ground.layer = groundLayer;
            }

            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                groundRenderer.sharedMaterial = GetOrCreateEnvironmentGroundMaterial(environmentType);
            }

            CreateWall("Wall_Left", new Vector3(-50f, 2.5f, trackLength * 0.5f), new Vector3(1f, 5f, trackLength), scene);
            CreateWall("Wall_Right", new Vector3(50f, 2.5f, trackLength * 0.5f), new Vector3(1f, 5f, trackLength), scene);

            CreateRamp("Ramp_01", new Vector3(0f, 0f, trackLength * 0.2f), new Vector3(12f, 0.15f, 20f), 14f, scene);

            if (includeSideNarrowSection)
            {
                CreateWall("Narrow_L", new Vector3(-3f, 2.5f, trackLength * 0.43f), new Vector3(1f, 5f, 34f), scene);
                CreateWall("Narrow_R", new Vector3(3f, 2.5f, trackLength * 0.43f), new Vector3(1f, 5f, 34f), scene);
            }

            GameObject startLine = EnsureRootObject(scene, "StartLine");
            startLine.transform.position = new Vector3(0f, 1f, 0f);
            BoxCollider startCollider = EnsureComponent<BoxCollider>(startLine);
            startCollider.size = new Vector3(15f, 3f, 1f);
            startCollider.isTrigger = true;

            GameObject finishLine = EnsureRootObject(scene, "FinishLine");
            finishLine.transform.position = new Vector3(0f, 1f, trackLength - 20f);
            TrySetTag(finishLine, GameConstants.TAG_FINISH);
            BoxCollider finishCollider = EnsureComponent<BoxCollider>(finishLine);
            finishCollider.size = new Vector3(15f, 3f, 1f);
            finishCollider.isTrigger = true;
            FinishLine finishComponent = EnsureComponent<FinishLine>(finishLine);

            List<Checkpoint> checkpoints = new List<Checkpoint>();
            float checkpointSpacing = (trackLength - 40f) / Mathf.Max(1, checkpointCount);
            for (int i = 0; i < checkpointCount; i++)
            {
                float z = 20f + ((i + 1) * checkpointSpacing);
                Checkpoint cp = CreateCheckpoint($"Checkpoint_{i + 1}", new Vector3(0f, 1f, z), i, scene);
                checkpoints.Add(cp);
            }

            Transform spawnPoint = EnsureRootObject(scene, "PlayerSpawn").transform;
            spawnPoint.position = new Vector3(0f, 2f, -5f);
            spawnPoint.rotation = Quaternion.identity;

            GameObject hovercraft = EnsureSceneHovercraftInstance(spawnPoint.position);
            if (hovercraft != null)
            {
                hovercraft.transform.rotation = spawnPoint.rotation;
            }

            GameObject managerObject = EnsureRootObject(scene, "CourseManager");
            CourseManager courseManager = EnsureComponent<CourseManager>(managerObject);
            CourseTimer timer = EnsureComponent<CourseTimer>(managerObject);

            CourseDataSO courseData = FindCourseBySceneName(sceneName);
            if (courseData == null)
            {
                courseData = FindCourseBySceneName(SceneManager.GetActiveScene().name);
            }

            SetSerializedReference(courseManager, "courseData", courseData);
            SetSerializedReference(courseManager, "courseTimer", timer);
            SetSerializedReference(courseManager, "finishLine", finishComponent);
            SetSerializedReference(courseManager, "defaultSpawnPoint", spawnPoint);

            GameConfigSO config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(DataRoot + "/GameConfig.asset");
            if (config != null)
            {
                SetSerializedFloat(courseManager, "countdownSeconds", config.countdownSeconds);
                SetSerializedFloat(courseManager, "respawnDelaySeconds", config.respawnDelay);
            }

            GameObject hazardMarkerRoot = EnsureRootObject(scene, "HazardMarkers");
            for (int i = 0; i < hazardMarkerCount; i++)
            {
                float t = hazardMarkerCount <= 1 ? 0.5f : i / (float)(hazardMarkerCount - 1);
                float z = Mathf.Lerp(35f, trackLength - 35f, t);
                float x = (i % 2 == 0 ? -7f : 7f);
                GameObject marker = EnsureChild(hazardMarkerRoot.transform, $"HazardSpawn_{i + 1:00}");
                marker.transform.position = new Vector3(x, 0.5f, z);
                marker.transform.rotation = Quaternion.identity;
            }

            if (includeCollectibles)
            {
                CreateCollectible("Collectible_01", new Vector3(3f, 1.5f, 30f), scene);
                CreateCollectible("Collectible_02", new Vector3(-3f, 1.5f, 80f), scene);
                CreateCollectible("Collectible_03", new Vector3(0f, 3f, 55f), scene);
                CreateCollectible("Collectible_04", new Vector3(2f, 1.5f, 130f), scene);
                CreateCollectible("Collectible_05", new Vector3(-5f, 1.5f, 200f), scene);
            }

            CreateHudCanvas(scene);
            CreateEventSystem();
        }

        private static void CreateHudCanvas(Scene scene)
        {
            GameObject canvas = CreateCanvas("HUDCanvas");
            HUD hud = EnsureComponent<HUD>(canvas);

            GameObject healthBar = CreateBar(canvas.transform, "HealthBar", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(210f, -56f), new Vector2(280f, 26f), new Color(0.25f, 0.07f, 0.07f, 0.8f));
            Image healthFill = CreateBarFill(healthBar.transform, "HealthFill", Color.green);
            SetSerializedReference(hud, "healthFill", healthFill);

            GameObject shieldBar = CreateBar(canvas.transform, "ShieldBar", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(210f, -88f), new Vector2(280f, 18f), new Color(0.05f, 0.08f, 0.16f, 0.8f));
            Image shieldFill = CreateBarFill(shieldBar.transform, "ShieldFill", new Color(0.2f, 0.6f, 1f));
            SetSerializedReference(hud, "shieldFill", shieldFill);

            TMP_Text speedText = CreateUIText(canvas.transform, "SpeedText", "000", 44, Color.white, new Vector2(0f, -460f));
            SetSerializedReference(hud, "speedText", speedText);

            TMP_Text timerText = CreateUIText(canvas.transform, "TimerText", "00:00.00", 40, Color.white, new Vector2(0f, 460f));
            SetSerializedReference(hud, "timerText", timerText);

            GameObject boostBar = CreateBar(canvas.transform, "BoostBar", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-210f, -56f), new Vector2(280f, 22f), new Color(0.1f, 0.1f, 0.1f, 0.8f));
            Image boostFill = CreateBarFill(boostBar.transform, "BoostFill", Color.cyan);
            SetSerializedReference(hud, "boostFill", boostFill);
        }

        private static void UpdateBuildScenes()
        {
            EditorBuildSettingsScene[] scenes =
            {
                new EditorBuildSettingsScene(PersistentScenePath, true),
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(WorkshopScenePath, true),
                new EditorBuildSettingsScene(TestCourseScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/Lava_Course_01.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Lava_Course_02.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Lava_Course_03.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Ice_Course_01.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Ice_Course_02.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Ice_Course_03.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Toxic_Course_01.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Toxic_Course_02.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Toxic_Course_03.unity", true)
            };

            EditorBuildSettings.scenes = scenes;
        }

        private static void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
            {
                SerializedProperty current = tags.GetArrayElementAtIndex(i);
                if (current != null && current.stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetLayer(int index, string name)
        {
            if (index < 8 || index > 31 || string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || layers.arraySize <= index)
            {
                return;
            }

            layers.GetArrayElementAtIndex(index).stringValue = name;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            EnsureDirectoryExists(path);

            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureDirectoryExists(string assetPath)
        {
            string normalized = assetPath.Replace("\\", "/");
            string folder = Path.GetExtension(normalized).Length > 0
                ? Path.GetDirectoryName(normalized)
                : normalized;

            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            EnsureFolder(folder.Replace("\\", "/"));
        }

        private static void EnsureFolder(string assetFolderPath)
        {
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string[] parts = assetFolderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new InvalidOperationException($"Expected Assets-relative path, got: {assetFolderPath}");
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static Scene OpenOrCreateScene(string scenePath, NewSceneSetup setup)
        {
            if (File.Exists(scenePath))
            {
                return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            return EditorSceneManager.NewScene(setup, NewSceneMode.Single);
        }

        private static GameObject EnsureRootObject(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == name)
                {
                    return roots[i];
                }
            }

            GameObject created = new GameObject(name);
            SceneManager.MoveGameObjectToScene(created, scene);
            return created;
        }

        private static GameObject EnsurePrimitiveRoot(Scene scene, string name, PrimitiveType type)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject created = GameObject.CreatePrimitive(type);
            created.name = name;
            SceneManager.MoveGameObjectToScene(created, scene);
            return created;
        }

        private static GameObject EnsureChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject EnsureUIChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject EnsurePrimitiveChild(Transform parent, string name, PrimitiveType type)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            return go;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }

            return component;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (layer < 0)
            {
                return;
            }

            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, layer);
                }
            }
        }

        private static void TrySetTag(GameObject go, string tag)
        {
            if (go == null || string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    go.tag = tag;
                    return;
                }
            }
        }

        private static GameObject EnsureSceneHovercraftInstance(Vector3 position)
        {
            GameObject existing = GameObject.Find("Hovercraft");
            if (existing != null)
            {
                existing.transform.position = position;
                return existing;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HovercraftPrefabPath);
            if (prefab == null)
            {
                prefab = CreateHovercraftPrefab();
            }

            if (prefab == null)
            {
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(prefab);
            }

            instance.name = "Hovercraft";
            instance.transform.position = position;
            return instance;
        }

        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasObject = GameObject.Find(name);
            if (canvasObject == null)
            {
                canvasObject = new GameObject(name, typeof(RectTransform));
            }
            else if (canvasObject.GetComponent<RectTransform>() == null)
            {
                UnityEngine.Object.DestroyImmediate(canvasObject);
                canvasObject = new GameObject(name, typeof(RectTransform));
            }

            Canvas canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureComponent<GraphicRaycaster>(canvasObject);

            RectTransform rect = canvasObject.GetComponent<RectTransform>();

            SetupRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);
            return canvasObject;
        }

        private static TMP_Text CreateUIText(Transform parent, string name, string text, int size, Color color, Vector2 pos)
        {
            GameObject textObject = EnsureUIChild(parent, name);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            SetupRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700f, 90f), pos, pos);

            TextMeshProUGUI tmp = EnsureComponent<TextMeshProUGUI>(textObject);
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private static Image CreateUIImage(Transform parent, string name, Color color, Vector2 pos, Vector2 size)
        {
            GameObject imageObject = EnsureUIChild(parent, name);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            SetupRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, pos, pos);

            Image image = EnsureComponent<Image>(imageObject);
            image.color = color;
            return image;
        }

        private static Button CreateButton(Transform parent, string name, string label)
        {
            GameObject buttonObject = EnsureUIChild(parent, name);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetupRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(320f, 84f), Vector2.zero, Vector2.zero);

            Image image = EnsureComponent<Image>(buttonObject);
            image.color = new Color(0.16f, 0.16f, 0.18f, 0.92f);

            Button button = EnsureComponent<Button>(buttonObject);
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.24f, 0.24f, 0.28f, 0.95f);
            colors.pressedColor = new Color(0.34f, 0.34f, 0.39f, 0.95f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            button.colors = colors;

            TMP_Text text = CreateUIText(buttonObject.transform, "Label", label, 30, Color.white, Vector2.zero);
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private static GameObject CreateVerticalGroup(Transform parent, string name, Vector2 pos, float spacing)
        {
            GameObject group = EnsureUIChild(parent, name);
            RectTransform rect = group.GetComponent<RectTransform>();
            SetupRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 420f), pos, pos);

            VerticalLayoutGroup layout = EnsureComponent<VerticalLayoutGroup>(group);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = spacing;

            ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(group);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return group;
        }

        private static void CreateEventSystem()
        {
            EventSystem existing = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (existing != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static void CreateWall(string name, Vector3 pos, Vector3 scale, Scene scene)
        {
            GameObject wall = EnsurePrimitiveRoot(scene, name, PrimitiveType.Cube);
            wall.transform.position = pos;
            wall.transform.rotation = Quaternion.identity;
            wall.transform.localScale = scale;
            int groundLayer = LayerMask.NameToLayer(GameConstants.LAYER_GROUND);
            if (groundLayer >= 0)
            {
                wall.layer = groundLayer;
            }
        }

        private static void CreateRamp(string name, Vector3 pos, Vector3 scale, float angle, Scene scene)
        {
            GameObject ramp = EnsurePrimitiveRoot(scene, name, PrimitiveType.Cube);
            ramp.transform.position = pos;
            ramp.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
            ramp.transform.localScale = scale;
            int groundLayer = LayerMask.NameToLayer(GameConstants.LAYER_GROUND);
            if (groundLayer >= 0)
            {
                ramp.layer = groundLayer;
            }
        }

        private static Checkpoint CreateCheckpoint(string name, Vector3 pos, int index, Scene scene)
        {
            GameObject checkpoint = EnsureRootObject(scene, name);
            checkpoint.transform.position = pos;
            checkpoint.transform.rotation = Quaternion.identity;
            TrySetTag(checkpoint, GameConstants.TAG_CHECKPOINT);

            BoxCollider collider = EnsureComponent<BoxCollider>(checkpoint);
            collider.isTrigger = true;
            collider.size = new Vector3(10f, 4f, 2f);

            Checkpoint checkpointComponent = EnsureComponent<Checkpoint>(checkpoint);
            SetSerializedInt(checkpointComponent, "checkpointIndex", index);

            if (checkpoint.transform.childCount == 0)
            {
                GameObject visual = EnsurePrimitiveChild(checkpoint.transform, "Marker", PrimitiveType.Cube);
                visual.transform.localPosition = new Vector3(0f, 1.4f, 0f);
                visual.transform.localScale = new Vector3(10f, 2.8f, 0.25f);

                Collider visualCollider = visual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualCollider);
                }
            }

            Renderer markerRenderer = checkpoint.GetComponentInChildren<Renderer>();
            if (markerRenderer != null)
            {
                SetSerializedReference(checkpointComponent, "indicatorRenderer", markerRenderer);
            }

            return checkpointComponent;
        }

        private static void CreateCollectible(string name, Vector3 pos, Scene scene)
        {
            GameObject collectible = EnsureRootObject(scene, name);
            collectible.transform.position = pos;
            collectible.transform.rotation = Quaternion.identity;
            TrySetTag(collectible, GameConstants.TAG_COLLECTIBLE);

            SphereCollider trigger = EnsureComponent<SphereCollider>(collectible);
            trigger.isTrigger = true;
            trigger.radius = 0.6f;

            Collectible collectibleComponent = EnsureComponent<Collectible>(collectible);
            SetSerializedInt(collectibleComponent, "currencyAmount", 10);
            SetSerializedFloat(collectibleComponent, "restoreAmount", 10f);

            if (collectible.transform.childCount == 0)
            {
                GameObject visual = EnsurePrimitiveChild(collectible.transform, "Visual", PrimitiveType.Sphere);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = Vector3.one;

                Collider collider = visual.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }
        }

        private static ParticleSystem CreateParticleChild(Transform parent, string name, Vector3 localPos)
        {
            GameObject child = EnsureChild(parent, name);
            child.transform.localPosition = localPos;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            ParticleSystem particle = EnsureComponent<ParticleSystem>(child);
            ParticleSystem.MainModule main = particle.main;
            main.loop = true;
            main.startLifetime = 0.45f;
            main.startSpeed = 1.3f;
            main.startSize = 0.2f;

            ParticleSystem.EmissionModule emission = particle.emission;
            emission.rateOverTime = 0f;

            return particle;
        }

        private static GameObject CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject bar = EnsureUIChild(parent, name);
            RectTransform rect = bar.GetComponent<RectTransform>();
            SetupRect(rect, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), size, anchoredPos, anchoredPos);
            Image bg = EnsureComponent<Image>(bar);
            bg.color = color;
            return bar;
        }

        private static Image CreateBarFill(Transform parent, string name, Color color)
        {
            GameObject fillObj = EnsureUIChild(parent, name);
            RectTransform rect = fillObj.GetComponent<RectTransform>();
            SetupRect(rect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);

            Image fill = EnsureComponent<Image>(fillObj);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            fill.color = color;
            return fill;
        }

        private static void SetupRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredMin,
            Vector2 anchoredMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = (anchoredMin + anchoredMax) * 0.5f;
        }

        private static Material GetOrCreateEnvironmentGroundMaterial(EnvironmentType environmentType)
        {
            string folder = "Assets/Materials/Generated";
            EnsureFolder(folder);

            string materialPath = $"{folder}/{environmentType}_Ground.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Standard");
                material = new Material(shader == null ? Shader.Find("Diffuse") : shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            switch (environmentType)
            {
                case EnvironmentType.Lava:
                    material.color = new Color(0.30f, 0.12f, 0.08f);
                    break;
                case EnvironmentType.Ice:
                    material.color = new Color(0.15f, 0.28f, 0.40f);
                    break;
                case EnvironmentType.Toxic:
                    material.color = new Color(0.12f, 0.22f, 0.12f);
                    break;
                default:
                    material.color = new Color(0.2f, 0.2f, 0.2f);
                    break;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static CourseDataSO[] LoadAllCourseAssets()
        {
            List<CourseDataSO> results = new List<CourseDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:CourseDataSO", new[] { CoursesRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                CourseDataSO asset = AssetDatabase.LoadAssetAtPath<CourseDataSO>(path);
                if (asset != null)
                {
                    results.Add(asset);
                }
            }

            return results.ToArray();
        }

        private static CourseDataSO FindCourseBySceneName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            CourseDataSO[] courses = LoadAllCourseAssets();
            for (int i = 0; i < courses.Length; i++)
            {
                CourseDataSO course = courses[i];
                if (course != null && course.sceneName == sceneName)
                {
                    return course;
                }
            }

            return null;
        }

        private static Color ParseHex(string hex, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return fallback;
            }

            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : fallback;
        }

        private static void SetSerializedReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty prop = serialized.FindProperty(propertyName);
            if (prop == null)
            {
                return;
            }

            prop.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedReference(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty prop = serialized.FindProperty(propertyName);
            if (prop == null || !prop.isArray)
            {
                return;
            }

            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedArrayReference(UnityEngine.Object target, string propertyName, Transform[] values)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty prop = serialized.FindProperty(propertyName);
            if (prop == null || !prop.isArray)
            {
                return;
            }

            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedInt(UnityEngine.Object target, string propertyName, int value)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty prop = serialized.FindProperty(propertyName);
            if (prop == null)
            {
                return;
            }

            prop.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedFloat(UnityEngine.Object target, string propertyName, float value)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty prop = serialized.FindProperty(propertyName);
            if (prop == null)
            {
                return;
            }

            prop.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedBool(UnityEngine.Object target, string propertyName, bool value)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty prop = serialized.FindProperty(propertyName);
            if (prop == null)
            {
                return;
            }

            prop.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static UpgradeLevel Level(int cost, float multiplier)
        {
            return new UpgradeLevel
            {
                cost = cost,
                statMultiplier = multiplier,
                description = $"x{multiplier:0.##}"
            };
        }

        private static UpgradeLevel[] CreateLevels(params UpgradeLevel[] levels)
        {
            return levels;
        }

        private static StatModifier Modifier(string statName, float value)
        {
            return new StatModifier
            {
                statName = statName,
                valuePerLevel = value
            };
        }

        private static StatModifier[] CreateModifiers(params StatModifier[] modifiers)
        {
            return modifiers;
        }
    }
}
#endif
