# Agent 4: Progression, Data & Save Systems

> **Owner**: Agent 4
> **Priority**: MEDIUM — Needed for gameplay loop, not blocking core mechanics
> **Estimated scope**: ~15% of total project
> **Dependency**: Agent 1 (interfaces, ScriptableObjects, EventBus)

---

## Mission

You own all **data persistence, progression mechanics, and economy balancing**. Save/load, currency, upgrades, course unlocking, and cosmetic ownership are your domain. You also create the ScriptableObject *asset instances* (the actual .asset files with tuned values) that the entire game uses. You are the data backbone of the game.

---

## Files You OWN (Create / Modify)

```
Assets/
├── Scripts/
│   └── Progression/
│       ├── SaveSystem.cs             ✅ EXISTS - review & integrate
│       ├── SaveData.cs               🆕 CREATE (extract from SaveSystem)
│       ├── CurrencyManager.cs        ✅ EXISTS - rewrite (currently minimal)
│       ├── ProgressionManager.cs     ✅ EXISTS - rewrite (currently minimal)
│       ├── CourseUnlockData.cs        ✅ EXISTS - enhance
│       ├── UpgradeManager.cs         🆕 CREATE (replaces Workshop/UpgradeSystem.cs logic)
│       └── CosmeticManager.cs        🆕 CREATE
│
├── ScriptableObjects/
│   └── Data/                          🆕 CREATE — all .asset instances
│       ├── DefaultHovercraftStats.asset
│       ├── GameConfig.asset
│       │
│       ├── Courses/
│       │   ├── Lava_Course_01.asset
│       │   ├── Lava_Course_02.asset
│       │   ├── Lava_Course_03.asset
│       │   ├── Ice_Course_01.asset
│       │   ├── Ice_Course_02.asset
│       │   ├── Ice_Course_03.asset
│       │   ├── Toxic_Course_01.asset
│       │   ├── Toxic_Course_02.asset
│       │   └── Toxic_Course_03.asset
│       │
│       ├── Environments/
│       │   ├── LavaEnvironment.asset
│       │   ├── IceEnvironment.asset
│       │   └── ToxicEnvironment.asset
│       │
│       ├── Upgrades/
│       │   ├── Upgrade_Speed.asset
│       │   ├── Upgrade_Handling.asset
│       │   ├── Upgrade_Shield.asset
│       │   └── Upgrade_Boost.asset
│       │
│       ├── Cosmetics/
│       │   ├── Color_Default.asset
│       │   ├── Color_RedBlack.asset
│       │   ├── Color_BlueSilver.asset
│       │   ├── Color_GreenBlack.asset
│       │   ├── Color_Chrome.asset
│       │   ├── Decal_Skull.asset
│       │   ├── Decal_Flames.asset
│       │   ├── Decal_Lightning.asset
│       │   ├── Decal_RacingStripes.asset
│       │   └── Decal_Number73.asset
│       │
│       └── Hazards/
│           ├── Hazard_LavaFlow.asset
│           ├── Hazard_Eruption.asset
│           ├── Hazard_Geyser.asset
│           ├── Hazard_FallingDebris.asset
│           ├── Hazard_HeatZone.asset
│           ├── Hazard_IcePatch.asset
│           ├── Hazard_Icicle.asset
│           ├── Hazard_Blizzard.asset
│           ├── Hazard_ToxicGas.asset
│           ├── Hazard_Acid.asset
│           ├── Hazard_Press.asset
│           ├── Hazard_Electric.asset
│           └── Hazard_Barrel.asset
│
└── Config/
    └── BalanceSheet.md               🆕 CREATE - all tuning values documented
```

## Files You MUST NOT Touch

- `Assets/Scripts/Core/*` (Agent 1)
- `Assets/Scripts/Shared/*` (Agent 1)
- `Assets/Scripts/Hovercraft/*` (Agent 2)
- `Assets/Scripts/Course/*` (Agent 3)
- `Assets/Scripts/Hazards/*` (Agent 3)
- `Assets/Scripts/Workshop/*` (Agent 5)
- `Assets/Scripts/UI/*` (Agent 5)

## Files You REFERENCE (Read-Only)

```
Assets/Scripts/Shared/IProgressionData.cs   — implement this interface
Assets/Scripts/Shared/GameConstants.cs      — prefs keys, balance constants
Assets/Scripts/Shared/EventBus.cs           — raise/listen to events
Assets/ScriptableObjects/*.cs               — SO class definitions (Agent 1 owns these)
```

---

## What Already Exists

### SaveSystem.cs (EXISTS — good foundation)
- JSON to persistentDataPath
- SaveData class embedded (extract to own file)
- SerializableKeyValueCollection for dictionary workaround
- Save/Load/Reset methods
- **ENHANCE**: Add auto-save timer, add backup save (keep previous save), add data migration for future versions

### CurrencyManager.cs (EXISTS — minimal)
- AddCurrency/SpendCurrency, OnCurrencyChanged event
- **REWRITE**: Add full reward calculation, integrate with SaveSystem, integrate with EventBus

### ProgressionManager.cs (EXISTS — minimal)
- Just wires SaveSystem + CurrencyManager
- **REWRITE**: Implement `IProgressionData`, become the central progression authority

### CourseUnlockData.cs (EXISTS — basic)
- ScriptableObject with course array
- **ENHANCE**: Add unlock requirement validation logic

---

## Task List

### Task 1: SaveData Extraction & Enhancement

**SaveData.cs** — Extract from SaveSystem, make standalone:
```csharp
namespace MetalPod.Progression
{
    [System.Serializable]
    public class SaveData
    {
        public int version = 1;  // for future migrations
        public long lastSaveTimestamp;

        [Header("Currency")]
        public int currency = 0;

        [Header("Upgrades")]
        public SerializableDict upgradeLevels = new SerializableDict();
        // upgradeId -> level (0 = not purchased)

        [Header("Course Progress")]
        public SerializableFloatDict bestTimes = new SerializableFloatDict();
        // courseId -> best time in seconds

        public SerializableIntDict bestMedals = new SerializableIntDict();
        // courseId -> medal (0=None, 1=Bronze, 2=Silver, 3=Gold)

        public SerializableBoolDict completedCourses = new SerializableBoolDict();
        // courseId -> completed at least once

        [Header("Cosmetics")]
        public List<string> ownedCosmetics = new List<string>();
        public string equippedColorScheme = "default";
        public string equippedDecal = "";
        public string equippedPart = "";

        [Header("Stats")]
        public int totalMedals = 0;
        public float totalPlayTime = 0f;
        public int totalCoursesCompleted = 0;
        public int totalDeaths = 0;

        // Helper methods
        public int GetTotalMedals()
        {
            int count = 0;
            foreach (var kvp in bestMedals)
                if (kvp.Value > 0) count++;
            return count;
        }

        public static SaveData CreateDefault()
        {
            var data = new SaveData();
            data.ownedCosmetics.Add("default"); // default color scheme
            return data;
        }
    }
}
```

### Task 2: SaveSystem Enhancement

```csharp
public class SaveSystem
{
    private const string SAVE_FILE = "metalpod_save.json";
    private const string BACKUP_FILE = "metalpod_save_backup.json";

    public SaveData CurrentData { get; private set; }

    public void Initialize()
    {
        CurrentData = Load() ?? SaveData.CreateDefault();
    }

    public void Save()
    {
        // Backup current save before overwriting
        BackupCurrentSave();

        CurrentData.lastSaveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string json = JsonUtility.ToJson(CurrentData, true);
        string path = GetSavePath(SAVE_FILE);
        System.IO.File.WriteAllText(path, json);
    }

    public SaveData Load()
    {
        string path = GetSavePath(SAVE_FILE);
        if (!System.IO.File.Exists(path)) return null;

        try
        {
            string json = System.IO.File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            data = MigrateIfNeeded(data);
            return data;
        }
        catch
        {
            // Try backup
            return LoadBackup();
        }
    }

    private SaveData MigrateIfNeeded(SaveData data)
    {
        // Future: handle version migrations
        // if (data.version < 2) { migrate v1 -> v2 }
        return data;
    }

    private void BackupCurrentSave() { /* copy save to backup */ }
    private SaveData LoadBackup() { /* load from backup file */ }
    private string GetSavePath(string filename)
        => System.IO.Path.Combine(Application.persistentDataPath, filename);
}
```

### Task 3: CurrencyManager — Full Implementation

```csharp
public class CurrencyManager
{
    private SaveSystem _saveSystem;

    public int Currency => _saveSystem.CurrentData.currency;

    public void Initialize(SaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    /// <summary>
    /// Calculate and award currency for completing a course.
    /// </summary>
    public int AwardCourseCompletion(CourseDataSO course, float time, int medal, int collectiblesFound, bool isFirstCompletion)
    {
        int baseReward = GetBaseReward(course);

        // First completion bonus
        float multiplier = isFirstCompletion ? 1.0f : GameConstants.REPLAY_REWARD_MULTIPLIER;

        // Medal bonus
        float medalBonus = medal switch
        {
            3 => GameConstants.MEDAL_BONUS_GOLD,
            2 => GameConstants.MEDAL_BONUS_SILVER,
            1 => GameConstants.MEDAL_BONUS_BRONZE,
            _ => 0f
        };

        // Collectible bonus (10 currency per collectible)
        int collectibleBonus = collectiblesFound * 10;

        int total = Mathf.RoundToInt(baseReward * multiplier * (1f + medalBonus)) + collectibleBonus;

        AddCurrency(total);
        EventBus.RaiseCurrencyEarned(total);
        return total;
    }

    private int GetBaseReward(CourseDataSO course)
    {
        // Base reward scales with difficulty
        return course.difficulty switch
        {
            DifficultyLevel.Easy => 100,
            DifficultyLevel.Medium => 150,
            DifficultyLevel.Hard => 200,
            DifficultyLevel.Extreme => 300,
            _ => 100
        };
    }

    public bool CanAfford(int cost) => Currency >= cost;

    public bool SpendCurrency(int amount)
    {
        if (!CanAfford(amount)) return false;
        _saveSystem.CurrentData.currency -= amount;
        _saveSystem.Save();
        EventBus.RaiseCurrencyChanged(Currency);
        return true;
    }

    public void AddCurrency(int amount)
    {
        _saveSystem.CurrentData.currency += amount;
        _saveSystem.Save();
        EventBus.RaiseCurrencyChanged(Currency);
    }
}
```

### Task 4: UpgradeManager

```csharp
public class UpgradeManager
{
    private SaveSystem _saveSystem;
    private CurrencyManager _currencyManager;
    private UpgradeDataSO[] _allUpgrades;

    public void Initialize(SaveSystem save, CurrencyManager currency, UpgradeDataSO[] upgrades)
    {
        _saveSystem = save;
        _currencyManager = currency;
        _allUpgrades = upgrades;
    }

    public int GetUpgradeLevel(string upgradeId)
    {
        return _saveSystem.CurrentData.upgradeLevels.GetValueOrDefault(upgradeId, 0);
    }

    public int GetMaxLevel(string upgradeId)
    {
        var upgrade = GetUpgradeData(upgradeId);
        return upgrade != null ? upgrade.levels.Length : 0;
    }

    public int GetNextLevelCost(string upgradeId)
    {
        var upgrade = GetUpgradeData(upgradeId);
        int currentLevel = GetUpgradeLevel(upgradeId);
        if (upgrade == null || currentLevel >= upgrade.levels.Length) return -1;
        return upgrade.levels[currentLevel].cost;
    }

    public bool CanPurchaseUpgrade(string upgradeId)
    {
        int cost = GetNextLevelCost(upgradeId);
        if (cost < 0) return false; // max level
        return _currencyManager.CanAfford(cost);
    }

    public bool TryPurchaseUpgrade(string upgradeId)
    {
        if (!CanPurchaseUpgrade(upgradeId)) return false;

        int cost = GetNextLevelCost(upgradeId);
        if (!_currencyManager.SpendCurrency(cost)) return false;

        int newLevel = GetUpgradeLevel(upgradeId) + 1;
        _saveSystem.CurrentData.upgradeLevels[upgradeId] = newLevel;
        _saveSystem.Save();

        EventBus.RaiseUpgradePurchased(upgradeId, newLevel);
        return true;
    }

    /// <summary>
    /// Calculate effective stat multipliers from all upgrades.
    /// Called by Agent 2's HovercraftStats to apply upgrades.
    /// </summary>
    public (float speed, float handling, float shield, float boost) GetStatMultipliers()
    {
        float speed = 1f + GetUpgradeLevel("speed") * 0.1f;      // +10% per level
        float handling = 1f + GetUpgradeLevel("handling") * 0.08f; // +8% per level
        float shield = 1f + GetUpgradeLevel("shield") * 0.12f;    // +12% per level
        float boost = 1f + GetUpgradeLevel("boost") * 0.1f;       // +10% per level
        return (speed, handling, shield, boost);
    }

    private UpgradeDataSO GetUpgradeData(string id)
        => System.Array.Find(_allUpgrades, u => u.upgradeId == id);
}
```

### Task 5: CosmeticManager

```csharp
public class CosmeticManager
{
    private SaveSystem _saveSystem;
    private CurrencyManager _currencyManager;
    private CosmeticDataSO[] _allCosmetics;

    public string EquippedColorScheme => _saveSystem.CurrentData.equippedColorScheme;
    public string EquippedDecal => _saveSystem.CurrentData.equippedDecal;
    public string EquippedPart => _saveSystem.CurrentData.equippedPart;

    public bool OwnsCosmetic(string cosmeticId)
        => _saveSystem.CurrentData.ownedCosmetics.Contains(cosmeticId);

    public bool CanPurchaseCosmetic(string cosmeticId)
    {
        if (OwnsCosmetic(cosmeticId)) return false;
        var cosmetic = GetCosmeticData(cosmeticId);
        if (cosmetic == null) return false;
        if (cosmetic.requiredMedals > 0 && _saveSystem.CurrentData.totalMedals < cosmetic.requiredMedals)
            return false;
        return _currencyManager.CanAfford(cosmetic.cost);
    }

    public bool TryPurchaseCosmetic(string cosmeticId)
    {
        if (!CanPurchaseCosmetic(cosmeticId)) return false;
        var cosmetic = GetCosmeticData(cosmeticId);
        if (!_currencyManager.SpendCurrency(cosmetic.cost)) return false;

        _saveSystem.CurrentData.ownedCosmetics.Add(cosmeticId);
        _saveSystem.Save();
        return true;
    }

    public void EquipCosmetic(string cosmeticId)
    {
        if (!OwnsCosmetic(cosmeticId)) return;
        var cosmetic = GetCosmeticData(cosmeticId);
        if (cosmetic == null) return;

        switch (cosmetic.cosmeticType)
        {
            case CosmeticType.ColorScheme:
                _saveSystem.CurrentData.equippedColorScheme = cosmeticId;
                break;
            case CosmeticType.Decal:
                _saveSystem.CurrentData.equippedDecal = cosmeticId;
                break;
            case CosmeticType.Part:
                _saveSystem.CurrentData.equippedPart = cosmeticId;
                break;
        }
        _saveSystem.Save();
        EventBus.RaiseCosmeticEquipped(cosmeticId);
    }

    private CosmeticDataSO GetCosmeticData(string id)
        => System.Array.Find(_allCosmetics, c => c.cosmeticId == id);
}
```

### Task 6: ProgressionManager — Implement IProgressionData

```csharp
public class ProgressionManager : MonoBehaviour, IProgressionData
{
    public static ProgressionManager Instance { get; private set; }

    private SaveSystem _saveSystem;
    private CurrencyManager _currencyManager;
    private UpgradeManager _upgradeManager;
    private CosmeticManager _cosmeticManager;

    [SerializeField] private UpgradeDataSO[] _allUpgrades;
    [SerializeField] private CosmeticDataSO[] _allCosmetics;
    [SerializeField] private CourseDataSO[] _allCourses;

    // IProgressionData
    public int Currency => _currencyManager.Currency;
    public int TotalMedals => _saveSystem.CurrentData.totalMedals;
    public int GetUpgradeLevel(string upgradeId) => _upgradeManager.GetUpgradeLevel(upgradeId);
    public float GetBestTime(string courseId) => _saveSystem.CurrentData.bestTimes.GetValueOrDefault(courseId, 0f);
    public int GetBestMedal(string courseId) => _saveSystem.CurrentData.bestMedals.GetValueOrDefault(courseId, 0);
    public bool IsCourseUnlocked(string courseId) => CheckCourseUnlocked(courseId);

    void Awake()
    {
        Instance = this;

        _saveSystem = new SaveSystem();
        _saveSystem.Initialize();

        _currencyManager = new CurrencyManager();
        _currencyManager.Initialize(_saveSystem);

        _upgradeManager = new UpgradeManager();
        _upgradeManager.Initialize(_saveSystem, _currencyManager, _allUpgrades);

        _cosmeticManager = new CosmeticManager();
        _cosmeticManager.Initialize(_saveSystem, _currencyManager, _allCosmetics);

        // Listen for course completions from Agent 3
        EventBus.OnCourseCompleted += HandleCourseCompleted;
    }

    private void HandleCourseCompleted(string courseId, float time, int medal)
    {
        var courseData = System.Array.Find(_allCourses, c => c.courseId == courseId);
        if (courseData == null) return;

        bool isFirstCompletion = !_saveSystem.CurrentData.completedCourses.ContainsKey(courseId);

        // Update best time
        float currentBest = GetBestTime(courseId);
        if (currentBest == 0f || time < currentBest)
            _saveSystem.CurrentData.bestTimes[courseId] = time;

        // Update best medal
        int currentBestMedal = GetBestMedal(courseId);
        if (medal > currentBestMedal)
        {
            _saveSystem.CurrentData.bestMedals[courseId] = medal;
            _saveSystem.CurrentData.totalMedals = _saveSystem.CurrentData.GetTotalMedals();
        }

        // Mark completed
        _saveSystem.CurrentData.completedCourses[courseId] = true;
        _saveSystem.CurrentData.totalCoursesCompleted++;

        // Award currency
        int collectibles = 0; // TODO: get from CourseManager
        _currencyManager.AwardCourseCompletion(courseData, time, medal, collectibles, isFirstCompletion);

        // Check if any new courses unlocked
        CheckAndUnlockCourses();

        _saveSystem.Save();
    }

    private bool CheckCourseUnlocked(string courseId)
    {
        var course = System.Array.Find(_allCourses, c => c.courseId == courseId);
        if (course == null) return false;

        // First course always unlocked
        if (course.courseIndex == 0 && course.environmentType == EnvironmentType.Lava)
            return true;

        // Check prerequisite completed
        if (course.prerequisiteCourse != null)
        {
            if (!_saveSystem.CurrentData.completedCourses.ContainsKey(course.prerequisiteCourse.courseId))
                return false;
        }

        // Check medal requirement
        if (_saveSystem.CurrentData.totalMedals < course.requiredMedals)
            return false;

        return true;
    }

    private void CheckAndUnlockCourses()
    {
        foreach (var course in _allCourses)
        {
            if (CheckCourseUnlocked(course.courseId))
            {
                string key = course.courseId;
                if (!_saveSystem.CurrentData.completedCourses.ContainsKey(key))
                {
                    // Newly unlocked — raise event for UI notification
                    EventBus.RaiseCourseUnlocked(key);
                }
            }
        }
    }

    // Public accessors for UI (Agent 5)
    public UpgradeManager Upgrades => _upgradeManager;
    public CosmeticManager Cosmetics => _cosmeticManager;
    public CurrencyManager CurrencyMgr => _currencyManager;
    public CourseDataSO[] AllCourses => _allCourses;
}
```

### Task 7: ScriptableObject Asset Values

Create ALL .asset files with tuned values. Here are the exact values:

**DefaultHovercraftStats.asset**
```
baseSpeed: 20
maxSpeed: 40
boostMultiplier: 1.5
boostDuration: 3
boostCooldown: 5
brakeForce: 15
turnSpeed: 3
hoverHeight: 2
hoverForce: 65
hoverDamping: 5
raycastCount: 4
maxHealth: 100
maxShield: 50
shieldRegenRate: 5
shieldRegenDelay: 3
driftFactor: 0.95
tiltSensitivity: 1
stabilizationForce: 10
```

**Upgrade Assets**
```
Upgrade_Speed:
  upgradeId: "speed"
  upgradeName: "Thruster Output"
  category: Speed
  levels: [
    { cost: 100, multiplier: 1.10, desc: "+10% max speed" }
    { cost: 250, multiplier: 1.20, desc: "+20% max speed" }
    { cost: 500, multiplier: 1.30, desc: "+30% max speed" }
    { cost: 1000, multiplier: 1.40, desc: "+40% max speed" }
    { cost: 2000, multiplier: 1.50, desc: "+50% max speed" }
  ]

Upgrade_Handling:
  upgradeId: "handling"
  upgradeName: "Gyro Stabilizers"
  category: Handling
  levels: [
    { cost: 100, multiplier: 1.08, desc: "+8% handling" }
    { cost: 250, multiplier: 1.16, desc: "+16% handling" }
    { cost: 500, multiplier: 1.24, desc: "+24% handling" }
    { cost: 1000, multiplier: 1.32, desc: "+32% handling" }
    { cost: 2000, multiplier: 1.40, desc: "+40% handling" }
  ]

Upgrade_Shield:
  upgradeId: "shield"
  upgradeName: "Energy Barrier"
  category: Shield
  levels: [
    { cost: 150, multiplier: 1.12, desc: "+12% shield capacity" }
    { cost: 350, multiplier: 1.24, desc: "+24% shield capacity" }
    { cost: 600, multiplier: 1.36, desc: "+36% shield capacity" }
    { cost: 1200, multiplier: 1.48, desc: "+48% shield capacity" }
    { cost: 2500, multiplier: 1.60, desc: "+60% shield capacity" }
  ]

Upgrade_Boost:
  upgradeId: "boost"
  upgradeName: "Nitro Injector"
  category: Boost
  levels: [
    { cost: 100, multiplier: 1.10, desc: "+10% boost power" }
    { cost: 250, multiplier: 1.20, desc: "+20% boost power" }
    { cost: 500, multiplier: 1.30, desc: "+30% boost power" }
    { cost: 1000, multiplier: 1.40, desc: "+40% boost power" }
    { cost: 2000, multiplier: 1.50, desc: "+50% boost power" }
  ]
```

**Course Assets** (See Agent 3 for level design details, you define the data)
```
Lava_Course_01: courseId="lava_01", name="Inferno Gate", env=Lava, index=0
  gold=50, silver=65, bronze=80, requiredMedals=0, prerequisite=null, difficulty=Easy

Lava_Course_02: courseId="lava_02", name="Magma Run", env=Lava, index=1
  gold=70, silver=90, bronze=110, requiredMedals=0, prerequisite=lava_01, difficulty=Medium

Lava_Course_03: courseId="lava_03", name="Eruption", env=Lava, index=2
  gold=90, silver=120, bronze=150, requiredMedals=3, prerequisite=lava_02, difficulty=Hard

Ice_Course_01: courseId="ice_01", name="Frozen Lake", env=Ice, index=0
  gold=60, silver=80, bronze=100, requiredMedals=5, prerequisite=lava_03, difficulty=Medium

Ice_Course_02: courseId="ice_02", name="Crystal Caverns", env=Ice, index=1
  gold=80, silver=105, bronze=130, requiredMedals=7, prerequisite=ice_01, difficulty=Hard

Ice_Course_03: courseId="ice_03", name="Avalanche Pass", env=Ice, index=2
  gold=100, silver=130, bronze=160, requiredMedals=9, prerequisite=ice_02, difficulty=Extreme

Toxic_Course_01: courseId="toxic_01", name="Waste Disposal", env=Toxic, index=0
  gold=65, silver=85, bronze=105, requiredMedals=12, prerequisite=ice_03, difficulty=Medium

Toxic_Course_02: courseId="toxic_02", name="The Foundry", env=Toxic, index=1
  gold=85, silver=110, bronze=140, requiredMedals=15, prerequisite=toxic_01, difficulty=Hard

Toxic_Course_03: courseId="toxic_03", name="Meltdown", env=Toxic, index=2
  gold=110, silver=145, bronze=180, requiredMedals=18, prerequisite=toxic_02, difficulty=Extreme
```

**Cosmetic Assets**
```
Color_Default: id="default", name="Hazard Yellow", type=ColorScheme, cost=0
  primary=#D4A017, secondary=#2A2A2A, accent=#FF8800

Color_RedBlack: id="red_black", name="Hellfire", type=ColorScheme, cost=200
  primary=#CC0000, secondary=#1A1A1A, accent=#FF4400

Color_BlueSilver: id="blue_silver", name="Frost Runner", type=ColorScheme, cost=200
  primary=#2244AA, secondary=#C0C0C0, accent=#00AAFF

Color_GreenBlack: id="green_black", name="Toxic Avenger", type=ColorScheme, cost=200
  primary=#228B22, secondary=#1A1A1A, accent=#44FF00

Color_Chrome: id="chrome", name="Chrome Crusher", type=ColorScheme, cost=500
  primary=#E0E0E0, secondary=#808080, accent=#FFFFFF

Decal_Skull: id="decal_skull", name="Death's Head", type=Decal, cost=100
Decal_Flames: id="decal_flames", name="Hellfire", type=Decal, cost=100
Decal_Lightning: id="decal_lightning", name="Thunder Strike", type=Decal, cost=150
Decal_RacingStripes: id="decal_stripes", name="Speed Demon", type=Decal, cost=75
Decal_Number73: id="decal_73", name="Lucky 73", type=Decal, cost=0  // default owned
```

### Task 8: Balance Sheet Document

Create `Config/BalanceSheet.md` documenting all tuning values:
- Currency earn rates per course per difficulty
- Upgrade costs and stat effects
- Medal time rationale
- Progression pacing (how many runs to afford each upgrade tier)
- Full playthrough estimate: ~4-6 hours to unlock all courses with silver medals

**Progression Pacing Target:**
```
First upgrade (any):   ~2-3 course completions (200-300 currency)
Full Tier 1 upgrades:  ~8-10 completions
Full Tier 2 upgrades:  ~20-25 completions
Full Tier 5 upgrades:  ~100+ completions (endgame goal)
All cosmetics:         ~50-60 completions
```

---

## Acceptance Criteria

- [ ] SaveSystem saves/loads JSON reliably, with backup and corruption handling
- [ ] SaveData extracted to own class with version field for future migrations
- [ ] CurrencyManager calculates rewards correctly (base + medal bonus + collectible + replay)
- [ ] UpgradeManager handles purchase flow: check afford → deduct → save → notify
- [ ] CosmeticManager handles ownership, purchase, and equip
- [ ] ProgressionManager implements IProgressionData, listens to EventBus.OnCourseCompleted
- [ ] Course unlock logic works correctly (prerequisite + medal count)
- [ ] ALL ScriptableObject .asset files created with correct values
- [ ] Balance sheet documents all tuning values
- [ ] Auto-save after every course completion and purchase
- [ ] No direct references to Hovercraft/Course/UI code — only via interfaces and EventBus

---

## Integration Contract

**What you provide to other agents:**
- Agent 2 needs: Upgrade multipliers via `UpgradeManager.GetStatMultipliers()` → applied to HovercraftStats
- Agent 3 needs: Course completion handling via `EventBus.OnCourseCompleted` → you listen and process
- Agent 5 needs: `IProgressionData` for all UI display, `UpgradeManager`/`CosmeticManager` for purchase flows

**What you consume from other agents:**
- Agent 1: `IProgressionData`, `EventBus`, `GameConstants`, all ScriptableObject class definitions
- Agent 3: `EventBus.OnCourseCompleted` (Agent 3's CourseManager raises this)
