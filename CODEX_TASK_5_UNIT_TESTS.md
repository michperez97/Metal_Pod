# Codex Task 5: Unit Tests

> **Goal**: Write comprehensive NUnit/Unity Test Framework tests for all systems that can be tested without Unity runtime (pure C# logic). This catches bugs before we ever open Unity.

---

## Context

Metal Pod has multiple systems with testable pure logic: save/load, currency calculations, medal evaluation, upgrade math, unlock progression, damage calculations, and more. Write tests using Unity Test Framework (NUnit) in EditMode — these run without entering Play mode.

**Read these files to understand what to test**:
- `Assets/Scripts/Progression/` — SaveSystem, CurrencyManager, UpgradeManager, CosmeticManager, ProgressionManager, SaveData
- `Assets/Scripts/Course/MedalSystem.cs` — Medal evaluation
- `Assets/Scripts/Shared/` — Interfaces, EventBus, GameConstants
- `Assets/ScriptableObjects/` — All SO class definitions
- `Config/BalanceSheet.md` — Expected balance values

---

## Files to Create

```
Assets/Tests/
├── EditMode/
│   ├── MetalPod.Tests.EditMode.asmdef    # Assembly definition for EditMode tests
│   ├── SaveSystemTests.cs
│   ├── SaveDataTests.cs
│   ├── CurrencyManagerTests.cs
│   ├── UpgradeManagerTests.cs
│   ├── CosmeticManagerTests.cs
│   ├── MedalSystemTests.cs
│   ├── ProgressionManagerTests.cs
│   ├── CourseUnlockTests.cs
│   ├── EventBusTests.cs
│   ├── GameConstantsTests.cs
│   ├── DamageCalculationTests.cs
│   └── BalanceValidationTests.cs
└── PlayMode/
    ├── MetalPod.Tests.PlayMode.asmdef    # Assembly definition for PlayMode tests
    └── IntegrationTestStubs.cs            # Placeholder for future play mode tests
```

---

## Assembly Definition Files

### MetalPod.Tests.EditMode.asmdef
```json
{
    "name": "MetalPod.Tests.EditMode",
    "rootNamespace": "MetalPod.Tests.EditMode",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

NOTE: If the main project scripts aren't in an assembly definition, the test assembly may need to reference them. If they ARE in an asmdef, add that reference. If they're NOT, the tests can access them directly since they're in the default assembly. Adjust the .asmdef as needed.

### MetalPod.Tests.PlayMode.asmdef
```json
{
    "name": "MetalPod.Tests.PlayMode",
    "rootNamespace": "MetalPod.Tests.PlayMode",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

---

## Test Specifications

### SaveDataTests.cs

```csharp
using NUnit.Framework;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class SaveDataTests
    {
        [Test]
        public void CreateDefault_HasZeroCurrency()
        {
            var data = SaveData.CreateDefault();
            Assert.AreEqual(0, data.currency);
        }

        [Test]
        public void CreateDefault_OwnsDefaultCosmetic()
        {
            var data = SaveData.CreateDefault();
            Assert.IsTrue(data.ownedCosmetics.Contains("default"));
        }

        [Test]
        public void CreateDefault_VersionIsOne()
        {
            var data = SaveData.CreateDefault();
            Assert.AreEqual(1, data.version);
        }

        [Test]
        public void GetTotalMedals_NoMedals_ReturnsZero()
        {
            var data = SaveData.CreateDefault();
            Assert.AreEqual(0, data.GetTotalMedals());
        }

        [Test]
        public void GetTotalMedals_WithMedals_CountsCorrectly()
        {
            var data = SaveData.CreateDefault();
            // Add medals - the exact API depends on the SaveData implementation
            // This tests that only non-zero medals are counted
            // Adapt to actual SaveData API
        }

        [Test]
        public void CreateDefault_NoCourseProgress()
        {
            var data = SaveData.CreateDefault();
            Assert.AreEqual(0, data.totalCoursesCompleted);
            Assert.AreEqual(0, data.totalMedals);
        }

        [Test]
        public void CreateDefault_NoUpgrades()
        {
            var data = SaveData.CreateDefault();
            // Verify no upgrade levels set
            // Adapt to actual dictionary API
        }
    }
}
```

### SaveSystemTests.cs

```csharp
[TestFixture]
public class SaveSystemTests
{
    private string _testSavePath;

    [SetUp]
    public void SetUp()
    {
        // Use a temp directory for test saves
        _testSavePath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "MetalPodTestSave");
        System.IO.Directory.CreateDirectory(_testSavePath);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test files
        if (System.IO.Directory.Exists(_testSavePath))
            System.IO.Directory.Delete(_testSavePath, true);
    }

    [Test]
    public void Save_ThenLoad_DataPreserved()
    {
        // Create SaveSystem, set some data, save, create new instance, load
        // Verify data matches
    }

    [Test]
    public void Load_NoSaveFile_ReturnsNull()
    {
        // Verify graceful handling of missing save file
    }

    [Test]
    public void Load_CorruptedFile_ReturnsNull()
    {
        // Write garbage to save file path
        // Verify it doesn't crash, returns null
    }

    [Test]
    public void Save_CreatesBackup()
    {
        // Save once, save again
        // Verify backup file exists
    }

    [Test]
    public void Save_PreservesCurrency()
    {
        // Set currency to 500, save, load, verify 500
    }

    [Test]
    public void Save_PreservesUpgrades()
    {
        // Set upgrade level, save, load, verify
    }

    [Test]
    public void Save_PreservesBestTimes()
    {
        // Set best time, save, load, verify
    }

    [Test]
    public void Save_PreservesCosmetics()
    {
        // Add cosmetic, save, load, verify owned
    }
}
```

### CurrencyManagerTests.cs

```csharp
[TestFixture]
public class CurrencyManagerTests
{
    // NOTE: CurrencyManager may require SaveSystem dependency.
    // Either test via ProgressionManager or mock/stub the dependency.
    // Adapt to actual class structure.

    [Test]
    public void AddCurrency_IncreasesBalance()
    {
        // Start at 0, add 100, verify 100
    }

    [Test]
    public void SpendCurrency_DecreasesBalance()
    {
        // Start at 200, spend 100, verify 100
    }

    [Test]
    public void SpendCurrency_InsufficientFunds_ReturnsFalse()
    {
        // Start at 50, try to spend 100, verify returns false, balance unchanged
    }

    [Test]
    public void SpendCurrency_ExactAmount_ReturnsTrue_BalanceZero()
    {
        // Start at 100, spend 100, verify returns true, balance = 0
    }

    [Test]
    public void CanAfford_SufficientFunds_ReturnsTrue()
    {
        // Balance 200, canAfford(150) = true
    }

    [Test]
    public void CanAfford_InsufficientFunds_ReturnsFalse()
    {
        // Balance 50, canAfford(100) = false
    }

    [Test]
    public void AwardCourseCompletion_GoldMedal_HigherReward()
    {
        // Complete course with gold → more currency than bronze
    }

    [Test]
    public void AwardCourseCompletion_FirstCompletion_FullReward()
    {
        // First completion = 100% reward
    }

    [Test]
    public void AwardCourseCompletion_Replay_ReducedReward()
    {
        // Replay = 50% of base reward (REPLAY_REWARD_MULTIPLIER)
    }

    [Test]
    public void AwardCourseCompletion_WithCollectibles_BonusAdded()
    {
        // 5 collectibles = 50 bonus (10 each)
    }

    [Test]
    public void AwardCourseCompletion_NoMedal_BaseRewardOnly()
    {
        // Complete without medal → base reward only, no medal bonus
    }

    [Test]
    public void AwardCourseCompletion_EasyVsHard_DifferentBaseRewards()
    {
        // Easy = 100 base, Hard = 200 base (per BalanceSheet)
    }
}
```

### MedalSystemTests.cs

```csharp
[TestFixture]
public class MedalSystemTests
{
    // MedalSystem.EvaluatePerformance(float time, CourseDataSO course)
    // Need to create CourseDataSO instances with known medal times

    [Test]
    public void EvaluatePerformance_UnderGoldTime_ReturnsGold()
    {
        // Course gold=50, time=45 → Gold
    }

    [Test]
    public void EvaluatePerformance_ExactGoldTime_ReturnsGold()
    {
        // Course gold=50, time=50 → Gold
    }

    [Test]
    public void EvaluatePerformance_BetweenGoldAndSilver_ReturnsSilver()
    {
        // Course gold=50, silver=65, time=55 → Silver
    }

    [Test]
    public void EvaluatePerformance_ExactSilverTime_ReturnsSilver()
    {
        // Course silver=65, time=65 → Silver
    }

    [Test]
    public void EvaluatePerformance_BetweenSilverAndBronze_ReturnsBronze()
    {
        // Course silver=65, bronze=80, time=70 → Bronze
    }

    [Test]
    public void EvaluatePerformance_ExactBronzeTime_ReturnsBronze()
    {
        // Course bronze=80, time=80 → Bronze
    }

    [Test]
    public void EvaluatePerformance_AboveBronze_ReturnsNone()
    {
        // Course bronze=80, time=100 → None
    }

    [Test]
    public void EvaluatePerformance_AllLavaCourses_ValidThresholds()
    {
        // Verify: gold < silver < bronze for each lava course
        // Lava 1: 50 < 65 < 80  ✓
        // Lava 2: 70 < 90 < 110 ✓
        // Lava 3: 90 < 120 < 150 ✓
    }

    [Test]
    public void EvaluatePerformance_AllIceCourses_ValidThresholds()
    {
        // Ice 1: 60 < 80 < 100
        // Ice 2: 80 < 105 < 130
        // Ice 3: 100 < 130 < 160
    }

    [Test]
    public void EvaluatePerformance_AllToxicCourses_ValidThresholds()
    {
        // Toxic 1: 65 < 85 < 105
        // Toxic 2: 85 < 110 < 140
        // Toxic 3: 110 < 145 < 180
    }
}
```

### UpgradeManagerTests.cs

```csharp
[TestFixture]
public class UpgradeManagerTests
{
    [Test]
    public void GetUpgradeLevel_NoPurchases_ReturnsZero()
    {
        // New game, no upgrades → level 0 for all
    }

    [Test]
    public void TryPurchaseUpgrade_SufficientFunds_ReturnsTrue()
    {
        // 200 currency, speed level 0, cost 100 → success, level becomes 1
    }

    [Test]
    public void TryPurchaseUpgrade_InsufficientFunds_ReturnsFalse()
    {
        // 50 currency, cost 100 → fail, level stays 0
    }

    [Test]
    public void TryPurchaseUpgrade_MaxLevel_ReturnsFalse()
    {
        // Already at level 5 (max) → can't purchase more
    }

    [Test]
    public void TryPurchaseUpgrade_DeductsCurrency()
    {
        // Start 500, upgrade costs 100 → balance becomes 400
    }

    [Test]
    public void GetNextLevelCost_IncreasesPerLevel()
    {
        // Speed: 100, 250, 500, 1000, 2000
        // Verify each level costs more
    }

    [Test]
    public void GetStatMultipliers_NoUpgrades_AllOnes()
    {
        // No upgrades → speed=1.0, handling=1.0, shield=1.0, boost=1.0
    }

    [Test]
    public void GetStatMultipliers_SpeedLevel3_Correct()
    {
        // Speed level 3 → 1.0 + 3*0.1 = 1.3
    }

    [Test]
    public void GetStatMultipliers_AllMaxLevel_Correct()
    {
        // All at level 5 → speed=1.5, handling=1.4, shield=1.6, boost=1.5
    }

    [Test]
    public void UpgradeCosts_MatchBalanceSheet()
    {
        // Verify costs match Config/BalanceSheet.md exactly:
        // Speed:    100, 250, 500, 1000, 2000
        // Handling: 100, 250, 500, 1000, 2000
        // Shield:   150, 350, 600, 1200, 2500
        // Boost:    100, 250, 500, 1000, 2000
    }

    [Test]
    public void TotalCostToMaxAllUpgrades()
    {
        // Speed: 100+250+500+1000+2000 = 3850
        // Handling: same = 3850
        // Shield: 150+350+600+1200+2500 = 4800
        // Boost: 3850
        // Total: 16350
        // Verify this is achievable with reasonable play (per BalanceSheet)
    }
}
```

### CosmeticManagerTests.cs

```csharp
[TestFixture]
public class CosmeticManagerTests
{
    [Test]
    public void OwnsCosmetic_Default_ReturnsTrue()
    {
        // "default" color scheme is owned from start
    }

    [Test]
    public void OwnsCosmetic_Unpurchased_ReturnsFalse()
    {
        // "red_black" not purchased → false
    }

    [Test]
    public void TryPurchaseCosmetic_SufficientFunds_Succeeds()
    {
        // 300 currency, "red_black" costs 200 → success
    }

    [Test]
    public void TryPurchaseCosmetic_AlreadyOwned_ReturnsFalse()
    {
        // Can't buy something you already own
    }

    [Test]
    public void EquipCosmetic_Owned_ChangesEquipped()
    {
        // Own "red_black", equip it → equipped changes
    }

    [Test]
    public void EquipCosmetic_NotOwned_NoChange()
    {
        // Don't own it → equip fails silently
    }
}
```

### CourseUnlockTests.cs

```csharp
[TestFixture]
public class CourseUnlockTests
{
    [Test]
    public void FirstCourse_AlwaysUnlocked()
    {
        // lava_01 is always unlocked regardless of state
    }

    [Test]
    public void SecondCourse_RequiresFirstCompleted()
    {
        // lava_02 requires lava_01 completed
    }

    [Test]
    public void IceFirstCourse_RequiresAllLavaAndFiveMedals()
    {
        // ice_01 requires: lava_03 completed + 5 total medals
    }

    [Test]
    public void ToxicFirstCourse_RequiresTwelveMedals()
    {
        // toxic_01 requires: ice_03 completed + 12 total medals
    }

    [Test]
    public void FinalCourse_RequiresEighteenMedals()
    {
        // toxic_03 requires: toxic_02 completed + 18 total medals
    }

    [Test]
    public void CourseNotUnlocked_PrerequisiteNotMet()
    {
        // lava_02 with lava_01 NOT completed → locked
    }

    [Test]
    public void CourseNotUnlocked_MedalRequirementNotMet()
    {
        // ice_01 with lava_03 completed but only 3 medals → locked
    }

    [Test]
    public void FullProgression_AllUnlocksInOrder()
    {
        // Simulate completing every course with gold (3 medals each)
        // Verify each course unlocks at the right point:
        // After lava_01 (1 medal): lava_02 unlocks
        // After lava_02 (2 medals): nothing new yet (need 3 for lava_03)
        // After lava_03 (3 medals): lava_03 itself needed 3, ice_01 needs 5
        // After replaying for 5 medals: ice_01 unlocks
        // ... etc
    }
}
```

### EventBusTests.cs

```csharp
[TestFixture]
public class EventBusTests
{
    [SetUp]
    public void SetUp()
    {
        // Reset EventBus state if it has Initialize/Shutdown
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up listeners
    }

    [Test]
    public void OnCurrencyChanged_FiresWithCorrectValue()
    {
        int received = -1;
        EventBus.OnCurrencyChanged += (val) => received = val;
        EventBus.RaiseCurrencyChanged(500);
        Assert.AreEqual(500, received);
    }

    [Test]
    public void OnCourseCompleted_FiresWithAllParams()
    {
        string receivedId = null;
        float receivedTime = 0;
        int receivedMedal = 0;

        EventBus.OnCourseCompleted += (id, time, medal) => {
            receivedId = id;
            receivedTime = time;
            receivedMedal = medal;
        };

        EventBus.RaiseCourseCompleted("lava_01", 52.5f, 3);

        Assert.AreEqual("lava_01", receivedId);
        Assert.AreEqual(52.5f, receivedTime, 0.01f);
        Assert.AreEqual(3, receivedMedal);
    }

    [Test]
    public void OnUpgradePurchased_FiresWithIdAndLevel()
    {
        string receivedId = null;
        int receivedLevel = 0;

        EventBus.OnUpgradePurchased += (id, level) => {
            receivedId = id;
            receivedLevel = level;
        };

        EventBus.RaiseUpgradePurchased("speed", 3);

        Assert.AreEqual("speed", receivedId);
        Assert.AreEqual(3, receivedLevel);
    }

    [Test]
    public void MultipleListeners_AllReceiveEvent()
    {
        int count = 0;
        EventBus.OnCurrencyChanged += (_) => count++;
        EventBus.OnCurrencyChanged += (_) => count++;
        EventBus.RaiseCurrencyChanged(100);
        Assert.AreEqual(2, count);
    }

    [Test]
    public void NoListeners_RaiseDoesNotThrow()
    {
        Assert.DoesNotThrow(() => EventBus.RaiseCurrencyChanged(100));
        Assert.DoesNotThrow(() => EventBus.RaiseCourseCompleted("test", 0, 0));
    }
}
```

### BalanceValidationTests.cs

```csharp
[TestFixture]
public class BalanceValidationTests
{
    [Test]
    public void AllCourses_GoldLessThanSilverLessThanBronze()
    {
        // For every course, verify: gold < silver < bronze
        var courses = new (float gold, float silver, float bronze)[]
        {
            (50, 65, 80),     // Lava 1
            (70, 90, 110),    // Lava 2
            (90, 120, 150),   // Lava 3
            (60, 80, 100),    // Ice 1
            (80, 105, 130),   // Ice 2
            (100, 130, 160),  // Ice 3
            (65, 85, 105),    // Toxic 1
            (85, 110, 140),   // Toxic 2
            (110, 145, 180),  // Toxic 3
        };

        foreach (var c in courses)
        {
            Assert.Less(c.gold, c.silver, "Gold must be less than Silver");
            Assert.Less(c.silver, c.bronze, "Silver must be less than Bronze");
        }
    }

    [Test]
    public void DifficultyProgression_TimesIncreaseWithDifficulty()
    {
        // Later courses should generally have longer gold times
        // Lava 1 (50) < Lava 2 (70) < Lava 3 (90) ✓
        // Ice 1 (60) < Ice 2 (80) < Ice 3 (100) ✓
        // Toxic 1 (65) < Toxic 2 (85) < Toxic 3 (110) ✓
    }

    [Test]
    public void UnlockRequirements_AreAchievable()
    {
        // Max medals possible from courses before each unlock:
        // After Lava (3 courses max 3 medals each) = 9 possible medals
        // Ice 1 needs 5: achievable with 5/9 ✓
        // After Lava+Ice (6 courses) = 18 possible
        // Toxic 1 needs 12: achievable with 12/18 ✓
        // Toxic 3 needs 18: achievable with 18/18 (requires gold on everything!)
    }

    [Test]
    public void UpgradeEconomy_FirstUpgradeAfterThreeCompletions()
    {
        // Easy course base reward: 100
        // Gold bonus: +100% → 200 per easy course gold
        // After 1 gold completion: 200 currency → can afford first Speed/Handling/Boost upgrade (100)
        // This is good — players should afford first upgrade quickly
    }

    [Test]
    public void UpgradeEconomy_MaxUpgradesRequireSignificantPlay()
    {
        // Total cost to max all: 16350
        // Average earn per course (medium, silver): 150 * 1.5 * 0.5(replay) ≈ 112
        // Courses needed: ~146 runs to max everything
        // This seems like a lot — verify it's intentional endgame content
    }

    [Test]
    public void CosmeticPricing_ReasonableForCasualPlayers()
    {
        // Most cosmetics: 100-200 currency
        // After 2-3 completions players can buy first cosmetic
        // Chrome (most expensive at 500): after ~5-6 completions
    }

    [Test]
    public void MedalGapRatio_ConsistentAcrossCourses()
    {
        // Gap between gold-silver and silver-bronze should be proportional
        // Lava 1: gold=50, gap to silver=15, gap to bronze=15 (equal gaps)
        // Lava 2: gold=70, gap=20, gap=20 (equal)
        // Lava 3: gold=90, gap=30, gap=30 (equal)
        // This is consistent — 30% gap pattern
    }
}
```

### DamageCalculationTests.cs

```csharp
[TestFixture]
public class DamageCalculationTests
{
    [Test]
    public void DamageTypeModifier_Physical_IsOne()
    {
        // Physical damage has no modifier (1.0x)
    }

    [Test]
    public void DamageTypeModifier_Electric_IsHigher()
    {
        // Electric does 1.2x (bypasses some shield)
    }

    [Test]
    public void DamageTypeModifier_Explosive_IsHighest()
    {
        // Explosive does 1.5x
    }

    [Test]
    public void ShieldAbsorbsFirst()
    {
        // 50 shield, 100 health, take 30 damage
        // Result: 20 shield, 100 health
    }

    [Test]
    public void DamageOverflowsToHealth()
    {
        // 20 shield, 100 health, take 50 damage
        // Result: 0 shield, 70 health
    }

    [Test]
    public void PerformanceDegradation_Below50()
    {
        // Health at 40% → speed multiplier = 0.8
    }

    [Test]
    public void PerformanceDegradation_Below25()
    {
        // Health at 20% → speed multiplier = 0.6, handling = 0.7
    }

    [Test]
    public void PerformanceDegradation_Above50_NoEffect()
    {
        // Health at 80% → no speed/handling reduction
    }
}
```

### GameConstantsTests.cs

```csharp
[TestFixture]
public class GameConstantsTests
{
    [Test]
    public void AllTagConstants_NotEmpty()
    {
        Assert.IsNotEmpty(GameConstants.TAG_PLAYER);
        Assert.IsNotEmpty(GameConstants.TAG_CHECKPOINT);
        Assert.IsNotEmpty(GameConstants.TAG_HAZARD);
        Assert.IsNotEmpty(GameConstants.TAG_COLLECTIBLE);
        Assert.IsNotEmpty(GameConstants.TAG_FINISH);
    }

    [Test]
    public void AllLayerConstants_NotEmpty()
    {
        Assert.IsNotEmpty(GameConstants.LAYER_HOVERCRAFT);
        Assert.IsNotEmpty(GameConstants.LAYER_GROUND);
        Assert.IsNotEmpty(GameConstants.LAYER_HAZARD);
        Assert.IsNotEmpty(GameConstants.LAYER_COLLECTIBLE);
    }

    [Test]
    public void MedalBonuses_Increasing()
    {
        Assert.Greater(GameConstants.MEDAL_BONUS_SILVER, GameConstants.MEDAL_BONUS_BRONZE);
        Assert.Greater(GameConstants.MEDAL_BONUS_GOLD, GameConstants.MEDAL_BONUS_SILVER);
    }

    [Test]
    public void ReplayMultiplier_LessThanOne()
    {
        Assert.Less(GameConstants.REPLAY_REWARD_MULTIPLIER, 1f);
        Assert.Greater(GameConstants.REPLAY_REWARD_MULTIPLIER, 0f);
    }

    [Test]
    public void HealthThresholds_Ordered()
    {
        Assert.Less(GameConstants.HEALTH_SPEED_THRESHOLD_25,
            GameConstants.HEALTH_SPEED_THRESHOLD_50);
    }
}
```

---

## Implementation Notes

- Since many systems (CurrencyManager, UpgradeManager, etc.) depend on SaveSystem and ScriptableObjects, you may need to:
  1. Create test helper methods that instantiate ScriptableObjects in memory: `ScriptableObject.CreateInstance<CourseDataSO>()`
  2. Create mock/stub SaveSystem instances with test data
  3. Use `[SetUp]` to initialize clean state before each test
  4. Use `[TearDown]` to clean up temp files

- For tests that need a CourseDataSO with specific medal times, create them inline:
```csharp
private CourseDataSO CreateTestCourse(float gold, float silver, float bronze)
{
    var course = ScriptableObject.CreateInstance<CourseDataSO>();
    course.goldTime = gold;
    course.silverTime = silver;
    course.bronzeTime = bronze;
    return course;
}
```

- If classes are tightly coupled to MonoBehaviour, test the pure logic methods only or extract logic into testable helper classes.

---

## Acceptance Criteria

- [ ] Assembly definition files created for both EditMode and PlayMode
- [ ] SaveDataTests: 7+ tests covering creation, defaults, medal counting
- [ ] SaveSystemTests: 8+ tests covering save/load/corruption/backup
- [ ] CurrencyManagerTests: 11+ tests covering add/spend/afford/rewards
- [ ] MedalSystemTests: 10+ tests covering all medal thresholds for all courses
- [ ] UpgradeManagerTests: 11+ tests covering purchase/cost/multipliers
- [ ] CosmeticManagerTests: 6+ tests covering ownership/purchase/equip
- [ ] CourseUnlockTests: 8+ tests covering full progression chain
- [ ] EventBusTests: 5+ tests covering event firing/listening
- [ ] BalanceValidationTests: 7+ tests validating game economy
- [ ] DamageCalculationTests: 8+ tests for damage pipeline
- [ ] GameConstantsTests: 5+ tests for constant validity
- [ ] Total: ~90+ test cases
- [ ] All tests should pass when run in Unity Test Runner (EditMode)
