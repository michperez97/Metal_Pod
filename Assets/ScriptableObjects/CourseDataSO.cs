using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CourseData", menuName = "MetalPod/CourseData")]
    public class CourseDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique course ID used for save data and unlock checks.")]
        public string courseId;
        [Tooltip("Display name shown in UI.")]
        public string courseName;

        [Tooltip("Course summary shown in selection panels.")]
        [TextArea]
        public string description;

        [Tooltip("Environment bucket this course belongs to.")]
        public EnvironmentType environmentType;
        [Tooltip("Order index used for sorting within environment.")]
        [Min(0)]
        public int courseIndex;
        [Tooltip("Scene name used by SceneLoader.")]
        public string sceneName;

        [Header("Medal Thresholds (seconds)")]
        [Tooltip("Gold medal threshold. Lower completion time is better.")]
        [Min(1f)]
        public float goldTime = 60f;
        [Tooltip("Silver medal threshold. Must be >= gold time.")]
        [Min(1f)]
        public float silverTime = 80f;
        [Tooltip("Bronze medal threshold. Must be >= silver time.")]
        [Min(1f)]
        public float bronzeTime = 100f;

        [Header("Unlock Requirements")]
        [Tooltip("Total medals required to unlock this course.")]
        [Min(0)]
        public int requiredMedals;
        [Tooltip("Optional prerequisite course that must be completed first.")]
        public CourseDataSO prerequisiteCourse;

        [Header("Preview")]
        [Tooltip("Preview image shown in course cards.")]
        public Sprite previewImage;
        [Tooltip("Short hazard bullet points shown in UI.")]
        public string[] hazardDescriptions;
        [Tooltip("Relative challenge level.")]
        public DifficultyLevel difficulty;

        private void OnValidate()
        {
            courseIndex = Mathf.Max(0, courseIndex);
            requiredMedals = Mathf.Max(0, requiredMedals);

            goldTime = Mathf.Max(1f, goldTime);
            silverTime = Mathf.Max(1f, silverTime);
            bronzeTime = Mathf.Max(1f, bronzeTime);

            if (silverTime < goldTime)
            {
                silverTime = goldTime;
            }

            if (bronzeTime < silverTime)
            {
                bronzeTime = silverTime;
            }
        }

        public bool IsValid(out string validationError)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                validationError = "Course ID is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                validationError = "Scene name is required.";
                return false;
            }

            if (goldTime > silverTime || silverTime > bronzeTime)
            {
                validationError = "Medal times must follow Gold <= Silver <= Bronze.";
                return false;
            }

            validationError = string.Empty;
            return true;
        }
    }

    public enum EnvironmentType
    {
        Lava,
        Ice,
        Toxic
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Extreme
    }
}
