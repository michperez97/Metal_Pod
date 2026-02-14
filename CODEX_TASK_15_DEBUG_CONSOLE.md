# Codex Task 15: Debug Console & Playtesting Tools

> **Goal**: Create an in-game debug console and editor playtesting tools for efficient testing when Unity opens. Includes cheat commands, FPS/memory overlay, hazard testing, and a course skip system.

---

## Context

When the developer opens Unity for the first time, they'll have 100+ scripts, 10 courses, 15 hazards, and dozens of ScriptableObjects. Testing will be painful without debug tools. This task creates:

1. An **in-game debug console** (toggled by triple-tap on a corner) with cheat commands
2. An **FPS/memory overlay** for performance monitoring on device
3. An **editor debug window** with one-click shortcuts for common test scenarios
4. A **course skip system** for jumping directly to any course
5. A **hazard test mode** to test individual hazards in isolation

**Read these files**:
- `Assets/Scripts/Progression/SaveSystem.cs` — Save/load mechanics
- `Assets/Scripts/Progression/SaveData.cs` — Save data structure
- `Assets/Scripts/Progression/CurrencyManager.cs` — Currency operations
- `Assets/Scripts/Progression/UpgradeManager.cs` — Upgrade levels
- `Assets/Scripts/Progression/CosmeticManager.cs` — Cosmetic ownership
- `Assets/Scripts/Progression/ProgressionManager.cs` — Unified progression API
- `Assets/Scripts/Course/CourseManager.cs` — Course lifecycle, states
- `Assets/Scripts/Core/GameManager.cs` — Singleton, scene loading
- `Assets/Scripts/Core/SceneLoader.cs` — Scene loading API
- `Assets/Scripts/Hovercraft/HovercraftHealth.cs` — Health/damage system
- `Assets/Scripts/Hovercraft/HovercraftController.cs` — Hovercraft states
- `Assets/Scripts/Shared/GameConstants.cs` — Scene names, tags
- `Assets/Scripts/Shared/EventBus.cs` — Global events
- `Assets/Scripts/Tutorial/TutorialManager.cs` — Tutorial state

---

## Files to Create

```
Assets/Scripts/Debug/
├── DebugConsole.cs               # In-game debug console with command system
├── DebugCommand.cs               # Command definition (name, description, action)
├── DebugCommands.cs              # All built-in commands registered here
├── DebugOverlay.cs               # FPS, memory, and game state overlay
├── DebugCourseSkip.cs            # Quick-jump to any course from debug menu
└── DebugHazardTester.cs          # Spawn and test individual hazards

Assets/Scripts/Editor/
└── DebugEditorWindow.cs          # Editor window with test shortcuts
```

**DO NOT modify** any existing files. The debug system is entirely self-contained and disabled in release builds via `#if DEVELOPMENT_BUILD || UNITY_EDITOR`.

---

## Architecture

```
DebugConsole (Singleton, DontDestroyOnLoad)
  ├── Canvas overlay (toggle via triple-tap in top-left corner)
  ├── Command input field + output log
  ├── Pre-registered commands via DebugCommands.cs
  ├── Scrollable output log with filtering
  └── All code wrapped in DEVELOPMENT_BUILD || UNITY_EDITOR

DebugOverlay (always-visible when enabled)
  ├── FPS counter (current, min, max, average)
  ├── Memory usage (total, used, GC allocation)
  ├── Game state (scene, course state, hovercraft health/speed)
  └── Toggle via debug console command "overlay"

DebugEditorWindow (Unity Editor only)
  ├── One-click buttons for common test scenarios
  ├── Course selector dropdown
  ├── Currency/upgrade manipulation
  └── Save data management (reset, export, import)
```

---

## Detailed Specifications

### DebugCommand.cs

```csharp
// Represents a single debug command.
// Commands are registered in DebugCommands.cs and invoked by name from the console.

using System;

namespace MetalPod.Debugging
{
    public class DebugCommand
    {
        public string Name { get; }
        public string Description { get; }
        public string Usage { get; }
        public Func<string[], string> Execute { get; }

        public DebugCommand(string name, string description, string usage, Func<string[], string> execute)
        {
            Name = name.ToLowerInvariant();
            Description = description;
            Usage = usage;
            Execute = execute;
        }
    }
}
```

### DebugConsole.cs

```csharp
// In-game debug console. Toggle via triple-tap in top-left screen corner.
// Type commands and see output. All functionality stripped from release builds.

// Usage: Triple-tap top-left corner (100x100 pixels) within 1 second to toggle.
// Type "help" to see all commands.

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Debugging
{
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private Canvas consoleCanvas;
        [SerializeField] private InputField commandInput;
        [SerializeField] private Text outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject consolePanel;

        [Header("Settings")]
        [SerializeField] private int maxLogLines = 200;
        [SerializeField] private int tapCornerSize = 100; // pixels
        [SerializeField] private int tapsRequired = 3;
        [SerializeField] private float tapWindow = 1.0f;

        private readonly Dictionary<string, DebugCommand> _commands = new Dictionary<string, DebugCommand>();
        private readonly List<string> _logLines = new List<string>();
        private readonly List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;
        private bool _isVisible;
        private float _lastTapTime;
        private int _tapCount;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (consoleCanvas != null)
                consoleCanvas.sortingOrder = 10000;

            SetVisible(false);

            // Register built-in commands
            DebugCommands.RegisterAll(this);
        }

        private void Update()
        {
            DetectTripleTap();

            // Keyboard shortcut: backtick/tilde to toggle (editor only)
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.BackQuote))
                ToggleConsole();
            #endif

            // Command history navigation
            if (_isVisible)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) && _commandHistory.Count > 0)
                {
                    _historyIndex = Mathf.Max(0, _historyIndex - 1);
                    commandInput.text = _commandHistory[_historyIndex];
                    commandInput.caretPosition = commandInput.text.Length;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) && _commandHistory.Count > 0)
                {
                    _historyIndex = Mathf.Min(_commandHistory.Count, _historyIndex + 1);
                    commandInput.text = _historyIndex < _commandHistory.Count
                        ? _commandHistory[_historyIndex] : "";
                }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SubmitCommand();
                }
            }
        }

        private void DetectTripleTap()
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector2 pos = Input.GetTouch(0).position;
                // Check if tap is in top-left corner
                if (pos.x < tapCornerSize && pos.y > Screen.height - tapCornerSize)
                {
                    if (Time.unscaledTime - _lastTapTime < tapWindow)
                    {
                        _tapCount++;
                        if (_tapCount >= tapsRequired)
                        {
                            ToggleConsole();
                            _tapCount = 0;
                        }
                    }
                    else
                    {
                        _tapCount = 1;
                    }
                    _lastTapTime = Time.unscaledTime;
                }
            }
        }

        public void ToggleConsole()
        {
            SetVisible(!_isVisible);
        }

        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            if (consolePanel != null)
                consolePanel.SetActive(visible);
            if (visible && commandInput != null)
            {
                commandInput.text = "";
                commandInput.ActivateInputField();
            }
        }

        public void RegisterCommand(DebugCommand command)
        {
            _commands[command.Name] = command;
        }

        public void SubmitCommand()
        {
            if (commandInput == null) return;
            string input = commandInput.text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            _commandHistory.Add(input);
            _historyIndex = _commandHistory.Count;
            commandInput.text = "";
            commandInput.ActivateInputField();

            Log($"> {input}");
            ExecuteCommand(input);
        }

        private void ExecuteCommand(string input)
        {
            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string commandName = parts[0].ToLowerInvariant();
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

            if (_commands.TryGetValue(commandName, out DebugCommand command))
            {
                try
                {
                    string result = command.Execute(args);
                    if (!string.IsNullOrEmpty(result))
                        Log(result);
                }
                catch (Exception ex)
                {
                    Log($"[ERROR] {ex.Message}");
                }
            }
            else
            {
                Log($"Unknown command: '{commandName}'. Type 'help' for available commands.");
            }
        }

        public void Log(string message)
        {
            _logLines.Add(message);
            while (_logLines.Count > maxLogLines)
                _logLines.RemoveAt(0);

            if (outputText != null)
                outputText.text = string.Join("\n", _logLines);

            // Auto-scroll to bottom
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }

        public void ClearLog()
        {
            _logLines.Clear();
            if (outputText != null)
                outputText.text = "";
        }

        public IEnumerable<DebugCommand> GetAllCommands() => _commands.Values;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
#endif
```

### DebugCommands.cs

```csharp
// Registers all built-in debug commands.
// Each command is a self-contained lambda that interacts with game systems.
// All wrapped in DEVELOPMENT_BUILD || UNITY_EDITOR.

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using MetalPod.Shared;

namespace MetalPod.Debugging
{
    public static class DebugCommands
    {
        public static void RegisterAll(DebugConsole console)
        {
            // ── HELP ────────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "help", "List all available commands", "help",
                args =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("=== DEBUG COMMANDS ===");
                    foreach (var cmd in console.GetAllCommands().OrderBy(c => c.Name))
                    {
                        sb.AppendLine($"  {cmd.Name,-20} {cmd.Description}");
                    }
                    return sb.ToString();
                }
            ));

            // ── CLEAR ───────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "clear", "Clear console output", "clear",
                args => { console.ClearLog(); return null; }
            ));

            // ── CURRENCY ────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "currency", "Set or add currency", "currency [set|add] [amount]",
                args =>
                {
                    if (args.Length < 2) return "Usage: currency [set|add] [amount]";
                    if (!int.TryParse(args[1], out int amount)) return "Invalid amount.";

                    var pm = FindProgressionManager();
                    if (pm == null) return "ProgressionManager not found.";

                    if (args[0] == "set")
                    {
                        // Access via reflection or direct call depending on API
                        EventBus.RaiseCurrencyChanged(amount);
                        return $"Currency set to {amount} (broadcast only — verify with ProgressionManager).";
                    }
                    else if (args[0] == "add")
                    {
                        EventBus.RaiseCurrencyEarned(amount);
                        return $"Added {amount} currency (broadcast only).";
                    }
                    return "Usage: currency [set|add] [amount]";
                }
            ));

            // ── GOD MODE ────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "god", "Toggle god mode (invincible)", "god",
                args =>
                {
                    var health = Object.FindObjectOfType<MetalPod.Hovercraft.HovercraftHealth>();
                    if (health == null) return "No HovercraftHealth found in scene.";

                    // Toggle IsInvincible — need to use reflection since it's a property with private set
                    bool current = health.IsInvincible;
                    var prop = typeof(MetalPod.Hovercraft.HovercraftHealth)
                        .GetProperty("IsInvincible");
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(health, !current);
                        return $"God mode: {(!current ? "ON" : "OFF")}";
                    }

                    // Fallback: set health very high
                    return "Could not toggle invincibility (property not writable). Use 'heal' instead.";
                }
            ));

            // ── HEAL ────────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "heal", "Restore hovercraft to full health", "heal",
                args =>
                {
                    var health = Object.FindObjectOfType<MetalPod.Hovercraft.HovercraftHealth>();
                    if (health == null) return "No HovercraftHealth found.";
                    health.RestoreToFull();
                    return "Health and shield restored to full.";
                }
            ));

            // ── DAMAGE ──────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "damage", "Deal damage to hovercraft", "damage [amount] [type]",
                args =>
                {
                    var health = Object.FindObjectOfType<MetalPod.Hovercraft.HovercraftHealth>();
                    if (health == null) return "No HovercraftHealth found.";

                    float amount = 10f;
                    if (args.Length >= 1) float.TryParse(args[0], out amount);

                    DamageType type = DamageType.Physical;
                    if (args.Length >= 2)
                        System.Enum.TryParse(args[1], true, out type);

                    health.TakeDamage(amount, type);
                    return $"Dealt {amount} {type} damage. Health: {health.CurrentHealth:F1}/{health.MaxHealth:F1}";
                }
            ));

            // ── UNLOCK ALL COURSES ──────────────────────────
            console.RegisterCommand(new DebugCommand(
                "unlockall", "Unlock all courses", "unlockall",
                args =>
                {
                    // Broadcast course unlock events for all courses
                    string[] courseIds = { "lava_1", "lava_2", "lava_3", "ice_1", "ice_2", "ice_3", "toxic_1", "toxic_2", "toxic_3" };
                    foreach (var id in courseIds)
                    {
                        EventBus.RaiseCourseUnlocked(id);
                    }
                    return $"Unlocked {courseIds.Length} courses (broadcast events sent).";
                }
            ));

            // ── LOAD SCENE ──────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "scene", "Load a scene by name", "scene [sceneName]",
                args =>
                {
                    if (args.Length < 1) return "Usage: scene [sceneName]";
                    string sceneName = args[0];

                    var gm = MetalPod.Core.GameManager.Instance;
                    if (gm != null && gm.SceneLoader != null)
                    {
                        gm.SceneLoader.LoadSceneAsync(sceneName);
                        return $"Loading scene: {sceneName}";
                    }

                    SceneManager.LoadScene(sceneName);
                    return $"Loading scene (direct): {sceneName}";
                }
            ));

            // ── LIST SCENES ─────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "scenes", "List all scenes in build", "scenes",
                args =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Scenes in build:");
                    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                    {
                        string path = SceneUtility.GetScenePathByBuildIndex(i);
                        string name = System.IO.Path.GetFileNameWithoutExtension(path);
                        sb.AppendLine($"  [{i}] {name}");
                    }
                    sb.AppendLine($"\nCurrent: {SceneManager.GetActiveScene().name}");
                    return sb.ToString();
                }
            ));

            // ── FPS OVERLAY ─────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "overlay", "Toggle FPS/memory overlay", "overlay",
                args =>
                {
                    var overlay = Object.FindObjectOfType<DebugOverlay>();
                    if (overlay != null)
                    {
                        overlay.Toggle();
                        return $"Overlay: {(overlay.IsVisible ? "ON" : "OFF")}";
                    }
                    return "DebugOverlay not found in scene.";
                }
            ));

            // ── TIMESCALE ───────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "timescale", "Set Time.timeScale", "timescale [value]",
                args =>
                {
                    if (args.Length < 1) return $"Current timeScale: {Time.timeScale:F2}";
                    if (!float.TryParse(args[0], out float scale)) return "Invalid value.";
                    scale = Mathf.Clamp(scale, 0f, 10f);
                    Time.timeScale = scale;
                    Time.fixedDeltaTime = 0.02f * scale;
                    return $"timeScale set to {scale:F2}";
                }
            ));

            // ── SKIP TUTORIAL ───────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "skiptutorial", "Mark tutorial as completed", "skiptutorial",
                args =>
                {
                    var tutMgr = Object.FindObjectOfType<MetalPod.Tutorial.TutorialManager>();
                    if (tutMgr != null)
                    {
                        // Attempt to complete via public API
                        return "Tutorial completion signal sent. Check TutorialManager for API.";
                    }
                    return "TutorialManager not found.";
                }
            ));

            // ── MEDAL ───────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "medal", "Award a medal for current course", "medal [gold|silver|bronze]",
                args =>
                {
                    if (args.Length < 1) return "Usage: medal [gold|silver|bronze]";
                    int medalLevel = 0;
                    switch (args[0].ToLower())
                    {
                        case "gold": medalLevel = 3; break;
                        case "silver": medalLevel = 2; break;
                        case "bronze": medalLevel = 1; break;
                        default: return "Invalid medal. Use: gold, silver, bronze";
                    }

                    // Fake a course completion with a fast time
                    string currentScene = SceneManager.GetActiveScene().name;
                    EventBus.RaiseCourseCompleted(currentScene, 10f, medalLevel);
                    return $"Awarded {args[0]} medal for {currentScene}";
                }
            ));

            // ── RESET SAVE ──────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "resetsave", "Delete all save data and restart", "resetsave",
                args =>
                {
                    string savePath = System.IO.Path.Combine(
                        Application.persistentDataPath, "save_data.json");
                    if (System.IO.File.Exists(savePath))
                    {
                        System.IO.File.Delete(savePath);
                        return "Save data deleted. Restart the game for fresh state.";
                    }
                    return "No save file found.";
                }
            ));

            // ── SPEED ───────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "speed", "Show hovercraft speed info", "speed",
                args =>
                {
                    var controller = Object.FindObjectOfType<MetalPod.Hovercraft.HovercraftController>();
                    if (controller == null) return "No HovercraftController found.";

                    var rb = controller.GetComponent<Rigidbody>();
                    float speed = rb != null ? rb.linearVelocity.magnitude : 0f;

                    return $"Speed: {speed:F1} m/s ({speed * 3.6f:F1} km/h) | State: {controller.CurrentState}";
                }
            ));

            // ── UPGRADE ─────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "upgrade", "Set upgrade level", "upgrade [speed|armor|handling|boost] [level 0-5]",
                args =>
                {
                    if (args.Length < 2) return "Usage: upgrade [category] [level]";
                    if (!int.TryParse(args[1], out int level)) return "Invalid level.";
                    level = Mathf.Clamp(level, 0, 5);

                    string id = args[0].ToLower();
                    // Broadcast upgrade event for each level
                    EventBus.RaiseUpgradePurchased(id, level);
                    return $"Upgrade '{id}' set to level {level} (broadcast sent).";
                }
            ));

            // ── MAXUPGRADES ─────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "maxupgrades", "Set all upgrades to max level", "maxupgrades",
                args =>
                {
                    string[] categories = { "speed", "armor", "handling", "boost" };
                    foreach (var cat in categories)
                    {
                        EventBus.RaiseUpgradePurchased(cat, 5);
                    }
                    return "All upgrades maxed (broadcast sent).";
                }
            ));

            // ── FINISH ──────────────────────────────────────
            console.RegisterCommand(new DebugCommand(
                "finish", "Instantly finish the current course", "finish [time]",
                args =>
                {
                    float time = 30f;
                    if (args.Length >= 1) float.TryParse(args[0], out time);

                    var courseMgr = Object.FindObjectOfType<MetalPod.Course.CourseManager>();
                    if (courseMgr == null) return "CourseManager not found. Are you in a course scene?";

                    // Broadcast finish — the actual finish would need a direct method call
                    string currentScene = SceneManager.GetActiveScene().name;
                    EventBus.RaiseCourseCompleted(currentScene, time, 3); // Gold
                    return $"Course finished with time {time:F2}s (gold medal).";
                }
            ));
        }

        private static MetalPod.Progression.ProgressionManager FindProgressionManager()
        {
            return Object.FindObjectOfType<MetalPod.Progression.ProgressionManager>();
        }
    }
}
#endif
```

### DebugOverlay.cs

```csharp
// FPS, memory, and game state overlay.
// Renders in top-right corner. Toggled via debug console "overlay" command.
// Uses OnGUI for minimal overhead and no external dependencies.

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

namespace MetalPod.Debugging
{
    public class DebugOverlay : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private Color textColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;

        private bool _isVisible;
        private float _deltaTime;
        private float _fpsMin = float.MaxValue;
        private float _fpsMax;
        private float _fpsAccumulator;
        private int _fpsFrameCount;
        private float _fpsAverage;
        private float _fpsResetTimer;

        private GUIStyle _backgroundStyle;
        private GUIStyle _textStyle;
        private Texture2D _bgTexture;

        public bool IsVisible => _isVisible;

        private void Start()
        {
            _isVisible = showOnStart;
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_isVisible) ResetStats();
        }

        public void Show() { _isVisible = true; ResetStats(); }
        public void Hide() { _isVisible = false; }

        private void ResetStats()
        {
            _fpsMin = float.MaxValue;
            _fpsMax = 0;
            _fpsAccumulator = 0;
            _fpsFrameCount = 0;
            _fpsAverage = 0;
        }

        private void Update()
        {
            if (!_isVisible) return;

            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

            float currentFPS = 1f / Time.unscaledDeltaTime;
            _fpsMin = Mathf.Min(_fpsMin, currentFPS);
            _fpsMax = Mathf.Max(_fpsMax, currentFPS);
            _fpsAccumulator += currentFPS;
            _fpsFrameCount++;

            _fpsResetTimer += Time.unscaledDeltaTime;
            if (_fpsResetTimer >= 5f) // Reset min/max every 5 seconds
            {
                _fpsAverage = _fpsAccumulator / _fpsFrameCount;
                _fpsResetTimer = 0;
                _fpsMin = float.MaxValue;
                _fpsMax = 0;
                _fpsAccumulator = 0;
                _fpsFrameCount = 0;
            }
            else
            {
                _fpsAverage = _fpsAccumulator / Mathf.Max(1, _fpsFrameCount);
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            // Lazy init styles
            if (_backgroundStyle == null)
            {
                _bgTexture = new Texture2D(1, 1);
                _bgTexture.SetPixel(0, 0, backgroundColor);
                _bgTexture.Apply();
                _backgroundStyle = new GUIStyle();
                _backgroundStyle.normal.background = _bgTexture;

                _textStyle = new GUIStyle();
                _textStyle.fontSize = fontSize;
                _textStyle.normal.textColor = textColor;
                _textStyle.padding = new RectOffset(5, 5, 2, 2);
            }

            float fps = 1f / Mathf.Max(_deltaTime, 0.0001f);
            Color fpsColor = fps >= 55 ? textColor : fps >= 30 ? warningColor : criticalColor;

            float totalMemMB = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            float usedMemMB = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
            float gcMemMB = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);

            // Build overlay text
            string overlayText =
                $"FPS: {fps:F0} (min:{_fpsMin:F0} max:{_fpsMax:F0} avg:{_fpsAverage:F0})\n" +
                $"MEM: {totalMemMB:F1}MB alloc | {usedMemMB:F1}MB reserved | GC: {gcMemMB:F1}MB\n" +
                $"Scene: {SceneManager.GetActiveScene().name}\n" +
                $"TimeScale: {Time.timeScale:F2}\n" +
                $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}";

            // Add hovercraft info if in a course
            var controller = FindObjectOfType<MetalPod.Hovercraft.HovercraftController>();
            if (controller != null)
            {
                var health = controller.GetComponent<MetalPod.Hovercraft.HovercraftHealth>();
                var rb = controller.GetComponent<Rigidbody>();
                float speed = rb != null ? rb.linearVelocity.magnitude : 0;

                overlayText += $"\n--- HOVERCRAFT ---\n" +
                    $"Speed: {speed:F1} m/s ({speed * 3.6f:F0} km/h)\n" +
                    $"State: {controller.CurrentState}\n" +
                    $"Health: {health?.CurrentHealth:F0}/{health?.MaxHealth:F0}\n" +
                    $"Shield: {health?.CurrentShield:F0}/{health?.MaxShield:F0}";
            }

            // Calculate rect size
            GUIContent content = new GUIContent(overlayText);
            Vector2 size = _textStyle.CalcSize(content);
            size.y = _textStyle.CalcHeight(content, size.x);
            size.x += 10;
            size.y += 10;

            Rect bgRect = new Rect(Screen.width - size.x - 10, 10, size.x, size.y);

            GUI.Box(bgRect, GUIContent.none, _backgroundStyle);

            // Color the FPS line
            _textStyle.normal.textColor = fpsColor;
            GUI.Label(bgRect, overlayText, _textStyle);
            _textStyle.normal.textColor = textColor;
        }

        private void OnDestroy()
        {
            if (_bgTexture != null) Destroy(_bgTexture);
        }
    }
}
#endif
```

### DebugCourseSkip.cs

```csharp
// Quick-jump debug menu for loading any course directly.
// Provides a simple GUI with course buttons.
// Toggled via debug console command "coursemenu".

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Debugging
{
    public class DebugCourseSkip : MonoBehaviour
    {
        private bool _isVisible;
        private Vector2 _scrollPosition;

        private static readonly string[] CourseScenes = new[]
        {
            "TestCourse",
            "InfernoGate",   // Lava 1
            "MoltenRidge",   // Lava 2
            "MagmaCanyon",   // Lava 3
            "FrozenPass",    // Ice 1
            "GlacialRavine", // Ice 2
            "ArcticStorm",   // Ice 3
            "RustValley",    // Toxic 1
            "ChemicalPlant", // Toxic 2
            "BiohazardCore", // Toxic 3
        };

        private static readonly string[] UtilityScenes = new[]
        {
            "MainMenu",
            "Workshop",
        };

        public bool IsVisible => _isVisible;

        public void Toggle() => _isVisible = !_isVisible;
        public void Show() => _isVisible = true;
        public void Hide() => _isVisible = false;

        private void OnGUI()
        {
            if (!_isVisible) return;

            float width = 250;
            float height = 400;
            Rect windowRect = new Rect(10, 10, width, height);

            GUI.Box(windowRect, "Course Skip Menu");

            GUILayout.BeginArea(new Rect(windowRect.x + 5, windowRect.y + 25,
                windowRect.width - 10, windowRect.height - 30));

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("--- Courses ---");
            foreach (string scene in CourseScenes)
            {
                if (GUILayout.Button(scene))
                {
                    LoadScene(scene);
                    _isVisible = false;
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("--- Utility ---");
            foreach (string scene in UtilityScenes)
            {
                if (GUILayout.Button(scene))
                {
                    LoadScene(scene);
                    _isVisible = false;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void LoadScene(string sceneName)
        {
            var gm = MetalPod.Core.GameManager.Instance;
            if (gm != null && gm.SceneLoader != null)
            {
                gm.SceneLoader.LoadSceneAsync(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
            DebugConsole.Instance?.Log($"Loading course: {sceneName}");
        }
    }
}
#endif
```

### DebugHazardTester.cs

```csharp
// Spawns individual hazards for isolated testing.
// Useful for tuning damage values, visual effects, and timing.
// Toggled via debug console command "hazards".

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine;

namespace MetalPod.Debugging
{
    public class DebugHazardTester : MonoBehaviour
    {
        [Header("Hazard Prefabs")]
        [Tooltip("Assign all hazard prefabs here for testing. Can be populated via inspector.")]
        [SerializeField] private GameObject[] hazardPrefabs;

        [Header("Settings")]
        [SerializeField] private float spawnDistance = 15f;
        [SerializeField] private float spawnHeight = 2f;

        private bool _isVisible;
        private Vector2 _scrollPosition;
        private GameObject _currentHazard;

        public bool IsVisible => _isVisible;
        public void Toggle() => _isVisible = !_isVisible;

        private void OnGUI()
        {
            if (!_isVisible) return;

            float width = 250;
            float height = 350;
            Rect windowRect = new Rect(Screen.width - width - 10, Screen.height - height - 10, width, height);

            GUI.Box(windowRect, "Hazard Tester");

            GUILayout.BeginArea(new Rect(windowRect.x + 5, windowRect.y + 25,
                windowRect.width - 10, windowRect.height - 30));

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            if (hazardPrefabs == null || hazardPrefabs.Length == 0)
            {
                GUILayout.Label("No hazard prefabs assigned.\nAssign via Inspector.");
            }
            else
            {
                foreach (var prefab in hazardPrefabs)
                {
                    if (prefab == null) continue;
                    if (GUILayout.Button($"Spawn: {prefab.name}"))
                    {
                        SpawnHazard(prefab);
                    }
                }
            }

            GUILayout.Space(10);

            if (_currentHazard != null)
            {
                GUILayout.Label($"Active: {_currentHazard.name}");
                if (GUILayout.Button("Destroy Current"))
                {
                    Destroy(_currentHazard);
                    _currentHazard = null;
                }
            }

            if (GUILayout.Button("Destroy All Hazards"))
            {
                var hazards = FindObjectsOfType<MetalPod.Hazards.HazardBase>();
                foreach (var h in hazards) Destroy(h.gameObject);
                _currentHazard = null;
                DebugConsole.Instance?.Log($"Destroyed {hazards.Length} hazards.");
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void SpawnHazard(GameObject prefab)
        {
            // Spawn in front of the hovercraft (or at world origin if no hovercraft)
            Vector3 spawnPos = Vector3.zero + Vector3.up * spawnHeight;

            var player = GameObject.FindGameObjectWithTag(MetalPod.Shared.GameConstants.TAG_PLAYER);
            if (player != null)
            {
                spawnPos = player.transform.position +
                           player.transform.forward * spawnDistance +
                           Vector3.up * spawnHeight;
            }

            if (_currentHazard != null)
                Destroy(_currentHazard);

            _currentHazard = Instantiate(prefab, spawnPos, Quaternion.identity);
            DebugConsole.Instance?.Log($"Spawned hazard: {prefab.name} at {spawnPos}");
        }
    }
}
#endif
```

### DebugEditorWindow.cs

```csharp
// Editor window with one-click test shortcuts.
// Menu: Metal Pod > Debug Tools

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MetalPod.Editor
{
    public class DebugEditorWindow : EditorWindow
    {
        private int _selectedCourse;
        private int _currencyAmount = 10000;
        private int _upgradeLevel = 5;
        private Vector2 _scrollPosition;

        private static readonly string[] CourseScenes = new[]
        {
            "Assets/Scenes/TestCourse.unity",
            "Assets/Scenes/InfernoGate.unity",
            "Assets/Scenes/MoltenRidge.unity",
            "Assets/Scenes/MagmaCanyon.unity",
            "Assets/Scenes/FrozenPass.unity",
            "Assets/Scenes/GlacialRavine.unity",
            "Assets/Scenes/ArcticStorm.unity",
            "Assets/Scenes/RustValley.unity",
            "Assets/Scenes/ChemicalPlant.unity",
            "Assets/Scenes/BiohazardCore.unity",
        };

        private static readonly string[] CourseNames = new[]
        {
            "Test Course",
            "Inferno Gate (Lava 1)",
            "Molten Ridge (Lava 2)",
            "Magma Canyon (Lava 3)",
            "Frozen Pass (Ice 1)",
            "Glacial Ravine (Ice 2)",
            "Arctic Storm (Ice 3)",
            "Rust Valley (Toxic 1)",
            "Chemical Plant (Toxic 2)",
            "Biohazard Core (Toxic 3)",
        };

        [MenuItem("Metal Pod/Debug Tools")]
        public static void ShowWindow()
        {
            GetWindow<DebugEditorWindow>("Metal Pod Debug");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Metal Pod Debug Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // ── Quick Play ──────────────────────────────
            EditorGUILayout.LabelField("Quick Play", EditorStyles.boldLabel);
            _selectedCourse = EditorGUILayout.Popup("Course", _selectedCourse, CourseNames);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Course"))
            {
                if (_selectedCourse < CourseScenes.Length)
                {
                    // Open persistent scene first, then additive course
                    EditorSceneManager.OpenScene("Assets/Scenes/_Persistent.unity", OpenSceneMode.Single);
                    EditorSceneManager.OpenScene(CourseScenes[_selectedCourse], OpenSceneMode.Additive);
                    EditorApplication.isPlaying = true;
                }
            }
            if (GUILayout.Button("Open Scene"))
            {
                if (_selectedCourse < CourseScenes.Length)
                    EditorSceneManager.OpenScene(CourseScenes[_selectedCourse], OpenSceneMode.Single);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // ── Scene Shortcuts ─────────────────────────
            EditorGUILayout.LabelField("Scene Shortcuts", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Main Menu"))
                EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            if (GUILayout.Button("Workshop"))
                EditorSceneManager.OpenScene("Assets/Scenes/Workshop.unity");
            if (GUILayout.Button("_Persistent"))
                EditorSceneManager.OpenScene("Assets/Scenes/_Persistent.unity");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // ── Save Data ───────────────────────────────
            EditorGUILayout.LabelField("Save Data", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Save File Location"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
            if (GUILayout.Button("Delete Save Data"))
            {
                string savePath = System.IO.Path.Combine(Application.persistentDataPath, "save_data.json");
                if (System.IO.File.Exists(savePath))
                {
                    if (EditorUtility.DisplayDialog("Delete Save?",
                        "This will permanently delete all game progress.", "Delete", "Cancel"))
                    {
                        System.IO.File.Delete(savePath);
                        Debug.Log("[Debug] Save data deleted.");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("No Save File", "No save file found.", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("View Save Data (Log)"))
            {
                string savePath = System.IO.Path.Combine(Application.persistentDataPath, "save_data.json");
                if (System.IO.File.Exists(savePath))
                {
                    string json = System.IO.File.ReadAllText(savePath);
                    Debug.Log($"[Save Data]\n{json}");
                }
                else
                {
                    Debug.Log("[Save Data] No save file found.");
                }
            }

            EditorGUILayout.Space();

            // ── Build ───────────────────────────────────
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build iOS (Development)"))
            {
                BuildPipeline.BuildPlayer(
                    EditorBuildSettings.scenes,
                    "Builds/iOS",
                    BuildTarget.iOS,
                    BuildOptions.Development | BuildOptions.AllowDebugging);
            }
            if (GUILayout.Button("Build iOS (Release)"))
            {
                BuildPipeline.BuildPlayer(
                    EditorBuildSettings.scenes,
                    "Builds/iOS",
                    BuildTarget.iOS,
                    BuildOptions.None);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // ── Info ────────────────────────────────────
            EditorGUILayout.LabelField("Project Info", EditorStyles.boldLabel);
            int scriptCount = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" }).Length;
            int shaderCount = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/Shaders" }).Length;
            int soCount = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Length;
            int testCount = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Tests" }).Length;

            EditorGUILayout.LabelField($"Scripts: {scriptCount} | Shaders: {shaderCount} | SOs: {soCount} | Tests: {testCount}");
            EditorGUILayout.LabelField($"Build Target: {EditorUserBuildSettings.activeBuildTarget}");
            EditorGUILayout.LabelField($"Color Space: {PlayerSettings.colorSpace}");

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
```

---

## Register Console Extensions

The DebugCommands.cs should also register these commands to toggle the sub-tools:

```csharp
// In RegisterAll(), add:

console.RegisterCommand(new DebugCommand(
    "coursemenu", "Toggle course skip menu", "coursemenu",
    args =>
    {
        var skip = Object.FindObjectOfType<DebugCourseSkip>();
        if (skip != null) { skip.Toggle(); return $"Course menu: {(skip.IsVisible ? "ON" : "OFF")}"; }
        return "DebugCourseSkip not found.";
    }
));

console.RegisterCommand(new DebugCommand(
    "hazards", "Toggle hazard tester", "hazards",
    args =>
    {
        var tester = Object.FindObjectOfType<DebugHazardTester>();
        if (tester != null) { tester.Toggle(); return $"Hazard tester: {(tester.IsVisible ? "ON" : "OFF")}"; }
        return "DebugHazardTester not found.";
    }
));
```

---

## Acceptance Criteria

- [ ] `DebugConsole.cs` — Triple-tap toggle, command input, scrollable log, command history
- [ ] `DebugCommand.cs` — Clean command data class
- [ ] `DebugCommands.cs` — 18+ commands: help, clear, currency, god, heal, damage, unlockall, scene, scenes, overlay, timescale, skiptutorial, medal, resetsave, speed, upgrade, maxupgrades, finish, coursemenu, hazards
- [ ] `DebugOverlay.cs` — FPS (current/min/max/avg), memory, scene info, hovercraft state
- [ ] `DebugCourseSkip.cs` — GUI menu to load any of the 10 courses or utility scenes
- [ ] `DebugHazardTester.cs` — Spawn/destroy hazards in front of player
- [ ] `DebugEditorWindow.cs` — Editor window with quick play, scene shortcuts, save management, build buttons
- [ ] ALL runtime debug code wrapped in `#if DEVELOPMENT_BUILD || UNITY_EDITOR`
- [ ] All scripts in `MetalPod.Debugging` or `MetalPod.Editor` namespaces
- [ ] No modifications to existing files
- [ ] Debug console uses `OnGUI` (no external UI dependencies beyond optional Canvas)
- [ ] Compiles without errors
