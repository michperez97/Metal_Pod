using System;
using System.IO;
using System.Reflection;
using MetalPod.Progression;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace MetalPod.CloudSave
{
    /// <summary>
    /// Manages iCloud save backup. Mirrors local SaveSystem data to NSUbiquitousKeyValueStore.
    /// </summary>
    public class CloudSaveManager : MonoBehaviour
    {
        public static CloudSaveManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool autoBackupOnSave = true;
        [SerializeField] private bool autoRestoreOnStart = true;
        [SerializeField] private bool showConflictPrompt = true;
        [SerializeField] private ConflictResolution autoConflictStrategy = ConflictResolution.UseNewest;

        public event Action OnBackupComplete;
        public event Action OnRestoreComplete;
        public event Action<CloudSaveConflict> OnConflictDetected;

        public bool IsCloudAvailable { get; private set; }
        public bool HasCloudSave { get; private set; }
        public long CloudTimestamp { get; private set; }
        public long LocalTimestamp { get; private set; }

        private SaveSystem _saveSystem;
        private bool _initialized;
        private bool _restoringFromCloud;
        private long _observedLocalTimestamp;

        private const string CloudSaveKey = "MetalPodSaveData";
        private const string CloudTimestampKey = "MetalPodSaveTimestamp";

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern bool _CloudSave_IsAvailable();
        [DllImport("__Internal")] private static extern void _CloudSave_SetString(string key, string value);
        [DllImport("__Internal")] private static extern void _CloudSave_SetTimestamp(string key, long timestamp);
        [DllImport("__Internal")] private static extern string _CloudSave_GetString(string key);
        [DllImport("__Internal")] private static extern long _CloudSave_GetTimestamp(string key);
        [DllImport("__Internal")] private static extern bool _CloudSave_Synchronize();
        [DllImport("__Internal")] private static extern void _CloudSave_Remove(string key);
        [DllImport("__Internal")] private static extern void _CloudSave_RegisterForNotifications();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject go = new GameObject("CloudSaveManager");
            DontDestroyOnLoad(go);
            go.AddComponent<CloudSaveManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.name = "CloudSaveManager";
        }

        private void Start()
        {
            _saveSystem = FindObjectOfType<SaveSystem>();
            if (_saveSystem == null)
            {
                Debug.LogWarning("[CloudSave] SaveSystem not found at startup. Cloud save will wait for SaveSystem.");
            }

            Initialize();
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            if (_saveSystem == null)
            {
                _saveSystem = FindObjectOfType<SaveSystem>();
                return;
            }

            LocalTimestamp = _saveSystem.CurrentData?.lastSaveTimestamp ?? 0L;
            if (_observedLocalTimestamp == 0L && LocalTimestamp > 0L)
            {
                _observedLocalTimestamp = LocalTimestamp;
            }

            // Auto-backup based on local timestamp changes so SaveSystem integration is optional.
            if (!autoBackupOnSave || !IsCloudAvailable || _restoringFromCloud)
            {
                return;
            }

            if (LocalTimestamp > 0L && LocalTimestamp != _observedLocalTimestamp)
            {
                _observedLocalTimestamp = LocalTimestamp;
                OnLocalSaveCompleted();
            }
        }

        private void Initialize()
        {
#if UNITY_IOS && !UNITY_EDITOR
            IsCloudAvailable = _CloudSave_IsAvailable();
            if (IsCloudAvailable)
            {
                _CloudSave_RegisterForNotifications();
            }
#else
            IsCloudAvailable = false;
#endif

            CheckCloudState();
            if (autoRestoreOnStart && IsCloudAvailable)
            {
                CompareAndResolve();
            }

            _initialized = true;
            Debug.Log($"[CloudSave] Initialized. Available: {IsCloudAvailable}, HasCloud: {HasCloudSave}");
        }

        private void CheckCloudState()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_Synchronize();
            string cloudJson = _CloudSave_GetString(CloudSaveKey);
            CloudTimestamp = _CloudSave_GetTimestamp(CloudTimestampKey);
            HasCloudSave = !string.IsNullOrEmpty(cloudJson) && cloudJson.Length > 2;
#else
            CloudTimestamp = 0L;
            HasCloudSave = false;
#endif

            LocalTimestamp = _saveSystem?.CurrentData?.lastSaveTimestamp ?? 0L;
            if (_observedLocalTimestamp == 0L && LocalTimestamp > 0L)
            {
                _observedLocalTimestamp = LocalTimestamp;
            }
        }

        /// <summary>
        /// Upload current local save to iCloud.
        /// </summary>
        public void BackupToCloud()
        {
            if (!IsCloudAvailable)
            {
                Debug.LogWarning("[CloudSave] iCloud not available.");
                return;
            }

            if (_saveSystem == null)
            {
                _saveSystem = FindObjectOfType<SaveSystem>();
            }

            if (_saveSystem?.CurrentData == null)
            {
                Debug.LogWarning("[CloudSave] No local save data to backup.");
                return;
            }

            string json = JsonUtility.ToJson(_saveSystem.CurrentData);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[CloudSave] Local save JSON is empty. Backup skipped.");
                return;
            }

            long timestamp = _saveSystem.CurrentData.lastSaveTimestamp;
            if (timestamp <= 0L)
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_SetString(CloudSaveKey, json);
            _CloudSave_SetTimestamp(CloudTimestampKey, timestamp);
            _CloudSave_Synchronize();
#endif

            CloudTimestamp = timestamp;
            HasCloudSave = true;

            Debug.Log($"[CloudSave] Backed up to iCloud ({json.Length} bytes)");
            OnBackupComplete?.Invoke();
        }

        /// <summary>
        /// Download cloud save and replace local save.
        /// </summary>
        public void RestoreFromCloud()
        {
            if (!IsCloudAvailable || !HasCloudSave)
            {
                Debug.LogWarning("[CloudSave] No cloud save to restore.");
                return;
            }

            if (_saveSystem == null)
            {
                _saveSystem = FindObjectOfType<SaveSystem>();
            }

            if (_saveSystem == null)
            {
                Debug.LogError("[CloudSave] SaveSystem not found; cannot restore cloud save.");
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_Synchronize();
            string cloudJson = _CloudSave_GetString(CloudSaveKey);
#else
            string cloudJson = string.Empty;
#endif

            if (string.IsNullOrEmpty(cloudJson))
            {
                Debug.LogWarning("[CloudSave] Cloud save is empty.");
                return;
            }

            try
            {
                SaveData cloudData = JsonUtility.FromJson<SaveData>(cloudJson);
                if (cloudData == null)
                {
                    Debug.LogError("[CloudSave] Failed to parse cloud save data.");
                    return;
                }

                _restoringFromCloud = true;
                File.WriteAllText(_saveSystem.SavePath, cloudJson);
                ApplyRestoredDataToSaveSystem(cloudData);

                LocalTimestamp = cloudData.lastSaveTimestamp;
                _observedLocalTimestamp = LocalTimestamp;

                Debug.Log("[CloudSave] Restored from iCloud successfully.");
                OnRestoreComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CloudSave] Restore failed: {ex.Message}");
            }
            finally
            {
                _restoringFromCloud = false;
            }
        }

        /// <summary>
        /// Delete cloud save data.
        /// </summary>
        public void DeleteCloudSave()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_Remove(CloudSaveKey);
            _CloudSave_Remove(CloudTimestampKey);
            _CloudSave_Synchronize();
#endif
            HasCloudSave = false;
            CloudTimestamp = 0L;
            Debug.Log("[CloudSave] Cloud save deleted.");
        }

        /// <summary>
        /// Get info about the cloud save without restoring it.
        /// </summary>
        public CloudSaveInfo GetCloudSaveInfo()
        {
            CheckCloudState();

            string cloudJson = string.Empty;
#if UNITY_IOS && !UNITY_EDITOR
            cloudJson = _CloudSave_GetString(CloudSaveKey);
#endif

            SaveData cloudData = null;
            if (!string.IsNullOrEmpty(cloudJson) && cloudJson.Length > 2)
            {
                try
                {
                    cloudData = JsonUtility.FromJson<SaveData>(cloudJson);
                }
                catch
                {
                    // Ignore parse errors and return minimal metadata.
                }
            }

            return new CloudSaveInfo
            {
                exists = HasCloudSave,
                timestamp = CloudTimestamp,
                sizeBytes = cloudJson?.Length ?? 0,
                currency = cloudData?.currency ?? 0,
                totalMedals = cloudData?.GetTotalMedals() ?? 0,
                totalCoursesCompleted = cloudData?.totalCoursesCompleted ?? 0
            };
        }

        private void CompareAndResolve()
        {
            CheckCloudState();

            if (!HasCloudSave)
            {
                if (_saveSystem?.CurrentData != null && _saveSystem.CurrentData.lastSaveTimestamp > 0L)
                {
                    BackupToCloud();
                }
                return;
            }

            if (LocalTimestamp == 0L)
            {
                RestoreFromCloud();
                return;
            }

            if (CloudTimestamp > LocalTimestamp)
            {
                var conflict = new CloudSaveConflict
                {
                    localTimestamp = LocalTimestamp,
                    cloudTimestamp = CloudTimestamp,
                    localInfo = GetLocalSaveInfo(),
                    cloudInfo = GetCloudSaveInfo()
                };

                if (showConflictPrompt)
                {
                    OnConflictDetected?.Invoke(conflict);
                }
                else
                {
                    ConflictResolution resolution = CloudSaveConflictResolver.Resolve(conflict, autoConflictStrategy);
                    if (resolution == ConflictResolution.UseCloud)
                    {
                        RestoreFromCloud();
                    }
                    else
                    {
                        BackupToCloud();
                    }
                }
            }
            else if (LocalTimestamp > CloudTimestamp)
            {
                BackupToCloud();
            }
        }

        private CloudSaveInfo GetLocalSaveInfo()
        {
            SaveData data = _saveSystem?.CurrentData;
            int sizeBytes = 0;

            if (_saveSystem != null && File.Exists(_saveSystem.SavePath))
            {
                sizeBytes = (int)new FileInfo(_saveSystem.SavePath).Length;
            }

            return new CloudSaveInfo
            {
                exists = data != null,
                timestamp = data?.lastSaveTimestamp ?? 0L,
                sizeBytes = sizeBytes,
                currency = data?.currency ?? 0,
                totalMedals = data?.GetTotalMedals() ?? 0,
                totalCoursesCompleted = data?.totalCoursesCompleted ?? 0
            };
        }

        /// <summary>
        /// Called by native code via UnitySendMessage when iCloud data changes externally.
        /// </summary>
        public void OnCloudDataChanged(string reasonCode)
        {
            int reason = 0;
            int.TryParse(reasonCode, out reason);
            Debug.Log($"[CloudSave] External change detected. Reason: {reason}");

            // 0 = ServerChange, 1 = InitialSyncChange
            if (reason == 0 || reason == 1)
            {
                CompareAndResolve();
            }
            // 2 = QuotaViolation
            else if (reason == 2)
            {
                Debug.LogWarning("[CloudSave] iCloud quota violation! Save data may be too large.");
            }
            // 3 = AccountChange
            else if (reason == 3)
            {
#if UNITY_IOS && !UNITY_EDITOR
                IsCloudAvailable = _CloudSave_IsAvailable();
#endif
                CheckCloudState();
            }
        }

        /// <summary>
        /// Call this after every local save to auto-backup to cloud.
        /// </summary>
        public void OnLocalSaveCompleted()
        {
            if (autoBackupOnSave && IsCloudAvailable)
            {
                BackupToCloud();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsCloudAvailable)
            {
#if UNITY_IOS && !UNITY_EDITOR
                _CloudSave_Synchronize();
#endif
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ApplyRestoredDataToSaveSystem(SaveData cloudData)
        {
            // SaveSystem doesn't expose a setter/reload API; update CurrentData via backing field.
            FieldInfo currentDataField = typeof(SaveSystem).GetField("<CurrentData>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (currentDataField == null)
            {
                Debug.LogWarning("[CloudSave] Could not update SaveSystem.CurrentData at runtime. Data will apply on next launch.");
                return;
            }

            currentDataField.SetValue(_saveSystem, cloudData);
        }
    }

    [Serializable]
    public struct CloudSaveInfo
    {
        public bool exists;
        public long timestamp;
        public int sizeBytes;
        public int currency;
        public int totalMedals;
        public int totalCoursesCompleted;

        public string FormattedTimestamp
        {
            get
            {
                if (timestamp <= 0L)
                {
                    return "Never";
                }

                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
                return dateTime.ToString("MMM d, yyyy h:mm tt");
            }
        }
    }

    [Serializable]
    public struct CloudSaveConflict
    {
        public long localTimestamp;
        public long cloudTimestamp;
        public CloudSaveInfo localInfo;
        public CloudSaveInfo cloudInfo;
    }
}

