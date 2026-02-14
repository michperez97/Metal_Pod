#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using MetalPod.Localization;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Editor
{
    /// <summary>
    /// Editor window for viewing, searching, and validating localization strings.
    /// </summary>
    public class LocalizationEditorWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _searchFilter = string.Empty;
        private string _selectedLanguage = "en";

        private string[] _availableLanguages = Array.Empty<string>();
        private Dictionary<string, string> _referenceStrings = new Dictionary<string, string>(StringComparer.Ordinal);
        private Dictionary<string, string> _currentStrings = new Dictionary<string, string>(StringComparer.Ordinal);
        private List<string> _missingKeys = new List<string>();

        [MenuItem("Metal Pod/Localization Editor")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization");
        }

        private void OnEnable()
        {
            Reload();
        }

        private void Reload()
        {
            _availableLanguages = LocalizationEditor.GetAvailableLanguageCodes();
            if (_availableLanguages.Length == 0)
            {
                _availableLanguages = new[] { "en" };
            }

            if (Array.IndexOf(_availableLanguages, _selectedLanguage) < 0)
            {
                _selectedLanguage = _availableLanguages[0];
            }

            _referenceStrings = LocalizationEditor.LoadLanguageFromDisk("en");
            _currentStrings = LocalizationEditor.LoadLanguageFromDisk(_selectedLanguage);
            _missingKeys = LocalizationEditor.FindMissingKeys(_referenceStrings, _currentStrings);

            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Localization Editor", EditorStyles.boldLabel);

            DrawToolbar();
            DrawStats();
            DrawStringsList();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            int currentIndex = Mathf.Max(0, Array.IndexOf(_availableLanguages, _selectedLanguage));
            int newIndex = EditorGUILayout.Popup("Language", currentIndex, _availableLanguages);
            if (newIndex != currentIndex && newIndex >= 0 && newIndex < _availableLanguages.Length)
            {
                _selectedLanguage = _availableLanguages[newIndex];
                Reload();
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(72f)))
            {
                Reload();
            }

            EditorGUILayout.EndHorizontal();

            _searchFilter = EditorGUILayout.TextField("Search", _searchFilter);
        }

        private void DrawStats()
        {
            int total = _referenceStrings != null ? _referenceStrings.Count : 0;
            int translated = _currentStrings != null ? _currentStrings.Count : 0;
            int missing = _missingKeys != null ? _missingKeys.Count : 0;

            EditorGUILayout.LabelField($"Strings: {translated}/{total} | Missing: {missing}");

            if (missing > 0)
            {
                EditorGUILayout.HelpBox($"{missing} keys are missing in {_selectedLanguage}.json", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"No missing keys in {_selectedLanguage}.json", MessageType.Info);
            }

            EditorGUILayout.Space(6f);
        }

        private void DrawStringsList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_referenceStrings == null || _referenceStrings.Count == 0)
            {
                EditorGUILayout.HelpBox("Reference English file is empty or missing.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            foreach (KeyValuePair<string, string> entry in _referenceStrings)
            {
                if (!MatchesSearch(entry.Key, entry.Value))
                {
                    continue;
                }

                bool isMissing = _currentStrings == null || !_currentStrings.ContainsKey(entry.Key);
                Color previousColor = GUI.color;
                if (isMissing)
                {
                    GUI.color = Color.yellow;
                }

                EditorGUILayout.LabelField(entry.Key, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("  EN: " + entry.Value, EditorStyles.wordWrappedLabel);

                string translated = isMissing ? "[MISSING]" : _currentStrings[entry.Key];
                EditorGUILayout.LabelField($"  {_selectedLanguage.ToUpperInvariant()}: {translated}", EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space(4f);

                GUI.color = previousColor;
            }

            EditorGUILayout.EndScrollView();
        }

        private bool MatchesSearch(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(_searchFilter))
            {
                return true;
            }

            return (key != null && key.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                || (value != null && value.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
#endif
