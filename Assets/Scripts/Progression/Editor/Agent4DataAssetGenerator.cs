#if UNITY_EDITOR
using System;
using MetalPod.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Progression.Editor
{
    public static class Agent4DataAssetGenerator
    {
        private const string DataRoot = "Assets/ScriptableObjects/Data";

        [MenuItem("MetalPod/Progression/Generate Agent 4 Data Assets")]
        public static void Generate()
        {
            EnsureFolder(DataRoot);
            EnsureFolder(DataRoot + "/Courses");
            EnsureFolder(DataRoot + "/Environments");
            EnsureFolder(DataRoot + "/Upgrades");
            EnsureFolder(DataRoot + "/Cosmetics");
            EnsureFolder(DataRoot + "/Hazards");

            CreateHovercraftDefaults();
            CreateGameConfig();
            CourseDataSO[] courses = CreateCourses();
            CreateEnvironments(courses);
            CreateUpgrades();
            CreateCosmetics();
            CreateHazards();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Agent 4 data assets generated/updated.");
        }

        private static void CreateHovercraftDefaults()
        {
            HovercraftStatsSO stats = CreateOrLoad<HovercraftStatsSO>(DataRoot + "/DefaultHovercraftStats.asset");
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
            GameConfigSO config = CreateOrLoad<GameConfigSO>(DataRoot + "/GameConfig.asset");
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

        private static CourseDataSO[] CreateCourses()
        {
            CourseDataSO lava01 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Lava_Course_01.asset");
            CourseDataSO lava02 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Lava_Course_02.asset");
            CourseDataSO lava03 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Lava_Course_03.asset");
            CourseDataSO ice01 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Ice_Course_01.asset");
            CourseDataSO ice02 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Ice_Course_02.asset");
            CourseDataSO ice03 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Ice_Course_03.asset");
            CourseDataSO toxic01 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Toxic_Course_01.asset");
            CourseDataSO toxic02 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Toxic_Course_02.asset");
            CourseDataSO toxic03 = CreateOrLoad<CourseDataSO>(DataRoot + "/Courses/Toxic_Course_03.asset");

            ConfigureCourse(lava01, "lava_01", "Inferno Gate", EnvironmentType.Lava, 0, "Lava_Course_01", 50f, 65f, 80f, 0, null, DifficultyLevel.Easy);
            ConfigureCourse(lava02, "lava_02", "Magma Run", EnvironmentType.Lava, 1, "Lava_Course_02", 70f, 90f, 110f, 0, lava01, DifficultyLevel.Medium);
            ConfigureCourse(lava03, "lava_03", "Eruption", EnvironmentType.Lava, 2, "Lava_Course_03", 90f, 120f, 150f, 3, lava02, DifficultyLevel.Hard);

            ConfigureCourse(ice01, "ice_01", "Frozen Lake", EnvironmentType.Ice, 0, "Ice_Course_01", 60f, 80f, 100f, 5, lava03, DifficultyLevel.Medium);
            ConfigureCourse(ice02, "ice_02", "Crystal Caverns", EnvironmentType.Ice, 1, "Ice_Course_02", 80f, 105f, 130f, 7, ice01, DifficultyLevel.Hard);
            ConfigureCourse(ice03, "ice_03", "Avalanche Pass", EnvironmentType.Ice, 2, "Ice_Course_03", 100f, 130f, 160f, 9, ice02, DifficultyLevel.Extreme);

            ConfigureCourse(toxic01, "toxic_01", "Waste Disposal", EnvironmentType.Toxic, 0, "Toxic_Course_01", 65f, 85f, 105f, 12, ice03, DifficultyLevel.Medium);
            ConfigureCourse(toxic02, "toxic_02", "The Foundry", EnvironmentType.Toxic, 1, "Toxic_Course_02", 85f, 110f, 140f, 15, toxic01, DifficultyLevel.Hard);
            ConfigureCourse(toxic03, "toxic_03", "Meltdown", EnvironmentType.Toxic, 2, "Toxic_Course_03", 110f, 145f, 180f, 18, toxic02, DifficultyLevel.Extreme);

            return new[] { lava01, lava02, lava03, ice01, ice02, ice03, toxic01, toxic02, toxic03 };
        }

        private static void CreateEnvironments(CourseDataSO[] courses)
        {
            CourseDataSO lava01 = FindCourse(courses, "lava_01");
            CourseDataSO lava02 = FindCourse(courses, "lava_02");
            CourseDataSO lava03 = FindCourse(courses, "lava_03");
            CourseDataSO ice01 = FindCourse(courses, "ice_01");
            CourseDataSO ice02 = FindCourse(courses, "ice_02");
            CourseDataSO ice03 = FindCourse(courses, "ice_03");
            CourseDataSO toxic01 = FindCourse(courses, "toxic_01");
            CourseDataSO toxic02 = FindCourse(courses, "toxic_02");
            CourseDataSO toxic03 = FindCourse(courses, "toxic_03");

            EnvironmentDataSO lava = CreateOrLoad<EnvironmentDataSO>(DataRoot + "/Environments/LavaEnvironment.asset");
            lava.environmentId = "lava";
            lava.environmentName = "Lava";
            lava.environmentType = EnvironmentType.Lava;
            lava.description = "Volcanic courses with intense heat and eruptive hazards.";
            lava.primaryColor = Hex("#A32900");
            lava.secondaryColor = Hex("#2A1A14");
            lava.requiredMedalsToUnlock = 0;
            lava.courses = new[] { lava01, lava02, lava03 };
            EditorUtility.SetDirty(lava);

            EnvironmentDataSO ice = CreateOrLoad<EnvironmentDataSO>(DataRoot + "/Environments/IceEnvironment.asset");
            ice.environmentId = "ice";
            ice.environmentName = "Ice";
            ice.environmentType = EnvironmentType.Ice;
            ice.description = "Frozen courses with reduced traction and visibility hazards.";
            ice.primaryColor = Hex("#7FB2D9");
            ice.secondaryColor = Hex("#1E2A3A");
            ice.requiredMedalsToUnlock = 5;
            ice.courses = new[] { ice01, ice02, ice03 };
            EditorUtility.SetDirty(ice);

            EnvironmentDataSO toxic = CreateOrLoad<EnvironmentDataSO>(DataRoot + "/Environments/ToxicEnvironment.asset");
            toxic.environmentId = "toxic";
            toxic.environmentName = "Toxic";
            toxic.environmentType = EnvironmentType.Toxic;
            toxic.description = "Industrial wasteland courses with corrosive and mechanical hazards.";
            toxic.primaryColor = Hex("#3A8F2C");
            toxic.secondaryColor = Hex("#2B241D");
            toxic.requiredMedalsToUnlock = 12;
            toxic.courses = new[] { toxic01, toxic02, toxic03 };
            EditorUtility.SetDirty(toxic);
        }

        private static void CreateUpgrades()
        {
            UpgradeDataSO speed = CreateOrLoad<UpgradeDataSO>(DataRoot + "/Upgrades/Upgrade_Speed.asset");
            speed.upgradeId = "speed";
            speed.upgradeName = "Thruster Output";
            speed.category = UpgradeCategory.Speed;
            speed.levels = new[]
            {
                Level(100, 1.10f, "+10% max speed"),
                Level(250, 1.20f, "+20% max speed"),
                Level(500, 1.30f, "+30% max speed"),
                Level(1000, 1.40f, "+40% max speed"),
                Level(2000, 1.50f, "+50% max speed")
            };
            speed.statModifiers = new[] { Modifier("maxSpeed", 0.10f) };
            EditorUtility.SetDirty(speed);

            UpgradeDataSO handling = CreateOrLoad<UpgradeDataSO>(DataRoot + "/Upgrades/Upgrade_Handling.asset");
            handling.upgradeId = "handling";
            handling.upgradeName = "Gyro Stabilizers";
            handling.category = UpgradeCategory.Handling;
            handling.levels = new[]
            {
                Level(100, 1.08f, "+8% handling"),
                Level(250, 1.16f, "+16% handling"),
                Level(500, 1.24f, "+24% handling"),
                Level(1000, 1.32f, "+32% handling"),
                Level(2000, 1.40f, "+40% handling")
            };
            handling.statModifiers = new[] { Modifier("handling", 0.08f) };
            EditorUtility.SetDirty(handling);

            UpgradeDataSO shield = CreateOrLoad<UpgradeDataSO>(DataRoot + "/Upgrades/Upgrade_Shield.asset");
            shield.upgradeId = "shield";
            shield.upgradeName = "Energy Barrier";
            shield.category = UpgradeCategory.Shield;
            shield.levels = new[]
            {
                Level(150, 1.12f, "+12% shield capacity"),
                Level(350, 1.24f, "+24% shield capacity"),
                Level(600, 1.36f, "+36% shield capacity"),
                Level(1200, 1.48f, "+48% shield capacity"),
                Level(2500, 1.60f, "+60% shield capacity")
            };
            shield.statModifiers = new[] { Modifier("maxShield", 0.12f) };
            EditorUtility.SetDirty(shield);

            UpgradeDataSO boost = CreateOrLoad<UpgradeDataSO>(DataRoot + "/Upgrades/Upgrade_Boost.asset");
            boost.upgradeId = "boost";
            boost.upgradeName = "Nitro Injector";
            boost.category = UpgradeCategory.Boost;
            boost.levels = new[]
            {
                Level(100, 1.10f, "+10% boost power"),
                Level(250, 1.20f, "+20% boost power"),
                Level(500, 1.30f, "+30% boost power"),
                Level(1000, 1.40f, "+40% boost power"),
                Level(2000, 1.50f, "+50% boost power")
            };
            boost.statModifiers = new[] { Modifier("boost", 0.10f) };
            EditorUtility.SetDirty(boost);
        }

        private static void CreateCosmetics()
        {
            CosmeticDataSO colorDefault = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Color_Default.asset");
            ConfigureColorCosmetic(colorDefault, "default", "Hazard Yellow", 0, "Default scheme", "#D4A017", "#2A2A2A", "#FF8800");

            CosmeticDataSO colorRedBlack = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Color_RedBlack.asset");
            ConfigureColorCosmetic(colorRedBlack, "red_black", "Hellfire", 200, "Aggressive red and black scheme.", "#CC0000", "#1A1A1A", "#FF4400");

            CosmeticDataSO colorBlueSilver = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Color_BlueSilver.asset");
            ConfigureColorCosmetic(colorBlueSilver, "blue_silver", "Frost Runner", 200, "Cool blue and silver scheme.", "#2244AA", "#C0C0C0", "#00AAFF");

            CosmeticDataSO colorGreenBlack = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Color_GreenBlack.asset");
            ConfigureColorCosmetic(colorGreenBlack, "green_black", "Toxic Avenger", 200, "Toxic green and black scheme.", "#228B22", "#1A1A1A", "#44FF00");

            CosmeticDataSO colorChrome = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Color_Chrome.asset");
            ConfigureColorCosmetic(colorChrome, "chrome", "Chrome Crusher", 500, "Premium chrome finish.", "#E0E0E0", "#808080", "#FFFFFF");

            CosmeticDataSO decalSkull = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Decal_Skull.asset");
            ConfigureDecalCosmetic(decalSkull, "decal_skull", "Death's Head", 100);

            CosmeticDataSO decalFlames = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Decal_Flames.asset");
            ConfigureDecalCosmetic(decalFlames, "decal_flames", "Hellfire", 100);

            CosmeticDataSO decalLightning = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Decal_Lightning.asset");
            ConfigureDecalCosmetic(decalLightning, "decal_lightning", "Thunder Strike", 150);

            CosmeticDataSO decalStripes = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Decal_RacingStripes.asset");
            ConfigureDecalCosmetic(decalStripes, "decal_stripes", "Speed Demon", 75);

            CosmeticDataSO decal73 = CreateOrLoad<CosmeticDataSO>(DataRoot + "/Cosmetics/Decal_Number73.asset");
            ConfigureDecalCosmetic(decal73, "decal_73", "Lucky 73", 0);
        }

        private static void CreateHazards()
        {
            ConfigureHazard("Hazard_LavaFlow", "lava_flow", "Lava Flow", 20f, 10f, 18f, 2);
            ConfigureHazard("Hazard_Eruption", "eruption", "Volcanic Eruption", 0f, 30f, 25f, 3);
            ConfigureHazard("Hazard_Geyser", "lava_geyser", "Lava Geyser", 0f, 25f, 20f, 3);
            ConfigureHazard("Hazard_FallingDebris", "falling_debris", "Falling Debris", 0f, 20f, 20f, 2);
            ConfigureHazard("Hazard_HeatZone", "heat_zone", "Heat Zone", 12f, 5f, 16f, 2);

            ConfigureHazard("Hazard_IcePatch", "ice_patch", "Ice Patch", 0f, 0f, 12f, 1);
            ConfigureHazard("Hazard_Icicle", "falling_icicle", "Falling Icicle", 0f, 18f, 16f, 2);
            ConfigureHazard("Hazard_Blizzard", "blizzard", "Blizzard", 4f, 0f, 24f, 2);

            ConfigureHazard("Hazard_ToxicGas", "toxic_gas", "Toxic Gas", 14f, 6f, 18f, 2);
            ConfigureHazard("Hazard_Acid", "acid_pool", "Acid Pool", 22f, 12f, 16f, 3);
            ConfigureHazard("Hazard_Press", "industrial_press", "Industrial Press", 0f, 999f, 14f, 3);
            ConfigureHazard("Hazard_Electric", "electric_fence", "Electric Fence", 10f, 15f, 15f, 2);
            ConfigureHazard("Hazard_Barrel", "barrel_explosion", "Barrel Explosion", 0f, 25f, 14f, 2);
        }

        private static void ConfigureHazard(string assetName, string id, string name, float dps, float hit, float radius, int threat)
        {
            HazardDataSO hazard = CreateOrLoad<HazardDataSO>($"{DataRoot}/Hazards/{assetName}.asset");
            hazard.hazardId = id;
            hazard.hazardName = name;
            hazard.description = name;
            hazard.damagePerSecond = dps;
            hazard.damagePerHit = hit;
            hazard.warningRadius = radius;
            hazard.threatLevel = threat;
            EditorUtility.SetDirty(hazard);
        }

        private static void ConfigureCourse(
            CourseDataSO course,
            string id,
            string name,
            EnvironmentType environmentType,
            int index,
            string sceneName,
            float gold,
            float silver,
            float bronze,
            int requiredMedals,
            CourseDataSO prerequisite,
            DifficultyLevel difficulty)
        {
            course.courseId = id;
            course.courseName = name;
            course.description = name;
            course.environmentType = environmentType;
            course.courseIndex = index;
            course.sceneName = sceneName;
            course.goldTime = gold;
            course.silverTime = silver;
            course.bronzeTime = bronze;
            course.requiredMedals = requiredMedals;
            course.prerequisiteCourse = prerequisite;
            course.difficulty = difficulty;
            EditorUtility.SetDirty(course);
        }

        private static void ConfigureColorCosmetic(
            CosmeticDataSO cosmetic,
            string id,
            string name,
            int cost,
            string description,
            string primaryHex,
            string secondaryHex,
            string accentHex)
        {
            cosmetic.cosmeticId = id;
            cosmetic.cosmeticName = name;
            cosmetic.cosmeticType = CosmeticType.ColorScheme;
            cosmetic.cost = cost;
            cosmetic.requiredMedals = 0;
            cosmetic.description = description;
            cosmetic.primaryColor = Hex(primaryHex);
            cosmetic.secondaryColor = Hex(secondaryHex);
            cosmetic.accentColor = Hex(accentHex);
            EditorUtility.SetDirty(cosmetic);
        }

        private static void ConfigureDecalCosmetic(CosmeticDataSO cosmetic, string id, string name, int cost)
        {
            cosmetic.cosmeticId = id;
            cosmetic.cosmeticName = name;
            cosmetic.cosmeticType = CosmeticType.Decal;
            cosmetic.cost = cost;
            cosmetic.requiredMedals = 0;
            cosmetic.description = name;
            EditorUtility.SetDirty(cosmetic);
        }

        private static UpgradeLevel Level(int cost, float multiplier, string description)
        {
            return new UpgradeLevel
            {
                cost = cost,
                statMultiplier = multiplier,
                description = description
            };
        }

        private static StatModifier Modifier(string statName, float valuePerLevel)
        {
            return new StatModifier
            {
                statName = statName,
                valuePerLevel = valuePerLevel
            };
        }

        private static CourseDataSO FindCourse(CourseDataSO[] courses, string courseId)
        {
            for (int i = 0; i < courses.Length; i++)
            {
                CourseDataSO course = courses[i];
                if (course != null && course.courseId == courseId)
                {
                    return course;
                }
            }

            return null;
        }

        private static T CreateOrLoad<T>(string assetPath) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            T instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, assetPath);
            return instance;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string[] parts = path.Split('/');
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

        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            throw new InvalidOperationException($"Invalid color hex: {hex}");
        }
    }
}
#endif
