using System;
using System.Collections.Generic;
using System.Linq;
using MetalPod.Accessibility;
using MetalPod.Hovercraft;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Achievements
{
    /// <summary>
    /// Singleton that tracks achievement state, listens to progression events, and persists unlocks.
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        private const float FastFinishSeconds = 60f;
        private const float PlayTimeReevaluationInterval = 1f;
        private const float PlayTimeDirtyInterval = 5f;
        private const int DefaultMaxUpgradeLevel = 5;

        private static readonly string[] CoreUpgradeIds = { "speed", "handling", "shield", "boost" };

        public static AchievementManager Instance { get; private set; }

        [Header("Definitions")]
        [SerializeField] private AchievementDataSO[] achievementDefinitions;

        [Header("Behavior")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool logUnlocks = true;

        public event Action<Achievement> OnAchievementUnlocked;

        private readonly Dictionary<string, Achievement> _achievements = new Dictionary<string, Achievement>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _courseCompletionCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<HovercraftHealth> _trackedHealthSources = new List<HovercraftHealth>();

        private SaveSystem _saveSystem;
        private ProgressionManager _progressionManager;

        private bool _initialized;
        private bool _eventBusSubscribed;
        private bool _sceneEventsSubscribed;
        private bool _isEvaluating;
        private bool _pendingReevaluation;
        private float _nextInitializeAttemptAt;
        private float _playTimeReevaluationTimer;
        private float _playTimeDirtyTimer;

        private bool _noDamageFinishTriggered;
        private bool _lowHealthFinishTriggered;
        private bool _fastFinishTriggered;
        private bool _maxSpeedReachedTriggered;
        private bool _tutorialSkippedTriggered;

        public IReadOnlyCollection<Achievement> All => _achievements.Values;
        public int UnlockedCount => _achievements.Values.Count(item => item.IsUnlocked);
        public int TotalCount => _achievements.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            TryInitialize();
        }

        private void Update()
        {
            if (!_initialized)
            {
                if (Time.unscaledTime >= _nextInitializeAttemptAt)
                {
                    _nextInitializeAttemptAt = Time.unscaledTime + 1f;
                    TryInitialize();
                }

                return;
            }

            TrackPlayTime();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            UnsubscribeSceneEvents();
            UnbindHovercraftHealthSources();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void TryInitialize()
        {
            ResolveDependencies();
            if (_saveSystem == null)
            {
                return;
            }

            if (_achievements.Count == 0)
            {
                InitializeAchievements();
            }

            SubscribeToEvents();
            SubscribeSceneEvents();
            BindHovercraftHealthSources();

            _initialized = true;
            RequestReevaluation();
        }

        private void ResolveDependencies()
        {
            if (_progressionManager == null)
            {
                _progressionManager = ProgressionManager.Instance ??
                                      FindFirstObjectByType<ProgressionManager>(FindObjectsInactive.Include);
            }

            if (_saveSystem == null && _progressionManager != null)
            {
                _saveSystem = _progressionManager.GetComponent<SaveSystem>();
            }

            if (_saveSystem == null)
            {
                _saveSystem = FindFirstObjectByType<SaveSystem>(FindObjectsInactive.Include);
            }

            _saveSystem?.Initialize();
        }

        private void InitializeAchievements()
        {
            _achievements.Clear();

            AchievementDataSO[] definitions = ResolveDefinitions();
            if (definitions == null || definitions.Length == 0)
            {
                Debug.LogWarning("[Achievement] No achievement definitions assigned.");
                return;
            }

            SaveData saveData = _saveSystem != null ? _saveSystem.CurrentData : null;
            SerializableBoolDict store = GetAchievementStore(saveData);

            for (int i = 0; i < definitions.Length; i++)
            {
                AchievementDataSO definition = definitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.achievementId))
                {
                    continue;
                }

                string achievementId = definition.achievementId.Trim();
                if (_achievements.ContainsKey(achievementId))
                {
                    Debug.LogWarning($"[Achievement] Duplicate definition ID skipped: {achievementId}");
                    continue;
                }

                bool unlocked = store != null && store.GetValueOrDefault(achievementId, false);
                int progress = unlocked ? Mathf.Max(1, definition.targetValue) : 0;
                _achievements.Add(achievementId, new Achievement(definition, unlocked, progress));
            }

            SyncRuntimeFlagsFromPersistedUnlocks();
        }

        private AchievementDataSO[] ResolveDefinitions()
        {
            if (achievementDefinitions != null && achievementDefinitions.Length > 0)
            {
                return achievementDefinitions;
            }

            AchievementDataSO[] fromResources = Resources.LoadAll<AchievementDataSO>("Achievements");
            if (fromResources != null && fromResources.Length > 0)
            {
                achievementDefinitions = fromResources;
                return achievementDefinitions;
            }

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AchievementDataSO", new[] { "Assets/ScriptableObjects/Achievements" });
            if (guids != null && guids.Length > 0)
            {
                List<AchievementDataSO> found = new List<AchievementDataSO>(guids.Length);
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                    AchievementDataSO asset = UnityEditor.AssetDatabase.LoadAssetAtPath<AchievementDataSO>(path);
                    if (asset != null)
                    {
                        found.Add(asset);
                    }
                }

                if (found.Count > 0)
                {
                    achievementDefinitions = found.ToArray();
                }
            }
#endif

            return achievementDefinitions;
        }

        private void SubscribeToEvents()
        {
            if (_eventBusSubscribed)
            {
                return;
            }

            EventBus.OnCourseCompleted += HandleCourseCompleted;
            EventBus.OnCurrencyEarned += HandleCurrencyEarned;
            EventBus.OnCurrencyChanged += HandleCurrencyChanged;
            EventBus.OnUpgradePurchased += HandleUpgradePurchased;
            EventBus.OnCosmeticEquipped += HandleCosmeticEquipped;
            EventBus.OnCourseUnlocked += HandleCourseUnlocked;
            _eventBusSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!_eventBusSubscribed)
            {
                return;
            }

            EventBus.OnCourseCompleted -= HandleCourseCompleted;
            EventBus.OnCurrencyEarned -= HandleCurrencyEarned;
            EventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            EventBus.OnUpgradePurchased -= HandleUpgradePurchased;
            EventBus.OnCosmeticEquipped -= HandleCosmeticEquipped;
            EventBus.OnCourseUnlocked -= HandleCourseUnlocked;
            _eventBusSubscribed = false;
        }

        private void SubscribeSceneEvents()
        {
            if (_sceneEventsSubscribed)
            {
                return;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
            _sceneEventsSubscribed = true;
        }

        private void UnsubscribeSceneEvents()
        {
            if (!_sceneEventsSubscribed)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            _sceneEventsSubscribed = false;
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            BindHovercraftHealthSources();
            RequestReevaluation();
        }

        private void BindHovercraftHealthSources()
        {
            UnbindHovercraftHealthSources();

            HovercraftHealth[] sources = FindObjectsByType<HovercraftHealth>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < sources.Length; i++)
            {
                HovercraftHealth source = sources[i];
                if (source == null)
                {
                    continue;
                }

                source.OnDestroyed += HandleHovercraftDestroyed;
                _trackedHealthSources.Add(source);
            }
        }

        private void UnbindHovercraftHealthSources()
        {
            for (int i = 0; i < _trackedHealthSources.Count; i++)
            {
                HovercraftHealth source = _trackedHealthSources[i];
                if (source != null)
                {
                    source.OnDestroyed -= HandleHovercraftDestroyed;
                }
            }

            _trackedHealthSources.Clear();
        }

        private void HandleHovercraftDestroyed()
        {
            SaveData saveData = _saveSystem != null ? _saveSystem.CurrentData : null;
            if (saveData == null)
            {
                return;
            }

            saveData.totalDeaths += 1;
            _saveSystem.MarkDirty();
            RequestReevaluation();
        }

        private void TrackPlayTime()
        {
            SaveData saveData = _saveSystem != null ? _saveSystem.CurrentData : null;
            if (saveData == null)
            {
                return;
            }

            float delta = Time.unscaledDeltaTime;
            if (delta <= 0f)
            {
                return;
            }

            saveData.totalPlayTime += delta;
            _playTimeReevaluationTimer += delta;
            _playTimeDirtyTimer += delta;

            if (_playTimeReevaluationTimer >= PlayTimeReevaluationInterval)
            {
                _playTimeReevaluationTimer = 0f;
                RequestReevaluation();
            }

            if (_playTimeDirtyTimer >= PlayTimeDirtyInterval)
            {
                _playTimeDirtyTimer = 0f;
                _saveSystem.MarkDirty();
            }
        }

        private void HandleCourseCompleted(string courseId, float completionTime, int medal)
        {
            if (!string.IsNullOrWhiteSpace(courseId))
            {
                if (!_courseCompletionCounts.TryGetValue(courseId, out int completions))
                {
                    completions = 0;
                }

                _courseCompletionCounts[courseId] = completions + 1;
            }

            if (completionTime > 0f && completionTime < FastFinishSeconds)
            {
                _fastFinishTriggered = true;
            }

            RequestReevaluation();
        }

        private void HandleCurrencyEarned(int _) => RequestReevaluation();
        private void HandleCurrencyChanged(int _) => RequestReevaluation();

        private void HandleUpgradePurchased(string _, int __)
        {
            if (AreCoreUpgradesMaxed())
            {
                _maxSpeedReachedTriggered = true;
            }

            RequestReevaluation();
        }

        private void HandleCosmeticEquipped(string _) => RequestReevaluation();
        private void HandleCourseUnlocked(string _) => RequestReevaluation();

        public void TriggerNoDamageFinish()
        {
            _noDamageFinishTriggered = true;
            RequestReevaluation();
        }

        public void TriggerLowHealthFinish()
        {
            _lowHealthFinishTriggered = true;
            RequestReevaluation();
        }

        public void TriggerFastFinish()
        {
            _fastFinishTriggered = true;
            RequestReevaluation();
        }

        public void TriggerMaxSpeedReached()
        {
            _maxSpeedReachedTriggered = true;
            RequestReevaluation();
        }

        public void TriggerTutorialSkipped()
        {
            _tutorialSkippedTriggered = true;
            RequestReevaluation();
        }

        private void RequestReevaluation()
        {
            if (_achievements.Count == 0)
            {
                return;
            }

            if (_isEvaluating)
            {
                _pendingReevaluation = true;
                return;
            }

            _isEvaluating = true;
            try
            {
                do
                {
                    _pendingReevaluation = false;
                    ReevaluateAllInternal();
                } while (_pendingReevaluation);
            }
            finally
            {
                _isEvaluating = false;
            }
        }

        private void ReevaluateAllInternal()
        {
            SaveData saveData = _saveSystem != null ? _saveSystem.CurrentData : null;
            if (saveData == null)
            {
                return;
            }

            AchievementEvaluationContext context = BuildContext(saveData);
            bool unlockedAny = false;

            foreach (KeyValuePair<string, Achievement> entry in _achievements)
            {
                Achievement achievement = entry.Value;
                if (achievement == null || achievement.Definition == null || achievement.IsUnlocked)
                {
                    continue;
                }

                if (IsMetaAchievement(achievement))
                {
                    continue;
                }

                int progress = AchievementCondition.EvaluateProgress(
                    achievement.Definition,
                    saveData,
                    _progressionManager,
                    context);

                achievement.CurrentProgress = Mathf.Max(0, progress);
                if (achievement.IsComplete && UnlockAchievement(achievement))
                {
                    unlockedAny = true;
                }
            }

            Achievement metaAchievement = GetAchievement("all_achievements");
            if (metaAchievement != null && metaAchievement.Definition != null)
            {
                int unlockedWithoutMeta = CountUnlockedWithoutMeta();
                metaAchievement.CurrentProgress = unlockedWithoutMeta;

                int totalWithoutMeta = CountTotalWithoutMeta();
                int target = Mathf.Max(metaAchievement.Definition.targetValue, totalWithoutMeta);

                if (!metaAchievement.IsUnlocked && unlockedWithoutMeta >= target && UnlockAchievement(metaAchievement))
                {
                    unlockedAny = true;
                }
            }

            if (unlockedAny)
            {
                _pendingReevaluation = true;
            }
        }

        private AchievementEvaluationContext BuildContext(SaveData saveData)
        {
            int maxReplays = 0;
            foreach (KeyValuePair<string, int> entry in _courseCompletionCounts)
            {
                int replayCount = Mathf.Max(0, entry.Value - 1);
                if (replayCount > maxReplays)
                {
                    maxReplays = replayCount;
                }
            }

            bool hasFastTimeFromSave = false;
            if (saveData.bestTimes != null)
            {
                foreach (KeyValuePair<string, float> entry in saveData.bestTimes)
                {
                    if (entry.Value > 0f && entry.Value < FastFinishSeconds)
                    {
                        hasFastTimeFromSave = true;
                        break;
                    }
                }
            }

            return new AchievementEvaluationContext(
                _noDamageFinishTriggered || IsUnlocked("no_damage"),
                _lowHealthFinishTriggered || IsUnlocked("close_call"),
                _fastFinishTriggered || hasFastTimeFromSave,
                _maxSpeedReachedTriggered || AreCoreUpgradesMaxed(),
                _tutorialSkippedTriggered || IsUnlocked("tutorial_skip"),
                maxReplays,
                CountUnlockedWithoutMeta(),
                CountTotalWithoutMeta());
        }

        private bool AreCoreUpgradesMaxed()
        {
            SaveData saveData = _saveSystem != null ? _saveSystem.CurrentData : null;
            if (saveData == null)
            {
                return false;
            }

            for (int i = 0; i < CoreUpgradeIds.Length; i++)
            {
                string upgradeId = CoreUpgradeIds[i];
                int currentLevel = GetUpgradeLevel(saveData, upgradeId);

                int maxLevel = DefaultMaxUpgradeLevel;
                if (_progressionManager != null && _progressionManager.Upgrades != null)
                {
                    int configuredMax = _progressionManager.Upgrades.GetMaxLevel(upgradeId);
                    if (configuredMax > 0)
                    {
                        maxLevel = configuredMax;
                    }
                }

                if (currentLevel < maxLevel)
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetUpgradeLevel(SaveData data, string upgradeId)
        {
            if (data == null || data.upgradeLevels == null || string.IsNullOrWhiteSpace(upgradeId))
            {
                return 0;
            }

            string normalized = upgradeId.Trim().ToLowerInvariant();
            int level = data.upgradeLevels.GetValueOrDefault(normalized, 0);
            if (normalized == "shield")
            {
                level = Mathf.Max(level, data.upgradeLevels.GetValueOrDefault("armor", 0));
            }

            return level;
        }

        private int CountUnlockedWithoutMeta()
        {
            int count = 0;
            foreach (KeyValuePair<string, Achievement> entry in _achievements)
            {
                Achievement achievement = entry.Value;
                if (achievement == null || !achievement.IsUnlocked || IsMetaAchievement(achievement))
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private int CountTotalWithoutMeta()
        {
            int count = 0;
            foreach (KeyValuePair<string, Achievement> entry in _achievements)
            {
                Achievement achievement = entry.Value;
                if (achievement == null || IsMetaAchievement(achievement))
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private static bool IsMetaAchievement(Achievement achievement)
        {
            if (achievement == null || achievement.Definition == null)
            {
                return false;
            }

            return achievement.Definition.conditionType == AchievementConditionType.AllAchievementsUnlocked ||
                   string.Equals(achievement.Definition.achievementId, "all_achievements", StringComparison.Ordinal);
        }

        private bool UnlockAchievement(Achievement achievement)
        {
            if (achievement == null || achievement.Definition == null || achievement.IsUnlocked)
            {
                return false;
            }

            achievement.IsUnlocked = true;
            achievement.CurrentProgress = Mathf.Max(1, achievement.Definition.targetValue);
            PersistUnlockState(achievement.Definition.achievementId, true);

            int boltReward = Mathf.Max(0, achievement.Definition.boltReward);
            if (boltReward > 0)
            {
                GrantBoltReward(boltReward);
            }

            HapticFeedbackManager.Instance?.TriggerHaptic(HapticFeedbackManager.HapticType.Success);
            AccessibilityManager.Instance?.Announce($"Achievement unlocked: {achievement.Definition.title}");

            if (logUnlocks)
            {
                Debug.Log($"[Achievement] Unlocked: {achievement.Definition.title} (+{boltReward} bolts)");
            }

            OnAchievementUnlocked?.Invoke(achievement);
            return true;
        }

        private void GrantBoltReward(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrencyManager currencyManager = _progressionManager != null ? _progressionManager.CurrencyMgr : null;
            if (currencyManager != null)
            {
                currencyManager.AddCurrency(amount);
            }
            else if (_saveSystem != null && _saveSystem.CurrentData != null)
            {
                _saveSystem.CurrentData.currency += amount;
                _saveSystem.MarkDirty();
                _saveSystem.Save();
                EventBus.RaiseCurrencyChanged(_saveSystem.CurrentData.currency);
            }

            EventBus.RaiseCurrencyEarned(amount);
        }

        private void PersistUnlockState(string achievementId, bool unlocked)
        {
            SaveData saveData = _saveSystem != null ? _saveSystem.CurrentData : null;
            SerializableBoolDict store = GetAchievementStore(saveData);
            if (store == null || string.IsNullOrWhiteSpace(achievementId))
            {
                return;
            }

            if (unlocked)
            {
                store.Set(achievementId, true);
            }
            else
            {
                store.Remove(achievementId);
            }

            _saveSystem.MarkDirty();
            _saveSystem.Save();
        }

        private static SerializableBoolDict GetAchievementStore(SaveData saveData)
        {
            if (saveData == null)
            {
                return null;
            }

            // Reuse the existing SerializableBoolDict without changing SaveData schema.
            if (saveData.completedTutorials == null)
            {
                saveData.completedTutorials = new SerializableBoolDict();
            }

            return saveData.completedTutorials;
        }

        private void SyncRuntimeFlagsFromPersistedUnlocks()
        {
            _noDamageFinishTriggered = IsUnlocked("no_damage");
            _lowHealthFinishTriggered = IsUnlocked("close_call");
            _fastFinishTriggered = IsUnlocked("speed_demon");
            _maxSpeedReachedTriggered = IsUnlocked("speed_max");
            _tutorialSkippedTriggered = IsUnlocked("tutorial_skip");
        }

        public Achievement GetAchievement(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            _achievements.TryGetValue(id, out Achievement achievement);
            return achievement;
        }

        public List<Achievement> GetAll()
        {
            return _achievements.Values
                .Where(item => item != null && item.Definition != null)
                .OrderBy(item => item.Definition.category)
                .ThenBy(item => item.Definition.title)
                .ToList();
        }

        public List<Achievement> GetUnlocked()
        {
            return _achievements.Values
                .Where(item => item != null && item.IsUnlocked)
                .OrderBy(item => item.Definition != null ? item.Definition.title : string.Empty)
                .ToList();
        }

        public List<Achievement> GetLocked()
        {
            return _achievements.Values
                .Where(item => item != null && !item.IsUnlocked)
                .OrderBy(item => item.Definition != null ? item.Definition.title : string.Empty)
                .ToList();
        }

        public List<Achievement> GetByCategory(AchievementCategory category)
        {
            return _achievements.Values
                .Where(item => item != null && item.Definition != null && item.Definition.category == category)
                .OrderBy(item => item.Definition.title)
                .ToList();
        }

        public bool IsUnlocked(string id)
        {
            Achievement achievement = GetAchievement(id);
            return achievement != null && achievement.IsUnlocked;
        }

        public int GetProgress(string id)
        {
            Achievement achievement = GetAchievement(id);
            return achievement != null ? achievement.CurrentProgress : 0;
        }

        public bool ForceUnlock(string id)
        {
            Achievement achievement = GetAchievement(id);
            if (achievement == null || achievement.IsUnlocked)
            {
                return false;
            }

            return UnlockAchievement(achievement);
        }

        public bool ForceLock(string id)
        {
            Achievement achievement = GetAchievement(id);
            if (achievement == null || !achievement.IsUnlocked)
            {
                return false;
            }

            achievement.IsUnlocked = false;
            achievement.CurrentProgress = 0;
            PersistUnlockState(id, false);
            RequestReevaluation();
            return true;
        }

        public void ResetAll()
        {
            SaveData saveData = _saveSystem != null ? _saveSystem.CurrentData : null;
            SerializableBoolDict store = GetAchievementStore(saveData);

            foreach (KeyValuePair<string, Achievement> entry in _achievements)
            {
                if (entry.Value != null)
                {
                    entry.Value.IsUnlocked = false;
                    entry.Value.CurrentProgress = 0;
                }

                store?.Remove(entry.Key);
            }

            _courseCompletionCounts.Clear();
            _noDamageFinishTriggered = false;
            _lowHealthFinishTriggered = false;
            _fastFinishTriggered = false;
            _maxSpeedReachedTriggered = false;
            _tutorialSkippedTriggered = false;

            if (_saveSystem != null)
            {
                _saveSystem.MarkDirty();
                _saveSystem.Save();
            }

            RequestReevaluation();
        }

        public void ReevaluateNow()
        {
            RequestReevaluation();
        }
    }
}
