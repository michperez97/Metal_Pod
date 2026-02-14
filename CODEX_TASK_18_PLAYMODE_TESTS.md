# Codex Task 18: PlayMode Integration Tests

> **Goal**: Replace the PlayMode test stubs with real runtime integration tests that verify game systems work together correctly. Complements the existing 90+ EditMode tests.

---

## Context

The current PlayMode tests are a single stub file (`Assets/Tests/PlayMode/IntegrationTestStubs.cs`) with placeholder tests. The EditMode tests (Task 5) cover individual systems in isolation, but don't test:
- MonoBehaviour lifecycle (Awake/Start/Update)
- Coroutine execution
- Physics interactions
- Cross-system integration at runtime
- Event propagation through EventBus
- Save/Load round-trip at runtime

PlayMode tests run in a real Unity play context with GameObjects, physics, and coroutines.

**Read these files**:
- `Assets/Tests/PlayMode/IntegrationTestStubs.cs` — Current stub (REPLACE this file)
- `Assets/Tests/EditMode/TestDataFactory.cs` — Existing factory for test data
- `Assets/Scripts/Hovercraft/HovercraftController.cs` — States, RequireComponent
- `Assets/Scripts/Hovercraft/HovercraftHealth.cs` — IDamageReceiver, events
- `Assets/Scripts/Hovercraft/HovercraftPhysics.cs` — Rigidbody, hover logic
- `Assets/Scripts/Hovercraft/HovercraftInput.cs` — Input processing
- `Assets/Scripts/Course/CourseManager.cs` — Course lifecycle, ICourseEvents
- `Assets/Scripts/Course/Checkpoint.cs` — Checkpoint trigger
- `Assets/Scripts/Course/Collectible.cs` — Collectible pickup
- `Assets/Scripts/Shared/EventBus.cs` — Global events
- `Assets/Scripts/Progression/SaveSystem.cs` — Save/Load
- `Assets/Scripts/Progression/SaveData.cs` — Data structure
- `Assets/Scripts/Progression/CurrencyManager.cs` — Currency
- `Assets/Scripts/Progression/ProgressionManager.cs` — Unified progression
- `Assets/Scripts/Core/GameManager.cs` — Singleton bootstrap
- `Assets/Scripts/Core/AudioManager.cs` — Audio pooling

---

## Files to Create/Replace

```
Assets/Tests/PlayMode/
├── IntegrationTestStubs.cs        ← REPLACE (delete old content)
├── HovercraftIntegrationTests.cs  # Hovercraft spawn, physics, damage, boost
├── CourseFlowTests.cs             # Course start → checkpoint → finish flow
├── EventBusIntegrationTests.cs    # Event propagation across systems
├── SaveLoadIntegrationTests.cs    # Save → kill → load round-trip
├── ProgressionIntegrationTests.cs # Currency earn → upgrade → apply flow
├── HazardDamageTests.cs           # Hazard → hovercraft damage pipeline
├── PlayModeTestFactory.cs         # Helper factory for runtime test objects
└── PlayModeTestBase.cs            # Base class with common setup/teardown
```

---

## Architecture

```
PlayModeTestBase (abstract)
  ├── [SetUp]: Create test scene, EventBus.Initialize()
  ├── [TearDown]: Destroy all test objects, EventBus.Shutdown()
  ├── Helper: CreateHovercraft() → fully configured hovercraft GO
  ├── Helper: CreateCheckpoint(position, index)
  ├── Helper: CreateCollectible(position, type)
  ├── Helper: WaitFrames(int)
  └── Helper: WaitSeconds(float)

PlayModeTestFactory
  ├── Creates ScriptableObject instances for tests (no .asset files needed)
  ├── CreateHovercraftStats() → HovercraftStatsSO
  ├── CreateCourseData() → CourseDataSO
  ├── CreateHazardData() → HazardDataSO
  └── CreateUpgradeData() → UpgradeDataSO
```

---

## Detailed Test Specifications

### PlayModeTestBase.cs

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MetalPod.Shared;

namespace MetalPod.Tests.PlayMode
{
    public abstract class PlayModeTestBase
    {
        protected readonly List<GameObject> createdObjects = new List<GameObject>();

        [SetUp]
        public void BaseSetUp()
        {
            EventBus.Initialize();
        }

        [TearDown]
        public void BaseTearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) Object.Destroy(obj);
            }
            createdObjects.Clear();
            EventBus.Shutdown();
        }

        protected GameObject CreateTestObject(string name = "TestObject")
        {
            var go = new GameObject(name);
            createdObjects.Add(go);
            return go;
        }

        protected IEnumerator WaitFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
                yield return null;
        }

        protected IEnumerator WaitSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
    }
}
```

### PlayModeTestFactory.cs

```csharp
// Factory for creating runtime test objects with all required components.
// No .asset files needed — creates ScriptableObjects in memory.

using UnityEngine;
using MetalPod.ScriptableObjects;
using MetalPod.Hovercraft;

namespace MetalPod.Tests.PlayMode
{
    public static class PlayModeTestFactory
    {
        public static HovercraftStatsSO CreateHovercraftStats(
            float maxSpeed = 20f, float maxHealth = 100f, float maxShield = 50f,
            float boostMultiplier = 1.5f, float boostDuration = 2f)
        {
            var stats = ScriptableObject.CreateInstance<HovercraftStatsSO>();
            // Set fields via reflection or direct assignment
            // The agent should read HovercraftStatsSO.cs to know the field names
            // and set them appropriately for testing
            return stats;
        }

        public static CourseDataSO CreateCourseData(string courseId = "test_course",
            float goldTime = 30f, float silverTime = 45f, float bronzeTime = 60f)
        {
            var data = ScriptableObject.CreateInstance<CourseDataSO>();
            // Set fields matching CourseDataSO structure
            return data;
        }

        /// <summary>
        /// Create a fully configured hovercraft GameObject with all required components.
        /// </summary>
        public static GameObject CreateHovercraft(HovercraftStatsSO stats = null)
        {
            if (stats == null) stats = CreateHovercraftStats();

            var go = new GameObject("TestHovercraft");
            go.tag = GameConstants.TAG_PLAYER;
            go.layer = LayerMask.NameToLayer("Default"); // Hovercraft layer may not exist in test

            // Add Rigidbody (required by HovercraftPhysics)
            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 10f;

            // Add required components in order
            // HovercraftController requires: HovercraftPhysics, HovercraftHealth, HovercraftInput
            go.AddComponent<HovercraftPhysics>();
            go.AddComponent<HovercraftHealth>();
            go.AddComponent<HovercraftInput>();
            go.AddComponent<HovercraftController>();

            // Assign stats via serialized field (use reflection for test)
            // The agent should use SerializedObject or reflection to set the private
            // [SerializeField] stats fields on HovercraftHealth and HovercraftController

            return go;
        }

        public static GameObject CreateCheckpoint(Vector3 position, int index)
        {
            var go = new GameObject($"Checkpoint_{index}");
            go.tag = GameConstants.TAG_CHECKPOINT;
            go.transform.position = position;

            var collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(5f, 5f, 1f);

            go.AddComponent<MetalPod.Course.Checkpoint>();
            // Set CheckpointIndex via reflection or public setter

            return go;
        }

        public static GameObject CreateCollectible(Vector3 position)
        {
            var go = new GameObject("TestCollectible");
            go.tag = GameConstants.TAG_COLLECTIBLE;
            go.transform.position = position;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1f;

            go.AddComponent<MetalPod.Course.Collectible>();

            return go;
        }
    }
}
```

### HovercraftIntegrationTests.cs

Test the hovercraft as a complete unit:

```
[UnityTest] HovercraftSpawn_HasAllRequiredComponents
  — Instantiate via factory, verify all components present

[UnityTest] HovercraftDamage_ReducesHealth
  — Spawn hovercraft, call TakeDamage(), verify health decreased after frame

[UnityTest] HovercraftDamage_FiresOnDamageEvent
  — Subscribe to OnDamage, deal damage, verify event fired with correct args

[UnityTest] HovercraftDestruction_FiresOnDestroyedEvent
  — Deal enough damage to destroy, verify OnDestroyed fires

[UnityTest] HovercraftRestoreToFull_ResetsHealth
  — Damage then RestoreToFull(), verify health back to max

[UnityTest] HovercraftShield_AbsorbsDamageFirst
  — With shield, deal damage, verify shield reduces before health

[UnityTest] HovercraftState_ChangesOnBoost
  — Simulate boost input, verify state changes to Boosting after physics tick

[UnityTest] HovercraftPhysics_RigidbodyExists
  — Verify Rigidbody is attached and configured

[UnityTest] HovercraftHealth_SpeedMultiplier_AtLowHealth
  — Set health to 25%, verify SpeedMultiplier returns 0.6
```

### CourseFlowTests.cs

Test the full course lifecycle:

```
[UnityTest] CourseManager_StartsInReadyState
  — Create CourseManager, verify CurrentState == Ready

[UnityTest] CourseManager_CountdownToRacing
  — Start course, wait through countdown, verify state == Racing

[UnityTest] Checkpoint_ActivatesOnPlayerTrigger
  — Move hovercraft through checkpoint trigger, verify OnActivated fires

[UnityTest] CourseFinish_TransitionsToFinishedState
  — Simulate crossing finish line, verify state == Finished

[UnityTest] CourseFinish_BroadcastsEventBus
  — Finish course, verify EventBus.OnCourseCompleted was invoked

[UnityTest] MedalSystem_AwardsGoldForFastTime
  — Set gold threshold, finish under gold time, verify medal == Gold

[UnityTest] MedalSystem_AwardsBronzeForSlowTime
  — Finish above silver but below bronze threshold, verify medal == Bronze
```

### EventBusIntegrationTests.cs

Test event propagation across real MonoBehaviours:

```
[UnityTest] EventBus_CurrencyChanged_PropagatesAcrossFrames
  — Subscribe in one component, raise in another, verify delivery next frame

[UnityTest] EventBus_CourseCompleted_MultipleSubscribers
  — Register 3 subscribers, raise event, verify all 3 received it

[UnityTest] EventBus_Initialize_ClearsStaleListeners
  — Subscribe, then Initialize(), verify old subscriber no longer receives

[UnityTest] EventBus_Shutdown_ClearsAllListeners
  — Subscribe, Shutdown(), raise event, verify no delivery

[UnityTest] EventBus_UpgradePurchased_CarriesCorrectData
  — Raise with id="speed", level=3, verify subscriber receives exact values
```

### SaveLoadIntegrationTests.cs

Test save persistence at runtime:

```
[UnityTest] SaveSystem_InitializeCreatesDefaultData
  — Create SaveSystem, Initialize(), verify CurrentData is not null

[UnityTest] SaveSystem_SaveAndLoad_PreservesCurrency
  — Set currency to 500, Save(), create new SaveSystem, Load(), verify 500

[UnityTest] SaveSystem_SaveAndLoad_PreservesUpgradeLevels
  — Set speed upgrade to 3, save, reload, verify level == 3

[UnityTest] SaveSystem_SaveAndLoad_PreservesBestTimes
  — Set best time for course, save, reload, verify time matches

[UnityTest] SaveSystem_SaveAndLoad_PreservesCosmetics
  — Add cosmetic to owned list, save, reload, verify ownership

[UnityTest] SaveData_CreateDefault_HasDefaultCosmetics
  — Create default, verify "default" and "decal_73" are owned

[UnityTest] SaveSystem_CorruptFile_FallsBackToDefault
  — Write garbage to save file, Initialize(), verify default data loaded
```

**IMPORTANT**: For file-based tests, use a temporary save path (not the real one) to avoid corrupting actual save data. Override the save path or use Application.temporaryCachePath.

### ProgressionIntegrationTests.cs

Test the full progression pipeline:

```
[UnityTest] ProgressionManager_InitializesAllSubsystems
  — Create ProgressionManager, verify CurrencyMgr, Upgrades, Cosmetics not null

[UnityTest] CurrencyFlow_EarnAndSpend
  — Earn 100 bolts, verify currency == 100, spend 30, verify == 70

[UnityTest] UpgradeFlow_PurchaseIncreasesLevel
  — Give currency, purchase speed upgrade, verify level increased

[UnityTest] UpgradeFlow_InsufficientFunds_Fails
  — Set currency to 0, attempt purchase, verify fails and level unchanged

[UnityTest] CosmeticFlow_PurchaseAndEquip
  — Purchase cosmetic, verify owned, equip, verify equipped

[UnityTest] CourseCompletion_AwardsCurrency
  — Complete course via EventBus, verify currency increased

[UnityTest] MedalImprovement_AwardsBonus
  — Get bronze, then get gold on same course, verify bonus awarded
```

### HazardDamageTests.cs

Test the hazard → damage pipeline:

```
[UnityTest] HazardBase_DealsDamageOnContact
  — Create hazard with DamageZone, move hovercraft into it, verify damage applied

[UnityTest] DamageType_AppliedCorrectly
  — Create Fire damage hazard, deal damage, verify DamageType in event is Fire

[UnityTest] HazardWarning_ActivatesBeforeDamage
  — If HazardWarning exists, verify it activates before hazard triggers

[UnityTest] MultipleHazards_StackDamage
  — Place hovercraft in two damage zones, verify both apply damage

[UnityTest] Invincibility_BlocksDamage
  — Set IsInvincible, apply hazard damage, verify health unchanged
```

---

## Implementation Notes

1. **Use `[UnityTest]`** for all tests that need coroutine support (most of them)
2. **Use `[Test]`** only for synchronous assertions
3. **Use `ScriptableObject.CreateInstance<T>()`** for test SOs — never load from assets
4. **Use `Application.temporaryCachePath`** for save file tests to avoid side effects
5. **Clean up in `[TearDown]`** — destroy all GameObjects, clear EventBus
6. **Use `yield return new WaitForFixedUpdate()`** before physics assertions
7. **Use `yield return null`** (one frame) before checking MonoBehaviour state after Awake/Start
8. **Use reflection** to set `[SerializeField]` private fields in tests when needed:
   ```csharp
   var field = typeof(HovercraftHealth).GetField("stats",
       System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
   field.SetValue(healthComponent, testStats);
   ```

---

## Acceptance Criteria

- [ ] `IntegrationTestStubs.cs` — REPLACED (old stubs deleted, clean file)
- [ ] `PlayModeTestBase.cs` — Base class with setup/teardown and helpers
- [ ] `PlayModeTestFactory.cs` — Factory creating test GameObjects and ScriptableObjects
- [ ] `HovercraftIntegrationTests.cs` — 9+ tests covering spawn, damage, shield, state, physics
- [ ] `CourseFlowTests.cs` — 7+ tests covering course lifecycle and medal system
- [ ] `EventBusIntegrationTests.cs` — 5+ tests covering event propagation
- [ ] `SaveLoadIntegrationTests.cs` — 7+ tests covering save/load round-trip
- [ ] `ProgressionIntegrationTests.cs` — 7+ tests covering currency/upgrade/cosmetic flow
- [ ] `HazardDamageTests.cs` — 5+ tests covering hazard → damage pipeline
- [ ] **Total: 40+ PlayMode tests**
- [ ] All tests in `MetalPod.Tests.PlayMode` namespace
- [ ] Tests use temporary paths (not real save data)
- [ ] Tests clean up after themselves (no leaked GameObjects)
- [ ] All `using` statements resolve to existing namespaces
- [ ] No modifications to existing files except replacing IntegrationTestStubs.cs
