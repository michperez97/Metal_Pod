using System;
using System.Collections.Generic;

namespace MetalPod.Localization
{
    [Serializable]
    public class LanguageManifest
    {
        public string defaultLanguage = "en";
        public LanguageInfo[] languages = Array.Empty<LanguageInfo>();
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
            _strings = strings ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "[]";
            }

            return _strings.TryGetValue(key, out string value) ? value : $"[{key}]";
        }

        public string Get(string key, params object[] args)
        {
            string template = Get(key);
            if (args == null || args.Length == 0)
            {
                return template;
            }

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
            return !string.IsNullOrEmpty(key) && _strings.ContainsKey(key);
        }

        public int StringCount => _strings.Count;

        public IEnumerable<string> AllKeys => _strings.Keys;
    }
}
