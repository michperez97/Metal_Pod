using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Achievements
{
    /// <summary>
    /// Runtime state for a single achievement.
    /// </summary>
    public class Achievement
    {
        public AchievementDataSO Definition { get; }
        public bool IsUnlocked { get; set; }
        public int CurrentProgress { get; set; }

        public float ProgressNormalized =>
            Definition != null && Definition.targetValue > 0
                ? Mathf.Clamp01((float)CurrentProgress / Definition.targetValue)
                : (IsUnlocked ? 1f : 0f);

        public bool IsComplete => Definition != null && CurrentProgress >= Definition.targetValue;

        public Achievement(AchievementDataSO definition, bool unlocked = false, int progress = 0)
        {
            Definition = definition;
            IsUnlocked = unlocked;
            CurrentProgress = Mathf.Max(0, progress);
        }
    }
}
