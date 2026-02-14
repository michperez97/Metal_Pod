# Agent 5: UI, Workshop & Player Experience

> **Owner**: Agent 5
> **Priority**: MEDIUM — User-facing layer, depends on most other agents
> **Estimated scope**: ~20% of total project
> **Dependency**: Agent 1 (interfaces, EventBus), Agent 2 (IHovercraftData), Agent 3 (ICourseEvents), Agent 4 (IProgressionData)

---

## Mission

You own everything the **player sees and interacts with** outside of the hovercraft controls and hazards. All UI screens, the workshop hub scene, menus, the HUD, settings, and the heavy metal visual theme. You are the front-end of the game. Your work makes or breaks the player's first impression and ongoing experience.

---

## Files You OWN (Create / Modify)

```
Assets/
├── Scripts/
│   ├── UI/
│   │   ├── UIManager.cs              ✅ EXISTS - rewrite (currently minimal)
│   │   ├── MainMenuUI.cs             ✅ EXISTS - rewrite
│   │   ├── HUD.cs                    ✅ EXISTS - enhance
│   │   ├── PauseMenuUI.cs            ✅ EXISTS - rewrite
│   │   ├── ResultsScreenUI.cs        ✅ EXISTS - rewrite
│   │   ├── SettingsUI.cs             ✅ EXISTS - rewrite
│   │   ├── CountdownUI.cs            🆕 CREATE
│   │   ├── LoadingScreenUI.cs        🆕 CREATE
│   │   ├── CourseUnlockedPopup.cs     🆕 CREATE
│   │   └── CurrencyDisplay.cs        🆕 CREATE
│   └── Workshop/
│       ├── WorkshopManager.cs         ✅ EXISTS - rewrite
│       ├── WorkshopCameraController.cs 🆕 CREATE
│       ├── UpgradeUI.cs              🆕 CREATE (UI side of upgrade system)
│       ├── CustomizationUI.cs        🆕 CREATE
│       ├── CourseSelectionUI.cs       ✅ EXISTS - rewrite
│       └── ProtagonistController.cs   🆕 CREATE
│
├── Scenes/
│   ├── MainMenu.unity                🆕 CREATE
│   └── Workshop.unity                🆕 CREATE
│
├── Prefabs/
│   └── UI/
│       ├── MainMenuCanvas.prefab
│       ├── HUDCanvas.prefab
│       ├── PauseMenuCanvas.prefab
│       ├── ResultsCanvas.prefab
│       ├── SettingsPanel.prefab
│       ├── LoadingScreen.prefab
│       └── Widgets/
│           ├── HealthBar.prefab
│           ├── ShieldBar.prefab
│           ├── BoostIndicator.prefab
│           ├── MedalDisplay.prefab
│           ├── UpgradeSlot.prefab
│           ├── CosmeticSlot.prefab
│           └── CourseCard.prefab
│
├── Materials/
│   └── UI/
│       ├── MetalPanel.mat
│       ├── RivetedFrame.mat
│       └── GlowAccent.mat
│
├── Fonts/
│   ├── MetalFont_Title.ttf           (heavy metal display font)
│   └── MetalFont_Body.ttf            (readable body font)
│
└── Textures/
    └── UI/
        ├── panel_metal.png
        ├── panel_riveted.png
        ├── button_metal.png
        ├── button_metal_pressed.png
        ├── icon_medal_gold.png
        ├── icon_medal_silver.png
        ├── icon_medal_bronze.png
        ├── icon_locked.png
        ├── icon_currency.png
        ├── icon_speed.png
        ├── icon_handling.png
        ├── icon_shield.png
        ├── icon_boost.png
        ├── skull_motif.png
        └── gear_motif.png
```

## Files You MUST NOT Touch

- `Assets/Scripts/Core/*` (Agent 1)
- `Assets/Scripts/Shared/*` (Agent 1)
- `Assets/Scripts/Hovercraft/*` (Agent 2)
- `Assets/Scripts/Course/*` (Agent 3)
- `Assets/Scripts/Hazards/*` (Agent 3)
- `Assets/Scripts/Progression/*` (Agent 4)
- `Assets/ScriptableObjects/*` (Agent 1 defines, Agent 4 creates instances)

## Files You REFERENCE (Read-Only)

```
Assets/Scripts/Shared/IHovercraftData.cs    — read health, speed, boost for HUD
Assets/Scripts/Shared/ICourseEvents.cs      — listen for race start/finish for HUD
Assets/Scripts/Shared/IProgressionData.cs   — read currency, medals, unlocks for UI
Assets/Scripts/Shared/GameConstants.cs      — prefs keys, tags
Assets/Scripts/Shared/EventBus.cs           — listen for currency, unlock, upgrade events
```

---

## What Already Exists

All existing UI scripts are **minimal stubs** that need rewriting. They have basic structure but no real functionality, no styling, no animation, and no proper data binding.

### HUD.cs (EXISTS — enhance)
- Has health bar, shield bar, speed, timer, boost cooldown references
- **ENHANCE**: Bind to IHovercraftData and ICourseEvents interfaces, add hazard warning display, add animations

### MainMenuUI.cs (EXISTS — stub, rewrite)
### PauseMenuUI.cs (EXISTS — stub, rewrite)
### ResultsScreenUI.cs (EXISTS — stub, rewrite)
### SettingsUI.cs (EXISTS — stub, rewrite)
### UIManager.cs (EXISTS — stub, rewrite)
### WorkshopManager.cs (EXISTS — stub, rewrite)
### CourseSelectionUI.cs (EXISTS — stub, rewrite)

---

## Task List

### Task 1: UIManager — Central UI Controller

```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _hudPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _resultsPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private GameObject _loadingPanel;

    private Stack<GameObject> _panelHistory = new Stack<GameObject>();

    public void ShowPanel(GameObject panel)
    {
        // Hide current, push to stack, show new
        // Animate transition (slide in from right, fade, etc.)
    }

    public void GoBack()
    {
        // Pop from stack, show previous
        // Reverse animation
    }

    public void ShowHUD() => ShowPanel(_hudPanel);
    public void ShowPause() => ShowPanel(_pausePanel);
    public void ShowResults() => ShowPanel(_resultsPanel);
    public void HideAll() { /* deactivate all panels */ }

    // Panel transition animations
    private IEnumerator AnimateSlideIn(RectTransform panel, float duration = 0.3f) { }
    private IEnumerator AnimateFadeIn(CanvasGroup group, float duration = 0.3f) { }
}
```

### Task 2: HUD — In-Game Display

```csharp
public class HUD : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image _healthFill;          // red bar
    [SerializeField] private Image _shieldFill;          // blue bar, overlays health
    [SerializeField] private Image _healthDamageFlash;   // red flash on hit

    [Header("Speed")]
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private Image _speedNeedle;         // optional gauge needle

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI _timerText;

    [Header("Boost")]
    [SerializeField] private Image _boostFill;           // radial cooldown
    [SerializeField] private Image _boostIcon;
    [SerializeField] private Color _boostReadyColor = Color.cyan;
    [SerializeField] private Color _boostCooldownColor = Color.gray;

    [Header("Hazard Warning")]
    [SerializeField] private Image _warningArrowLeft;
    [SerializeField] private Image _warningArrowRight;
    [SerializeField] private Image _warningArrowForward;
    [SerializeField] private CanvasGroup _warningGroup;

    private IHovercraftData _hovercraftData;
    private ICourseEvents _courseEvents;

    void Update()
    {
        if (_hovercraftData == null) return;

        // Smooth lerp health/shield bars
        _healthFill.fillAmount = Mathf.Lerp(_healthFill.fillAmount,
            _hovercraftData.HealthNormalized, Time.deltaTime * 8f);
        _shieldFill.fillAmount = Mathf.Lerp(_shieldFill.fillAmount,
            _hovercraftData.ShieldNormalized, Time.deltaTime * 8f);

        // Speed display
        _speedText.text = $"{Mathf.RoundToInt(_hovercraftData.CurrentSpeed)}";

        // Boost cooldown (radial fill)
        _boostFill.fillAmount = _hovercraftData.BoostCooldownNormalized;
        _boostIcon.color = _hovercraftData.BoostCooldownNormalized >= 1f
            ? _boostReadyColor : _boostCooldownColor;

        // Health bar color shift: green → yellow → red
        _healthFill.color = Color.Lerp(Color.red, Color.green, _hovercraftData.HealthNormalized);
    }

    public void OnDamageReceived()
    {
        // Flash red overlay briefly
        StartCoroutine(DamageFlash());
    }

    public void UpdateTimer(float elapsed)
    {
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        int ms = Mathf.FloorToInt((elapsed * 100f) % 100f);
        _timerText.text = $"{minutes:00}:{seconds:00}.{ms:00}";
    }

    private IEnumerator DamageFlash()
    {
        _healthDamageFlash.gameObject.SetActive(true);
        _healthDamageFlash.color = new Color(1, 0, 0, 0.3f);
        yield return new WaitForSeconds(0.15f);
        _healthDamageFlash.gameObject.SetActive(false);
    }
}
```

### Task 3: Main Menu

**MainMenuUI.cs**:
```csharp
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _settingsButton;

    [Header("Title")]
    [SerializeField] private TextMeshProUGUI _titleText;   // "METAL POD"
    [SerializeField] private Animator _titleAnimator;       // optional entrance anim

    [Header("Background")]
    [SerializeField] private RawImage _backgroundImage;     // workshop blurred BG

    void Start()
    {
        // Show Continue only if save exists
        bool hasSave = /* check via IProgressionData */ true;
        _continueButton.gameObject.SetActive(hasSave);

        _continueButton.onClick.AddListener(OnContinue);
        _newGameButton.onClick.AddListener(OnNewGame);
        _settingsButton.onClick.AddListener(OnSettings);
    }

    private void OnContinue()
    {
        // Load Workshop scene
        // Play transition animation
    }

    private void OnNewGame()
    {
        // Reset save data (with confirmation popup)
        // Load first course or workshop
    }
}
```

**Scene: MainMenu.unity**
```
MainMenu (Scene)
├── Canvas (Screen Space - Overlay)
│   ├── Background (RawImage - workshop render or concept art)
│   ├── TitleGroup
│   │   ├── Title_Text ("METAL POD" - large metal font)
│   │   └── Subtitle_Text ("A Crocobyte Game" - smaller)
│   ├── ButtonGroup (VerticalLayoutGroup, centered)
│   │   ├── ContinueButton (metal styled button)
│   │   ├── NewGameButton
│   │   └── SettingsButton
│   └── VersionText (bottom corner)
├── MainMenuUI (script)
├── EventSystem
└── Audio
    └── AudioSource (menu music)
```

### Task 4: Pause Menu

```csharp
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private CanvasGroup _canvasGroup;

    public void Show()
    {
        gameObject.SetActive(true);
        // Fade in canvas group
        // Blur background (post-processing or screenshot + blur shader)
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        // Fade out
        gameObject.SetActive(false);
    }

    private void OnResume() => Hide();
    private void OnRestart()
    {
        Time.timeScale = 1f;
        // Reload current course scene
    }
    private void OnQuit()
    {
        Time.timeScale = 1f;
        // Load Workshop scene
    }
}
```

### Task 5: Results Screen

```csharp
public class ResultsScreenUI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _bestTimeText;
    [SerializeField] private Image _medalImage;
    [SerializeField] private TextMeshProUGUI _medalText;

    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI _baseCurrencyText;
    [SerializeField] private TextMeshProUGUI _bonusCurrencyText;
    [SerializeField] private TextMeshProUGUI _totalCurrencyText;

    [Header("Details")]
    [SerializeField] private TextMeshProUGUI _collectiblesText;
    [SerializeField] private TextMeshProUGUI _healthRemainingText;
    [SerializeField] private TextMeshProUGUI _newRecordBadge;

    [Header("Buttons")]
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _nextCourseButton;
    [SerializeField] private Button _workshopButton;

    [Header("Medal Sprites")]
    [SerializeField] private Sprite _goldMedalSprite;
    [SerializeField] private Sprite _silverMedalSprite;
    [SerializeField] private Sprite _bronzeMedalSprite;
    [SerializeField] private Sprite _noMedalSprite;

    public void Show(CourseResultData result)
    {
        gameObject.SetActive(true);

        // Animate number count-up for time
        StartCoroutine(AnimateCountUp(_timeText, 0, result.completionTime, 1.5f));

        // Show medal with fanfare
        StartCoroutine(ShowMedalDelayed(result.medal, 1.5f));

        // Animate currency earned
        StartCoroutine(AnimateCurrencyCountUp(result));

        // Show "NEW RECORD" badge if applicable
        _newRecordBadge.gameObject.SetActive(result.isNewBestTime);

        // Collectibles: "7/10"
        _collectiblesText.text = $"{result.collectiblesFound}/{result.collectiblesTotal}";

        // Next course button only if next course is unlocked
        _nextCourseButton.interactable = result.nextCourseUnlocked;
    }

    private IEnumerator AnimateCountUp(TextMeshProUGUI text, float from, float to, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float current = Mathf.Lerp(from, to, elapsed / duration);
            int minutes = Mathf.FloorToInt(current / 60f);
            int seconds = Mathf.FloorToInt(current % 60f);
            int ms = Mathf.FloorToInt((current * 100) % 100);
            text.text = $"{minutes:00}:{seconds:00}.{ms:00}";
            yield return null;
        }
    }

    private IEnumerator ShowMedalDelayed(int medal, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        // Scale-bounce animation for medal icon
        // Play medal sound (gold > silver > bronze > none)
        _medalImage.sprite = medal switch
        {
            3 => _goldMedalSprite,
            2 => _silverMedalSprite,
            1 => _bronzeMedalSprite,
            _ => _noMedalSprite
        };
        // Animate: scale from 0 → 1.2 → 1.0 (bounce)
    }
}

[System.Serializable]
public class CourseResultData
{
    public float completionTime;
    public int medal;           // 0-3
    public bool isNewBestTime;
    public int currencyEarned;
    public int collectiblesFound;
    public int collectiblesTotal;
    public float healthRemaining;
    public bool nextCourseUnlocked;
}
```

### Task 6: Settings Menu

```csharp
public class SettingsUI : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private TextMeshProUGUI _sensitivityValueText;
    [SerializeField] private Toggle _invertTiltToggle;
    [SerializeField] private Toggle _hapticsToggle;

    [Header("Audio")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown _qualityDropdown;   // Low, Medium, High

    [SerializeField] private Button _backButton;

    void OnEnable()
    {
        // Load current values from PlayerPrefs
        _sensitivitySlider.value = PlayerPrefs.GetFloat(GameConstants.PREF_TILT_SENSITIVITY, 1f);
        _invertTiltToggle.isOn = PlayerPrefs.GetInt(GameConstants.PREF_INVERT_TILT, 0) == 1;
        _hapticsToggle.isOn = PlayerPrefs.GetInt(GameConstants.PREF_HAPTICS_ENABLED, 1) == 1;
        _masterVolumeSlider.value = PlayerPrefs.GetFloat(GameConstants.PREF_MASTER_VOLUME, 1f);
        _musicVolumeSlider.value = PlayerPrefs.GetFloat(GameConstants.PREF_MUSIC_VOLUME, 0.7f);
        _sfxVolumeSlider.value = PlayerPrefs.GetFloat(GameConstants.PREF_SFX_VOLUME, 1f);
        _qualityDropdown.value = PlayerPrefs.GetInt(GameConstants.PREF_QUALITY_LEVEL, 1);

        // Add listeners
        _sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        _invertTiltToggle.onValueChanged.AddListener(OnInvertChanged);
        // ... etc for all settings
    }

    private void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(GameConstants.PREF_TILT_SENSITIVITY, value);
        _sensitivityValueText.text = $"{value:F1}x";
    }

    // ... other handlers save to PlayerPrefs + apply immediately
}
```

### Task 7: Countdown UI

```csharp
public class CountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private Animator _countdownAnimator; // scale pop animation

    public void ShowCountdown(int number)
    {
        gameObject.SetActive(true);
        _countdownText.text = number > 0 ? number.ToString() : "GO!";
        _countdownAnimator.SetTrigger("Pop"); // scale 0→1.3→1.0
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
```

### Task 8: Loading Screen

```csharp
public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private Image _progressFill;
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private TextMeshProUGUI _tipText;
    [SerializeField] private CanvasGroup _canvasGroup;

    private string[] _tips = {
        "Use boost to break through ice walls!",
        "Shield regenerates after a few seconds of safety.",
        "Collect currency to upgrade your pod in the workshop.",
        "Gold medals require mastering every shortcut.",
        "Watch for ground shadows — something is falling!",
        "The avalanche waits for no one. Keep moving!",
        "Upgrade handling to tame ice patches.",
        "Electric fences stun your controls briefly."
    };

    public void Show()
    {
        gameObject.SetActive(true);
        _tipText.text = _tips[Random.Range(0, _tips.Length)];
        _progressFill.fillAmount = 0f;
        StartCoroutine(FadeIn());
    }

    public void UpdateProgress(float progress)
    {
        _progressFill.fillAmount = progress;
        _loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
    }

    public IEnumerator FadeOut()
    {
        float elapsed = 0;
        while (elapsed < 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = 1f - (elapsed / 0.5f);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
```

### Task 9: Workshop Scene & Manager

**WorkshopManager.cs**:
```csharp
public class WorkshopManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private GameObject _customizationPanel;
    [SerializeField] private GameObject _courseSelectionPanel;

    [Header("References")]
    [SerializeField] private Transform _hovercraftDisplay;    // hovercraft on lift
    [SerializeField] private Transform _protagonistPosition;
    [SerializeField] private WorkshopCameraController _camera;
    [SerializeField] private CurrencyDisplay _currencyDisplay;

    private WorkshopPanel _activePanel = WorkshopPanel.None;

    public enum WorkshopPanel { None, Upgrades, Customization, CourseSelection }

    public void OpenUpgrades()
    {
        SetPanel(WorkshopPanel.Upgrades);
        _camera.FocusOnHovercraft();  // zoom to hovercraft for upgrade view
    }

    public void OpenCustomization()
    {
        SetPanel(WorkshopPanel.Customization);
        _camera.FocusOnHovercraft();  // zoom for color/decal preview
    }

    public void OpenCourseSelection()
    {
        SetPanel(WorkshopPanel.CourseSelection);
        _camera.FocusOnMap();  // pull back to show course selection
    }

    public void LaunchCourse(string courseId)
    {
        EventBus.RaiseCourseSelected(courseId);
        // Transition animation: hovercraft powers up, camera pushes forward
        // Load course scene
    }
}
```

**Scene: Workshop.unity**
```
Workshop (Scene)
├── Environment
│   ├── Workshop_Floor (concrete/metal)
│   ├── Workshop_Walls (industrial panels)
│   ├── Workshop_Ceiling (with hanging lights)
│   ├── Workbench (with tools, parts)
│   ├── Amplifier_Stack (heavy metal aesthetic)
│   ├── Posters (metal band posters on walls)
│   ├── Tool_Rack
│   ├── Parts_Shelves
│   └── Hovercraft_Lift (center platform)
│
├── Characters
│   ├── Hovercraft (display instance from Agent 2 prefab)
│   │   Position: center of lift platform
│   │   Slow idle rotation
│   └── Protagonist
│       ├── Character Model
│       └── ProtagonistController (idle/working/celebrating anims)
│
├── Camera
│   └── Main Camera + WorkshopCameraController
│       Default: wide shot showing full workshop
│       Upgrade/Custom: zoomed on hovercraft
│       Course Select: pulled back, slight tilt
│
├── Lighting
│   ├── Overhead_Fluorescent (warm yellow, slight flicker)
│   ├── Hovercraft_Spotlight (highlight the pod)
│   └── Accent_Lights (orange/amber rim lights)
│
├── Canvas (Screen Space - Overlay)
│   ├── TopBar
│   │   ├── CurrencyDisplay (icon + amount)
│   │   ├── TotalMedals (icon + count)
│   │   └── SettingsButton (gear icon)
│   ├── BottomNav (3 buttons)
│   │   ├── UpgradesButton
│   │   ├── CustomizeButton
│   │   └── CoursesButton
│   ├── UpgradePanel (slides in from right)
│   ├── CustomizationPanel (slides in from right)
│   ├── CourseSelectionPanel (slides in from right)
│   └── SettingsPanel (overlay)
│
├── WorkshopManager (script)
├── EventSystem
└── Audio
    └── Workshop ambient + light music
```

### Task 10: Upgrade UI

```csharp
public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private Transform _upgradeSlotContainer;
    [SerializeField] private GameObject _upgradeSlotPrefab;

    [Header("Preview")]
    [SerializeField] private TextMeshProUGUI _currentStatText;
    [SerializeField] private TextMeshProUGUI _newStatText;
    [SerializeField] private Image _statChangeArrow;

    [Header("Purchase")]
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _maxedOutText;

    // Read upgrade data from IProgressionData
    // Display 4 upgrade categories (Speed, Handling, Shield, Boost)
    // Each shows: icon, name, current level (dots/bars), cost of next level
    // On select: show stat preview (current → new values)
    // Purchase button: calls UpgradeManager.TryPurchaseUpgrade() via ProgressionManager
    // On purchase: play upgrade VFX on hovercraft, refresh display
}
```

### Task 11: Course Selection UI

```csharp
public class CourseSelectionUI : MonoBehaviour
{
    [Header("Environment Tabs")]
    [SerializeField] private Button _lavaTab;
    [SerializeField] private Button _iceTab;
    [SerializeField] private Button _toxicTab;
    [SerializeField] private Image _lavaTabHighlight;
    [SerializeField] private Image _iceTabHighlight;
    [SerializeField] private Image _toxicTabHighlight;

    [Header("Course List")]
    [SerializeField] private Transform _courseListContainer;
    [SerializeField] private GameObject _courseCardPrefab;

    [Header("Course Detail")]
    [SerializeField] private TextMeshProUGUI _courseNameText;
    [SerializeField] private TextMeshProUGUI _difficultyText;
    [SerializeField] private TextMeshProUGUI _bestTimeText;
    [SerializeField] private Image _bestMedalImage;
    [SerializeField] private TextMeshProUGUI _hazardListText;
    [SerializeField] private Button _launchButton;
    [SerializeField] private TextMeshProUGUI _lockedText;       // "Requires X medals"

    // Show courses per environment tab
    // Each course card: name, difficulty stars, medal icon, locked overlay
    // Locked courses show: lock icon + "Requires X medals to unlock"
    // Selected course: show detail panel with best time, hazard descriptions
    // Launch button: triggers WorkshopManager.LaunchCourse()

    // Course card layout:
    // ┌────────────────────┐
    // │ 🔥 Inferno Gate    │
    // │ ★☆☆ Easy           │
    // │ Best: 00:52.34  🥇 │
    // └────────────────────┘
}
```

### Task 12: Customization UI

```csharp
public class CustomizationUI : MonoBehaviour
{
    [Header("Tabs")]
    [SerializeField] private Button _colorsTab;
    [SerializeField] private Button _decalsTab;
    [SerializeField] private Button _partsTab;

    [Header("Grid")]
    [SerializeField] private Transform _itemGrid;
    [SerializeField] private GameObject _cosmeticSlotPrefab;

    [Header("Preview")]
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemDescText;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _ownedText;

    // Grid of cosmetic items per tab
    // Each slot: icon, name, owned/locked indicator, equipped checkmark
    // On select: show preview on actual hovercraft model (real-time via HovercraftCustomization)
    // Purchase flow: confirm popup → deduct currency → unlock item
    // Equip: immediate application + save
}
```

### Task 13: Heavy Metal UI Theme

**Design System** — Apply consistently across ALL UI:

**Colors:**
```
Background: #1A1A1A (near black)
Panel:      #2D2D2D (dark gray) with subtle metal texture
Accent:     #FF8800 (amber/orange)
Text:       #E0E0E0 (light gray)
Highlight:  #FFB030 (gold)
Danger:     #FF2222 (red)
Success:    #44CC44 (green)
Currency:   #FFD700 (gold)
```

**Fonts:**
- Title/Headers: Heavy, aggressive display font (like "Metal Mania" or "Rusty Nails")
  - Used for: game title, section headers, medal announcements
- Body: Clean, readable sans-serif (like "Rajdhani" or "Orbitron")
  - Used for: stats, descriptions, settings labels, timer

**Button Style:**
- Background: Riveted metal panel (texture overlay)
- Normal: Dark metal (#3A3A3A) with subtle bevel
- Hover/Selected: Orange glow edge (#FF8800)
- Pressed: Darker, inset look
- Disabled: Desaturated, dim
- Border: 2px, subtle metal rim
- Min size: 44x44pt touch targets

**Panel Style:**
- Background: Metal texture with slight noise
- Border: Riveted frame (rivet circles at corners)
- Shadow: Subtle drop shadow for depth
- Headers: Orange underline separator

**Decorative Elements:**
- Skull motifs in corners of major panels
- Gear/cog icons as bullet points
- Scratched metal divider lines
- Orange glow accents on active/selected items

### Task 14: Protagonist Character Controller

```csharp
public class ProtagonistController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WorkingHash = Animator.StringToHash("Working");
    private static readonly int CelebratingHash = Animator.StringToHash("Celebrating");

    public void SetIdle() => _animator.CrossFade(IdleHash, 0.2f);
    public void SetWorking() => _animator.CrossFade(WorkingHash, 0.2f);
    public void SetCelebrating() => _animator.CrossFade(CelebratingHash, 0.2f);

    // In Workshop:
    //   Default: Idle (leaning on workbench)
    //   When upgrade purchased: Working animation → Celebrating
    //   When browsing courses: Idle, looking at course selection
}
```

Character description:
- 18-year-old, heavy metal fan
- Outfit: Band t-shirt (black with logo), ripped jeans, boots, fingerless gloves
- Hair: Messy/long, dark
- Low-poly mobile-appropriate (1000-2000 triangles)
- 3 animation states: Idle, Working (wrench), Celebrating (fist pump)

---

## Acceptance Criteria

- [ ] UIManager manages all panel transitions with animations
- [ ] HUD displays health, shield, speed, timer, boost cooldown — all bound to interfaces
- [ ] HUD has damage flash and hazard warning arrows
- [ ] Main Menu with Continue/New Game/Settings — Continue only if save exists
- [ ] Pause Menu with Resume/Restart/Settings/Quit — properly pauses/resumes time
- [ ] Results Screen with animated time count-up, medal reveal, currency breakdown
- [ ] Settings Menu with all controls (sensitivity, audio, graphics, haptics)
- [ ] Workshop scene with 3D environment matching heavy metal aesthetic
- [ ] Workshop navigation: Upgrades / Customize / Courses tabs
- [ ] Upgrade UI shows all 4 categories, current levels, costs, stat previews
- [ ] Course Selection with environment tabs, locked/unlocked courses, best times/medals
- [ ] Customization UI for colors, decals, parts with real-time preview
- [ ] Countdown UI with "3, 2, 1, GO!" animation
- [ ] Loading screen with progress bar and tips
- [ ] Heavy metal theme applied consistently (fonts, colors, metal textures)
- [ ] All touch targets minimum 44x44pt
- [ ] Protagonist character in workshop with idle/working/celebrating states
- [ ] No direct references to Agent 2/3/4 code — only via interfaces and EventBus

---

## Integration Contract

**What you provide to other agents:**
- Nobody depends on your code directly — you are the consumer end

**What you consume from other agents:**
- Agent 1: `IHovercraftData` (HUD reads health/speed/boost), `ICourseEvents` (HUD timer, results trigger), `EventBus` (currency changes, unlock notifications), `GameConstants` (prefs keys)
- Agent 2: `HovercraftCustomization` public methods (apply color/decal from customization UI)
- Agent 3: `ICourseEvents` on CourseManager (listen for countdown, race start/finish)
- Agent 4: `IProgressionData` (read currency, medals, unlocks), `ProgressionManager.Upgrades/Cosmetics` (purchase flows)
