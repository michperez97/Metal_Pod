using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MetalPod.Localization
{
    /// <summary>
    /// Singleton managing language loading, switching, and string access.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private const string PrefLanguage = "SelectedLanguage";

        public static LocalizationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string manifestPath = "Localization/language_manifest";
        [SerializeField] private bool autoDetectLanguage = true;

        public event Action OnLanguageChanged;

        public LanguageData CurrentLanguage { get; private set; }
        public string CurrentLanguageCode { get; private set; }
        public LanguageManifest Manifest { get; private set; }

        private readonly Dictionary<string, LanguageData> _loadedLanguages =
            new Dictionary<string, LanguageData>(StringComparer.OrdinalIgnoreCase);

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

            string initialLanguage = DetermineLanguage();
            SetLanguage(initialLanguage);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void LoadManifest()
        {
            TextAsset manifestAsset = Resources.Load<TextAsset>(manifestPath);
            if (manifestAsset == null)
            {
                Debug.LogError($"[Localization] Manifest not found at Resources/{manifestPath}");
                Manifest = new LanguageManifest
                {
                    defaultLanguage = "en",
                    languages = new[]
                    {
                        new LanguageInfo
                        {
                            code = "en",
                            name = "English",
                            nativeName = "English",
                            file = "en",
                            isRTL = false
                        }
                    }
                };
                return;
            }

            Manifest = JsonUtility.FromJson<LanguageManifest>(manifestAsset.text);
            if (Manifest == null || Manifest.languages == null || Manifest.languages.Length == 0)
            {
                Debug.LogError("[Localization] Manifest was invalid. Falling back to English.");
                Manifest = new LanguageManifest
                {
                    defaultLanguage = "en",
                    languages = new[]
                    {
                        new LanguageInfo
                        {
                            code = "en",
                            name = "English",
                            nativeName = "English",
                            file = "en",
                            isRTL = false
                        }
                    }
                };
            }

            if (string.IsNullOrWhiteSpace(Manifest.defaultLanguage))
            {
                Manifest.defaultLanguage = "en";
            }
        }

        private string DetermineLanguage()
        {
            if (PlayerPrefs.HasKey(PrefLanguage))
            {
                string savedLanguage = PlayerPrefs.GetString(PrefLanguage);
                if (IsLanguageAvailable(savedLanguage))
                {
                    return savedLanguage;
                }
            }

            if (autoDetectLanguage)
            {
                string mapped = MapSystemLanguage(Application.systemLanguage);
                if (IsLanguageAvailable(mapped))
                {
                    return mapped;
                }
            }

            return Manifest != null ? Manifest.defaultLanguage : "en";
        }

        public bool IsLanguageAvailable(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || Manifest?.languages == null)
            {
                return false;
            }

            for (int i = 0; i < Manifest.languages.Length; i++)
            {
                LanguageInfo language = Manifest.languages[i];
                if (language != null && string.Equals(language.code, code, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetLanguage(string languageCode)
        {
            if (Manifest == null)
            {
                LoadManifest();
            }

            if (!IsLanguageAvailable(languageCode))
            {
                Debug.LogWarning($"[Localization] Language '{languageCode}' not available. Using default.");
                languageCode = Manifest != null ? Manifest.defaultLanguage : "en";
            }

            if (CurrentLanguage != null && string.Equals(CurrentLanguageCode, languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
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

            PlayerPrefs.SetString(PrefLanguage, languageCode);
            PlayerPrefs.Save();

            Debug.Log($"[Localization] Language set to: {languageCode} ({data.StringCount} strings)");
            OnLanguageChanged?.Invoke();
        }

        public string GetString(string key)
        {
            return CurrentLanguage?.Get(key) ?? $"[{key}]";
        }

        public string GetString(string key, params object[] args)
        {
            return CurrentLanguage?.Get(key, args) ?? $"[{key}]";
        }

        private LanguageData LoadLanguageFile(string code)
        {
            LanguageInfo info = null;
            if (Manifest?.languages != null)
            {
                for (int i = 0; i < Manifest.languages.Length; i++)
                {
                    LanguageInfo lang = Manifest.languages[i];
                    if (lang != null && string.Equals(lang.code, code, StringComparison.OrdinalIgnoreCase))
                    {
                        info = lang;
                        break;
                    }
                }
            }

            if (info == null)
            {
                return null;
            }

            TextAsset textAsset = Resources.Load<TextAsset>($"Localization/{info.file}");
            if (textAsset == null)
            {
                Debug.LogError($"[Localization] Language file not found: Resources/Localization/{info.file}");
                return null;
            }

            Dictionary<string, string> strings = ParseLanguageJson(textAsset.text);
            return new LanguageData(info, strings);
        }

        /// <summary>
        /// Parses a language JSON file into a string table. Supports a flat key/value map with an optional nested _meta object.
        /// </summary>
        public static Dictionary<string, string> ParseLanguageJson(string json)
        {
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(json))
            {
                return dict;
            }

            int index = 0;
            SkipWhitespace(json, ref index);
            if (!ConsumeChar(json, ref index, '{'))
            {
                return dict;
            }

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);

                if (index >= json.Length)
                {
                    break;
                }

                if (json[index] == '}')
                {
                    index++;
                    break;
                }

                if (!TryReadJsonString(json, ref index, out string key))
                {
                    break;
                }

                SkipWhitespace(json, ref index);
                if (!ConsumeChar(json, ref index, ':'))
                {
                    break;
                }

                SkipWhitespace(json, ref index);
                if (index >= json.Length)
                {
                    break;
                }

                char token = json[index];
                if (token == '"')
                {
                    if (TryReadJsonString(json, ref index, out string value) && !string.Equals(key, "_meta", StringComparison.Ordinal))
                    {
                        dict[key] = value;
                    }
                }
                else if (token == '{')
                {
                    SkipNestedBlock(json, ref index, '{', '}');
                }
                else if (token == '[')
                {
                    SkipNestedBlock(json, ref index, '[', ']');
                }
                else
                {
                    SkipLiteral(json, ref index);
                }

                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ',')
                {
                    index++;
                }
            }

            return dict;
        }

        private static string MapSystemLanguage(SystemLanguage systemLanguage)
        {
            switch (systemLanguage)
            {
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Portuguese:
                    return "pt";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Korean:
                    return "ko";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return "zh";
                case SystemLanguage.ChineseTraditional:
                    return "zh-TW";
                default:
                    return "en";
            }
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }

        private static bool ConsumeChar(string json, ref int index, char expected)
        {
            SkipWhitespace(json, ref index);
            if (index < json.Length && json[index] == expected)
            {
                index++;
                return true;
            }

            return false;
        }

        private static bool TryReadJsonString(string json, ref int index, out string value)
        {
            value = null;
            SkipWhitespace(json, ref index);

            if (index >= json.Length || json[index] != '"')
            {
                return false;
            }

            index++;
            var chars = new List<char>(64);

            while (index < json.Length)
            {
                char c = json[index++];
                if (c == '"')
                {
                    value = new string(chars.ToArray());
                    return true;
                }

                if (c != '\\')
                {
                    chars.Add(c);
                    continue;
                }

                if (index >= json.Length)
                {
                    break;
                }

                char escape = json[index++];
                switch (escape)
                {
                    case '"':
                        chars.Add('"');
                        break;
                    case '\\':
                        chars.Add('\\');
                        break;
                    case '/':
                        chars.Add('/');
                        break;
                    case 'b':
                        chars.Add('\b');
                        break;
                    case 'f':
                        chars.Add('\f');
                        break;
                    case 'n':
                        chars.Add('\n');
                        break;
                    case 'r':
                        chars.Add('\r');
                        break;
                    case 't':
                        chars.Add('\t');
                        break;
                    case 'u':
                        if (index + 3 < json.Length)
                        {
                            string hex = json.Substring(index, 4);
                            if (ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort codePoint))
                            {
                                chars.Add((char)codePoint);
                                index += 4;
                            }
                        }

                        break;
                    default:
                        chars.Add(escape);
                        break;
                }
            }

            return false;
        }

        private static void SkipNestedBlock(string json, ref int index, char openToken, char closeToken)
        {
            if (index >= json.Length || json[index] != openToken)
            {
                return;
            }

            int depth = 0;
            bool inString = false;
            bool escaped = false;

            while (index < json.Length)
            {
                char c = json[index++];

                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }

                    if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == openToken)
                {
                    depth++;
                    continue;
                }

                if (c == closeToken)
                {
                    depth--;
                    if (depth <= 0)
                    {
                        return;
                    }
                }
            }
        }

        private static void SkipLiteral(string json, ref int index)
        {
            while (index < json.Length)
            {
                char c = json[index];
                if (c == ',' || c == '}' || c == ']')
                {
                    return;
                }

                index++;
            }
        }
    }
}
