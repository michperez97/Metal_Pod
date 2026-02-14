using System;
using System.Collections.Generic;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Achievements
{
    public readonly struct AchievementEvaluationContext
    {
        public readonly bool NoDamageCourseCompleted;
        public readonly bool LowHealthFinishCompleted;
        public readonly bool FastFinishCompleted;
        public readonly bool MaxSpeedReached;
        public readonly bool TutorialSkipped;
        public readonly int MaxCourseReplayCount;
        public readonly int UnlockedAchievementsWithoutMeta;
        public readonly int TotalAchievementsWithoutMeta;

        public AchievementEvaluationContext(
            bool noDamageCourseCompleted,
            bool lowHealthFinishCompleted,
            bool fastFinishCompleted,
            bool maxSpeedReached,
            bool tutorialSkipped,
            int maxCourseReplayCount,
            int unlockedAchievementsWithoutMeta,
            int totalAchievementsWithoutMeta)
        {
            NoDamageCourseCompleted = noDamageCourseCompleted;
            LowHealthFinishCompleted = lowHealthFinishCompleted;
            FastFinishCompleted = fastFinishCompleted;
            MaxSpeedReached = maxSpeedReached;
            TutorialSkipped = tutorialSkipped;
            MaxCourseReplayCount = Mathf.Max(0, maxCourseReplayCount);
            UnlockedAchievementsWithoutMeta = Mathf.Max(0, unlockedAchievementsWithoutMeta);
            TotalAchievementsWithoutMeta = Mathf.Max(0, totalAchievementsWithoutMeta);
        }
    }

    /// <summary>
    /// Evaluates achievement conditions against current game state.
    /// Pure logic only.
    /// </summary>
    public static class AchievementCondition
    {
        private const int GoldMedalValue = 3;
        private const int DefaultMaxUpgradeLevel = 5;
        private const float SpeedDemonSeconds = 60f;

        private static readonly string[] CoreUpgradeIds = { "speed", "handling", "shield", "boost" };
        private static readonly int[] StandardUpgradeCosts = { 100, 250, 500, 1000, 2000 };
        private static readonly int[] ShieldUpgradeCosts = { 150, 350, 600, 1200, 2500 };

        public static int EvaluateProgress(AchievementDataSO definition, SaveData saveData, ProgressionManager progressionManager)
        {
            return EvaluateProgress(definition, saveData, progressionManager, default(AchievementEvaluationContext));
        }

        public static int EvaluateProgress(
            AchievementDataSO definition,
            SaveData saveData,
            ProgressionManager progressionManager,
            AchievementEvaluationContext context)
        {
            if (definition == null || saveData == null)
            {
                return 0;
            }

            switch (definition.conditionType)
            {
                case AchievementConditionType.CoursesCompleted:
                    return saveData.totalCoursesCompleted;

                case AchievementConditionType.GoldMedalsEarned:
                    return CountGoldMedals(saveData);

                case AchievementConditionType.TotalMedalsEarned:
                    return Mathf.Max(saveData.totalMedals, saveData.GetTotalMedals());

                case AchievementConditionType.UpgradeLevel:
                    return EvaluateUpgradeLevel(definition, saveData);

                case AchievementConditionType.AllUpgradesMaxed:
                    return CountMaxedUpgrades(saveData, progressionManager);

                case AchievementConditionType.CosmeticsOwned:
                    return CountPurchasedCosmetics(saveData);

                case AchievementConditionType.TotalBoltsEarned:
                    return EstimateTotalBoltsEarned(saveData, progressionManager);

                case AchievementConditionType.CurrentBolts:
                    return Mathf.Max(0, saveData.currency);

                case AchievementConditionType.SpecificCoursesCompleted:
                    return CountSpecificCoursesCompleted(saveData, definition.targetCourseIds);

                case AchievementConditionType.TotalDeaths:
                    return Mathf.Max(0, saveData.totalDeaths);

                case AchievementConditionType.TotalPlayTime:
                    return Mathf.FloorToInt(Mathf.Max(0f, saveData.totalPlayTime));

                case AchievementConditionType.CourseReplays:
                    return Mathf.Max(0, context.MaxCourseReplayCount);

                case AchievementConditionType.NoDamageCourse:
                    return context.NoDamageCourseCompleted ? 1 : 0;

                case AchievementConditionType.LowHealthFinish:
                    return context.LowHealthFinishCompleted ? 1 : 0;

                case AchievementConditionType.FastTime:
                    return context.FastFinishCompleted || HasFastBestTime(saveData) ? 1 : 0;

                case AchievementConditionType.MaxSpeedReached:
                    return context.MaxSpeedReached ? 1 : 0;

                case AchievementConditionType.TutorialSkipped:
                    return context.TutorialSkipped ? 1 : 0;

                case AchievementConditionType.AllAchievementsUnlocked:
                    return context.UnlockedAchievementsWithoutMeta;

                case AchievementConditionType.EnvironmentsVisited:
                    return CountEnvironmentsVisited(saveData);

                case AchievementConditionType.SpecificUpgradeMaxed:
                    return GetUpgradeLevel(saveData, definition.targetUpgradeId);

                default:
                    return 0;
            }
        }

        private static int CountGoldMedals(SaveData data)
        {
            if (data.bestMedals == null)
            {
                return 0;
            }

            int count = 0;
            foreach (KeyValuePair<string, int> entry in data.bestMedals)
            {
                if (entry.Value >= GoldMedalValue)
                {
                    count++;
                }
            }

            return count;
        }

        private static int EvaluateUpgradeLevel(AchievementDataSO definition, SaveData data)
        {
            if (!string.IsNullOrWhiteSpace(definition.targetUpgradeId))
            {
                return GetUpgradeLevel(data, definition.targetUpgradeId);
            }

            int maxLevel = 0;
            for (int i = 0; i < CoreUpgradeIds.Length; i++)
            {
                maxLevel = Mathf.Max(maxLevel, GetUpgradeLevel(data, CoreUpgradeIds[i]));
            }

            return maxLevel;
        }

        private static int CountMaxedUpgrades(SaveData data, ProgressionManager progressionManager)
        {
            int count = 0;
            for (int i = 0; i < CoreUpgradeIds.Length; i++)
            {
                string upgradeId = CoreUpgradeIds[i];
                int currentLevel = GetUpgradeLevel(data, upgradeId);
                int maxLevel = GetMaxUpgradeLevel(progressionManager, upgradeId);
                if (currentLevel >= maxLevel)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountPurchasedCosmetics(SaveData data)
        {
            if (data.ownedCosmetics == null || data.ownedCosmetics.Count == 0)
            {
                return 0;
            }

            int count = data.ownedCosmetics.Count;
            if (data.ownedCosmetics.Contains("default"))
            {
                count--;
            }

            if (data.ownedCosmetics.Contains("decal_73"))
            {
                count--;
            }

            return Mathf.Max(0, count);
        }

        private static int EstimateTotalBoltsEarned(SaveData data, ProgressionManager progressionManager)
        {
            int total = Mathf.Max(0, data.currency);
            total += EstimateUpgradeSpend(data, progressionManager);
            total += EstimateCosmeticSpend(data, progressionManager);
            return Mathf.Max(total, data.currency);
        }

        private static int EstimateUpgradeSpend(SaveData data, ProgressionManager progressionManager)
        {
            HashSet<string> allUpgradeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < CoreUpgradeIds.Length; i++)
            {
                allUpgradeIds.Add(CoreUpgradeIds[i]);
            }

            if (data.upgradeLevels != null)
            {
                foreach (KeyValuePair<string, int> entry in data.upgradeLevels)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Key))
                    {
                        allUpgradeIds.Add(NormalizeUpgradeId(entry.Key));
                    }
                }
            }

            int totalSpend = 0;
            foreach (string upgradeId in allUpgradeIds)
            {
                int level = GetUpgradeLevel(data, upgradeId);
                if (level <= 0)
                {
                    continue;
                }

                totalSpend += EstimateUpgradeCost(upgradeId, level, progressionManager);
            }

            return totalSpend;
        }

        private static int EstimateCosmeticSpend(SaveData data, ProgressionManager progressionManager)
        {
            if (data.ownedCosmetics == null || data.ownedCosmetics.Count == 0)
            {
                return 0;
            }

            int total = 0;
            CosmeticManager cosmeticManager = progressionManager != null ? progressionManager.Cosmetics : null;
            if (cosmeticManager != null)
            {
                for (int i = 0; i < data.ownedCosmetics.Count; i++)
                {
                    string cosmeticId = data.ownedCosmetics[i];
                    if (string.IsNullOrWhiteSpace(cosmeticId))
                    {
                        continue;
                    }

                    CosmeticDataSO cosmetic = cosmeticManager.GetCosmeticData(cosmeticId);
                    if (cosmetic != null)
                    {
                        total += Mathf.Max(0, cosmetic.cost);
                    }
                }
            }

            return total;
        }

        private static int CountSpecificCoursesCompleted(SaveData data, string[] courseIds)
        {
            if (data.completedCourses == null)
            {
                return 0;
            }

            if (courseIds == null || courseIds.Length == 0)
            {
                return data.totalCoursesCompleted;
            }

            int completed = 0;
            for (int i = 0; i < courseIds.Length; i++)
            {
                string courseId = courseIds[i];
                if (string.IsNullOrWhiteSpace(courseId))
                {
                    continue;
                }

                if (data.completedCourses.GetValueOrDefault(courseId, false))
                {
                    completed++;
                }
            }

            return completed;
        }

        private static int CountEnvironmentsVisited(SaveData data)
        {
            if (data.completedCourses == null)
            {
                return 0;
            }

            bool lava = false;
            bool ice = false;
            bool toxic = false;

            foreach (KeyValuePair<string, bool> entry in data.completedCourses)
            {
                if (!entry.Value || string.IsNullOrWhiteSpace(entry.Key))
                {
                    continue;
                }

                if (StartsWithIgnoreCase(entry.Key, "lava"))
                {
                    lava = true;
                }
                else if (StartsWithIgnoreCase(entry.Key, "ice"))
                {
                    ice = true;
                }
                else if (StartsWithIgnoreCase(entry.Key, "toxic"))
                {
                    toxic = true;
                }
            }

            return (lava ? 1 : 0) + (ice ? 1 : 0) + (toxic ? 1 : 0);
        }

        private static bool HasFastBestTime(SaveData data)
        {
            if (data.bestTimes == null)
            {
                return false;
            }

            foreach (KeyValuePair<string, float> entry in data.bestTimes)
            {
                if (entry.Value > 0f && entry.Value < SpeedDemonSeconds)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetUpgradeLevel(SaveData data, string upgradeId)
        {
            string normalized = NormalizeUpgradeId(upgradeId);
            if (string.IsNullOrEmpty(normalized) || data.upgradeLevels == null)
            {
                return 0;
            }

            int direct = data.upgradeLevels.GetValueOrDefault(normalized, 0);
            if (normalized != "shield")
            {
                return direct;
            }

            int legacyArmor = data.upgradeLevels.GetValueOrDefault("armor", 0);
            return Mathf.Max(direct, legacyArmor);
        }

        private static int GetMaxUpgradeLevel(ProgressionManager progressionManager, string upgradeId)
        {
            UpgradeManager upgradeManager = progressionManager != null ? progressionManager.Upgrades : null;
            if (upgradeManager == null)
            {
                return DefaultMaxUpgradeLevel;
            }

            int maxLevel = upgradeManager.GetMaxLevel(upgradeId);
            return maxLevel > 0 ? maxLevel : DefaultMaxUpgradeLevel;
        }

        private static int EstimateUpgradeCost(string upgradeId, int level, ProgressionManager progressionManager)
        {
            int clampedLevel = Mathf.Max(0, level);
            if (clampedLevel <= 0)
            {
                return 0;
            }

            UpgradeManager upgradeManager = progressionManager != null ? progressionManager.Upgrades : null;
            UpgradeDataSO upgradeData = upgradeManager != null ? upgradeManager.GetUpgradeData(upgradeId) : null;
            if (upgradeData != null && upgradeData.levels != null && upgradeData.levels.Length > 0)
            {
                int sum = 0;
                int capped = Mathf.Min(clampedLevel, upgradeData.levels.Length);
                for (int i = 0; i < capped; i++)
                {
                    UpgradeLevel levelData = upgradeData.levels[i];
                    if (levelData != null)
                    {
                        sum += Mathf.Max(0, levelData.cost);
                    }
                }

                return sum;
            }

            int[] fallbackCosts = NormalizeUpgradeId(upgradeId) == "shield"
                ? ShieldUpgradeCosts
                : StandardUpgradeCosts;

            int fallbackSum = 0;
            int fallbackLevel = Mathf.Min(clampedLevel, fallbackCosts.Length);
            for (int i = 0; i < fallbackLevel; i++)
            {
                fallbackSum += fallbackCosts[i];
            }

            return fallbackSum;
        }

        private static string NormalizeUpgradeId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string normalized = raw.Trim().ToLowerInvariant();
            return normalized == "armor" ? "shield" : normalized;
        }

        private static bool StartsWithIgnoreCase(string value, string prefix)
        {
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
