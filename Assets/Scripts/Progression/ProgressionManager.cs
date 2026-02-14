using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Progression
{
    public class ProgressionManager : MonoBehaviour, IProgressionData
    {
        public static ProgressionManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private CourseUnlockData courseUnlockData;

        [Header("Data")]
        [SerializeField] private UpgradeDataSO[] allUpgrades;
        [SerializeField] private CosmeticDataSO[] allCosmetics;
        [SerializeField] private CourseDataSO[] allCourses;

        private CurrencyManager _currencyManager;
        private UpgradeManager _upgradeManager;
        private CosmeticManager _cosmeticManager;

        public int Currency => _currencyManager != null ? _currencyManager.Currency : 0;
        public int TotalMedals => saveSystem != null && saveSystem.CurrentData != null ? saveSystem.CurrentData.totalMedals : 0;

        public UpgradeManager Upgrades => _upgradeManager;
        public CosmeticManager Cosmetics => _cosmeticManager;
        public CurrencyManager CurrencyMgr => _currencyManager;
        public CourseDataSO[] AllCourses => allCourses;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (saveSystem == null)
            {
                saveSystem = GetComponent<SaveSystem>();
                if (saveSystem == null)
                {
                    saveSystem = gameObject.AddComponent<SaveSystem>();
                }
            }

            saveSystem.Initialize();

            _currencyManager = new CurrencyManager();
            _currencyManager.Initialize(saveSystem);

            _upgradeManager = new UpgradeManager();
            _upgradeManager.Initialize(saveSystem, _currencyManager, allUpgrades);

            _cosmeticManager = new CosmeticManager();
            _cosmeticManager.Initialize(saveSystem, _currencyManager, allCosmetics);

            EnsureDefaults();
            CheckAndUnlockCourses(false);
            saveSystem.Save();

            EventBus.OnCourseCompleted += HandleCourseCompleted;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            EventBus.OnCourseCompleted -= HandleCourseCompleted;
        }

        public int GetUpgradeLevel(string upgradeId)
        {
            return _upgradeManager != null ? _upgradeManager.GetUpgradeLevel(upgradeId) : 0;
        }

        public float GetBestTime(string courseId)
        {
            if (saveSystem == null || saveSystem.CurrentData == null)
            {
                return 0f;
            }

            return saveSystem.CurrentData.bestTimes.GetValueOrDefault(courseId, 0f);
        }

        public int GetBestMedal(string courseId)
        {
            if (saveSystem == null || saveSystem.CurrentData == null)
            {
                return 0;
            }

            return saveSystem.CurrentData.bestMedals.GetValueOrDefault(courseId, 0);
        }

        public bool IsCourseUnlocked(string courseId)
        {
            if (saveSystem == null || saveSystem.CurrentData == null)
            {
                return false;
            }

            if (saveSystem.CurrentData.unlockedCourses.GetValueOrDefault(courseId, false))
            {
                return true;
            }

            CourseDataSO course = FindCourse(courseId);
            return IsCourseUnlockedByRules(course, saveSystem.CurrentData);
        }

        public void RecordCourseCompletion(string courseId, float completionTime, int medal, int collectiblesFound = 0)
        {
            if (saveSystem == null || saveSystem.CurrentData == null)
            {
                return;
            }

            SaveData data = saveSystem.CurrentData;
            CourseDataSO courseData = FindCourse(courseId);
            if (courseData == null)
            {
                return;
            }

            bool completedBefore = data.completedCourses.GetValueOrDefault(courseId, false);
            bool isFirstCompletion = !completedBefore;

            float currentBestTime = data.bestTimes.GetValueOrDefault(courseId, 0f);
            if (currentBestTime <= 0f || completionTime < currentBestTime)
            {
                data.bestTimes.Set(courseId, completionTime);
            }

            int currentBestMedal = data.bestMedals.GetValueOrDefault(courseId, 0);
            if (medal > currentBestMedal)
            {
                data.bestMedals.Set(courseId, medal);
            }

            if (!completedBefore)
            {
                data.completedCourses.Set(courseId, true);
                data.totalCoursesCompleted += 1;
            }

            data.totalMedals = data.GetTotalMedals();

            _currencyManager?.AwardCourseCompletion(courseData, completionTime, medal, collectiblesFound, isFirstCompletion);

            CheckAndUnlockCourses(true);
            saveSystem.MarkDirty();
            saveSystem.Save();
        }

        private void HandleCourseCompleted(string courseId, float time, int medal)
        {
            RecordCourseCompletion(courseId, time, medal, 0);
        }

        private void EnsureDefaults()
        {
            SaveData data = saveSystem.CurrentData;
            if (!data.ownedCosmetics.Contains("default"))
            {
                data.ownedCosmetics.Add("default");
            }

            if (!data.ownedCosmetics.Contains("decal_73"))
            {
                data.ownedCosmetics.Add("decal_73");
            }

            if (string.IsNullOrWhiteSpace(data.equippedColorScheme))
            {
                data.equippedColorScheme = "default";
            }
        }

        private void CheckAndUnlockCourses(bool raiseEvents)
        {
            if (allCourses == null || saveSystem == null || saveSystem.CurrentData == null)
            {
                return;
            }

            SaveData data = saveSystem.CurrentData;
            for (int i = 0; i < allCourses.Length; i++)
            {
                CourseDataSO course = allCourses[i];
                if (course == null || string.IsNullOrWhiteSpace(course.courseId))
                {
                    continue;
                }

                bool wasUnlocked = data.unlockedCourses.GetValueOrDefault(course.courseId, false);
                bool shouldUnlock = IsCourseUnlockedByRules(course, data);

                if (!shouldUnlock)
                {
                    continue;
                }

                data.unlockedCourses.Set(course.courseId, true);
                if (!wasUnlocked && raiseEvents)
                {
                    EventBus.RaiseCourseUnlocked(course.courseId);
                }
            }
        }

        private bool IsCourseUnlockedByRules(CourseDataSO course, SaveData data)
        {
            if (course == null || data == null)
            {
                return false;
            }

            if (courseUnlockData != null)
            {
                return courseUnlockData.IsCourseUnlocked(course, data);
            }

            if (course.environmentType == EnvironmentType.Lava && course.courseIndex == 0)
            {
                return true;
            }

            if (course.prerequisiteCourse != null)
            {
                bool prerequisiteComplete = data.completedCourses.GetValueOrDefault(course.prerequisiteCourse.courseId, false);
                if (!prerequisiteComplete)
                {
                    return false;
                }
            }

            return data.totalMedals >= course.requiredMedals;
        }

        private CourseDataSO FindCourse(string courseId)
        {
            if (allCourses == null || string.IsNullOrWhiteSpace(courseId))
            {
                return null;
            }

            for (int i = 0; i < allCourses.Length; i++)
            {
                CourseDataSO course = allCourses[i];
                if (course != null && course.courseId == courseId)
                {
                    return course;
                }
            }

            return null;
        }
    }
}
