using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    public enum AchievementCategory
    {
        Racing,
        Medals,
        Progression,
        Environment,
        Challenge,
        Hidden
    }

    public enum AchievementConditionType
    {
        CoursesCompleted,
        GoldMedalsEarned,
        TotalMedalsEarned,
        UpgradeLevel,
        AllUpgradesMaxed,
        CosmeticsOwned,
        TotalBoltsEarned,
        CurrentBolts,
        SpecificCoursesCompleted,
        TotalDeaths,
        TotalPlayTime,
        CourseReplays,
        NoDamageCourse,
        LowHealthFinish,
        FastTime,
        MaxSpeedReached,
        TutorialSkipped,
        AllAchievementsUnlocked,
        EnvironmentsVisited,
        SpecificUpgradeMaxed
    }

    [CreateAssetMenu(fileName = "Achievement", menuName = "MetalPod/Achievement")]
    public class AchievementDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string achievementId;
        public string title;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Classification")]
        public AchievementCategory category;
        public bool isHidden;

        [Header("Condition")]
        public AchievementConditionType conditionType;
        [Min(1)] public int targetValue = 1;
        [Tooltip("For course-specific conditions, list course IDs here.")]
        public string[] targetCourseIds;
        [Tooltip("For upgrade-specific conditions, the target upgrade ID.")]
        public string targetUpgradeId;

        [Header("Reward")]
        [Min(0)] public int boltReward;
    }
}
