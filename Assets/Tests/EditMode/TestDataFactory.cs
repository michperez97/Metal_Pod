using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    internal sealed class SaveFileIsolationScope : IDisposable
    {
        private readonly string _savePath;
        private readonly string _backupPath;

        private readonly bool _hadSave;
        private readonly bool _hadBackup;
        private readonly string _saveContent;
        private readonly string _backupContent;

        public SaveFileIsolationScope()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "metalpod_save.json");
            _backupPath = Path.Combine(Application.persistentDataPath, "metalpod_save_backup.json");

            _hadSave = File.Exists(_savePath);
            _hadBackup = File.Exists(_backupPath);
            _saveContent = _hadSave ? File.ReadAllText(_savePath) : string.Empty;
            _backupContent = _hadBackup ? File.ReadAllText(_backupPath) : string.Empty;

            DeleteIfExists(_savePath);
            DeleteIfExists(_backupPath);
        }

        public void Dispose()
        {
            DeleteIfExists(_savePath);
            DeleteIfExists(_backupPath);

            if (_hadSave)
            {
                File.WriteAllText(_savePath, _saveContent);
            }

            if (_hadBackup)
            {
                File.WriteAllText(_backupPath, _backupContent);
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    internal static class TestDataFactory
    {
        public static SaveSystem CreateInitializedSaveSystem(out GameObject gameObject)
        {
            gameObject = new GameObject("Test_SaveSystem");
            SaveSystem saveSystem = gameObject.AddComponent<SaveSystem>();
            saveSystem.Initialize();
            return saveSystem;
        }

        public static CourseDataSO CreateCourse(
            string courseId,
            float gold,
            float silver,
            float bronze,
            DifficultyLevel difficulty = DifficultyLevel.Easy,
            EnvironmentType environment = EnvironmentType.Lava,
            int index = 0,
            int requiredMedals = 0,
            CourseDataSO prerequisite = null)
        {
            CourseDataSO course = ScriptableObject.CreateInstance<CourseDataSO>();
            course.courseId = courseId;
            course.courseName = courseId;
            course.sceneName = $"Scene_{courseId}";
            course.goldTime = gold;
            course.silverTime = silver;
            course.bronzeTime = bronze;
            course.difficulty = difficulty;
            course.environmentType = environment;
            course.courseIndex = index;
            course.requiredMedals = requiredMedals;
            course.prerequisiteCourse = prerequisite;
            return course;
        }

        public static UpgradeDataSO CreateUpgrade(string id, UpgradeCategory category, int[] costs, float[] multipliers)
        {
            UpgradeDataSO upgrade = ScriptableObject.CreateInstance<UpgradeDataSO>();
            upgrade.upgradeId = id;
            upgrade.upgradeName = id;
            upgrade.category = category;
            upgrade.levels = new UpgradeLevel[costs.Length];
            for (int i = 0; i < costs.Length; i++)
            {
                upgrade.levels[i] = new UpgradeLevel
                {
                    cost = costs[i],
                    statMultiplier = multipliers != null && i < multipliers.Length ? multipliers[i] : 1f,
                    description = $"{id}-lvl-{i + 1}"
                };
            }

            return upgrade;
        }

        public static UpgradeDataSO[] CreateDefaultUpgrades()
        {
            return new[]
            {
                CreateUpgrade("speed", UpgradeCategory.Speed, new[] { 100, 250, 500, 1000, 2000 }, new[] { 1.1f, 1.2f, 1.3f, 1.4f, 1.5f }),
                CreateUpgrade("handling", UpgradeCategory.Handling, new[] { 100, 250, 500, 1000, 2000 }, new[] { 1.08f, 1.16f, 1.24f, 1.32f, 1.4f }),
                CreateUpgrade("shield", UpgradeCategory.Shield, new[] { 150, 350, 600, 1200, 2500 }, new[] { 1.12f, 1.24f, 1.36f, 1.48f, 1.6f }),
                CreateUpgrade("boost", UpgradeCategory.Boost, new[] { 100, 250, 500, 1000, 2000 }, new[] { 1.1f, 1.2f, 1.3f, 1.4f, 1.5f })
            };
        }

        public static CosmeticDataSO CreateCosmetic(
            string id,
            CosmeticType type,
            int cost,
            int requiredMedals = 0)
        {
            CosmeticDataSO cosmetic = ScriptableObject.CreateInstance<CosmeticDataSO>();
            cosmetic.cosmeticId = id;
            cosmetic.cosmeticName = id;
            cosmetic.cosmeticType = type;
            cosmetic.cost = cost;
            cosmetic.requiredMedals = requiredMedals;
            return cosmetic;
        }

        public static CosmeticDataSO[] CreateDefaultCosmetics()
        {
            return new[]
            {
                CreateCosmetic("default", CosmeticType.ColorScheme, 0),
                CreateCosmetic("decal_73", CosmeticType.Decal, 0),
                CreateCosmetic("red_black", CosmeticType.ColorScheme, 200),
                CreateCosmetic("chrome", CosmeticType.ColorScheme, 500, 5),
                CreateCosmetic("flame_decal", CosmeticType.Decal, 150),
                CreateCosmetic("spoiler_mk2", CosmeticType.Part, 300)
            };
        }

        public static Dictionary<string, CourseDataSO> CreateProgressionCourseMap()
        {
            CourseDataSO lava01 = CreateCourse("lava_01", 50f, 65f, 80f, DifficultyLevel.Easy, EnvironmentType.Lava, 0, 0, null);
            CourseDataSO lava02 = CreateCourse("lava_02", 70f, 90f, 110f, DifficultyLevel.Medium, EnvironmentType.Lava, 1, 0, lava01);
            CourseDataSO lava03 = CreateCourse("lava_03", 90f, 120f, 150f, DifficultyLevel.Hard, EnvironmentType.Lava, 2, 3, lava02);

            CourseDataSO ice01 = CreateCourse("ice_01", 60f, 80f, 100f, DifficultyLevel.Medium, EnvironmentType.Ice, 0, 5, lava03);
            CourseDataSO ice02 = CreateCourse("ice_02", 80f, 105f, 130f, DifficultyLevel.Hard, EnvironmentType.Ice, 1, 7, ice01);
            CourseDataSO ice03 = CreateCourse("ice_03", 100f, 130f, 160f, DifficultyLevel.Extreme, EnvironmentType.Ice, 2, 9, ice02);

            CourseDataSO toxic01 = CreateCourse("toxic_01", 65f, 85f, 105f, DifficultyLevel.Hard, EnvironmentType.Toxic, 0, 12, ice03);
            CourseDataSO toxic02 = CreateCourse("toxic_02", 85f, 110f, 140f, DifficultyLevel.Extreme, EnvironmentType.Toxic, 1, 15, toxic01);
            CourseDataSO toxic03 = CreateCourse("toxic_03", 110f, 145f, 180f, DifficultyLevel.Extreme, EnvironmentType.Toxic, 2, 18, toxic02);

            return new Dictionary<string, CourseDataSO>
            {
                { lava01.courseId, lava01 },
                { lava02.courseId, lava02 },
                { lava03.courseId, lava03 },
                { ice01.courseId, ice01 },
                { ice02.courseId, ice02 },
                { ice03.courseId, ice03 },
                { toxic01.courseId, toxic01 },
                { toxic02.courseId, toxic02 },
                { toxic03.courseId, toxic03 }
            };
        }

        public static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().FullName, fieldName);
            }

            field.SetValue(target, value);
        }

        public static object InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(target.GetType().FullName, methodName);
            }

            return method.Invoke(target, args);
        }

        public static void DestroyAll(params UnityEngine.Object[] objects)
        {
            if (objects == null)
            {
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(objects[i]);
                }
            }
        }
    }
}
