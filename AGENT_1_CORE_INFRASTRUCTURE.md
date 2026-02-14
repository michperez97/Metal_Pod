# Agent 1: Core Infrastructure & Shared Contracts

> **Owner**: Agent 1
> **Priority**: HIGHEST — Other agents depend on your output
> **Estimated scope**: ~20% of total project

---

## Mission

You are responsible for the **foundation layer** of the Metal Pod game. Every other agent depends on your ScriptableObject definitions, shared interfaces, manager singletons, and project configuration. Your code must compile first and define the contracts that all other agents code against.

---

## Files You OWN (Create / Modify)

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs            ✅ EXISTS - review & enhance
│   │   ├── GameStateManager.cs       ✅ EXISTS - review & enhance
│   │   ├── SceneLoader.cs            ✅ EXISTS - review & enhance
│   │   └── AudioManager.cs           ✅ EXISTS - review & enhance
│   └── Shared/
│       ├── IHovercraftData.cs         🆕 CREATE - shared interface
│       ├── ICourseEvents.cs           🆕 CREATE - shared interface
│       ├── IProgressionData.cs        🆕 CREATE - shared interface
│       ├── IDamageReceiver.cs         🆕 CREATE - shared interface
│       ├── GameConstants.cs           🆕 CREATE - shared constants
│       └── EventBus.cs               🆕 CREATE - global event system
├── ScriptableObjects/
│   ├── HovercraftStatsSO.cs          ✅ EXISTS - review & finalize
│   ├── CourseDataSO.cs               ✅ EXISTS - review & finalize
│   ├── UpgradeDataSO.cs              ✅ EXISTS - review & finalize
│   ├── HazardDataSO.cs              ✅ EXISTS - review & finalize
│   ├── EnvironmentDataSO.cs          🆕 CREATE
│   ├── CosmeticDataSO.cs             🆕 CREATE
│   └── GameConfigSO.cs               🆕 CREATE
├── Scenes/
│   └── _Persistent.unity             🆕 CREATE - bootstrap scene with managers
└── ProjectSettings/                   🆕 CONFIGURE
```

## Files You MUST NOT Touch

- `Assets/Scripts/Hovercraft/*` (Agent 2)
- `Assets/Scripts/Course/*` (Agent 3)
- `Assets/Scripts/Hazards/*` (Agent 3)
- `Assets/Scripts/Progression/*` (Agent 4)
- `Assets/Scripts/Workshop/*` (Agent 5)
- `Assets/Scripts/UI/*` (Agent 5)

---

## What Already Exists (From Previous Codex Pass)

The following files exist and are well-written. Review them, fix any issues, and enhance where noted:

### GameManager.cs (EXISTS — enhance)
- Singleton with DontDestroyOnLoad
- Tracks current course and active hovercraft
- **ENHANCE**: Add initialization sequence, add reference to GameConfigSO, add EventBus initialization

### GameStateManager.cs (EXISTS — enhance)
- States: MainMenu, Workshop, Loading, Racing, Paused, Results
- Time.timeScale pause management
- **ENHANCE**: Add OnBeforeStateChange event, add transition validation (e.g., can't go from MainMenu directly to Results)

### SceneLoader.cs (EXISTS — good)
- Async scene loading with progress callbacks
- **ENHANCE**: Add scene preloading, add transition screen support

### AudioManager.cs (EXISTS — enhance)
- Singleton, music + SFX sources, volume controls
- **ENHANCE**: Add audio pooling (5-10 SFX sources), add crossfade for music transitions, add ambient audio source, add PlaySFXAtPoint for 3D sound

### ScriptableObjects (ALL EXIST — finalize)
- HovercraftStatsSO, CourseDataSO, UpgradeDataSO, HazardDataSO all exist with correct fields
- **FINALIZE**: Ensure all fields have [Tooltip] attributes, validate ranges with [Range], add validation methods

---

## Task List

### Task 1: Shared Interfaces & Contracts

These interfaces define the API between agents. They MUST be created first.

**IHovercraftData.cs**
```csharp
namespace MetalPod.Shared
{
    public interface IHovercraftData
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentShield { get; }
        float MaxShield { get; }
        float CurrentSpeed { get; }
        float MaxSpeed { get; }
        float HealthNormalized { get; }
        float ShieldNormalized { get; }
        float BoostCooldownNormalized { get; }
        bool IsBoosting { get; }
        bool IsDestroyed { get; }
    }
}
```

**IDamageReceiver.cs**
```csharp
namespace MetalPod.Shared
{
    public interface IDamageReceiver
    {
        void TakeDamage(float amount);
        void TakeDamage(float amount, DamageType type);
        void RestoreHealth(float amount);
        void RestoreShield(float amount);
    }

    public enum DamageType
    {
        Physical,   // rocks, debris, collisions
        Fire,       // lava, heat
        Ice,        // ice, frost
        Toxic,      // acid, gas
        Electric,   // electric fences
        Explosive   // barrels, eruptions
    }
}
```

**ICourseEvents.cs**
```csharp
namespace MetalPod.Shared
{
    public interface ICourseEvents
    {
        event System.Action OnCountdownStarted;
        event System.Action<int> OnCountdownTick;   // seconds remaining
        event System.Action OnRaceStarted;
        event System.Action<float> OnRaceFinished;   // completion time
        event System.Action OnRaceFailed;
        event System.Action<int> OnCheckpointReached; // checkpoint index
        event System.Action OnPlayerRespawning;
    }
}
```

**IProgressionData.cs**
```csharp
namespace MetalPod.Shared
{
    public interface IProgressionData
    {
        int Currency { get; }
        int TotalMedals { get; }
        int GetUpgradeLevel(string upgradeId);
        float GetBestTime(string courseId);
        int GetBestMedal(string courseId);  // 0=None, 1=Bronze, 2=Silver, 3=Gold
        bool IsCourseUnlocked(string courseId);
    }
}
```

**GameConstants.cs**
```csharp
namespace MetalPod.Shared
{
    public static class GameConstants
    {
        // Tags
        public const string TAG_PLAYER = "Player";
        public const string TAG_CHECKPOINT = "Checkpoint";
        public const string TAG_HAZARD = "Hazard";
        public const string TAG_COLLECTIBLE = "Collectible";
        public const string TAG_FINISH = "FinishLine";

        // Layers
        public const string LAYER_HOVERCRAFT = "Hovercraft";
        public const string LAYER_GROUND = "Ground";
        public const string LAYER_HAZARD = "Hazard";
        public const string LAYER_COLLECTIBLE = "Collectible";

        // Scene Names
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_WORKSHOP = "Workshop";
        public const string SCENE_PERSISTENT = "_Persistent";

        // PlayerPrefs Keys
        public const string PREF_TILT_SENSITIVITY = "TiltSensitivity";
        public const string PREF_INVERT_TILT = "InvertTilt";
        public const string PREF_MASTER_VOLUME = "MasterVolume";
        public const string PREF_MUSIC_VOLUME = "MusicVolume";
        public const string PREF_SFX_VOLUME = "SFXVolume";
        public const string PREF_HAPTICS_ENABLED = "HapticsEnabled";
        public const string PREF_QUALITY_LEVEL = "QualityLevel";

        // Balance
        public const float MEDAL_BONUS_BRONZE = 0.25f;
        public const float MEDAL_BONUS_SILVER = 0.50f;
        public const float MEDAL_BONUS_GOLD = 1.00f;
        public const float REPLAY_REWARD_MULTIPLIER = 0.5f;
        public const float HEALTH_SPEED_THRESHOLD_50 = 0.5f;
        public const float HEALTH_SPEED_MULTIPLIER_50 = 0.8f;
        public const float HEALTH_SPEED_THRESHOLD_25 = 0.25f;
        public const float HEALTH_SPEED_MULTIPLIER_25 = 0.6f;
        public const float HEALTH_HANDLING_MULTIPLIER_25 = 0.7f;
    }
}
```

**EventBus.cs**
```csharp
namespace MetalPod.Shared
{
    /// <summary>
    /// Global event bus for cross-system communication.
    /// Use for events that don't have a clear owner or span multiple systems.
    /// Prefer direct C# events on components for tightly coupled systems.
    /// </summary>
    public static class EventBus
    {
        // Currency
        public static event System.Action<int> OnCurrencyChanged;        // new total
        public static event System.Action<int> OnCurrencyEarned;         // amount earned

        // Course
        public static event System.Action<string> OnCourseSelected;      // courseId
        public static event System.Action<string, float, int> OnCourseCompleted; // courseId, time, medal
        public static event System.Action<string> OnCourseUnlocked;      // courseId

        // Upgrades
        public static event System.Action<string, int> OnUpgradePurchased; // upgradeId, newLevel

        // Cosmetics
        public static event System.Action<string> OnCosmeticEquipped;    // cosmeticId

        // Invoke methods (only called by owning systems)
        public static void RaiseCurrencyChanged(int total) => OnCurrencyChanged?.Invoke(total);
        public static void RaiseCurrencyEarned(int amount) => OnCurrencyEarned?.Invoke(amount);
        public static void RaiseCourseSelected(string id) => OnCourseSelected?.Invoke(id);
        public static void RaiseCourseCompleted(string id, float time, int medal)
            => OnCourseCompleted?.Invoke(id, time, medal);
        public static void RaiseCourseUnlocked(string id) => OnCourseUnlocked?.Invoke(id);
        public static void RaiseUpgradePurchased(string id, int level)
            => OnUpgradePurchased?.Invoke(id, level);
        public static void RaiseCosmeticEquipped(string id) => OnCosmeticEquipped?.Invoke(id);
    }
}
```

### Task 2: New ScriptableObjects

**EnvironmentDataSO.cs**
```csharp
[CreateAssetMenu(fileName = "EnvironmentData", menuName = "MetalPod/EnvironmentData")]
public class EnvironmentDataSO : ScriptableObject
{
    public string environmentId;
    public string environmentName;
    public EnvironmentType environmentType;
    public string description;
    public Sprite environmentIcon;
    public Color primaryColor;
    public Color secondaryColor;
    public int requiredMedalsToUnlock;
    public CourseDataSO[] courses;

    [Header("Audio")]
    public AudioClip ambientLoop;
    public AudioClip musicTrack;

    [Header("Visual")]
    public Material skyboxMaterial;
    // Post-processing profile reference would go here
}
```

**CosmeticDataSO.cs**
```csharp
[CreateAssetMenu(fileName = "CosmeticData", menuName = "MetalPod/CosmeticData")]
public class CosmeticDataSO : ScriptableObject
{
    public string cosmeticId;
    public string cosmeticName;
    public CosmeticType cosmeticType;
    public Sprite icon;
    public int cost;                    // 0 = unlocked by default
    public int requiredMedals;          // 0 = no medal requirement
    public string description;

    [Header("Color Data")]
    public Color primaryColor;
    public Color secondaryColor;
    public Color accentColor;

    [Header("Decal Data")]
    public Texture2D decalTexture;

    [Header("Part Data")]
    public GameObject partPrefab;
    public string attachPoint;
}

public enum CosmeticType { ColorScheme, Decal, Part }
```

**GameConfigSO.cs**
```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "MetalPod/GameConfig")]
public class GameConfigSO : ScriptableObject
{
    [Header("Starting Values")]
    public int startingCurrency = 0;
    public string firstCourseId = "lava_01";

    [Header("Respawn")]
    public float respawnDelay = 2f;
    public float respawnInvincibilityDuration = 3f;

    [Header("Countdown")]
    public int countdownSeconds = 3;

    [Header("Performance")]
    public int targetFrameRate = 60;
    public int maxParticleCount = 500;

    [Header("Controls")]
    public float defaultTiltSensitivity = 1f;
    public float minTiltSensitivity = 0.5f;
    public float maxTiltSensitivity = 2f;
    public float tiltDeadzone = 0.05f;
}
```

### Task 3: Enhance Existing Managers

Review and enhance GameManager, GameStateManager, SceneLoader, AudioManager per the notes in "What Already Exists" section above. Key enhancements:

- GameManager: Add GameConfigSO reference, initialize EventBus, proper initialization sequence
- GameStateManager: Add state transition validation, OnBeforeStateChange event
- SceneLoader: Add scene preloading support, loading screen callbacks
- AudioManager: Add audio source pooling (8 SFX sources), music crossfade, ambient audio channel, PlaySFXAtPoint

### Task 4: Persistent Bootstrap Scene

Create `_Persistent.unity` scene concept:
- Document the exact GameObject hierarchy for the bootstrap scene
- GameManager object (DontDestroyOnLoad) with: GameManager, GameStateManager, SceneLoader, AudioManager
- This scene loads first and persists — all other scenes are loaded additively or via scene switching
- On Start: Load MainMenu scene

### Task 5: Project Configuration Document

Create a document specifying all Unity project settings other agents need:
- Tags: Player, Checkpoint, Hazard, Collectible, FinishLine
- Layers: Hovercraft (8), Ground (9), Hazard (10), Collectible (11)
- Physics layer collision matrix
- Quality settings for Low/Medium/High
- Input System configuration

---

## Acceptance Criteria

- [ ] All shared interfaces compile and are in `Assets/Scripts/Shared/`
- [ ] All ScriptableObject definitions compile with proper attributes
- [ ] GameManager properly initializes all core systems
- [ ] EventBus provides cross-system communication
- [ ] GameConstants contains all shared string constants (tags, layers, prefs keys)
- [ ] AudioManager supports pooled SFX, music crossfade, and ambient audio
- [ ] SceneLoader supports async loading with progress callback
- [ ] All existing code reviewed and enhanced per notes above
- [ ] No references to files owned by other agents (use interfaces only)

---

## Integration Contract

**What other agents expect from you:**
- Agent 2 needs: `IHovercraftData`, `IDamageReceiver`, `DamageType`, `HovercraftStatsSO`, `GameConstants`
- Agent 3 needs: `ICourseEvents`, `IDamageReceiver`, `CourseDataSO`, `HazardDataSO`, `GameConstants`, `EventBus`
- Agent 4 needs: `IProgressionData`, `UpgradeDataSO`, `CosmeticDataSO`, `GameConfigSO`, `EventBus`, `GameConstants`
- Agent 5 needs: `IHovercraftData`, `ICourseEvents`, `IProgressionData`, `EventBus`, `GameConstants`, all ScriptableObject types for display
