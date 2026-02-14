using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetalPod.Progression
{
    [Serializable]
    public struct IntEntry
    {
        public string key;
        public int value;

        public IntEntry(string key, int value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public struct FloatEntry
    {
        public string key;
        public float value;

        public FloatEntry(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public struct BoolEntry
    {
        public string key;
        public bool value;

        public BoolEntry(string key, bool value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class SerializableIntDict : IEnumerable<KeyValuePair<string, int>>
    {
        [SerializeField] private List<IntEntry> entries = new List<IntEntry>();

        public int this[string key]
        {
            get => GetValueOrDefault(key, 0);
            set => Set(key, value);
        }

        public int Count => entries.Count;

        public bool ContainsKey(string key)
        {
            return FindIndex(key) >= 0;
        }

        public bool TryGetValue(string key, out int value)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                value = default;
                return false;
            }

            value = entries[index].value;
            return true;
        }

        public int GetValueOrDefault(string key, int defaultValue = 0)
        {
            int index = FindIndex(key);
            return index < 0 ? defaultValue : entries[index].value;
        }

        public void Set(string key, int value)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                entries.Add(new IntEntry(key, value));
                return;
            }

            IntEntry entry = entries[index];
            entry.value = value;
            entries[index] = entry;
        }

        public bool Remove(string key)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                return false;
            }

            entries.RemoveAt(index);
            return true;
        }

        public Dictionary<string, int> ToDictionary()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                dictionary[entries[i].key] = entries[i].value;
            }

            return dictionary;
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                yield return new KeyValuePair<string, int>(entries[i].key, entries[i].value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int FindIndex(string key)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].key == key)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    [Serializable]
    public class SerializableFloatDict : IEnumerable<KeyValuePair<string, float>>
    {
        [SerializeField] private List<FloatEntry> entries = new List<FloatEntry>();

        public float this[string key]
        {
            get => GetValueOrDefault(key, 0f);
            set => Set(key, value);
        }

        public int Count => entries.Count;

        public bool ContainsKey(string key)
        {
            return FindIndex(key) >= 0;
        }

        public bool TryGetValue(string key, out float value)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                value = default;
                return false;
            }

            value = entries[index].value;
            return true;
        }

        public float GetValueOrDefault(string key, float defaultValue = 0f)
        {
            int index = FindIndex(key);
            return index < 0 ? defaultValue : entries[index].value;
        }

        public void Set(string key, float value)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                entries.Add(new FloatEntry(key, value));
                return;
            }

            FloatEntry entry = entries[index];
            entry.value = value;
            entries[index] = entry;
        }

        public bool Remove(string key)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                return false;
            }

            entries.RemoveAt(index);
            return true;
        }

        public Dictionary<string, float> ToDictionary()
        {
            Dictionary<string, float> dictionary = new Dictionary<string, float>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                dictionary[entries[i].key] = entries[i].value;
            }

            return dictionary;
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                yield return new KeyValuePair<string, float>(entries[i].key, entries[i].value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int FindIndex(string key)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].key == key)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    [Serializable]
    public class SerializableBoolDict : IEnumerable<KeyValuePair<string, bool>>
    {
        [SerializeField] private List<BoolEntry> entries = new List<BoolEntry>();

        public bool this[string key]
        {
            get => GetValueOrDefault(key, false);
            set => Set(key, value);
        }

        public int Count => entries.Count;

        public bool ContainsKey(string key)
        {
            return FindIndex(key) >= 0;
        }

        public bool TryGetValue(string key, out bool value)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                value = default;
                return false;
            }

            value = entries[index].value;
            return true;
        }

        public bool GetValueOrDefault(string key, bool defaultValue = false)
        {
            int index = FindIndex(key);
            return index < 0 ? defaultValue : entries[index].value;
        }

        public void Set(string key, bool value)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                entries.Add(new BoolEntry(key, value));
                return;
            }

            BoolEntry entry = entries[index];
            entry.value = value;
            entries[index] = entry;
        }

        public bool Remove(string key)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                return false;
            }

            entries.RemoveAt(index);
            return true;
        }

        public Dictionary<string, bool> ToDictionary()
        {
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                dictionary[entries[i].key] = entries[i].value;
            }

            return dictionary;
        }

        public IEnumerator<KeyValuePair<string, bool>> GetEnumerator()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                yield return new KeyValuePair<string, bool>(entries[i].key, entries[i].value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int FindIndex(string key)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].key == key)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public long lastSaveTimestamp;

        [Header("Currency")]
        public int currency = 0;

        [Header("Upgrades")]
        public SerializableIntDict upgradeLevels = new SerializableIntDict();

        [Header("Course Progress")]
        public SerializableFloatDict bestTimes = new SerializableFloatDict();
        public SerializableIntDict bestMedals = new SerializableIntDict();
        public SerializableBoolDict completedCourses = new SerializableBoolDict();
        public SerializableBoolDict unlockedCourses = new SerializableBoolDict();

        [Header("Tutorial")]
        public bool tutorialCompleted = false;
        public SerializableBoolDict completedTutorials = new SerializableBoolDict();

        [Header("Cosmetics")]
        public List<string> ownedCosmetics = new List<string>();
        public string equippedColorScheme = "default";
        public string equippedDecal = "";
        public string equippedPart = "";

        [Header("Stats")]
        public int totalMedals = 0;
        public float totalPlayTime = 0f;
        public int totalCoursesCompleted = 0;
        public int totalDeaths = 0;

        public int GetTotalMedals()
        {
            int count = 0;
            foreach (KeyValuePair<string, int> kvp in bestMedals)
            {
                if (kvp.Value > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetGoldMedalCount()
        {
            int count = 0;
            foreach (KeyValuePair<string, int> kvp in bestMedals)
            {
                if (kvp.Value >= (int)Course.Medal.Gold)
                {
                    count++;
                }
            }

            return count;
        }

        public static SaveData CreateDefault()
        {
            SaveData data = new SaveData();
            if (!data.ownedCosmetics.Contains("default"))
            {
                data.ownedCosmetics.Add("default");
            }

            if (!data.ownedCosmetics.Contains("decal_73"))
            {
                data.ownedCosmetics.Add("decal_73");
            }

            return data;
        }
    }
}
