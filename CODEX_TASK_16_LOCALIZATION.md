# Codex Task 16: Localization System

> **Goal**: Create a full localization framework and extract all hardcoded UI strings into a localizable format. Ship with English (en) and Spanish (es) translations. This widens App Store reach significantly.

---

## Context

All UI text in the project is currently hardcoded in C# scripts (HUD, menus, results, workshop, tutorial, accessibility labels). This task creates a centralized localization system that:
- Loads language files from JSON in `Resources/`
- Provides a static `Loc.Get("key")` API for all scripts to use
- Auto-detects device language on first launch
- Allows runtime language switching from Settings
- Ships with English and Spanish translations

**Read these files to extract all hardcoded strings**:
- `Assets/Scripts/UI/HUD.cs` — Speed, health, timer, checkpoint text
- `Assets/Scripts/UI/MainMenuUI.cs` — Menu button labels
- `Assets/Scripts/UI/ResultsScreenUI.cs` — Medal names, results text
- `Assets/Scripts/UI/PauseMenuUI.cs` — Pause menu labels
- `Assets/Scripts/UI/CountdownUI.cs` — "3", "2", "1", "GO!"
- `Assets/Scripts/UI/LoadingScreenUI.cs` — Loading text
- `Assets/Scripts/UI/SettingsUI.cs` — Settings labels
- `Assets/Scripts/UI/CurrencyDisplay.cs` — Currency format
- `Assets/Scripts/UI/CourseUnlockedPopup.cs` — Unlock notification
- `Assets/Scripts/Workshop/CourseSelectionUI.cs` — Course info, lock text
- `Assets/Scripts/Workshop/UpgradeUI.cs` — Upgrade labels, costs
- `Assets/Scripts/Workshop/CustomizationUI.cs` — Cosmetic labels
- `Assets/Scripts/Workshop/WorkshopManager.cs` — Tab labels
- `Assets/Scripts/Tutorial/TutorialManager.cs` — Tutorial prompt text
- `Assets/Scripts/Tutorial/TutorialUI.cs` — Tutorial UI text
- `Assets/Scripts/Accessibility/AccessibilityLabels.cs` — All VoiceOver labels
- `Assets/Scripts/Course/MedalSystem.cs` — Medal names

---

## Files to Create

```
Assets/Scripts/Localization/
├── LocalizationManager.cs        # Singleton managing language state
├── Loc.cs                        # Static shorthand: Loc.Get("key")
├── LanguageData.cs               # Deserialized language file
├── LocalizedText.cs              # MonoBehaviour for auto-localizing Text/TMP
└── LocalizationEditor.cs         # Editor tool for managing strings

Assets/Resources/Localization/
├── en.json                       # English (complete — every string)
├── es.json                       # Spanish (complete translation)
└── language_manifest.json        # List of available languages + metadata

Assets/Scripts/Editor/
└── LocalizationEditorWindow.cs   # Editor window to view/edit/export strings
```

**DO NOT modify** any existing files. The localization system is opt-in — existing scripts continue to work with hardcoded strings. A future integration pass will swap `"text"` for `Loc.Get("key")`.

---

## Architecture

```
LocalizationManager (Singleton, DontDestroyOnLoad)
  ├── Loads language_manifest.json at boot
  ├── Auto-detects device language (Application.systemLanguage)
  ├── Falls back to English if language not available
  ├── Loads the selected language JSON from Resources/
  ├── Provides GetString(key, params) with string.Format support
  ├── OnLanguageChanged event for UI refresh
  └── Saves selected language to PlayerPrefs

Loc (Static helper)
  ├── Loc.Get("key") → string
  ├── Loc.Get("key", arg0, arg1) → formatted string
  └── Wraps LocalizationManager.Instance

LocalizedText (MonoBehaviour)
  ├── Attach to any GameObject with Text or TextMeshProUGUI
  ├── Set localizationKey in inspector
  ├── Auto-updates on language change
  └── Supports format arguments from code

LanguageData
  ├── Deserialized from JSON
  ├── Dictionary<string, string> lookup
  └── Metadata (language name, code, direction)
```

---

## Detailed Specifications

### language_manifest.json

```json
{
  "defaultLanguage": "en",
  "languages": [
    {
      "code": "en",
      "name": "English",
      "nativeName": "English",
      "file": "en",
      "isRTL": false
    },
    {
      "code": "es",
      "name": "Spanish",
      "nativeName": "Español",
      "file": "es",
      "isRTL": false
    }
  ]
}
```

### en.json (English — Complete String Table)

The agent must create a comprehensive English JSON file with ALL game strings organized by category. Here is the required structure and key naming convention:

```json
{
  "_meta": {
    "language": "en",
    "version": 1,
    "author": "Metal Pod Team"
  },

  "menu.play": "Play",
  "menu.settings": "Settings",
  "menu.quit": "Quit",
  "menu.title": "Metal Pod",
  "menu.subtitle": "Heavy Metal Hovercraft Racing",

  "workshop.courses": "Courses",
  "workshop.upgrades": "Upgrades",
  "workshop.customize": "Customize",
  "workshop.back": "Back",
  "workshop.launch": "Launch",

  "course.locked": "Locked",
  "course.locked_requires": "Requires: {0}",
  "course.best_time": "Best: {0}",
  "course.no_time": "No time set",
  "course.medal_gold": "Gold",
  "course.medal_silver": "Silver",
  "course.medal_bronze": "Bronze",
  "course.medal_none": "No Medal",
  "course.checkpoint": "Checkpoint {0}/{1}",

  "course.lava1": "Inferno Gate",
  "course.lava2": "Molten Ridge",
  "course.lava3": "Magma Canyon",
  "course.ice1": "Frozen Pass",
  "course.ice2": "Glacial Ravine",
  "course.ice3": "Arctic Storm",
  "course.toxic1": "Rust Valley",
  "course.toxic2": "Chemical Plant",
  "course.toxic3": "Biohazard Core",
  "course.test": "Test Course",

  "env.lava": "Lava Zone",
  "env.ice": "Ice Zone",
  "env.toxic": "Toxic Zone",

  "hud.speed": "{0} km/h",
  "hud.health": "Health",
  "hud.shield": "Shield",
  "hud.boost": "Boost",
  "hud.timer": "{0}",
  "hud.lap": "Lap {0}/{1}",

  "countdown.3": "3",
  "countdown.2": "2",
  "countdown.1": "1",
  "countdown.go": "GO!",

  "results.title": "Race Complete!",
  "results.failed": "Race Failed",
  "results.time": "Time: {0}",
  "results.best_time": "Best: {0}",
  "results.new_record": "New Record!",
  "results.medal_earned": "{0} Medal!",
  "results.bolts_earned": "+{0} Bolts",
  "results.retry": "Retry",
  "results.next": "Next Course",
  "results.workshop_return": "Workshop",

  "upgrade.speed": "Speed",
  "upgrade.armor": "Armor",
  "upgrade.handling": "Handling",
  "upgrade.boost": "Boost",
  "upgrade.level": "Level {0}/{1}",
  "upgrade.max": "MAX",
  "upgrade.cost": "{0} Bolts",
  "upgrade.purchase": "Upgrade",
  "upgrade.insufficient": "Not enough bolts",
  "upgrade.stat_current": "Current: {0}",
  "upgrade.stat_next": "Next: {0}",

  "cosmetic.colors": "Colors",
  "cosmetic.decals": "Decals",
  "cosmetic.parts": "Parts",
  "cosmetic.owned": "Owned",
  "cosmetic.equipped": "Equipped",
  "cosmetic.locked": "Locked — {0} Bolts",
  "cosmetic.purchase": "Buy",
  "cosmetic.equip": "Equip",

  "pause.title": "Paused",
  "pause.resume": "Resume",
  "pause.restart": "Restart",
  "pause.settings": "Settings",
  "pause.quit": "Quit to Menu",

  "settings.title": "Settings",
  "settings.master_volume": "Master Volume",
  "settings.music_volume": "Music",
  "settings.sfx_volume": "SFX",
  "settings.tilt_sensitivity": "Tilt Sensitivity",
  "settings.invert_tilt": "Invert Tilt",
  "settings.haptics": "Haptic Feedback",
  "settings.language": "Language",
  "settings.quality": "Graphics Quality",
  "settings.quality_low": "Low",
  "settings.quality_medium": "Medium",
  "settings.quality_high": "High",

  "loading.tip_prefix": "Tip: ",
  "loading.loading": "Loading...",

  "currency.bolts": "{0} Bolts",

  "unlock.title": "Course Unlocked!",
  "unlock.message": "{0} is now available!",

  "tutorial.welcome": "Welcome to Metal Pod!",
  "tutorial.steer": "Tilt your device to steer",
  "tutorial.speed": "You automatically accelerate forward",
  "tutorial.boost": "Tap the right side to boost!",
  "tutorial.brake": "Tap the left side to brake",
  "tutorial.health": "Watch your health bar — avoid hazards!",
  "tutorial.hazard": "Hazards deal different types of damage",
  "tutorial.checkpoint": "Pass through checkpoints to save progress",
  "tutorial.collectible": "Collect bolts to earn currency",
  "tutorial.finish": "Cross the finish line to complete the course!",
  "tutorial.skip": "Skip Tutorial",
  "tutorial.next": "Next",
  "tutorial.got_it": "Got it!",
  "tutorial.workshop_welcome": "Welcome to the Workshop!",
  "tutorial.workshop_tabs": "Use the tabs to browse courses, upgrades, and cosmetics",
  "tutorial.workshop_launch": "Select a course and tap Launch to race!",

  "accessibility.play_button": "Play. Double tap to enter the Workshop.",
  "accessibility.settings_button": "Settings. Double tap to open settings.",
  "accessibility.speed_display": "Current speed: {0} kilometers per hour",
  "accessibility.health_bar": "Health: {0} percent",
  "accessibility.shield_bar": "Shield: {0} percent",
  "accessibility.boost_bar": "Boost: {0} percent",
  "accessibility.checkpoint_reached": "Checkpoint reached. {0} of {1}.",
  "accessibility.damage_taken": "Damage taken. Health at {0} percent.",
  "accessibility.medal_earned": "{0} medal earned!",
  "accessibility.course_unlocked": "New course unlocked: {0}!",
  "accessibility.race_complete": "Race complete! Time: {0}. Medal: {1}. Bolts earned: {2}."
}
```

### es.json (Spanish — Complete Translation)

The agent must create a full Spanish translation matching every key in `en.json`. Here are representative entries — the agent should translate ALL keys:

```json
{
  "_meta": {
    "language": "es",
    "version": 1,
    "author": "Metal Pod Team"
  },

  "menu.play": "Jugar",
  "menu.settings": "Ajustes",
  "menu.quit": "Salir",
  "menu.title": "Metal Pod",
  "menu.subtitle": "Carreras de Aerodeslizador Heavy Metal",

  "workshop.courses": "Circuitos",
  "workshop.upgrades": "Mejoras",
  "workshop.customize": "Personalizar",
  "workshop.back": "Atrás",
  "workshop.launch": "Iniciar",

  "course.locked": "Bloqueado",
  "course.locked_requires": "Requiere: {0}",
  "course.best_time": "Mejor: {0}",
  "course.medal_gold": "Oro",
  "course.medal_silver": "Plata",
  "course.medal_bronze": "Bronce",
  "course.medal_none": "Sin Medalla"
}
```

**(The agent must complete ALL keys — this is just a sample. Every key in en.json must appear in es.json.)**

---

## C# Specifications

### LanguageData.cs

```csharp
// Represents a loaded language file. Deserialized from JSON.

using System;
using System.Collections.Generic;

namespace MetalPod.Localization
{
    [Serializable]
    public class LanguageManifest
    {
        public string defaultLanguage;
        public LanguageInfo[] languages;
    }

    [Serializable]
    public class LanguageInfo
    {
        public string code;
        public string name;
        public string nativeName;
        public string file;
        public bool isRTL;
    }

    [Serializable]
    public class LanguageFileWrapper
    {
        // Unity's JsonUtility doesn't support Dictionary, so we use a list of entries
        public LocalizationEntry[] entries;
    }

    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string value;
    }

    /// <summary>
    /// Runtime language data with fast key lookup.
    /// </summary>
    public class LanguageData
    {
        public LanguageInfo Info { get; }
        private readonly Dictionary<string, string> _strings;

        public LanguageData(LanguageInfo info, Dictionary<string, string> strings)
        {
            Info = info;
            _strings = strings;
        }

        public string Get(string key)
        {
            if (_strings.TryGetValue(key, out string value))
                return value;
            return $"[{key}]"; // Missing key indicator
        }

        public string Get(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        public bool HasKey(string key)
        {
            return _strings.ContainsKey(key);
        }

        public int StringCount => _strings.Count;

        public IEnumerable<string> AllKeys => _strings.Keys;
    }
}
```

### LocalizationManager.cs

```csharp
// Singleton managing language loading, switching, and string access.
// Lives on _Persistent scene, DontDestroyOnLoad.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetalPod.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        private const string PREF_LANGUAGE = "SelectedLanguage";

        public static LocalizationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string manifestPath = "Localization/language_manifest";
        [SerializeField] private bool autoDetectLanguage = true;

        public event Action OnLanguageChanged;

        public LanguageData CurrentLanguage { get; private set; }
        public string CurrentLanguageCode { get; private set; }
        public LanguageManifest Manifest { get; private set; }

        private readonly Dictionary<string, LanguageData> _loadedLanguages
            = new Dictionary<string, LanguageData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadManifest();
            string langCode = DetermineLanguage();
            SetLanguage(langCode);
        }

        private void LoadManifest()
        {
            var manifestAsset = Resources.Load<TextAsset>(manifestPath);
            if (manifestAsset == null)
            {
                Debug.LogError($"[Localization] Manifest not found at Resources/{manifestPath}");
                Manifest = new LanguageManifest
                {
                    defaultLanguage = "en",
                    languages = new[] { new LanguageInfo { code = "en", name = "English", nativeName = "English", file = "en" } }
                };
                return;
            }
            Manifest = JsonUtility.FromJson<LanguageManifest>(manifestAsset.text);
        }

        private string DetermineLanguage()
        {
            // Check saved preference first
            if (PlayerPrefs.HasKey(PREF_LANGUAGE))
            {
                string saved = PlayerPrefs.GetString(PREF_LANGUAGE);
                if (IsLanguageAvailable(saved)) return saved;
            }

            // Auto-detect from device
            if (autoDetectLanguage)
            {
                string detected = MapSystemLanguage(Application.systemLanguage);
                if (IsLanguageAvailable(detected)) return detected;
            }

            return Manifest.defaultLanguage;
        }

        public bool IsLanguageAvailable(string code)
        {
            if (Manifest?.languages == null) return false;
            foreach (var lang in Manifest.languages)
            {
                if (lang.code == code) return true;
            }
            return false;
        }

        public void SetLanguage(string languageCode)
        {
            if (!IsLanguageAvailable(languageCode))
            {
                Debug.LogWarning($"[Localization] Language '{languageCode}' not available. Using default.");
                languageCode = Manifest.defaultLanguage;
            }

            if (!_loadedLanguages.TryGetValue(languageCode, out LanguageData data))
            {
                data = LoadLanguageFile(languageCode);
                if (data == null)
                {
                    Debug.LogError($"[Localization] Failed to load language: {languageCode}");
                    return;
                }
                _loadedLanguages[languageCode] = data;
            }

            CurrentLanguage = data;
            CurrentLanguageCode = languageCode;

            PlayerPrefs.SetString(PREF_LANGUAGE, languageCode);
            PlayerPrefs.Save();

            Debug.Log($"[Localization] Language set to: {languageCode} ({data.StringCount} strings)");
            OnLanguageChanged?.Invoke();
        }

        private LanguageData LoadLanguageFile(string code)
        {
            LanguageInfo info = null;
            foreach (var lang in Manifest.languages)
            {
                if (lang.code == code) { info = lang; break; }
            }
            if (info == null) return null;

            var textAsset = Resources.Load<TextAsset>($"Localization/{info.file}");
            if (textAsset == null)
            {
                Debug.LogError($"[Localization] Language file not found: Resources/Localization/{info.file}");
                return null;
            }

            // Parse JSON manually since Unity JsonUtility doesn't support Dictionary
            var strings = ParseJsonToDictionary(textAsset.text);
            return new LanguageData(info, strings);
        }

        /// <summary>
        /// Simple JSON parser for flat key-value string objects.
        /// Handles the format: { "key": "value", ... }
        /// Skips the "_meta" object.
        /// </summary>
        private Dictionary<string, string> ParseJsonToDictionary(string json)
        {
            var dict = new Dictionary<string, string>();

            // Remove outer braces and split by lines
            json = json.Trim();
            if (json.StartsWith("{")) json = json.Substring(1);
            if (json.EndsWith("}")) json = json.Substring(0, json.Length - 1);

            string[] lines = json.Split('\n');
            bool inMeta = false;
            int braceDepth = 0;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim().TrimEnd(',');

                // Skip meta block
                if (line.Contains("\"_meta\""))
                {
                    inMeta = true;
                    braceDepth = 0;
                }
                if (inMeta)
                {
                    braceDepth += CountChar(line, '{') - CountChar(line, '}');
                    if (braceDepth <= 0 && line.Contains("}"))
                    {
                        inMeta = false;
                    }
                    continue;
                }

                // Parse "key": "value"
                int colonIndex = line.IndexOf(':');
                if (colonIndex < 0) continue;

                string key = ExtractQuotedString(line.Substring(0, colonIndex));
                string value = ExtractQuotedString(line.Substring(colonIndex + 1));

                if (!string.IsNullOrEmpty(key) && value != null)
                {
                    dict[key] = value;
                }
            }

            return dict;
        }

        private string ExtractQuotedString(string s)
        {
            s = s.Trim();
            int first = s.IndexOf('"');
            int last = s.LastIndexOf('"');
            if (first >= 0 && last > first)
            {
                return s.Substring(first + 1, last - first - 1)
                    .Replace("\\n", "\n")
                    .Replace("\\\"", "\"");
            }
            return null;
        }

        private int CountChar(string s, char c)
        {
            int count = 0;
            foreach (char ch in s) if (ch == c) count++;
            return count;
        }

        private string MapSystemLanguage(SystemLanguage sysLang)
        {
            switch (sysLang)
            {
                case SystemLanguage.English: return "en";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Chinese: return "zh";
                case SystemLanguage.ChineseSimplified: return "zh";
                case SystemLanguage.ChineseTraditional: return "zh-TW";
                default: return "en";
            }
        }

        public string GetString(string key)
        {
            return CurrentLanguage?.Get(key) ?? $"[{key}]";
        }

        public string GetString(string key, params object[] args)
        {
            return CurrentLanguage?.Get(key, args) ?? $"[{key}]";
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
```

### Loc.cs

```csharp
// Static shorthand for localization access.
// Usage: string text = Loc.Get("menu.play");
//        string text = Loc.Get("hud.speed", speedValue);

namespace MetalPod.Localization
{
    public static class Loc
    {
        /// <summary>Get a localized string by key.</summary>
        public static string Get(string key)
        {
            if (LocalizationManager.Instance != null)
                return LocalizationManager.Instance.GetString(key);
            return $"[{key}]";
        }

        /// <summary>Get a localized string with format arguments.</summary>
        public static string Get(string key, params object[] args)
        {
            if (LocalizationManager.Instance != null)
                return LocalizationManager.Instance.GetString(key, args);
            return $"[{key}]";
        }

        /// <summary>Get the current language code (e.g., "en", "es").</summary>
        public static string CurrentLanguage =>
            LocalizationManager.Instance?.CurrentLanguageCode ?? "en";
    }
}
```

### LocalizedText.cs

```csharp
// Attach to any UI Text or TextMeshProUGUI to auto-localize.
// Set localizationKey in the inspector. Text updates automatically on language change.

using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Localization
{
    [AddComponentMenu("MetalPod/Localized Text")]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string localizationKey;

        private Text _uiText;
        private TMPro.TextMeshProUGUI _tmpText;

        public string Key
        {
            get => localizationKey;
            set { localizationKey = value; UpdateText(); }
        }

        private void Awake()
        {
            _uiText = GetComponent<Text>();
            _tmpText = GetComponent<TMPro.TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }

        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey)) return;

            string text = Loc.Get(localizationKey);

            if (_tmpText != null)
                _tmpText.text = text;
            else if (_uiText != null)
                _uiText.text = text;
        }

        /// <summary>
        /// Update text with format arguments (call from code).
        /// </summary>
        public void UpdateText(params object[] args)
        {
            if (string.IsNullOrEmpty(localizationKey)) return;

            string text = Loc.Get(localizationKey, args);

            if (_tmpText != null)
                _tmpText.text = text;
            else if (_uiText != null)
                _uiText.text = text;
        }
    }
}
```

### LocalizationEditorWindow.cs

```csharp
// Editor window for viewing, searching, and validating localization strings.
// Menu: Metal Pod > Localization Editor

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace MetalPod.Editor
{
    public class LocalizationEditorWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private string _selectedLanguage = "en";
        private Dictionary<string, string> _currentStrings;
        private Dictionary<string, string> _referenceStrings; // English as reference
        private List<string> _missingKeys = new List<string>();

        [MenuItem("Metal Pod/Localization Editor")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization");
        }

        private void OnEnable()
        {
            LoadStrings();
        }

        private void LoadStrings()
        {
            _referenceStrings = LoadJsonFile("en");
            _currentStrings = LoadJsonFile(_selectedLanguage);
            FindMissingKeys();
        }

        private Dictionary<string, string> LoadJsonFile(string langCode)
        {
            string path = $"Assets/Resources/Localization/{langCode}.json";
            if (!File.Exists(path)) return new Dictionary<string, string>();

            string json = File.ReadAllText(path);
            // Simple parse — same logic as LocalizationManager
            var dict = new Dictionary<string, string>();
            string[] lines = json.Split('\n');
            bool inMeta = false;
            int braceDepth = 0;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim().TrimEnd(',');
                if (line.Contains("\"_meta\"")) { inMeta = true; braceDepth = 0; }
                if (inMeta)
                {
                    foreach (char c in line) { if (c == '{') braceDepth++; if (c == '}') braceDepth--; }
                    if (braceDepth <= 0 && line.Contains("}")) inMeta = false;
                    continue;
                }

                int colon = line.IndexOf(':');
                if (colon < 0) continue;

                string key = ExtractQuoted(line.Substring(0, colon));
                string val = ExtractQuoted(line.Substring(colon + 1));
                if (!string.IsNullOrEmpty(key) && val != null) dict[key] = val;
            }
            return dict;
        }

        private string ExtractQuoted(string s)
        {
            s = s.Trim();
            int f = s.IndexOf('"'), l = s.LastIndexOf('"');
            return (f >= 0 && l > f) ? s.Substring(f + 1, l - f - 1) : null;
        }

        private void FindMissingKeys()
        {
            _missingKeys.Clear();
            if (_referenceStrings == null || _currentStrings == null) return;
            foreach (var key in _referenceStrings.Keys)
            {
                if (!_currentStrings.ContainsKey(key))
                    _missingKeys.Add(key);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Localization Editor", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            string[] langs = { "en", "es" };
            int idx = System.Array.IndexOf(langs, _selectedLanguage);
            int newIdx = EditorGUILayout.Popup("Language", Mathf.Max(0, idx), langs);
            if (newIdx != idx) { _selectedLanguage = langs[newIdx]; LoadStrings(); }

            if (GUILayout.Button("Refresh", GUILayout.Width(60))) LoadStrings();
            EditorGUILayout.EndHorizontal();

            _searchFilter = EditorGUILayout.TextField("Search", _searchFilter);

            // Stats
            int total = _referenceStrings?.Count ?? 0;
            int translated = _currentStrings?.Count ?? 0;
            int missing = _missingKeys.Count;
            EditorGUILayout.LabelField($"Strings: {translated}/{total} | Missing: {missing}");

            if (missing > 0)
            {
                EditorGUILayout.HelpBox($"{missing} keys are missing in {_selectedLanguage}.json", MessageType.Warning);
            }

            EditorGUILayout.Space();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_referenceStrings != null)
            {
                foreach (var kvp in _referenceStrings)
                {
                    if (!string.IsNullOrEmpty(_searchFilter) &&
                        !kvp.Key.Contains(_searchFilter) && !kvp.Value.Contains(_searchFilter))
                        continue;

                    bool isMissing = !_currentStrings.ContainsKey(kvp.Key);
                    Color prev = GUI.color;
                    if (isMissing) GUI.color = Color.yellow;

                    EditorGUILayout.LabelField(kvp.Key, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  EN: {kvp.Value}");
                    string translated = _currentStrings.ContainsKey(kvp.Key) ? _currentStrings[kvp.Key] : "[MISSING]";
                    EditorGUILayout.LabelField($"  {_selectedLanguage.ToUpper()}: {translated}");
                    EditorGUILayout.Space(2);

                    GUI.color = prev;
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
```

---

## Acceptance Criteria

- [ ] `LocalizationManager.cs` — Singleton, loads manifest + language JSONs, auto-detect, runtime switching
- [ ] `Loc.cs` — Static `Loc.Get("key")` and `Loc.Get("key", args)` shorthand
- [ ] `LanguageData.cs` — Manifest, LanguageInfo, LanguageData with dictionary lookup
- [ ] `LocalizedText.cs` — MonoBehaviour for auto-localizing Text/TMP, responds to language changes
- [ ] `LocalizationEditorWindow.cs` — Editor window with search, missing key detection, stats
- [ ] `language_manifest.json` — Manifest listing en + es
- [ ] `en.json` — **100+ keys** covering ALL game strings (menu, workshop, courses, HUD, results, upgrades, cosmetics, pause, settings, tutorial, accessibility)
- [ ] `es.json` — Complete Spanish translation matching every key in en.json
- [ ] All C# in `MetalPod.Localization` or `MetalPod.Editor` namespaces
- [ ] No modifications to existing files
- [ ] JSON files are valid and parseable
- [ ] Compiles without errors
