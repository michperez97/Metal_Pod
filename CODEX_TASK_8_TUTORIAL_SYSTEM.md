# Codex Task 8: Tutorial & Onboarding System

> **Goal**: Write the complete tutorial system that guides first-time players through controls, mechanics, and gameplay. SRS requirements USE-1 and USE-2 mandate this.

---

## Context

The SRS states:
- **USE-1**: Players shall be able to learn basic controls within the first course or tutorial
- **USE-2**: System shall provide optional tutorial course explaining all control mechanisms

The tutorial should integrate with the existing course system (CourseManager, HUD) and play during the first course (Lava Course 1 "Inferno Gate") or as a separate tutorial overlay.

**Read these files**:
- `Assets/Scripts/Course/CourseManager.cs` — Course lifecycle
- `Assets/Scripts/Hovercraft/HovercraftInput.cs` — Control scheme (tilt + touch zones)
- `Assets/Scripts/Hovercraft/HovercraftController.cs` — States (boost, brake)
- `Assets/Scripts/Hovercraft/HovercraftHealth.cs` — Health/shield mechanics
- `Assets/Scripts/Course/Checkpoint.cs` — Checkpoint system
- `Assets/Scripts/Course/Collectible.cs` — Collectible types
- `Assets/Scripts/Shared/ICourseEvents.cs` — Course events
- `Assets/Scripts/UI/HUD.cs` — Heads-up display

---

## Files to Create

```
Assets/Scripts/Tutorial/
├── TutorialManager.cs                 # Main tutorial controller
├── TutorialStep.cs                    # Individual tutorial step data
├── TutorialSequence.cs                # Sequence of steps for a scenario
├── TutorialUI.cs                      # UI overlay for tutorial prompts
├── TutorialTrigger.cs                 # Placed in world to trigger steps
├── TutorialHighlight.cs               # UI element highlight/pulse effect
└── TutorialSaveData.cs               # Track which tutorials completed

Assets/ScriptableObjects/
└── TutorialStepSO.cs                  # ScriptableObject for tutorial step data

Assets/Scripts/Editor/
└── TutorialSetup.cs                   # Editor script to generate tutorial assets
```

---

## Architecture

The tutorial system is **non-intrusive** — it overlays on top of normal gameplay without pausing (except for the very first prompt). It uses a step-based system where each step waits for a condition to be met before advancing.

```
TutorialManager
  ├── Checks if tutorial has been completed before (via SaveSystem)
  ├── Loads TutorialSequence for current context
  ├── Steps through each TutorialStep
  │   ├── Shows prompt via TutorialUI
  │   ├── Optionally highlights a UI element
  │   ├── Optionally slows time (Time.timeScale = 0.3)
  │   ├── Waits for completion condition
  │   └── Advances to next step
  └── Marks tutorial as complete when all steps done
```

---

## TutorialStep.cs

```csharp
namespace MetalPod.Tutorial
{
    public enum TutorialCondition
    {
        None,                      // Advance on tap/timer
        TiltDevice,                // Player tilts device
        TapBoost,                  // Player taps boost
        TapBrake,                  // Player holds brake
        ReachSpeed,                // Speed exceeds threshold
        TakeDamage,                // Player takes any damage
        UseShield,                 // Shield absorbs damage
        ReachCheckpoint,           // Any checkpoint reached
        CollectItem,               // Any collectible picked up
        FinishCourse,              // Cross finish line
        WaitSeconds,               // Just wait N seconds
    }

    public enum TutorialPromptPosition
    {
        TopCenter,
        BottomCenter,
        Center,
        NearControl,               // Near the relevant touch zone
    }

    [System.Serializable]
    public class TutorialStep
    {
        [Header("Content")]
        public string stepId;
        public string promptText;                    // Main instruction text
        public string subtitleText;                  // Smaller detail text
        public Sprite iconSprite;                    // Optional icon (control diagram, etc.)

        [Header("Behavior")]
        public TutorialCondition completionCondition;
        public float conditionValue;                 // e.g., speed threshold, wait seconds
        public float autoAdvanceDelay = 0f;          // 0 = wait for condition, >0 = auto advance after N seconds
        public bool pauseGame;                       // true = Time.timeScale = 0 (first prompt only)
        public bool slowMotion;                      // true = Time.timeScale = 0.3
        public bool requireTapToContinue;            // Show "Tap to continue" after condition met

        [Header("Highlight")]
        public string highlightUIElement;            // Name of UI element to pulse/highlight
        public TutorialPromptPosition promptPosition;

        [Header("Visual")]
        public bool showArrowPointing;               // Arrow pointing at something
        public Vector2 arrowTarget;                  // Screen-space target for arrow
        public bool dimBackground;                   // Dim everything except highlighted area
    }
}
```

## TutorialSequence.cs

```csharp
namespace MetalPod.Tutorial
{
    [System.Serializable]
    public class TutorialSequence
    {
        public string sequenceId;
        public string sequenceName;
        public TutorialStep[] steps;

        public static TutorialSequence CreateFirstPlaySequence()
        {
            return new TutorialSequence
            {
                sequenceId = "first_play",
                sequenceName = "First Time Playing",
                steps = new TutorialStep[]
                {
                    // Step 1: Welcome
                    new TutorialStep
                    {
                        stepId = "welcome",
                        promptText = "WELCOME TO METAL POD",
                        subtitleText = "Let's learn the basics. Tap to continue.",
                        completionCondition = TutorialCondition.None,
                        requireTapToContinue = true,
                        pauseGame = true,
                        promptPosition = TutorialPromptPosition.Center,
                        dimBackground = true,
                    },

                    // Step 2: Steering
                    new TutorialStep
                    {
                        stepId = "steering",
                        promptText = "TILT TO STEER",
                        subtitleText = "Tilt your device left and right to turn.",
                        completionCondition = TutorialCondition.TiltDevice,
                        slowMotion = true,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        autoAdvanceDelay = 8f, // auto-advance if they don't tilt in 8s
                    },

                    // Step 3: Speed
                    new TutorialStep
                    {
                        stepId = "speed",
                        promptText = "YOU'RE MOVING!",
                        subtitleText = "Your pod accelerates automatically. Watch your speed on the HUD.",
                        completionCondition = TutorialCondition.ReachSpeed,
                        conditionValue = 10f,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        highlightUIElement = "SpeedDisplay",
                    },

                    // Step 4: Boost
                    new TutorialStep
                    {
                        stepId = "boost",
                        promptText = "TAP TO BOOST",
                        subtitleText = "Tap the right side of the screen for a speed burst!",
                        completionCondition = TutorialCondition.TapBoost,
                        promptPosition = TutorialPromptPosition.NearControl,
                        highlightUIElement = "BoostIndicator",
                        showArrowPointing = true,
                        arrowTarget = new Vector2(0.8f, 0.2f), // right-bottom area
                    },

                    // Step 5: Brake
                    new TutorialStep
                    {
                        stepId = "brake",
                        promptText = "HOLD TO BRAKE",
                        subtitleText = "Hold the left side of the screen to slow down.",
                        completionCondition = TutorialCondition.TapBrake,
                        promptPosition = TutorialPromptPosition.NearControl,
                        showArrowPointing = true,
                        arrowTarget = new Vector2(0.2f, 0.2f), // left-bottom area
                        autoAdvanceDelay = 10f,
                    },

                    // Step 6: Health/Shield
                    new TutorialStep
                    {
                        stepId = "health",
                        promptText = "WATCH YOUR HEALTH",
                        subtitleText = "Your shield (blue) absorbs damage first. Health (red) is below it. Avoid hazards!",
                        completionCondition = TutorialCondition.WaitSeconds,
                        conditionValue = 4f,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        highlightUIElement = "HealthBar",
                    },

                    // Step 7: Damage (triggered near first hazard)
                    new TutorialStep
                    {
                        stepId = "damage_warning",
                        promptText = "HAZARD AHEAD!",
                        subtitleText = "Lava flows deal damage. Steer clear or boost past them!",
                        completionCondition = TutorialCondition.None,
                        requireTapToContinue = true,
                        slowMotion = true,
                        promptPosition = TutorialPromptPosition.Center,
                    },

                    // Step 8: Checkpoint
                    new TutorialStep
                    {
                        stepId = "checkpoint",
                        promptText = "CHECKPOINT!",
                        subtitleText = "If you're destroyed, you'll respawn here.",
                        completionCondition = TutorialCondition.ReachCheckpoint,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        highlightUIElement = "TimerDisplay",
                    },

                    // Step 9: Collectible
                    new TutorialStep
                    {
                        stepId = "collectible",
                        promptText = "GRAB COLLECTIBLES",
                        subtitleText = "Floating pickups give bonus currency and health.",
                        completionCondition = TutorialCondition.CollectItem,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        autoAdvanceDelay = 15f,
                    },

                    // Step 10: Finish
                    new TutorialStep
                    {
                        stepId = "finish_line",
                        promptText = "REACH THE FINISH!",
                        subtitleText = "Race to the end. Faster times earn better medals!",
                        completionCondition = TutorialCondition.FinishCourse,
                        promptPosition = TutorialPromptPosition.TopCenter,
                    },
                }
            };
        }
    }
}
```

## TutorialManager.cs

```csharp
namespace MetalPod.Tutorial
{
    using MetalPod.Shared;

    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [SerializeField] private TutorialUI _tutorialUI;
        [SerializeField] private bool _forceShowTutorial = false; // editor debug

        private TutorialSequence _currentSequence;
        private int _currentStepIndex = -1;
        private TutorialStep _currentStep;
        private bool _isActive = false;
        private bool _conditionMet = false;
        private float _stepTimer = 0f;

        // References discovered at runtime
        private IHovercraftData _hovercraftData;
        private HovercraftInput _hovercraftInput; // to detect tilt/boost/brake

        void Awake() { Instance = this; }

        void Start()
        {
            // Check if tutorial already completed
            if (!_forceShowTutorial && TutorialSaveData.IsTutorialCompleted("first_play"))
            {
                enabled = false;
                return;
            }

            // Start tutorial
            _currentSequence = TutorialSequence.CreateFirstPlaySequence();
            _isActive = true;

            // Subscribe to events for condition checking
            SubscribeToEvents();

            // Advance to first step
            AdvanceStep();
        }

        void Update()
        {
            if (!_isActive || _currentStep == null) return;

            _stepTimer += Time.unscaledDeltaTime; // unscaled so it works during slow-mo

            // Check completion condition
            if (!_conditionMet)
            {
                _conditionMet = CheckCondition(_currentStep);
            }

            // Auto advance
            if (_currentStep.autoAdvanceDelay > 0 && _stepTimer >= _currentStep.autoAdvanceDelay)
            {
                _conditionMet = true;
            }

            // If condition met and tap required, wait for tap
            if (_conditionMet)
            {
                if (_currentStep.requireTapToContinue)
                {
                    _tutorialUI.ShowTapToContinue();
                    if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
                    {
                        AdvanceStep();
                    }
                }
                else
                {
                    // Brief delay then advance
                    AdvanceStep();
                }
            }
        }

        private void AdvanceStep()
        {
            // Restore time scale from previous step
            RestoreTimeScale();

            _currentStepIndex++;
            _conditionMet = false;
            _stepTimer = 0f;

            if (_currentStepIndex >= _currentSequence.steps.Length)
            {
                // Tutorial complete
                CompleteTutorial();
                return;
            }

            _currentStep = _currentSequence.steps[_currentStepIndex];

            // Apply time effects
            if (_currentStep.pauseGame)
                Time.timeScale = 0f;
            else if (_currentStep.slowMotion)
                Time.timeScale = 0.3f;

            // Show UI
            _tutorialUI.ShowStep(_currentStep);

            // Condition None = just show, advance on tap or timer
            if (_currentStep.completionCondition == TutorialCondition.None
                && !_currentStep.requireTapToContinue)
            {
                _conditionMet = true;
            }
        }

        private bool CheckCondition(TutorialStep step)
        {
            switch (step.completionCondition)
            {
                case TutorialCondition.TiltDevice:
                    // Check if accelerometer shows significant tilt
                    return Mathf.Abs(Input.acceleration.x) > 0.15f;

                case TutorialCondition.TapBoost:
                    return _hovercraftInput != null && _hovercraftInput.BoostPressed;

                case TutorialCondition.TapBrake:
                    return _hovercraftInput != null && _hovercraftInput.BrakeHeld;

                case TutorialCondition.ReachSpeed:
                    return _hovercraftData != null && _hovercraftData.CurrentSpeed >= step.conditionValue;

                case TutorialCondition.TakeDamage:
                    return _tookDamageFlag;

                case TutorialCondition.UseShield:
                    return _shieldUsedFlag;

                case TutorialCondition.ReachCheckpoint:
                    return _checkpointReachedFlag;

                case TutorialCondition.CollectItem:
                    return _collectiblePickedFlag;

                case TutorialCondition.FinishCourse:
                    return _courseFinishedFlag;

                case TutorialCondition.WaitSeconds:
                    return _stepTimer >= step.conditionValue;

                default:
                    return false;
            }
        }

        // Event flags (set by event listeners, reset on step advance)
        private bool _tookDamageFlag, _shieldUsedFlag, _checkpointReachedFlag;
        private bool _collectiblePickedFlag, _courseFinishedFlag;

        private void SubscribeToEvents()
        {
            // Listen to relevant events from course system and hovercraft
            // These will be wired via ICourseEvents and health events
        }

        private void RestoreTimeScale()
        {
            if (_currentStep != null && (_currentStep.pauseGame || _currentStep.slowMotion))
                Time.timeScale = 1f;
        }

        private void CompleteTutorial()
        {
            _isActive = false;
            RestoreTimeScale();
            _tutorialUI.Hide();
            TutorialSaveData.SetTutorialCompleted("first_play");
        }

        // Called when tutorial should be triggered mid-course (e.g., approaching first hazard)
        public void TriggerContextualStep(string stepId)
        {
            if (!_isActive) return;
            // Jump to specific step if not yet passed
            for (int i = _currentStepIndex + 1; i < _currentSequence.steps.Length; i++)
            {
                if (_currentSequence.steps[i].stepId == stepId)
                {
                    _currentStepIndex = i - 1; // will be incremented by AdvanceStep
                    AdvanceStep();
                    return;
                }
            }
        }
    }
}
```

## TutorialUI.cs

```csharp
namespace MetalPod.Tutorial
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class TutorialUI : MonoBehaviour
    {
        [Header("Prompt Box")]
        [SerializeField] private GameObject _promptPanel;
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _tapToContinueText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Positioning")]
        [SerializeField] private RectTransform _promptRect;

        [Header("Highlight")]
        [SerializeField] private Image _dimOverlay;            // Full-screen dark overlay
        [SerializeField] private Image _highlightRing;         // Ring around highlighted element
        [SerializeField] private Image _arrowPointer;          // Pointing arrow

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.2f;

        public void ShowStep(TutorialStep step)
        {
            StopAllCoroutines();
            _promptPanel.SetActive(true);

            // Set text
            _promptText.text = step.promptText;
            _subtitleText.text = step.subtitleText ?? "";
            _tapToContinueText.gameObject.SetActive(false);

            // Icon
            _iconImage.gameObject.SetActive(step.iconSprite != null);
            if (step.iconSprite != null) _iconImage.sprite = step.iconSprite;

            // Position
            PositionPrompt(step.promptPosition);

            // Highlight
            _dimOverlay.gameObject.SetActive(step.dimBackground);
            _arrowPointer.gameObject.SetActive(step.showArrowPointing);
            if (step.showArrowPointing)
                PositionArrow(step.arrowTarget);

            // Highlight UI element
            if (!string.IsNullOrEmpty(step.highlightUIElement))
                HighlightElement(step.highlightUIElement);
            else
                _highlightRing.gameObject.SetActive(false);

            // Fade in
            StartCoroutine(FadeIn());
        }

        public void ShowTapToContinue()
        {
            _tapToContinueText.gameObject.SetActive(true);
            // Pulse animation on tap text
        }

        public void Hide()
        {
            StartCoroutine(FadeOutAndDisable());
        }

        private void PositionPrompt(TutorialPromptPosition pos)
        {
            switch (pos)
            {
                case TutorialPromptPosition.TopCenter:
                    _promptRect.anchorMin = new Vector2(0.1f, 0.75f);
                    _promptRect.anchorMax = new Vector2(0.9f, 0.95f);
                    break;
                case TutorialPromptPosition.BottomCenter:
                    _promptRect.anchorMin = new Vector2(0.1f, 0.05f);
                    _promptRect.anchorMax = new Vector2(0.9f, 0.25f);
                    break;
                case TutorialPromptPosition.Center:
                    _promptRect.anchorMin = new Vector2(0.15f, 0.35f);
                    _promptRect.anchorMax = new Vector2(0.85f, 0.65f);
                    break;
                case TutorialPromptPosition.NearControl:
                    _promptRect.anchorMin = new Vector2(0.1f, 0.25f);
                    _promptRect.anchorMax = new Vector2(0.9f, 0.45f);
                    break;
            }
            _promptRect.anchoredPosition = Vector2.zero;
        }

        private void PositionArrow(Vector2 normalizedScreenPos)
        {
            // Convert normalized (0-1) screen position to canvas position
            var canvas = GetComponentInParent<Canvas>();
            var canvasRect = canvas.GetComponent<RectTransform>();
            _arrowPointer.rectTransform.anchorMin = normalizedScreenPos;
            _arrowPointer.rectTransform.anchorMax = normalizedScreenPos;
        }

        private void HighlightElement(string elementName)
        {
            // Find UI element by name in scene
            var element = FindUIElement(elementName);
            if (element != null)
            {
                _highlightRing.gameObject.SetActive(true);
                _highlightRing.rectTransform.position = element.position;
                // Pulse animation on highlight ring
                StartCoroutine(PulseHighlight());
            }
        }

        private RectTransform FindUIElement(string name)
        {
            // Search all canvases for named element
            var all = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
            foreach (var rt in all)
            {
                if (rt.name == name) return rt;
            }
            return null;
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float t = 0;
            while (t < _fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = t / _fadeInDuration;
                yield return null;
            }
            _canvasGroup.alpha = 1;
        }

        private System.Collections.IEnumerator FadeOutAndDisable()
        {
            float t = 0;
            while (t < _fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1 - (t / _fadeOutDuration);
                yield return null;
            }
            _promptPanel.SetActive(false);
        }

        private System.Collections.IEnumerator PulseHighlight()
        {
            while (_highlightRing.gameObject.activeSelf)
            {
                float scale = 1f + 0.15f * Mathf.Sin(Time.unscaledTime * 3f);
                _highlightRing.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
    }
}
```

## TutorialTrigger.cs

```csharp
namespace MetalPod.Tutorial
{
    // Place in the world to trigger specific tutorial steps when player enters
    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private string _stepId;       // e.g., "damage_warning"
        [SerializeField] private bool _triggerOnce = true;
        private bool _triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered && _triggerOnce) return;
            if (!other.CompareTag(GameConstants.TAG_PLAYER)) return;

            _triggered = true;
            TutorialManager.Instance?.TriggerContextualStep(_stepId);
        }
    }
}
```

## TutorialSaveData.cs

```csharp
namespace MetalPod.Tutorial
{
    public static class TutorialSaveData
    {
        private const string PREFIX = "Tutorial_";

        public static bool IsTutorialCompleted(string tutorialId)
        {
            return PlayerPrefs.GetInt(PREFIX + tutorialId, 0) == 1;
        }

        public static void SetTutorialCompleted(string tutorialId)
        {
            PlayerPrefs.SetInt(PREFIX + tutorialId, 1);
            PlayerPrefs.Save();
        }

        public static void ResetTutorial(string tutorialId)
        {
            PlayerPrefs.DeleteKey(PREFIX + tutorialId);
            PlayerPrefs.Save();
        }

        public static void ResetAllTutorials()
        {
            // Known tutorial IDs
            ResetTutorial("first_play");
            ResetTutorial("workshop_intro");
        }
    }
}
```

## TutorialHighlight.cs

```csharp
namespace MetalPod.Tutorial
{
    // Attach to any UI element to make it highlightable by the tutorial system
    public class TutorialHighlight : MonoBehaviour
    {
        [SerializeField] private string _highlightId;  // matches TutorialStep.highlightUIElement
        public string HighlightId => _highlightId;
    }
}
```

## Workshop Tutorial Sequence

Also create a second tutorial for when the player first enters the workshop:

```csharp
public static TutorialSequence CreateWorkshopIntroSequence()
{
    return new TutorialSequence
    {
        sequenceId = "workshop_intro",
        sequenceName = "Workshop Introduction",
        steps = new TutorialStep[]
        {
            new TutorialStep
            {
                stepId = "workshop_welcome",
                promptText = "THE WORKSHOP",
                subtitleText = "This is your base. Upgrade your pod, customize it, and pick your next course.",
                completionCondition = TutorialCondition.None,
                requireTapToContinue = true,
                pauseGame = true,
                promptPosition = TutorialPromptPosition.Center,
            },
            new TutorialStep
            {
                stepId = "workshop_upgrades",
                promptText = "UPGRADES",
                subtitleText = "Spend currency to improve Speed, Handling, Shields, and Boost.",
                completionCondition = TutorialCondition.None,
                requireTapToContinue = true,
                promptPosition = TutorialPromptPosition.BottomCenter,
                highlightUIElement = "UpgradesButton",
            },
            new TutorialStep
            {
                stepId = "workshop_courses",
                promptText = "CHOOSE A COURSE",
                subtitleText = "Tap Courses to pick your next challenge. Earn medals to unlock more!",
                completionCondition = TutorialCondition.None,
                requireTapToContinue = true,
                promptPosition = TutorialPromptPosition.BottomCenter,
                highlightUIElement = "CoursesButton",
            },
        }
    };
}
```

## TutorialSetup.cs (Editor)

```csharp
// Editor script to add TutorialTrigger objects to course scenes
[MenuItem("Metal Pod/Tutorial/Add Tutorial Triggers to Test Course")]
public static void AddTutorialTriggers()
{
    // Find or create TutorialManager in scene
    // Add TutorialTrigger objects at key positions:
    //   - Before first hazard area: "damage_warning" trigger
    //   - Near first collectible: triggers collectible step early
    // Add TutorialUI canvas to scene
}
```

---

## Acceptance Criteria

- [ ] TutorialManager runs on first play, skips if already completed
- [ ] 10-step first-play tutorial covering: welcome, steer, speed, boost, brake, health, hazard warning, checkpoint, collectible, finish
- [ ] 3-step workshop intro tutorial
- [ ] TutorialUI shows prompts with fade in/out animations
- [ ] Pause and slow-motion work correctly with Time.timeScale
- [ ] Tap to continue works for paused steps
- [ ] Auto-advance works for timed steps
- [ ] UI element highlighting with pulse effect
- [ ] Arrow pointer for control zone guidance
- [ ] TutorialTrigger for world-space triggers
- [ ] TutorialSaveData persists completion via PlayerPrefs
- [ ] Tutorial can be reset (for settings menu "Replay Tutorial" option)
- [ ] All scripts use MetalPod.Tutorial namespace
- [ ] No direct references to other agent code — use interfaces (IHovercraftData, ICourseEvents)
