# Codex Task 17: Achievement System

> **Goal**: Create an achievement/milestone system with 25+ achievements, EventBus integration, persistent unlock state, and a UI-ready data layer. Adds replayability and engagement depth.

---

## Context

Metal Pod has a medal system per course but no broader achievement/milestone tracking. Achievements reward players for cumulative progress and encourage exploration of all game systems. The system must:
- Define 25+ achievements with conditions, icons, and rewards
- Listen to EventBus events and game state to auto-detect completions
- Persist unlock state via the existing SaveSystem (SaveData)
- Provide data for a future achievements UI panel
- Optionally trigger haptic feedback and VoiceOver announcements on unlock

**Read these files**:
- `Assets/Scripts/Shared/EventBus.cs` — Events: OnCurrencyEarned, OnCourseCompleted, OnCourseUnlocked, OnUpgradePurchased, OnCosmeticEquipped
- `Assets/Scripts/Progression/SaveData.cs` — SaveData class with `SerializableBoolDict`, stats fields (totalMedals, totalCoursesCompleted, totalDeaths, totalPlayTime)
- `Assets/Scripts/Progression/SaveSystem.cs` — Save/Load API
- `Assets/Scripts/Progression/ProgressionManager.cs` — IProgressionData, Currency, Upgrades, Cosmetics
- `Assets/Scripts/Progression/CurrencyManager.cs` — Currency balance
- `Assets/Scripts/Progression/UpgradeManager.cs` — Upgrade levels
- `Assets/Scripts/Course/MedalSystem.cs` — Medal enum/values
- `Assets/Scripts/Accessibility/HapticFeedbackManager.cs` — HapticType.Success for achievement unlock
- `Assets/Scripts/Accessibility/AccessibilityManager.cs` — Announce() for VoiceOver

---

## Files to Create

```
Assets/Scripts/Achievements/
├── AchievementManager.cs          # Singleton tracking and unlocking achievements
├── Achievement.cs                 # Runtime achievement data (id, state, progress)
├── AchievementDefinition.cs       # Static definitions of all 25+ achievements
├── AchievementCondition.cs        # Condition evaluation logic
├── AchievementPopup.cs            # UI popup when achievement unlocks (code only)
└── AchievementListUI.cs           # UI panel showing all achievements (code only)

Assets/ScriptableObjects/
└── AchievementDataSO.cs           # ScriptableObject for achievement definitions

Assets/Scripts/Editor/
└── AchievementEditorWindow.cs     # Editor window to view/test achievements
```

**DO NOT modify** any existing files. Achievement state is stored via the existing `SerializableBoolDict` in SaveData (key = achievement ID, value = unlocked). The AchievementManager reads/writes through SaveSystem.

---

## Architecture

```
AchievementManager (Singleton, lives on _Persistent)
  ├── Loads AchievementDataSO[] at boot
  ├── Loads unlock state from SaveData.achievements (SerializableBoolDict)
  ├── Subscribes to EventBus events
  ├── On each event, evaluates all locked achievements
  ├── On unlock: saves, triggers popup, haptic, VoiceOver announce
  └── Public API: GetAll(), GetUnlocked(), GetLocked(), IsUnlocked(id), GetProgress(id)

AchievementDataSO (ScriptableObject per achievement)
  ├── string id (unique)
  ├── string title
  ├── string description
  ├── Sprite icon (nullable — assigned in Unity)
  ├── AchievementCategory category
  ├── AchievementConditionType conditionType
  ├── int targetValue (e.g., 3 for "earn 3 gold medals")
  ├── int boltReward (currency reward on unlock)
  └── bool isHidden (don't show until unlocked)

Achievement (runtime instance)
  ├── AchievementDataSO definition
  ├── bool isUnlocked
  ├── int currentProgress
  └── float progressNormalized
```

---

## Achievement Definitions (25 Total)

The agent must define ALL of these in `AchievementDefinition.cs` as static data:

### Racing Achievements
| ID | Title | Description | Condition | Target |
|----|-------|-------------|-----------|--------|
| `first_finish` | First Finish | Complete your first course | Courses completed >= 1 | 1 |
| `speed_demon` | Speed Demon | Complete any course in under 60 seconds | Best time < 60s on any course | 1 |
| `all_courses` | Course Conqueror | Complete all 9 courses | All courses completed | 9 |
| `no_damage` | Untouchable | Complete a course without taking damage | Course completed with full health | 1 |
| `close_call` | Close Call | Finish a course with less than 10% health | Finish with health < 10% | 1 |

### Medal Achievements
| ID | Title | Description | Condition | Target |
|----|-------|-------------|-----------|--------|
| `first_gold` | Golden Start | Earn your first gold medal | Gold medals >= 1 | 1 |
| `triple_gold` | Hat Trick | Earn 3 gold medals | Gold medals >= 3 | 3 |
| `all_gold` | Perfectionist | Earn gold on all 9 courses | Gold medals on all courses | 9 |
| `first_medal` | Medalist | Earn any medal | Total medals >= 1 | 1 |
| `medal_collector` | Medal Collector | Earn 15 medals total | Total medals >= 15 | 15 |

### Progression Achievements
| ID | Title | Description | Condition | Target |
|----|-------|-------------|-----------|--------|
| `first_upgrade` | Tuning Up | Purchase your first upgrade | Any upgrade level >= 1 | 1 |
| `max_speed` | Maxed Out Speed | Max out Speed upgrades | Speed upgrade at level 5 | 5 |
| `max_all` | Fully Loaded | Max out all upgrade categories | All 4 categories at level 5 | 4 |
| `first_cosmetic` | Fashion Statement | Purchase your first cosmetic | Owned cosmetics >= 2 (default + 1) | 1 |
| `rich_racer` | Rich Racer | Accumulate 5000 bolts total | Total bolts earned >= 5000 | 5000 |

### Environment Achievements
| ID | Title | Description | Condition | Target |
|----|-------|-------------|-----------|--------|
| `lava_master` | Lava Lord | Complete all 3 lava courses | Lava courses completed | 3 |
| `ice_master` | Ice King | Complete all 3 ice courses | Ice courses completed | 3 |
| `toxic_master` | Toxic Avenger | Complete all 3 toxic courses | Toxic courses completed | 3 |
| `env_explorer` | World Traveler | Complete at least 1 course in each environment | Environments visited >= 3 | 3 |

### Challenge Achievements
| ID | Title | Description | Condition | Target |
|----|-------|-------------|-----------|--------|
| `die_hard` | Die Hard | Get destroyed 10 times total | Total deaths >= 10 | 10 |
| `marathon` | Marathon Runner | Play for 60 minutes total | Total play time >= 3600s | 3600 |
| `replay_king` | Replay King | Replay any course 5 times | Replays on single course >= 5 | 5 |
| `bolt_hoarder` | Bolt Hoarder | Have 1000 bolts at once | Current currency >= 1000 | 1000 |

### Hidden Achievements
| ID | Title | Description | Condition | Target |
|----|-------|-------------|-----------|--------|
| `speed_max` | Ludicrous Speed | Reach maximum speed with all upgrades | Reach max speed stat | 1 |
| `tutorial_skip` | Impatient | Skip the tutorial | Tutorial skipped | 1 |
| `all_achievements` | Completionist | Unlock all other achievements | All achievements unlocked | 24 |

---

## Detailed Specifications

### AchievementDataSO.cs

```csharp
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
        public int targetValue = 1;
        [Tooltip("For course-specific conditions, list course IDs here")]
        public string[] targetCourseIds;
        [Tooltip("For upgrade-specific conditions, the upgrade category")]
        public string targetUpgradeId;

        [Header("Reward")]
        public int boltReward = 0;
    }
}
```

### Achievement.cs

```csharp
using MetalPod.ScriptableObjects;

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
            Definition.targetValue > 0
                ? (float)CurrentProgress / Definition.targetValue
                : (IsUnlocked ? 1f : 0f);

        public bool IsComplete => CurrentProgress >= Definition.targetValue;

        public Achievement(AchievementDataSO definition, bool unlocked = false, int progress = 0)
        {
            Definition = definition;
            IsUnlocked = unlocked;
            CurrentProgress = progress;
        }
    }
}
```

### AchievementCondition.cs

```csharp
// Evaluates achievement conditions against current game state.
// Pure logic — no MonoBehaviour. Called by AchievementManager.

using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Achievements
{
    public static class AchievementCondition
    {
        /// <summary>
        /// Evaluate the current progress for an achievement.
        /// Returns the current value toward the target.
        /// </summary>
        public static int EvaluateProgress(AchievementDataSO def, SaveData saveData, ProgressionManager pm)
        {
            if (saveData == null) return 0;

            switch (def.conditionType)
            {
                case AchievementConditionType.CoursesCompleted:
                    return saveData.totalCoursesCompleted;

                case AchievementConditionType.GoldMedalsEarned:
                    return CountMedals(saveData, 3); // 3 = Gold

                case AchievementConditionType.TotalMedalsEarned:
                    return saveData.GetTotalMedals();

                case AchievementConditionType.UpgradeLevel:
                    return pm != null && pm.Upgrades != null
                        ? pm.GetUpgradeLevel(def.targetUpgradeId) : 0;

                case AchievementConditionType.AllUpgradesMaxed:
                    return CountMaxedUpgrades(saveData);

                case AchievementConditionType.CosmeticsOwned:
                    return saveData.ownedCosmetics != null ? saveData.ownedCosmetics.Count : 0;

                case AchievementConditionType.TotalBoltsEarned:
                    // Track cumulative in SaveData — use currency + total spent as proxy
                    return saveData.currency; // Simplified — ideally track totalEarned separately

                case AchievementConditionType.CurrentBolts:
                    return saveData.currency;

                case AchievementConditionType.SpecificCoursesCompleted:
                    return CountSpecificCourses(saveData, def.targetCourseIds);

                case AchievementConditionType.TotalDeaths:
                    return saveData.totalDeaths;

                case AchievementConditionType.TotalPlayTime:
                    return Mathf.FloorToInt(saveData.totalPlayTime);

                case AchievementConditionType.EnvironmentsVisited:
                    return CountEnvironmentsVisited(saveData);

                case AchievementConditionType.SpecificUpgradeMaxed:
                    return saveData.upgradeLevels.GetValueOrDefault(def.targetUpgradeId, 0);

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Check special conditions that are event-triggered (not queryable from save data).
        /// These are set by AchievementManager when the triggering event occurs.
        /// </summary>
        public static bool CheckEventCondition(AchievementDataSO def, object eventData)
        {
            switch (def.conditionType)
            {
                case AchievementConditionType.NoDamageCourse:
                case AchievementConditionType.LowHealthFinish:
                case AchievementConditionType.FastTime:
                case AchievementConditionType.MaxSpeedReached:
                case AchievementConditionType.TutorialSkipped:
                    return true; // Triggered by specific events, handled in AchievementManager
                default:
                    return false;
            }
        }

        private static int CountMedals(SaveData data, int medalLevel)
        {
            int count = 0;
            foreach (var kvp in data.bestMedals)
            {
                if (kvp.Value >= medalLevel) count++;
            }
            return count;
        }

        private static int CountMaxedUpgrades(SaveData data)
        {
            int count = 0;
            string[] categories = { "speed", "armor", "handling", "boost" };
            foreach (string cat in categories)
            {
                if (data.upgradeLevels.GetValueOrDefault(cat, 0) >= 5) count++;
            }
            return count;
        }

        private static int CountSpecificCourses(SaveData data, string[] courseIds)
        {
            if (courseIds == null) return 0;
            int count = 0;
            foreach (string id in courseIds)
            {
                if (data.completedCourses.GetValueOrDefault(id, false)) count++;
            }
            return count;
        }

        private static int CountEnvironmentsVisited(SaveData data)
        {
            bool lava = false, ice = false, toxic = false;
            foreach (var kvp in data.completedCourses)
            {
                if (!kvp.Value) continue;
                if (kvp.Key.StartsWith("lava")) lava = true;
                else if (kvp.Key.StartsWith("ice")) ice = true;
                else if (kvp.Key.StartsWith("toxic")) toxic = true;
            }
            return (lava ? 1 : 0) + (ice ? 1 : 0) + (toxic ? 1 : 0);
        }
    }
}
```

### AchievementManager.cs

```csharp
// Singleton managing all achievements. Subscribes to EventBus, evaluates conditions,
// unlocks achievements, persists state, and triggers notifications.

using System;
using System.Collections.Generic;
using System.Linq;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Definitions")]
        [SerializeField] private AchievementDataSO[] achievementDefinitions;

        public event Action<Achievement> OnAchievementUnlocked;

        private readonly Dictionary<string, Achievement> _achievements = new Dictionary<string, Achievement>();
        private SaveSystem _saveSystem;
        private ProgressionManager _progressionManager;

        public IReadOnlyCollection<Achievement> All => _achievements.Values;
        public int UnlockedCount => _achievements.Values.Count(a => a.IsUnlocked);
        public int TotalCount => _achievements.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _progressionManager = ProgressionManager.Instance;
            _saveSystem = FindObjectOfType<SaveSystem>();

            InitializeAchievements();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (Instance == this) Instance = null;
        }

        private void InitializeAchievements()
        {
            if (achievementDefinitions == null) return;

            SaveData data = _saveSystem?.CurrentData;

            foreach (var def in achievementDefinitions)
            {
                if (string.IsNullOrEmpty(def.achievementId)) continue;

                bool unlocked = false;
                if (data != null && data.completedCourses != null)
                {
                    // Achievement unlock state stored in a dedicated dict
                    // We'll use a convention: save key = "ach_" + achievementId
                    unlocked = PlayerPrefs.GetInt($"ach_{def.achievementId}", 0) == 1;
                }

                int progress = 0;
                if (!unlocked && data != null)
                {
                    progress = AchievementCondition.EvaluateProgress(def, data, _progressionManager);
                }

                var achievement = new Achievement(def, unlocked, unlocked ? def.targetValue : progress);
                _achievements[def.achievementId] = achievement;
            }
        }

        private void SubscribeToEvents()
        {
            EventBus.OnCourseCompleted += HandleCourseCompleted;
            EventBus.OnCurrencyEarned += HandleCurrencyEarned;
            EventBus.OnCurrencyChanged += HandleCurrencyChanged;
            EventBus.OnUpgradePurchased += HandleUpgradePurchased;
            EventBus.OnCosmeticEquipped += HandleCosmeticEquipped;
            EventBus.OnCourseUnlocked += HandleCourseUnlocked;
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.OnCourseCompleted -= HandleCourseCompleted;
            EventBus.OnCurrencyEarned -= HandleCurrencyEarned;
            EventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            EventBus.OnUpgradePurchased -= HandleUpgradePurchased;
            EventBus.OnCosmeticEquipped -= HandleCosmeticEquipped;
            EventBus.OnCourseUnlocked -= HandleCourseUnlocked;
        }

        // ── Event Handlers ──────────────────────────────────

        private void HandleCourseCompleted(string courseId, float time, int medal)
        {
            ReevaluateAll();
        }

        private void HandleCurrencyEarned(int amount) => ReevaluateAll();
        private void HandleCurrencyChanged(int total) => ReevaluateAll();
        private void HandleUpgradePurchased(string id, int level) => ReevaluateAll();
        private void HandleCosmeticEquipped(string id) => ReevaluateAll();
        private void HandleCourseUnlocked(string id) => ReevaluateAll();

        // ── Special Event Triggers ──────────────────────────

        /// <summary>Call from CourseManager when player finishes with full health.</summary>
        public void TriggerNoDamageFinish()
        {
            TryUnlock("no_damage");
        }

        /// <summary>Call from CourseManager when player finishes with < 10% health.</summary>
        public void TriggerLowHealthFinish()
        {
            TryUnlock("close_call");
        }

        /// <summary>Call from CourseManager when time < 60s.</summary>
        public void TriggerFastFinish()
        {
            TryUnlock("speed_demon");
        }

        /// <summary>Call from TutorialManager when tutorial is skipped.</summary>
        public void TriggerTutorialSkipped()
        {
            TryUnlock("tutorial_skip");
        }

        // ── Core Logic ──────────────────────────────────────

        private void ReevaluateAll()
        {
            SaveData data = _saveSystem?.CurrentData;
            if (data == null) return;

            foreach (var kvp in _achievements)
            {
                var achievement = kvp.Value;
                if (achievement.IsUnlocked) continue;

                int progress = AchievementCondition.EvaluateProgress(
                    achievement.Definition, data, _progressionManager);
                achievement.CurrentProgress = progress;

                if (achievement.IsComplete)
                {
                    UnlockAchievement(achievement);
                }
            }

            // Check "all achievements" meta-achievement
            CheckAllAchievements();
        }

        private void TryUnlock(string achievementId)
        {
            if (!_achievements.TryGetValue(achievementId, out Achievement achievement)) return;
            if (achievement.IsUnlocked) return;

            achievement.CurrentProgress = achievement.Definition.targetValue;
            UnlockAchievement(achievement);
        }

        private void UnlockAchievement(Achievement achievement)
        {
            achievement.IsUnlocked = true;
            achievement.CurrentProgress = achievement.Definition.targetValue;

            // Persist
            PlayerPrefs.SetInt($"ach_{achievement.Definition.achievementId}", 1);
            PlayerPrefs.Save();

            // Grant bolt reward
            if (achievement.Definition.boltReward > 0)
            {
                EventBus.RaiseCurrencyEarned(achievement.Definition.boltReward);
            }

            // Haptic feedback
            var haptics = MetalPod.Accessibility.HapticFeedbackManager.Instance;
            haptics?.TriggerHaptic(MetalPod.Accessibility.HapticFeedbackManager.HapticType.Success);

            // VoiceOver announcement
            var accessibility = MetalPod.Accessibility.AccessibilityManager.Instance;
            accessibility?.Announce($"Achievement unlocked: {achievement.Definition.title}");

            Debug.Log($"[Achievement] Unlocked: {achievement.Definition.title} (+{achievement.Definition.boltReward} bolts)");

            OnAchievementUnlocked?.Invoke(achievement);
        }

        private void CheckAllAchievements()
        {
            if (!_achievements.TryGetValue("all_achievements", out Achievement meta)) return;
            if (meta.IsUnlocked) return;

            int unlocked = _achievements.Values.Count(a => a.IsUnlocked && a.Definition.achievementId != "all_achievements");
            int total = _achievements.Count - 1; // Exclude meta achievement
            meta.CurrentProgress = unlocked;

            if (unlocked >= total)
            {
                UnlockAchievement(meta);
            }
        }

        // ── Public API ──────────────────────────────────────

        public Achievement GetAchievement(string id)
        {
            _achievements.TryGetValue(id, out Achievement a);
            return a;
        }

        public bool IsUnlocked(string id)
        {
            return _achievements.TryGetValue(id, out Achievement a) && a.IsUnlocked;
        }

        public List<Achievement> GetByCategory(AchievementCategory category)
        {
            return _achievements.Values
                .Where(a => a.Definition.category == category)
                .OrderBy(a => a.IsUnlocked ? 1 : 0)
                .ToList();
        }

        public List<Achievement> GetUnlocked()
        {
            return _achievements.Values.Where(a => a.IsUnlocked).ToList();
        }

        public List<Achievement> GetLocked()
        {
            return _achievements.Values.Where(a => !a.IsUnlocked).ToList();
        }

        /// <summary>Reset all achievements (for debug).</summary>
        public void ResetAll()
        {
            foreach (var kvp in _achievements)
            {
                kvp.Value.IsUnlocked = false;
                kvp.Value.CurrentProgress = 0;
                PlayerPrefs.DeleteKey($"ach_{kvp.Key}");
            }
            PlayerPrefs.Save();
            Debug.Log("[Achievement] All achievements reset.");
        }
    }
}
```

### AchievementDefinition.cs

```csharp
// Static class that creates all 25 achievement definitions programmatically.
// Used by an Editor script to generate AchievementDataSO assets.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MetalPod.ScriptableObjects;

namespace MetalPod.Achievements
{
    public static class AchievementDefinition
    {
        // ... (define each of the 25 achievements listed in the table above)
        // The agent should create a GenerateAll() method that creates
        // AchievementDataSO assets in Assets/ScriptableObjects/Achievements/
        // for each achievement in the table.

        [MenuItem("Metal Pod/Generate Achievement Assets")]
        public static void GenerateAll()
        {
            string dir = "Assets/ScriptableObjects/Achievements";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Achievements");
            }

            // Create all 25 achievements...
            // (The agent must implement the full list from the table above)
            // Example:
            // CreateAchievement(dir, "first_finish", "First Finish", "Complete your first course",
            //     AchievementCategory.Racing, AchievementConditionType.CoursesCompleted, 1, 50);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Achievements] Generated all achievement assets.");
        }

        private static void CreateAchievement(string dir, string id, string title, string description,
            AchievementCategory category, AchievementConditionType condition, int target, int boltReward,
            bool hidden = false, string[] courseIds = null, string upgradeId = null)
        {
            AchievementDataSO asset = ScriptableObject.CreateInstance<AchievementDataSO>();
            asset.achievementId = id;
            asset.title = title;
            asset.description = description;
            asset.category = category;
            asset.conditionType = condition;
            asset.targetValue = target;
            asset.boltReward = boltReward;
            asset.isHidden = hidden;
            asset.targetCourseIds = courseIds;
            asset.targetUpgradeId = upgradeId;

            AssetDatabase.CreateAsset(asset, $"{dir}/Achievement_{id}.asset");
        }
    }
}
#endif
```

**IMPORTANT**: The agent must implement `GenerateAll()` with all 25 achievements from the table. The example above shows the pattern — every achievement needs a `CreateAchievement()` call.

### AchievementPopup.cs and AchievementListUI.cs

Create UI-ready MonoBehaviours that subscribe to `AchievementManager.OnAchievementUnlocked`:
- `AchievementPopup.cs` — Displays a toast notification (icon, title, description) that slides in and auto-dismisses
- `AchievementListUI.cs` — Panel showing all achievements in a scrollable list, categorized, with progress bars for locked achievements

### AchievementEditorWindow.cs

Editor window (Menu: Metal Pod > Achievements):
- Lists all achievements with unlock state
- Buttons to unlock/lock individual achievements
- Button to reset all
- Shows progress values
- Works in Play Mode

---

## Acceptance Criteria

- [ ] `AchievementManager.cs` — Singleton, EventBus subscription, auto-evaluation, unlock + persist
- [ ] `Achievement.cs` — Runtime data class with progress tracking
- [ ] `AchievementCondition.cs` — Pure evaluation logic for all condition types
- [ ] `AchievementDataSO.cs` — ScriptableObject with all fields
- [ ] `AchievementDefinition.cs` — Editor generator creating all 25 achievement assets
- [ ] `AchievementPopup.cs` — Toast notification on unlock
- [ ] `AchievementListUI.cs` — Scrollable achievement panel with categories and progress bars
- [ ] `AchievementEditorWindow.cs` — Editor debug window
- [ ] All 25 achievements from the table are defined
- [ ] Integrates with HapticFeedbackManager (Success haptic on unlock)
- [ ] Integrates with AccessibilityManager (VoiceOver announce on unlock)
- [ ] All scripts in `MetalPod.Achievements`, `MetalPod.ScriptableObjects`, or `MetalPod.Editor` namespaces
- [ ] No modifications to existing files
- [ ] Compiles without errors
