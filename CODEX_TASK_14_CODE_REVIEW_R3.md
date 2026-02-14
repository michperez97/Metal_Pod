# Codex Task 14: Second Code Review & Full Integration (Round 3)

> **Goal**: Review ALL code added in rounds 2 and 3 (Tasks 6-10), verify compilation, fix issues, and wire integration points between the new systems (tutorial, accessibility, haptics, particles, animators, iOS native) and the existing codebase.

---

## Context

Round 3 added 15+ new scripts across 5 tasks. These scripts reference each other and existing code, but **none of them have been cross-verified**. The first code review (Task 4) only covered round 1 scripts. Since then, 2 full rounds of code have been added.

Key integration gaps that likely exist:
1. **HapticFeedbackManager** — declares convenience methods (OnBoostActivated, OnDamageTaken, etc.) but nothing calls them
2. **AccessibilityManager** — has Announce() method but no existing scripts call it
3. **TutorialManager** — needs to hook into CourseManager events and HovercraftInput
4. **iOSNativePlugin** — referenced by AccessibilityManager and HapticFeedbackManager but namespace may not match
5. **ParticleSystemGenerator / AnimatorGenerator** — may reference types or paths that don't exist
6. **BuildPreprocessor / AppIconGenerator** — may have missing `using` statements
7. **TutorialSetup.cs** — generates TutorialStepSO assets, needs to match TutorialStep.cs field names
8. **EventBus** — round 3 scripts may need new events (e.g., OnHapticFeedback, OnTutorialStep)

**This agent must read EVERY file listed below and fix ALL issues found.**

---

## Files to Review (ALL of these)

### Round 2 Scripts (Tasks 6-7 - Editor generators)
```
Assets/Scripts/Editor/ParticleSystemGenerator.cs
Assets/Scripts/Editor/ParticlePresets.cs
Assets/Scripts/Editor/AnimatorGenerator.cs
```

### Round 3 Scripts (Tasks 8-10)

**Tutorial System (Task 8)**:
```
Assets/Scripts/Tutorial/TutorialManager.cs
Assets/Scripts/Tutorial/TutorialStep.cs
Assets/Scripts/Tutorial/TutorialSequence.cs
Assets/Scripts/Tutorial/TutorialUI.cs
Assets/Scripts/Tutorial/TutorialTrigger.cs
Assets/Scripts/Tutorial/TutorialHighlight.cs
Assets/Scripts/Tutorial/TutorialSaveData.cs
Assets/ScriptableObjects/TutorialStepSO.cs
Assets/Scripts/Editor/TutorialSetup.cs
```

**Accessibility & iOS (Task 10)**:
```
Assets/Scripts/Accessibility/AccessibilityManager.cs
Assets/Scripts/Accessibility/AccessibilityLabels.cs
Assets/Scripts/Accessibility/HapticFeedbackManager.cs
Assets/Scripts/Core/iOSNativePlugin.cs
Assets/Scripts/Editor/AppIconGenerator.cs
Assets/Scripts/Editor/BuildPreprocessor.cs
Assets/Plugins/iOS/PrivacyInfo.xcprivacy
Assets/Plugins/iOS/MetalPodNative.mm
```

### Existing Core Scripts (verify integration points)
```
Assets/Scripts/Shared/EventBus.cs
Assets/Scripts/Shared/GameConstants.cs
Assets/Scripts/Shared/IHovercraftData.cs
Assets/Scripts/Shared/IDamageReceiver.cs
Assets/Scripts/Shared/ICourseEvents.cs
Assets/Scripts/Shared/IProgressionData.cs
Assets/Scripts/Core/GameManager.cs
Assets/Scripts/Core/GameStateManager.cs
Assets/Scripts/Core/SceneLoader.cs
Assets/Scripts/Core/AudioManager.cs
Assets/Scripts/Hovercraft/HovercraftController.cs
Assets/Scripts/Hovercraft/HovercraftHealth.cs
Assets/Scripts/Hovercraft/HovercraftInput.cs
Assets/Scripts/Hovercraft/HovercraftPhysics.cs
Assets/Scripts/Hovercraft/HovercraftVisuals.cs
Assets/Scripts/Hovercraft/HovercraftAudio.cs
Assets/Scripts/Hovercraft/HovercraftStats.cs
Assets/Scripts/Hovercraft/HovercraftCustomization.cs
Assets/Scripts/Course/CourseManager.cs
Assets/Scripts/Course/CourseTimer.cs
Assets/Scripts/Course/Checkpoint.cs
Assets/Scripts/Course/Collectible.cs
Assets/Scripts/Course/FinishLine.cs
Assets/Scripts/Course/MedalSystem.cs
Assets/Scripts/Progression/SaveSystem.cs
Assets/Scripts/Progression/SaveData.cs
Assets/Scripts/Progression/CurrencyManager.cs
Assets/Scripts/Progression/UpgradeManager.cs
Assets/Scripts/Progression/CosmeticManager.cs
Assets/Scripts/Progression/ProgressionManager.cs
Assets/Scripts/UI/HUD.cs
Assets/Scripts/UI/MainMenuUI.cs
Assets/Scripts/UI/ResultsScreenUI.cs
Assets/Scripts/UI/CountdownUI.cs
Assets/Scripts/UI/PauseMenuUI.cs
Assets/Scripts/UI/LoadingScreenUI.cs
Assets/Scripts/Workshop/WorkshopManager.cs
Assets/Scripts/Workshop/CourseSelectionUI.cs
Assets/Scripts/Workshop/UpgradeUI.cs
Assets/Scripts/Workshop/CustomizationUI.cs
Assets/Scripts/Workshop/ProtagonistController.cs
Assets/Scripts/Workshop/WorkshopCameraController.cs
Assets/Scripts/Hazards/HazardBase.cs
Assets/Scripts/Rendering/HeatDistortionFeature.cs
Assets/Scripts/Rendering/FrostOverlayFeature.cs
Assets/Scripts/Rendering/ToxicScreenFeature.cs
Assets/Scripts/Editor/MetalPodSetupWizard.cs
Assets/Scripts/Editor/CourseBuilder/CourseBuilder.cs
```

---

## Review Checklist

### A. Namespace & Using Verification
For EVERY .cs file in the review list:
- [ ] Verify the `namespace` matches the folder structure convention:
  - `Assets/Scripts/Tutorial/` → `MetalPod.Tutorial`
  - `Assets/Scripts/Accessibility/` → `MetalPod.Accessibility`
  - `Assets/Scripts/Core/` → `MetalPod` (iOSNativePlugin) or `MetalPod.Core`
  - `Assets/Scripts/Editor/` → `MetalPod.Editor`
  - `Assets/ScriptableObjects/` → `MetalPod.ScriptableObjects`
- [ ] Verify all `using` statements resolve to existing namespaces
- [ ] Check for `using TMPro;` — if used, it's valid (TextMeshPro is included)
- [ ] Check for `using UnityEditor;` — must be wrapped in `#if UNITY_EDITOR`
- [ ] Check for `using UnityEditor.Animations;` in AnimatorGenerator (needed for AnimatorController)
- [ ] Check for `using UnityEditor.iOS.Xcode;` in BuildPreprocessor (needs `#if UNITY_IOS`)

### B. Cross-Reference Verification
- [ ] `iOSNativePlugin.cs` namespace matches what `AccessibilityManager.cs` and `HapticFeedbackManager.cs` expect
- [ ] `TutorialStepSO.cs` fields match what `TutorialSetup.cs` generates
- [ ] `TutorialStep.cs` fields match what `TutorialManager.cs` uses
- [ ] `TutorialSaveData.cs` integrates with `SaveSystem.cs` or `SaveData.cs` (check save/load path)
- [ ] `ParticlePresets.cs` references valid `ParticleSystem` properties
- [ ] `AnimatorGenerator.cs` uses correct `UnityEditor.Animations` API
- [ ] `AppIconGenerator.cs` correctly uses `RenderTexture` and `Texture2D` APIs
- [ ] `BuildPreprocessor.cs` correctly uses `PlayerSettings` and `PlistDocument` APIs

### C. Interface & Event Compliance
- [ ] `HovercraftHealth.cs` events (`OnDamage`, `OnDestroyed`, etc.) — verify signature matches what HapticFeedbackManager expects
- [ ] `CourseManager.cs` events (`OnCountdownStarted`, `OnRaceStarted`, `OnCheckpointReached`, etc.) — verify TutorialManager can subscribe
- [ ] `EventBus` events — check if any new scripts raise events that don't exist on EventBus
- [ ] `IDamageReceiver` — verify `DamageType` enum is accessible from Accessibility scripts
- [ ] `SaveData.cs` — verify TutorialSaveData can be stored (either embedded or separate)

### D. Integration Wiring (ADD CODE to these files)

**IMPORTANT**: For each integration point below, add the actual code. Don't just note the issue — fix it.

#### D.1 Wire HapticFeedbackManager into HovercraftController
In `HovercraftController.cs`, after boost activation:
```csharp
MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnBoostActivated();
```

#### D.2 Wire HapticFeedbackManager into HovercraftHealth
In `HovercraftHealth.cs`, in the damage method:
```csharp
MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnDamageTaken();
```
In the destroyed callback:
```csharp
MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnDestroyed();
```

#### D.3 Wire HapticFeedbackManager into Checkpoint
In `Checkpoint.cs`, on activation:
```csharp
MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnCheckpointReached();
```

#### D.4 Wire HapticFeedbackManager into Collectible
In `Collectible.cs`, on pickup:
```csharp
MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnCollectiblePickup();
```

#### D.5 Wire HapticFeedbackManager into ResultsScreenUI
In `ResultsScreenUI.cs`, on medal display:
```csharp
MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnMedalEarned();
```

#### D.6 Wire HapticFeedbackManager into UpgradeUI / CustomizationUI
On purchase:
```csharp
MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnUpgradePurchased();
```

#### D.7 Wire AccessibilityManager.Announce() into key moments
- `CourseManager.cs` — Announce checkpoint reached
- `HUD.cs` or `CountdownUI.cs` — Announce countdown numbers
- `ResultsScreenUI.cs` — Announce race results

#### D.8 Wire TutorialManager into CourseManager
TutorialManager should auto-start on the first course if tutorial hasn't been completed. Verify the wiring path exists.

#### D.9 Add iOSNativePlugin.RequestAppReview() call
In `ProgressionManager.cs`, after the 3rd gold medal is earned, call:
```csharp
MetalPod.iOSNativePlugin.RequestAppReview();
```

#### D.10 Verify SaveData includes tutorial completion state
`TutorialSaveData` needs to be persisted. Check if it's stored inside `SaveData.cs` or has its own save path. If neither, add a `bool tutorialCompleted` field to `SaveData`.

---

## Output

Create `Docs/CODE_REVIEW_R3_REPORT.md` listing:

```markdown
# Code Review Round 3 Report

## Files Reviewed
- Total files reviewed: [COUNT]

## Issues Found & Fixed
| # | File | Issue | Fix Applied |
|---|------|-------|-------------|
| 1 | ... | ... | ... |

## Integration Points Wired
| # | From → To | What Was Added |
|---|-----------|---------------|
| 1 | HovercraftController → HapticFeedbackManager | Added OnBoostActivated() call |
| ... | ... | ... |

## Namespace Map
| Directory | Expected Namespace | Actual | Status |
|-----------|-------------------|--------|--------|
| Scripts/Tutorial | MetalPod.Tutorial | ... | OK/Fixed |
| ... | ... | ... | ... |

## Remaining Manual Tasks (require Unity)
- [list anything that can't be verified from code alone]
```

---

## Acceptance Criteria

- [ ] Every file in the review list has been read and checked
- [ ] All namespace mismatches are fixed
- [ ] All missing `using` statements are added
- [ ] All cross-reference errors are fixed (types that don't exist, wrong method signatures)
- [ ] HapticFeedbackManager wired into 6+ existing scripts
- [ ] AccessibilityManager.Announce() wired into 3+ existing scripts
- [ ] TutorialManager integration with CourseManager verified or fixed
- [ ] iOSNativePlugin.RequestAppReview() wired into ProgressionManager
- [ ] SaveData includes tutorial completion state
- [ ] `Docs/CODE_REVIEW_R3_REPORT.md` created with full findings
- [ ] No dead references to files that don't exist
- [ ] All `#if UNITY_EDITOR` guards in place for editor-only code
