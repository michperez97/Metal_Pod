using System;
using System.IO;
using UnityEngine;

namespace MetalPod.Progression
{
    public class SaveSystem : MonoBehaviour
    {
        private const string SaveFileName = "metalpod_save.json";
        private const string BackupFileName = "metalpod_save_backup.json";
        private const int SaveFormatVersion = 1;

        [Header("Autosave")]
        [SerializeField] private float autoSaveIntervalSeconds = 30f;
        [SerializeField] private bool saveOnApplicationPause = true;

        public SaveData CurrentData { get; private set; }

        private bool _initialized;
        private bool _dirty;
        private float _autoSaveTimer;

        public string SavePath => GetSavePath(SaveFileName);
        public string BackupSavePath => GetSavePath(BackupFileName);

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_initialized || !_dirty || autoSaveIntervalSeconds <= 0f)
            {
                return;
            }

            _autoSaveTimer += Time.unscaledDeltaTime;
            if (_autoSaveTimer >= autoSaveIntervalSeconds)
            {
                Save();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && saveOnApplicationPause && _dirty)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            if (_dirty)
            {
                Save();
            }
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            CurrentData = Load() ?? SaveData.CreateDefault();
            MigrateIfNeeded(CurrentData);
            _initialized = true;
            _dirty = false;
            _autoSaveTimer = 0f;
        }

        public void MarkDirty()
        {
            if (!_initialized)
            {
                Initialize();
            }

            _dirty = true;
        }

        public void Save()
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (CurrentData == null)
            {
                CurrentData = SaveData.CreateDefault();
            }

            EnsureDirectory();
            BackupCurrentSave();

            CurrentData.version = SaveFormatVersion;
            CurrentData.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string json = JsonUtility.ToJson(CurrentData, true);
            File.WriteAllText(SavePath, json);

            _dirty = false;
            _autoSaveTimer = 0f;
        }

        public SaveData Load()
        {
            if (TryLoadFile(SavePath, out SaveData loaded))
            {
                return loaded;
            }

            if (TryLoadFile(BackupSavePath, out SaveData backup))
            {
                return backup;
            }

            return null;
        }

        public void ResetSave()
        {
            CurrentData = SaveData.CreateDefault();
            MarkDirty();
            Save();
        }

        private bool TryLoadFile(string path, out SaveData data)
        {
            data = null;
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                SaveData parsed = JsonUtility.FromJson<SaveData>(json);
                if (parsed == null)
                {
                    return false;
                }

                data = MigrateIfNeeded(parsed);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private SaveData MigrateIfNeeded(SaveData data)
        {
            if (data == null)
            {
                return SaveData.CreateDefault();
            }

            // Placeholder for future migration pipeline.
            // if (data.version < 2) { ... }

            if (data.ownedCosmetics == null)
            {
                data.ownedCosmetics = new System.Collections.Generic.List<string>();
            }

            if (!data.ownedCosmetics.Contains("default"))
            {
                data.ownedCosmetics.Add("default");
            }

            if (!data.ownedCosmetics.Contains("decal_73"))
            {
                data.ownedCosmetics.Add("decal_73");
            }

            data.totalMedals = data.GetTotalMedals();
            return data;
        }

        private void BackupCurrentSave()
        {
            if (!File.Exists(SavePath))
            {
                return;
            }

            try
            {
                File.Copy(SavePath, BackupSavePath, true);
            }
            catch
            {
                // Non-fatal: continue saving main file.
            }
        }

        private string GetSavePath(string filename)
        {
            return Path.Combine(Application.persistentDataPath, filename);
        }

        private void EnsureDirectory()
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
