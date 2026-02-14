#if UNITY_EDITOR
using MetalPod.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Achievements
{
    /// <summary>
    /// Creates/updates all achievement ScriptableObject assets.
    /// </summary>
    public static class AchievementDefinition
    {
        private static readonly string[] LavaCourseIds = { "lava_01", "lava_02", "lava_03" };
        private static readonly string[] IceCourseIds = { "ice_01", "ice_02", "ice_03" };
        private static readonly string[] ToxicCourseIds = { "toxic_01", "toxic_02", "toxic_03" };

        [MenuItem("Metal Pod/Generate Achievement Assets")]
        public static void GenerateAll()
        {
            string directory = "Assets/ScriptableObjects/Achievements";
            EnsureFolder(directory);

            // Racing
            CreateAchievement(directory, "first_finish", "First Finish", "Complete your first course.",
                AchievementCategory.Racing, AchievementConditionType.CoursesCompleted, 1, 50);
            CreateAchievement(directory, "speed_demon", "Speed Demon", "Complete any course in under 60 seconds.",
                AchievementCategory.Racing, AchievementConditionType.FastTime, 1, 100);
            CreateAchievement(directory, "all_courses", "Course Conqueror", "Complete all 9 courses.",
                AchievementCategory.Racing, AchievementConditionType.CoursesCompleted, 9, 400);
            CreateAchievement(directory, "no_damage", "Untouchable", "Complete a course without taking damage.",
                AchievementCategory.Racing, AchievementConditionType.NoDamageCourse, 1, 120);
            CreateAchievement(directory, "close_call", "Close Call", "Finish a course with less than 10% health.",
                AchievementCategory.Racing, AchievementConditionType.LowHealthFinish, 1, 100);

            // Medals
            CreateAchievement(directory, "first_gold", "Golden Start", "Earn your first gold medal.",
                AchievementCategory.Medals, AchievementConditionType.GoldMedalsEarned, 1, 75);
            CreateAchievement(directory, "triple_gold", "Hat Trick", "Earn 3 gold medals.",
                AchievementCategory.Medals, AchievementConditionType.GoldMedalsEarned, 3, 150);
            CreateAchievement(directory, "all_gold", "Perfectionist", "Earn gold on all 9 courses.",
                AchievementCategory.Medals, AchievementConditionType.GoldMedalsEarned, 9, 500);
            CreateAchievement(directory, "first_medal", "Medalist", "Earn any medal.",
                AchievementCategory.Medals, AchievementConditionType.TotalMedalsEarned, 1, 50);
            CreateAchievement(directory, "medal_collector", "Medal Collector", "Earn 15 medals total.",
                AchievementCategory.Medals, AchievementConditionType.TotalMedalsEarned, 15, 300);

            // Progression
            CreateAchievement(directory, "first_upgrade", "Tuning Up", "Purchase your first upgrade.",
                AchievementCategory.Progression, AchievementConditionType.UpgradeLevel, 1, 50);
            CreateAchievement(directory, "max_speed", "Maxed Out Speed", "Max out Speed upgrades.",
                AchievementCategory.Progression, AchievementConditionType.SpecificUpgradeMaxed, 5, 200, false, null, "speed");
            CreateAchievement(directory, "max_all", "Fully Loaded", "Max out all upgrade categories.",
                AchievementCategory.Progression, AchievementConditionType.AllUpgradesMaxed, 4, 400);
            CreateAchievement(directory, "first_cosmetic", "Fashion Statement", "Purchase your first cosmetic.",
                AchievementCategory.Progression, AchievementConditionType.CosmeticsOwned, 1, 75);
            CreateAchievement(directory, "rich_racer", "Rich Racer", "Accumulate 5000 bolts total.",
                AchievementCategory.Progression, AchievementConditionType.TotalBoltsEarned, 5000, 300);

            // Environment
            CreateAchievement(directory, "lava_master", "Lava Lord", "Complete all 3 lava courses.",
                AchievementCategory.Environment, AchievementConditionType.SpecificCoursesCompleted, 3, 150, false, LavaCourseIds);
            CreateAchievement(directory, "ice_master", "Ice King", "Complete all 3 ice courses.",
                AchievementCategory.Environment, AchievementConditionType.SpecificCoursesCompleted, 3, 150, false, IceCourseIds);
            CreateAchievement(directory, "toxic_master", "Toxic Avenger", "Complete all 3 toxic courses.",
                AchievementCategory.Environment, AchievementConditionType.SpecificCoursesCompleted, 3, 150, false, ToxicCourseIds);
            CreateAchievement(directory, "env_explorer", "World Traveler", "Complete at least 1 course in each environment.",
                AchievementCategory.Environment, AchievementConditionType.EnvironmentsVisited, 3, 200);

            // Challenge
            CreateAchievement(directory, "die_hard", "Die Hard", "Get destroyed 10 times total.",
                AchievementCategory.Challenge, AchievementConditionType.TotalDeaths, 10, 125);
            CreateAchievement(directory, "marathon", "Marathon Runner", "Play for 60 minutes total.",
                AchievementCategory.Challenge, AchievementConditionType.TotalPlayTime, 3600, 200);
            CreateAchievement(directory, "replay_king", "Replay King", "Replay any course 5 times.",
                AchievementCategory.Challenge, AchievementConditionType.CourseReplays, 5, 175);
            CreateAchievement(directory, "bolt_hoarder", "Bolt Hoarder", "Have 1000 bolts at once.",
                AchievementCategory.Challenge, AchievementConditionType.CurrentBolts, 1000, 150);

            // Hidden
            CreateAchievement(directory, "speed_max", "Ludicrous Speed", "Reach maximum speed with all upgrades.",
                AchievementCategory.Hidden, AchievementConditionType.MaxSpeedReached, 1, 250, true);
            CreateAchievement(directory, "tutorial_skip", "Impatient", "Skip the tutorial.",
                AchievementCategory.Hidden, AchievementConditionType.TutorialSkipped, 1, 100, true);
            CreateAchievement(directory, "all_achievements", "Completionist", "Unlock all other achievements.",
                AchievementCategory.Hidden, AchievementConditionType.AllAchievementsUnlocked, 24, 1000, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Achievements] Generated all achievement assets.");
        }

        private static void CreateAchievement(
            string directory,
            string id,
            string title,
            string description,
            AchievementCategory category,
            AchievementConditionType conditionType,
            int targetValue,
            int boltReward,
            bool hidden = false,
            string[] targetCourseIds = null,
            string targetUpgradeId = null)
        {
            string path = $"{directory}/Achievement_{id}.asset";
            AchievementDataSO asset = AssetDatabase.LoadAssetAtPath<AchievementDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<AchievementDataSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.achievementId = id;
            asset.title = title;
            asset.description = description;
            asset.category = category;
            asset.conditionType = conditionType;
            asset.targetValue = Mathf.Max(1, targetValue);
            asset.boltReward = Mathf.Max(0, boltReward);
            asset.isHidden = hidden;
            asset.targetCourseIds = targetCourseIds;
            asset.targetUpgradeId = targetUpgradeId;

            EditorUtility.SetDirty(asset);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] segments = folderPath.Split('/');
            if (segments.Length == 0)
            {
                return;
            }

            string currentPath = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string nextPath = $"{currentPath}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }

                currentPath = nextPath;
            }
        }
    }
}
#endif
