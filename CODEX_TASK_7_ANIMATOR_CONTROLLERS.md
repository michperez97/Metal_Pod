# Codex Task 7: Animator Controller Generator

> **Goal**: Write an Editor script that programmatically creates all Animator Controllers, animation clips, and state machines for hovercraft states, protagonist character, and UI animations.

---

## Context

Metal Pod needs animation state machines for: the hovercraft (visual states), the protagonist character in the workshop, and UI elements (panel transitions, medal reveal, countdown). Unity's Animator Controllers can be created entirely via code using `UnityEditor.Animations`.

**Read these files**:
- `AGENT_2_HOVERCRAFT_VEHICLE.md` — Hovercraft states (Normal, Boosting, Braking, Damaged, Destroyed)
- `AGENT_5_UI_AND_WORKSHOP.md` — Protagonist animations, UI animations
- `Assets/Scripts/Hovercraft/HovercraftController.cs` — State machine that drives animations
- `Assets/Scripts/Workshop/ProtagonistController.cs` — Animation trigger hashes

---

## Files to Create

```
Assets/Scripts/Editor/
└── AnimatorGenerator.cs               # Main generator

Assets/Animations/
├── Controllers/
│   ├── HovercraftAnimator.controller  # Generated
│   ├── ProtagonistAnimator.controller # Generated
│   ├── UICountdownAnimator.controller # Generated
│   ├── UIMedalAnimator.controller     # Generated
│   └── UIPanelAnimator.controller     # Generated
├── Clips/
│   ├── Hovercraft/
│   │   ├── Hover_Idle.anim
│   │   ├── Hover_Boost.anim
│   │   ├── Hover_Damaged.anim
│   │   ├── Hover_Destroyed.anim
│   │   └── Hover_Respawn.anim
│   ├── Protagonist/
│   │   ├── Proto_Idle.anim
│   │   ├── Proto_Working.anim
│   │   └── Proto_Celebrating.anim
│   └── UI/
│       ├── UI_SlideInRight.anim
│       ├── UI_SlideOutRight.anim
│       ├── UI_FadeIn.anim
│       ├── UI_FadeOut.anim
│       ├── UI_ScalePop.anim           # 0 → 1.2 → 1.0 bounce
│       ├── UI_MedalReveal.anim        # 0 → 1.3 → 1.0 with rotation
│       ├── UI_CountdownPop.anim       # Scale up + fade for "3, 2, 1, GO!"
│       └── UI_NumberCountUp.anim      # Placeholder for scripted count-up
```

---

## AnimatorGenerator.cs

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorGenerator
{
    [MenuItem("Metal Pod/Animations/Generate All Animator Controllers")]
    public static void GenerateAll()
    {
        EditorUtility.DisplayProgressBar("Generating Animators", "Creating controllers...", 0);
        try
        {
            GenerateHovercraftAnimator();
            GenerateProtagonistAnimator();
            GenerateUIAnimators();
            GenerateAnimationClips();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("Metal Pod/Animations/Generate Hovercraft Animator")]
    public static void GenerateHovercraftAnimator() { /* ... */ }

    [MenuItem("Metal Pod/Animations/Generate Protagonist Animator")]
    public static void GenerateProtagonistAnimator() { /* ... */ }

    [MenuItem("Metal Pod/Animations/Generate UI Animators")]
    public static void GenerateUIAnimators() { /* ... */ }
}
```

---

## Hovercraft Animator Controller

State machine matching HovercraftController states:

```
States:
  Idle (default)         — subtle hover bob animation
  Boosting               — intense thruster animation, slight forward tilt
  Damaged                — wobble/shake animation
  Destroyed              — no animation (hidden, explosion plays)
  Respawning             — fade-in shimmer

Parameters:
  bool IsBoosting
  bool IsDamaged
  bool IsDestroyed
  bool IsRespawning
  float Speed            (0-1 normalized, for blend)
  float HealthNormalized (0-1, for damage wobble intensity)

Transitions:
  Idle → Boosting:     IsBoosting = true
  Boosting → Idle:     IsBoosting = false
  Any → Damaged:       IsDamaged = true (health < 25%)
  Damaged → Idle:      IsDamaged = false
  Any → Destroyed:     IsDestroyed = true
  Destroyed → Respawning: IsRespawning = true
  Respawning → Idle:   IsRespawning = false

Transition Settings:
  All transitions: Duration 0.15s, no exit time (immediate response)
  Exception: Respawning → Idle: Duration 0.5s (smooth re-entry)
```

Implementation:
```csharp
private static void GenerateHovercraftAnimator()
{
    var controller = AnimatorController.CreateAnimatorControllerAtPath(
        "Assets/Animations/Controllers/HovercraftAnimator.controller");

    // Add parameters
    controller.AddParameter("IsBoosting", AnimatorControllerParameterType.Bool);
    controller.AddParameter("IsDamaged", AnimatorControllerParameterType.Bool);
    controller.AddParameter("IsDestroyed", AnimatorControllerParameterType.Bool);
    controller.AddParameter("IsRespawning", AnimatorControllerParameterType.Bool);
    controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
    controller.AddParameter("HealthNormalized", AnimatorControllerParameterType.Float);

    var rootStateMachine = controller.layers[0].stateMachine;

    // Create states
    var idleState = rootStateMachine.AddState("Idle", new Vector3(300, 0, 0));
    var boostState = rootStateMachine.AddState("Boosting", new Vector3(300, -100, 0));
    var damagedState = rootStateMachine.AddState("Damaged", new Vector3(500, 0, 0));
    var destroyedState = rootStateMachine.AddState("Destroyed", new Vector3(500, -100, 0));
    var respawnState = rootStateMachine.AddState("Respawning", new Vector3(500, -200, 0));

    rootStateMachine.defaultState = idleState;

    // Assign animation clips (create placeholder clips first)
    idleState.motion = CreateOrLoadClip("Assets/Animations/Clips/Hovercraft/Hover_Idle.anim");
    boostState.motion = CreateOrLoadClip("Assets/Animations/Clips/Hovercraft/Hover_Boost.anim");
    damagedState.motion = CreateOrLoadClip("Assets/Animations/Clips/Hovercraft/Hover_Damaged.anim");
    destroyedState.motion = CreateOrLoadClip("Assets/Animations/Clips/Hovercraft/Hover_Destroyed.anim");
    respawnState.motion = CreateOrLoadClip("Assets/Animations/Clips/Hovercraft/Hover_Respawn.anim");

    // Transitions
    AddTransition(idleState, boostState, "IsBoosting", true, 0.15f);
    AddTransition(boostState, idleState, "IsBoosting", false, 0.15f);
    AddAnyStateTransition(rootStateMachine, destroyedState, "IsDestroyed", true, 0.05f);
    AddTransition(destroyedState, respawnState, "IsRespawning", true, 0.1f);
    AddTransition(respawnState, idleState, "IsRespawning", false, 0.5f);
    // Damaged state via blend tree or direct transition
    AddTransition(idleState, damagedState, "IsDamaged", true, 0.2f);
    AddTransition(damagedState, idleState, "IsDamaged", false, 0.3f);
}
```

### Hovercraft Animation Clips

**Hover_Idle.anim** — Subtle hover bob:
```
Property: Transform.localPosition.y
Keyframes: [0s: 0.0], [0.5s: 0.05], [1.0s: 0.0], [1.5s: -0.03], [2.0s: 0.0]
Loop: true
Very subtle — just enough to feel alive
Also: slight rotation Z wobble ±0.5 degrees
```

**Hover_Boost.anim** — Forward lean + intense bob:
```
Property: Transform.localRotation (euler X)
Keyframes: [0s: 0], [0.2s: -5] (slight nose-down tilt during boost)
Property: Transform.localPosition.y
Keyframes: faster bob cycle (0.3s period instead of 2s)
Loop: true
```

**Hover_Damaged.anim** — Wobble/shake:
```
Property: Transform.localRotation (euler Z)
Keyframes: [0s: 0], [0.05s: 2], [0.1s: -1.5], [0.15s: 1], [0.2s: -0.5], [0.25s: 0]
Loop: true (continuous shake while damaged)
Intensity scales with HealthNormalized parameter
```

**Hover_Destroyed.anim** — Brief:
```
Property: Transform.localScale
Keyframes: [0s: 1,1,1], [0.1s: 0,0,0] (shrink to nothing)
Loop: false
Script handles explosion VFX separately
```

**Hover_Respawn.anim** — Fade in:
```
Property: Transform.localScale
Keyframes: [0s: 0,0,0], [0.3s: 1.1,1.1,1.1], [0.5s: 1,1,1] (scale bounce)
Loop: false
```

---

## Protagonist Animator Controller

```
States:
  Idle (default)    — leaning on workbench, breathing cycle
  Working           — wrench animation on hovercraft
  Celebrating       — fist pump

Parameters:
  trigger Work
  trigger Celebrate
  bool IsWorking

Transitions:
  Idle → Working:      IsWorking = true
  Working → Idle:      IsWorking = false
  Idle → Celebrating:  Celebrate trigger
  Celebrating → Idle:  Exit time 2.0s (auto-return)
  Working → Celebrating: Celebrate trigger
```

### Protagonist Animation Clips

**Proto_Idle.anim**:
```
Breathing cycle: slight chest scale Y (1.0 → 1.02 → 1.0, 3s period)
Head slight turn: rotation Y (0 → 5 → 0 → -3 → 0, 8s period)
Weight shift: position X (0 → 0.02 → 0, 5s period)
Loop: true
```

**Proto_Working.anim**:
```
Right arm: rotation Z swing -30 to 30 (wrench swing, 1s period)
Body lean forward: rotation X 0 → 10
Repeat swing motion
Loop: true
```

**Proto_Celebrating.anim**:
```
0.0s: Standing neutral
0.3s: Right arm up (rotation Z: 0 → -150, fist pump)
0.5s: Slight jump (position Y: 0 → 0.2)
0.7s: Land (position Y: 0.2 → 0)
1.0s: Arm down
1.5s: Return to idle pose
Loop: false
```

---

## UI Animator Controllers

### UICountdownAnimator

```
States:
  Hidden (default) — scale 0, alpha 0
  Pop              — scale 0 → 1.3 → 1.0, alpha 0 → 1

Parameters:
  trigger Pop

Transitions:
  Hidden → Pop: Pop trigger
  Pop → Hidden: Exit time 0.6s
```

### UIMedalAnimator

```
States:
  Hidden (default) — scale 0
  Reveal           — scale 0 → 1.3 → 1.0 with Y rotation 0 → 360

Parameters:
  trigger Reveal

Transitions:
  Hidden → Reveal: Reveal trigger
```

### UIPanelAnimator

```
States:
  Hidden (default) — position X offset +500 (off screen right)
  Visible          — position X = 0 (on screen)

Parameters:
  bool IsVisible

Transitions:
  Hidden → Visible: IsVisible = true, Duration 0.3s, Ease Out
  Visible → Hidden: IsVisible = false, Duration 0.2s, Ease In
```

### UI Animation Clips

**UI_SlideInRight.anim**:
```
Property: RectTransform.anchoredPosition.x
Keyframes: [0s: 600], [0.15s: -20], [0.25s: 5], [0.3s: 0]
Ease: overshoot then settle (bounce feel)
```

**UI_SlideOutRight.anim**:
```
Property: RectTransform.anchoredPosition.x
Keyframes: [0s: 0], [0.05s: -15], [0.2s: 600]
Ease: anticipation then fast exit
```

**UI_FadeIn.anim**:
```
Property: CanvasGroup.alpha
Keyframes: [0s: 0], [0.3s: 1]
```

**UI_FadeOut.anim**:
```
Property: CanvasGroup.alpha
Keyframes: [0s: 1], [0.2s: 0]
```

**UI_ScalePop.anim**:
```
Property: RectTransform.localScale
Keyframes: [0s: (0,0,0)], [0.15s: (1.2,1.2,1)], [0.25s: (0.95,0.95,1)], [0.3s: (1,1,1)]
Classic pop/bounce
```

**UI_MedalReveal.anim**:
```
Property: RectTransform.localScale
Keyframes: [0s: (0,0,0)], [0.2s: (1.3,1.3,1)], [0.35s: (0.9,0.9,1)], [0.45s: (1,1,1)]
Property: RectTransform.localRotation (euler Y)
Keyframes: [0s: 0], [0.45s: 360] (full spin)
```

**UI_CountdownPop.anim**:
```
Property: RectTransform.localScale
Keyframes: [0s: (2,2,1)], [0.1s: (1,1,1)], [0.4s: (1,1,1)], [0.5s: (0.5,0.5,1)]
Property: CanvasGroup.alpha
Keyframes: [0s: 0], [0.05s: 1], [0.4s: 1], [0.5s: 0]
Number appears big, shrinks to normal, holds, then fades
```

---

## Creating Animation Clips Programmatically

```csharp
private static AnimationClip CreateHoverIdleClip()
{
    var clip = new AnimationClip();
    clip.name = "Hover_Idle";

    // Position Y bob
    var curveY = new AnimationCurve();
    curveY.AddKey(new Keyframe(0f, 0f));
    curveY.AddKey(new Keyframe(0.5f, 0.05f));
    curveY.AddKey(new Keyframe(1.0f, 0f));
    curveY.AddKey(new Keyframe(1.5f, -0.03f));
    curveY.AddKey(new Keyframe(2.0f, 0f));

    clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);

    // Rotation Z wobble
    var curveZ = new AnimationCurve();
    curveZ.AddKey(new Keyframe(0f, 0f));
    curveZ.AddKey(new Keyframe(1.0f, 0.5f));
    curveZ.AddKey(new Keyframe(2.0f, 0f));
    curveZ.AddKey(new Keyframe(3.0f, -0.3f));
    curveZ.AddKey(new Keyframe(4.0f, 0f));

    clip.SetCurve("", typeof(Transform), "localEulerAngles.z", curveZ);

    // Set looping
    var settings = AnimationUtility.GetAnimationClipSettings(clip);
    settings.loopTime = true;
    AnimationUtility.SetAnimationClipSettings(clip, settings);

    return clip;
}

private static AnimationClip CreateUIScalePopClip()
{
    var clip = new AnimationClip();
    clip.name = "UI_ScalePop";

    var scaleX = new AnimationCurve();
    scaleX.AddKey(new Keyframe(0f, 0f));
    scaleX.AddKey(new Keyframe(0.15f, 1.2f));
    scaleX.AddKey(new Keyframe(0.25f, 0.95f));
    scaleX.AddKey(new Keyframe(0.3f, 1.0f));

    var scaleY = new AnimationCurve();
    scaleY.AddKey(new Keyframe(0f, 0f));
    scaleY.AddKey(new Keyframe(0.15f, 1.2f));
    scaleY.AddKey(new Keyframe(0.25f, 0.95f));
    scaleY.AddKey(new Keyframe(0.3f, 1.0f));

    clip.SetCurve("", typeof(RectTransform), "m_LocalScale.x", scaleX);
    clip.SetCurve("", typeof(RectTransform), "m_LocalScale.y", scaleY);

    var settings = AnimationUtility.GetAnimationClipSettings(clip);
    settings.loopTime = false;
    AnimationUtility.SetAnimationClipSettings(clip, settings);

    return clip;
}
```

Helper to save clips:
```csharp
private static AnimationClip CreateOrLoadClip(string path)
{
    var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    if (existing != null) return existing;

    var clip = new AnimationClip();
    EnsureDirectory(path);
    AssetDatabase.CreateAsset(clip, path);
    return clip;
}

private static void SaveClip(AnimationClip clip, string path)
{
    EnsureDirectory(path);
    AssetDatabase.CreateAsset(clip, path);
}
```

---

## Integration with Existing Scripts

After generating, the existing scripts should reference these controllers:

- `HovercraftVisuals.cs` has `[SerializeField] Animator` — assign HovercraftAnimator.controller
- `ProtagonistController.cs` references animation hashes `Idle`, `Working`, `Celebrating` — these match the state names
- `CountdownUI.cs` has `[SerializeField] Animator` — assign UICountdownAnimator.controller
- `ResultsScreenUI.cs` medal animation — assign UIMedalAnimator.controller
- `UIManager.cs` panel transitions — assign UIPanelAnimator.controller

The Setup Wizard (Task 1) should wire these after generation.

---

## Acceptance Criteria

- [ ] Menu item "Metal Pod/Animations/Generate All Animator Controllers" works
- [ ] HovercraftAnimator: 5 states, 6 parameters, all transitions configured
- [ ] ProtagonistAnimator: 3 states, proper triggers and transitions
- [ ] UICountdownAnimator: 2 states with Pop trigger
- [ ] UIMedalAnimator: 2 states with Reveal trigger
- [ ] UIPanelAnimator: 2 states with IsVisible bool
- [ ] All animation clips created with actual keyframe curves
- [ ] Hover idle has bob + wobble
- [ ] Hover boost has forward lean
- [ ] Hover damaged has shake
- [ ] UI clips have proper bounce/overshoot easing
- [ ] All assets saved to Assets/Animations/
- [ ] Controllers and clips are properly linked
