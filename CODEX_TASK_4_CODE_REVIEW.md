# Codex Task 4: Cross-Agent Code Review & Integration Fixes

> **Goal**: Review all 75+ C# scripts for compilation errors, namespace conflicts, missing references, interface mismatches, and integration gaps between the 5 agents. Fix everything found.

---

## Context

5 Codex agents wrote code independently for the Metal Pod project. While they followed shared interface contracts, they worked in isolation and may have:
- Used different namespace conventions
- Referenced methods/properties that don't exist on the target class
- Had mismatched event signatures
- Missing `using` statements
- Incompatible method signatures across interface implementations
- Dead code from deleted files (CourseEventBus.cs, ICourseEvents.cs in Course/, etc.)

**Your job**: Read EVERY .cs file, verify it compiles logically, and fix ALL issues.

---

## Files to Review (ALL of these)

Read every file. Fix in place. Do NOT create new files unless absolutely necessary.

### Shared Contracts (Agent 1) — The Source of Truth
```
Assets/Scripts/Shared/
├── IHovercraftData.cs
├── IDamageReceiver.cs      ← defines DamageType enum
├── ICourseEvents.cs
├── IProgressionData.cs
├── GameConstants.cs
├── EventBus.cs
```

### Core (Agent 1)
```
Assets/Scripts/Core/
├── GameManager.cs
├── GameStateManager.cs
├── SceneLoader.cs
└── AudioManager.cs
```

### Hovercraft (Agent 2)
```
Assets/Scripts/Hovercraft/
├── HovercraftPhysics.cs
├── HovercraftInput.cs
├── HovercraftController.cs
├── HovercraftHealth.cs      ← must implement IDamageReceiver
├── HovercraftStats.cs       ← must implement IHovercraftData
├── HovercraftVisuals.cs
├── HovercraftAudio.cs
└── HovercraftCustomization.cs
```

### Course (Agent 3)
```
Assets/Scripts/Course/
├── CourseManager.cs          ← must implement ICourseEvents
├── Checkpoint.cs
├── CourseTimer.cs
├── MedalSystem.cs
├── Collectible.cs
└── FinishLine.cs
```

### Hazards (Agent 3)
```
Assets/Scripts/Hazards/
├── HazardBase.cs             ← must use IDamageReceiver
├── DamageZone.cs
├── HazardWarning.cs
├── FallingDebris.cs
├── ToxicZone.cs
├── Lava/LavaFlow.cs
├── Lava/VolcanicEruption.cs
├── Lava/LavaGeyser.cs
├── Lava/HeatZone.cs
├── Ice/IcePatch.cs
├── Ice/FallingIcicle.cs
├── Ice/BlizzardZone.cs
├── Ice/IceWall.cs
├── Ice/Avalanche.cs
├── Toxic/ToxicGas.cs
├── Toxic/AcidPool.cs
├── Toxic/IndustrialPress.cs
├── Toxic/ElectricFence.cs
└── Toxic/BarrelExplosion.cs
```

### Progression (Agent 4)
```
Assets/Scripts/Progression/
├── SaveSystem.cs
├── SaveData.cs
├── CurrencyManager.cs
├── UpgradeManager.cs
├── CosmeticManager.cs
├── ProgressionManager.cs     ← must implement IProgressionData
├── CourseUnlockData.cs
└── Editor/Agent4DataAssetGenerator.cs
```

### UI (Agent 5)
```
Assets/Scripts/UI/
├── UIManager.cs
├── MainMenuUI.cs
├── HUD.cs                    ← must read from IHovercraftData + ICourseEvents
├── PauseMenuUI.cs
├── ResultsScreenUI.cs
├── SettingsUI.cs
├── CountdownUI.cs
├── LoadingScreenUI.cs
├── CourseUnlockedPopup.cs
├── CurrencyDisplay.cs
├── HeavyMetalTheme.cs
└── SharedContractsBridge.cs
```

### Workshop (Agent 5)
```
Assets/Scripts/Workshop/
├── WorkshopManager.cs
├── WorkshopCameraController.cs
├── UpgradeUI.cs
├── CustomizationUI.cs
├── CourseSelectionUI.cs
├── UpgradeSystem.cs
├── CustomizationSystem.cs
└── ProtagonistController.cs
```

### ScriptableObjects (Agent 1)
```
Assets/ScriptableObjects/
├── HovercraftStatsSO.cs
├── CourseDataSO.cs
├── UpgradeDataSO.cs
├── HazardDataSO.cs
├── EnvironmentDataSO.cs
├── CosmeticDataSO.cs
└── GameConfigSO.cs
```

---

## Specific Things to Check

### 1. Namespace Consistency
All scripts should use namespaces following this pattern:
```
MetalPod.Core
MetalPod.Shared
MetalPod.Hovercraft
MetalPod.Course
MetalPod.Hazards
MetalPod.Hazards.Lava
MetalPod.Hazards.Ice
MetalPod.Hazards.Toxic
MetalPod.Progression
MetalPod.UI
MetalPod.Workshop
```

If any scripts are missing namespaces or using wrong ones, fix them. Ensure all `using` statements reference the correct namespaces.

### 2. Interface Implementation Verification

**HovercraftHealth MUST implement IDamageReceiver:**
```csharp
public class HovercraftHealth : MonoBehaviour, IDamageReceiver
{
    public void TakeDamage(float amount) { ... }
    public void TakeDamage(float amount, DamageType type) { ... }
    public void RestoreHealth(float amount) { ... }
    public void RestoreShield(float amount) { ... }
}
```
Verify ALL 4 methods exist with exact signatures.

**HovercraftStats MUST implement IHovercraftData:**
```csharp
public class HovercraftStats : MonoBehaviour, IHovercraftData
{
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public float CurrentShield { get; }
    public float MaxShield { get; }
    public float CurrentSpeed { get; }
    public float MaxSpeed { get; }
    public float HealthNormalized { get; }
    public float ShieldNormalized { get; }
    public float BoostCooldownNormalized { get; }
    public bool IsBoosting { get; }
    public bool IsDestroyed { get; }
}
```
Verify ALL 11 properties exist.

**CourseManager MUST implement ICourseEvents:**
Verify all events from the interface are declared.

**ProgressionManager MUST implement IProgressionData:**
Verify all methods from the interface are implemented.

### 3. EventBus Usage Consistency

Check that:
- `EventBus.RaiseCourseCompleted(courseId, time, medal)` is called in CourseManager when race finishes
- `EventBus.RaiseCurrencyChanged(total)` is called in CurrencyManager after every change
- `EventBus.RaiseCurrencyEarned(amount)` is called on course completion
- `EventBus.RaiseUpgradePurchased(id, level)` is called in UpgradeManager after purchase
- `EventBus.RaiseCosmeticEquipped(id)` is called in CosmeticManager
- `EventBus.RaiseCourseUnlocked(id)` is called in ProgressionManager
- `EventBus.RaiseCourseSelected(id)` is called in WorkshopManager/CourseSelectionUI

Check event signatures match between raiser and listener.

### 4. DamageType Enum Location

`DamageType` should be defined in `Assets/Scripts/Shared/IDamageReceiver.cs` ONLY. No duplicate definitions. All hazards must `using MetalPod.Shared;` to access it.

### 5. Deleted Files — Dead References

These files were deleted by agents:
- `Assets/Scripts/Course/CourseEventBus.cs` — DELETED
- `Assets/Scripts/Course/ICourseEvents.cs` — DELETED (moved to Shared)
- `Assets/Scripts/Hazards/HovercraftDamageReceiverAdapter.cs` — DELETED
- `Assets/Scripts/Hazards/IDamageReceiver.cs` — DELETED (moved to Shared)
- `Assets/Scripts/Progression/IProgressionDataFallback.cs` — DELETED
- `Assets/Scripts/Progression/SharedBridge.cs` — DELETED

Check ALL remaining files for any `using` statements or references to these deleted files/classes. Remove dead references.

### 6. Method Signatures That Must Match

**HazardBase → IDamageReceiver:**
HazardBase calls `TakeDamage(amount, _damageType)` — verify IDamageReceiver has that overload.

**CourseManager → EventBus:**
On finish: `EventBus.RaiseCourseCompleted(courseId, completionTime, medalInt)` — verify EventBus accepts `(string, float, int)`.

**ProgressionManager → EventBus listener:**
`EventBus.OnCourseCompleted += HandleCourseCompleted` — handler must accept `(string, float, int)`.

**HUD → IHovercraftData:**
HUD reads `.HealthNormalized`, `.ShieldNormalized`, `.CurrentSpeed`, `.BoostCooldownNormalized` — verify these all exist on the interface AND the implementing class.

### 7. SerializeField References

Check that `[SerializeField]` private fields have matching types for what they're expected to reference. Common issues:
- `AudioClip` fields in scripts where audio isn't connected yet (fine, just nullable)
- `ParticleSystem` references (fine, will be null until prefabs are set up)
- `Image`, `TextMeshProUGUI`, `Button` references in UI scripts — ensure `using UnityEngine.UI` and `using TMPro` are present

### 8. Missing Using Statements

Common ones that might be missing:
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using MetalPod.Shared;
using MetalPod.Core;
```

### 9. Potential Null Reference Issues

Check for:
- Scripts getting components in `Awake()` or `Start()` that might not exist yet — use `GetComponent<T>()` with null checks
- Singleton access before initialization (`GameManager.Instance?.something`)
- EventBus listeners attached before EventBus is initialized
- `FindFirstObjectByType` calls that might return null

### 10. Enum/Type Consistency

- `EnvironmentType` (Lava, Ice, Toxic) — should be in CourseDataSO or a shared location. Verify all references use the same enum.
- `DifficultyLevel` (Easy, Medium, Hard, Extreme) — same, verify consistency.
- `Medal` enum vs int — some places use enum, others use int (0-3). Standardize.
- `CosmeticType` (ColorScheme, Decal, Part) — verify consistency.
- `UpgradeCategory` (Speed, Handling, Shield, Boost) — verify consistency.

---

## Output Requirements

For every issue found:
1. Fix it directly in the file
2. Keep a running list of all changes made

At the end, create a file `Docs/CODE_REVIEW_REPORT.md` listing:
- Total issues found
- Issues by category (namespace, interface, dead reference, missing using, null safety, etc.)
- Files modified
- Any remaining concerns that can't be fixed without Unity (e.g., SerializeField wiring)

---

## Acceptance Criteria

- [ ] Every .cs file reviewed
- [ ] All namespace declarations consistent with the pattern
- [ ] All interface implementations verified (4 interfaces, 4 implementing classes)
- [ ] All EventBus raise/listen signatures match
- [ ] All dead references to deleted files removed
- [ ] All missing `using` statements added
- [ ] No duplicate enum/type definitions
- [ ] Null safety checks added where needed
- [ ] CODE_REVIEW_REPORT.md created with findings
- [ ] Zero compilation errors expected when opened in Unity (barring package imports)
