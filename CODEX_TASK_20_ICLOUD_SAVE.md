# Codex Task 20: iCloud Save Backup

> **Goal**: Create an iCloud save backup system so players' progress persists across device upgrades and re-installs. Uses NSUbiquitousKeyValueStore for lightweight cloud sync with conflict resolution.

---

## Context

Metal Pod saves progress locally via `SaveSystem.cs` to `Application.persistentDataPath` as JSON. If a player gets a new iPhone or re-installs the game, all progress is lost. iCloud backup solves this.

Apple provides two iCloud storage APIs:
1. **NSUbiquitousKeyValueStore** — Key-value store, 1MB limit, auto-synced. Perfect for save data.
2. **CloudKit** — Full database, overkill for a single save file.

Metal Pod's save data (SaveData.cs) serializes to ~2-5KB JSON, well within the 1MB limit. We'll use NSUbiquitousKeyValueStore.

**Read these files**:
- `Assets/Scripts/Progression/SaveSystem.cs` — Current save/load (file-based JSON)
- `Assets/Scripts/Progression/SaveData.cs` — SaveData class structure (currency, upgrades, bestTimes, medals, cosmetics, stats)
- `Assets/Scripts/Core/GameManager.cs` — App lifecycle
- `Assets/Plugins/iOS/MetalPodNative.mm` — Existing native plugin (we'll add to a NEW file, not modify this)
- `Assets/Scripts/Core/iOSNativePlugin.cs` — Existing native bridge pattern

---

## Files to Create

```
Assets/Scripts/CloudSave/
├── CloudSaveManager.cs           # Singleton managing iCloud sync
├── CloudSaveConflictResolver.cs  # Resolves conflicts between local and cloud
└── CloudSaveUI.cs                # Optional UI for manual backup/restore

Assets/Plugins/iOS/
└── CloudSaveNative.mm            # Objective-C bridge to NSUbiquitousKeyValueStore

Assets/Scripts/Editor/
└── CloudSaveDebugWindow.cs       # Editor window for cloud save testing
```

**DO NOT modify** any existing files. CloudSaveManager works alongside SaveSystem — it reads SaveSystem's JSON and mirrors it to iCloud.

---

## Architecture

```
CloudSaveManager (Singleton, DontDestroyOnLoad)
  ├── On app start: check iCloud for newer save
  │   ├── If cloud is newer → prompt user to restore
  │   ├── If local is newer → auto-upload to cloud
  │   └── If same → do nothing
  ├── After every local save (SaveSystem.Save()) → upload to iCloud
  ├── On iCloud external change notification → check for newer data
  ├── Conflict resolution via CloudSaveConflictResolver
  └── Public API:
      ├── BackupToCloud()
      ├── RestoreFromCloud()
      ├── GetCloudSaveInfo() → (exists, timestamp, size)
      ├── DeleteCloudSave()
      └── IsCloudAvailable

CloudSaveConflictResolver
  ├── Strategy: Most Recent Wins (compare timestamps)
  ├── Fallback: Most Progress Wins (compare total medals + currency)
  └── User can force local or cloud via UI

CloudSaveNative.mm (Objective-C)
  ├── _CloudSave_IsAvailable() → bool
  ├── _CloudSave_SetString(key, value)
  ├── _CloudSave_GetString(key) → string
  ├── _CloudSave_Synchronize()
  ├── _CloudSave_Remove(key)
  ├── _CloudSave_RegisterForNotifications()
  └── Sends UnitySendMessage on external change
```

---

## Detailed Specifications

### CloudSaveNative.mm

```objectivec
// CloudSaveNative.mm
// Native iOS plugin for iCloud NSUbiquitousKeyValueStore access.
// Separate from MetalPodNative.mm to keep concerns isolated.

#import <Foundation/Foundation.h>

// Unity callback bridge
extern void UnitySendMessage(const char *obj, const char *method, const char *msg);

static NSString *const kCloudSaveKey = @"MetalPodSaveData";
static NSString *const kCloudTimestampKey = @"MetalPodSaveTimestamp";
static const char *kCallbackObject = "CloudSaveManager"; // GameObject name

extern "C" {

// ── Availability ─────────────────────────────────────

bool _CloudSave_IsAvailable() {
    // Check if iCloud is signed in
    NSFileManager *fm = [NSFileManager defaultManager];
    NSURL *ubiquityURL = [fm URLForUbiquityContainerIdentifier:nil];
    return ubiquityURL != nil;
}

// ── Write ────────────────────────────────────────────

void _CloudSave_SetString(const char *key, const char *value) {
    NSString *nsKey = [NSString stringWithUTF8String:key];
    NSString *nsValue = [NSString stringWithUTF8String:value];

    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    [store setString:nsValue forKey:nsKey];
}

void _CloudSave_SetTimestamp(const char *key, long long timestamp) {
    NSString *nsKey = [NSString stringWithUTF8String:key];
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    [store setLongLong:timestamp forKey:nsKey];
}

// ── Read ─────────────────────────────────────────────

const char* _CloudSave_GetString(const char *key) {
    NSString *nsKey = [NSString stringWithUTF8String:key];
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    NSString *value = [store stringForKey:nsKey];

    if (value == nil) return "";

    // Return a C string that Unity can marshal
    const char *utf8 = [value UTF8String];
    char *result = (char *)malloc(strlen(utf8) + 1);
    strcpy(result, utf8);
    return result;
}

long long _CloudSave_GetTimestamp(const char *key) {
    NSString *nsKey = [NSString stringWithUTF8String:key];
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    return [store longLongForKey:nsKey];
}

// ── Sync ─────────────────────────────────────────────

bool _CloudSave_Synchronize() {
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    return [store synchronize];
}

// ── Delete ───────────────────────────────────────────

void _CloudSave_Remove(const char *key) {
    NSString *nsKey = [NSString stringWithUTF8String:key];
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    [store removeObjectForKey:nsKey];
}

// ── Notifications ────────────────────────────────────

void _CloudSave_RegisterForNotifications() {
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];

    [[NSNotificationCenter defaultCenter] addObserverForName:NSUbiquitousKeyValueStoreDidChangeExternallyNotification
                                                      object:store
                                                       queue:[NSOperationQueue mainQueue]
                                                  usingBlock:^(NSNotification *notification) {
        NSDictionary *userInfo = notification.userInfo;
        NSNumber *reason = userInfo[NSUbiquitousKeyValueStoreChangeReasonKey];

        // reason: 0 = ServerChange, 1 = InitialSyncChange, 2 = QuotaViolation, 3 = AccountChange
        int reasonInt = reason ? [reason intValue] : 0;

        NSString *msg = [NSString stringWithFormat:@"%d", reasonInt];
        UnitySendMessage(kCallbackObject, "OnCloudDataChanged", [msg UTF8String]);
    }];

    // Trigger initial sync
    [store synchronize];
}

} // extern "C"
```

### CloudSaveManager.cs

```csharp
// Manages iCloud save backup. Mirrors local SaveSystem data to NSUbiquitousKeyValueStore.
// Singleton — attach to a GameObject named "CloudSaveManager" in _Persistent scene.

using System;
using UnityEngine;
using MetalPod.Progression;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace MetalPod.CloudSave
{
    public class CloudSaveManager : MonoBehaviour
    {
        public static CloudSaveManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool autoBackupOnSave = true;
        [SerializeField] private bool autoRestoreOnStart = true;
        [SerializeField] private bool showConflictPrompt = true;

        public event Action OnBackupComplete;
        public event Action OnRestoreComplete;
        public event Action<CloudSaveConflict> OnConflictDetected;

        public bool IsCloudAvailable { get; private set; }
        public bool HasCloudSave { get; private set; }
        public long CloudTimestamp { get; private set; }
        public long LocalTimestamp { get; private set; }

        private SaveSystem _saveSystem;
        private bool _initialized;

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

        private const string CLOUD_SAVE_KEY = "MetalPodSaveData";
        private const string CLOUD_TIMESTAMP_KEY = "MetalPodSaveTimestamp";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Ensure the GameObject name matches the native callback target
            gameObject.name = "CloudSaveManager";
        }

        private void Start()
        {
            _saveSystem = FindObjectOfType<SaveSystem>();
            Initialize();
        }

        private void Initialize()
        {
#if UNITY_IOS && !UNITY_EDITOR
            IsCloudAvailable = _CloudSave_IsAvailable();
            if (IsCloudAvailable)
            {
                _CloudSave_RegisterForNotifications();
                CheckCloudState();

                if (autoRestoreOnStart && HasCloudSave)
                {
                    CompareAndResolve();
                }
            }
#else
            IsCloudAvailable = false;
#endif
            _initialized = true;

            Debug.Log($"[CloudSave] Initialized. Available: {IsCloudAvailable}, HasCloud: {HasCloudSave}");
        }

        // ── Cloud State ─────────────────────────────────────

        private void CheckCloudState()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_Synchronize();
            string cloudJson = _CloudSave_GetString(CLOUD_SAVE_KEY);
            CloudTimestamp = _CloudSave_GetTimestamp(CLOUD_TIMESTAMP_KEY);
            HasCloudSave = !string.IsNullOrEmpty(cloudJson) && cloudJson.Length > 2;
#endif
            LocalTimestamp = _saveSystem?.CurrentData?.lastSaveTimestamp ?? 0;
        }

        // ── Backup ──────────────────────────────────────────

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

            if (_saveSystem?.CurrentData == null)
            {
                Debug.LogWarning("[CloudSave] No local save data to backup.");
                return;
            }

            string json = JsonUtility.ToJson(_saveSystem.CurrentData);
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_SetString(CLOUD_SAVE_KEY, json);
            _CloudSave_SetTimestamp(CLOUD_TIMESTAMP_KEY, timestamp);
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

#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_Synchronize();
            string cloudJson = _CloudSave_GetString(CLOUD_SAVE_KEY);
#else
            string cloudJson = "";
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

                // Write cloud data to local file
                string localPath = _saveSystem.SavePath;
                System.IO.File.WriteAllText(localPath, cloudJson);

                // Reinitialize save system to pick up new data
                _saveSystem.Initialize();

                Debug.Log("[CloudSave] Restored from iCloud successfully.");
                OnRestoreComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CloudSave] Restore failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete cloud save data.
        /// </summary>
        public void DeleteCloudSave()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CloudSave_Remove(CLOUD_SAVE_KEY);
            _CloudSave_Remove(CLOUD_TIMESTAMP_KEY);
            _CloudSave_Synchronize();
#endif
            HasCloudSave = false;
            CloudTimestamp = 0;
            Debug.Log("[CloudSave] Cloud save deleted.");
        }

        /// <summary>
        /// Get info about the cloud save without restoring it.
        /// </summary>
        public CloudSaveInfo GetCloudSaveInfo()
        {
            CheckCloudState();

            string cloudJson = "";
#if UNITY_IOS && !UNITY_EDITOR
            cloudJson = _CloudSave_GetString(CLOUD_SAVE_KEY);
#endif

            SaveData cloudData = null;
            if (!string.IsNullOrEmpty(cloudJson) && cloudJson.Length > 2)
            {
                try { cloudData = JsonUtility.FromJson<SaveData>(cloudJson); }
                catch { /* ignore parse errors */ }
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

        // ── Conflict Resolution ─────────────────────────────

        private void CompareAndResolve()
        {
            CheckCloudState();

            if (!HasCloudSave)
            {
                // No cloud save — just backup local
                if (_saveSystem?.CurrentData != null)
                    BackupToCloud();
                return;
            }

            if (LocalTimestamp == 0)
            {
                // No local save — restore from cloud
                RestoreFromCloud();
                return;
            }

            // Both exist — compare
            if (CloudTimestamp > LocalTimestamp)
            {
                // Cloud is newer
                if (showConflictPrompt)
                {
                    var conflict = new CloudSaveConflict
                    {
                        localTimestamp = LocalTimestamp,
                        cloudTimestamp = CloudTimestamp,
                        localInfo = GetLocalSaveInfo(),
                        cloudInfo = GetCloudSaveInfo()
                    };
                    OnConflictDetected?.Invoke(conflict);
                }
                else
                {
                    // Auto-resolve: cloud wins
                    RestoreFromCloud();
                }
            }
            else
            {
                // Local is newer or same — backup
                BackupToCloud();
            }
        }

        private CloudSaveInfo GetLocalSaveInfo()
        {
            var data = _saveSystem?.CurrentData;
            return new CloudSaveInfo
            {
                exists = data != null,
                timestamp = data?.lastSaveTimestamp ?? 0,
                currency = data?.currency ?? 0,
                totalMedals = data?.GetTotalMedals() ?? 0,
                totalCoursesCompleted = data?.totalCoursesCompleted ?? 0
            };
        }

        // ── Native Callback ─────────────────────────────────

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
                CheckCloudState();
                CompareAndResolve();
            }
            // 2 = QuotaViolation — log warning
            else if (reason == 2)
            {
                Debug.LogWarning("[CloudSave] iCloud quota violation! Save data may be too large.");
            }
            // 3 = AccountChange — re-check availability
            else if (reason == 3)
            {
#if UNITY_IOS && !UNITY_EDITOR
                IsCloudAvailable = _CloudSave_IsAvailable();
#endif
            }
        }

        // ── Auto-backup Hook ────────────────────────────────

        /// <summary>
        /// Call this after every local save to auto-backup to cloud.
        /// Integration: SaveSystem should call CloudSaveManager.Instance?.OnLocalSaveCompleted()
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
            if (Instance == this) Instance = null;
        }
    }

    // ── Data Structures ─────────────────────────────────

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
                if (timestamp == 0) return "Never";
                var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
                return dt.ToString("MMM d, yyyy h:mm tt");
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
```

### CloudSaveConflictResolver.cs

```csharp
// Resolves conflicts between local and cloud saves.
// Default strategy: most recent wins. Fallback: most progress wins.

using MetalPod.Progression;
using UnityEngine;

namespace MetalPod.CloudSave
{
    public enum ConflictResolution
    {
        UseLocal,
        UseCloud,
        UseNewest,
        UseMostProgress
    }

    public static class CloudSaveConflictResolver
    {
        /// <summary>
        /// Determine which save to use based on the conflict data.
        /// </summary>
        public static ConflictResolution Resolve(CloudSaveConflict conflict,
            ConflictResolution strategy = ConflictResolution.UseNewest)
        {
            switch (strategy)
            {
                case ConflictResolution.UseNewest:
                    return conflict.cloudTimestamp > conflict.localTimestamp
                        ? ConflictResolution.UseCloud
                        : ConflictResolution.UseLocal;

                case ConflictResolution.UseMostProgress:
                    int localScore = CalculateProgressScore(conflict.localInfo);
                    int cloudScore = CalculateProgressScore(conflict.cloudInfo);
                    return cloudScore > localScore
                        ? ConflictResolution.UseCloud
                        : ConflictResolution.UseLocal;

                default:
                    return strategy;
            }
        }

        /// <summary>
        /// Calculate a "progress score" to compare saves.
        /// Higher = more progress.
        /// </summary>
        private static int CalculateProgressScore(CloudSaveInfo info)
        {
            // Weight: medals (x100) + courses (x50) + currency (x1)
            return (info.totalMedals * 100) +
                   (info.totalCoursesCompleted * 50) +
                   info.currency;
        }
    }
}
```

### CloudSaveUI.cs

```csharp
// Optional UI component for manual backup/restore.
// Attach to Settings screen. Shows cloud save status and action buttons.

using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.CloudSave
{
    public class CloudSaveUI : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private TMPro.TextMeshProUGUI statusText;
        [SerializeField] private TMPro.TextMeshProUGUI cloudInfoText;

        [Header("Buttons")]
        [SerializeField] private Button backupButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Button deleteButton;

        [Header("Conflict Panel")]
        [SerializeField] private GameObject conflictPanel;
        [SerializeField] private TMPro.TextMeshProUGUI conflictLocalText;
        [SerializeField] private TMPro.TextMeshProUGUI conflictCloudText;
        [SerializeField] private Button useLocalButton;
        [SerializeField] private Button useCloudButton;

        private void OnEnable()
        {
            UpdateDisplay();

            if (backupButton != null) backupButton.onClick.AddListener(OnBackupPressed);
            if (restoreButton != null) restoreButton.onClick.AddListener(OnRestorePressed);
            if (deleteButton != null) deleteButton.onClick.AddListener(OnDeletePressed);
            if (useLocalButton != null) useLocalButton.onClick.AddListener(OnUseLocalPressed);
            if (useCloudButton != null) useCloudButton.onClick.AddListener(OnUseCloudPressed);

            var mgr = CloudSaveManager.Instance;
            if (mgr != null)
            {
                mgr.OnBackupComplete += UpdateDisplay;
                mgr.OnRestoreComplete += UpdateDisplay;
                mgr.OnConflictDetected += ShowConflictPanel;
            }
        }

        private void OnDisable()
        {
            if (backupButton != null) backupButton.onClick.RemoveListener(OnBackupPressed);
            if (restoreButton != null) restoreButton.onClick.RemoveListener(OnRestorePressed);
            if (deleteButton != null) deleteButton.onClick.RemoveListener(OnDeletePressed);
            if (useLocalButton != null) useLocalButton.onClick.RemoveListener(OnUseLocalPressed);
            if (useCloudButton != null) useCloudButton.onClick.RemoveListener(OnUseCloudPressed);

            var mgr = CloudSaveManager.Instance;
            if (mgr != null)
            {
                mgr.OnBackupComplete -= UpdateDisplay;
                mgr.OnRestoreComplete -= UpdateDisplay;
                mgr.OnConflictDetected -= ShowConflictPanel;
            }
        }

        private void UpdateDisplay()
        {
            var mgr = CloudSaveManager.Instance;
            if (mgr == null)
            {
                if (statusText != null) statusText.text = "Cloud Save: Unavailable";
                return;
            }

            if (statusText != null)
            {
                statusText.text = mgr.IsCloudAvailable
                    ? "iCloud: Connected"
                    : "iCloud: Not Available";
            }

            if (cloudInfoText != null)
            {
                if (mgr.HasCloudSave)
                {
                    var info = mgr.GetCloudSaveInfo();
                    cloudInfoText.text = $"Last backup: {info.FormattedTimestamp}\n" +
                                        $"Medals: {info.totalMedals} | Bolts: {info.currency}";
                }
                else
                {
                    cloudInfoText.text = "No cloud backup found.";
                }
            }

            if (restoreButton != null) restoreButton.interactable = mgr.HasCloudSave;
            if (deleteButton != null) deleteButton.interactable = mgr.HasCloudSave;
            if (backupButton != null) backupButton.interactable = mgr.IsCloudAvailable;

            if (conflictPanel != null) conflictPanel.SetActive(false);
        }

        private void ShowConflictPanel(CloudSaveConflict conflict)
        {
            if (conflictPanel == null) return;
            conflictPanel.SetActive(true);

            if (conflictLocalText != null)
            {
                conflictLocalText.text =
                    $"LOCAL SAVE\n" +
                    $"Medals: {conflict.localInfo.totalMedals}\n" +
                    $"Courses: {conflict.localInfo.totalCoursesCompleted}\n" +
                    $"Bolts: {conflict.localInfo.currency}\n" +
                    $"Saved: {conflict.localInfo.FormattedTimestamp}";
            }

            if (conflictCloudText != null)
            {
                conflictCloudText.text =
                    $"CLOUD SAVE\n" +
                    $"Medals: {conflict.cloudInfo.totalMedals}\n" +
                    $"Courses: {conflict.cloudInfo.totalCoursesCompleted}\n" +
                    $"Bolts: {conflict.cloudInfo.currency}\n" +
                    $"Saved: {conflict.cloudInfo.FormattedTimestamp}";
            }
        }

        private void OnBackupPressed() => CloudSaveManager.Instance?.BackupToCloud();
        private void OnRestorePressed() => CloudSaveManager.Instance?.RestoreFromCloud();
        private void OnDeletePressed() => CloudSaveManager.Instance?.DeleteCloudSave();

        private void OnUseLocalPressed()
        {
            CloudSaveManager.Instance?.BackupToCloud(); // Override cloud with local
            if (conflictPanel != null) conflictPanel.SetActive(false);
            UpdateDisplay();
        }

        private void OnUseCloudPressed()
        {
            CloudSaveManager.Instance?.RestoreFromCloud();
            if (conflictPanel != null) conflictPanel.SetActive(false);
            UpdateDisplay();
        }
    }
}
```

### CloudSaveDebugWindow.cs

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MetalPod.Editor
{
    public class CloudSaveDebugWindow : EditorWindow
    {
        [MenuItem("Metal Pod/Cloud Save Debug")]
        public static void ShowWindow()
        {
            GetWindow<CloudSaveDebugWindow>("Cloud Save");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Cloud Save Debug", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to interact with CloudSaveManager.\n\n" +
                    "Note: iCloud is only available on physical iOS devices. " +
                    "In the Editor, CloudSaveManager reports IsCloudAvailable = false.",
                    MessageType.Info);
                return;
            }

            var mgr = MetalPod.CloudSave.CloudSaveManager.Instance;
            if (mgr == null)
            {
                EditorGUILayout.HelpBox("CloudSaveManager not found in scene.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Status");
            EditorGUILayout.LabelField($"  Available: {mgr.IsCloudAvailable}");
            EditorGUILayout.LabelField($"  Has Cloud Save: {mgr.HasCloudSave}");
            EditorGUILayout.LabelField($"  Cloud Timestamp: {mgr.CloudTimestamp}");
            EditorGUILayout.LabelField($"  Local Timestamp: {mgr.LocalTimestamp}");

            EditorGUILayout.Space();

            if (GUILayout.Button("Backup to Cloud"))
                mgr.BackupToCloud();

            if (GUILayout.Button("Restore from Cloud"))
                mgr.RestoreFromCloud();

            if (GUILayout.Button("Delete Cloud Save"))
                mgr.DeleteCloudSave();

            if (GUILayout.Button("Get Cloud Info"))
            {
                var info = mgr.GetCloudSaveInfo();
                Debug.Log($"[CloudSave Debug] Exists: {info.exists}, " +
                         $"Timestamp: {info.FormattedTimestamp}, " +
                         $"Size: {info.sizeBytes}B, " +
                         $"Medals: {info.totalMedals}, " +
                         $"Currency: {info.currency}");
            }
        }
    }
}
#endif
```

---

## Xcode Configuration Required

For iCloud to work, the Xcode project needs:
1. **Capabilities > iCloud** enabled
2. **Key-value storage** checkbox checked
3. No additional container setup needed (NSUbiquitousKeyValueStore uses the default container)

The existing `BuildPreprocessor.cs` (from Task 10) could be extended to enable this capability, but that's a future integration task.

---

## Acceptance Criteria

- [ ] `CloudSaveManager.cs` — Singleton with backup/restore/delete/info API, auto-backup on save
- [ ] `CloudSaveConflictResolver.cs` — Newest-wins + most-progress-wins strategies
- [ ] `CloudSaveUI.cs` — UI component with status display, buttons, conflict resolution panel
- [ ] `CloudSaveNative.mm` — Objective-C bridge to NSUbiquitousKeyValueStore
- [ ] `CloudSaveDebugWindow.cs` — Editor debug window
- [ ] All native calls wrapped in `#if UNITY_IOS && !UNITY_EDITOR`
- [ ] CloudSaveManager receives native callback via `UnitySendMessage`
- [ ] Conflict detection compares timestamps and progress scores
- [ ] Auto-backup after each local save (configurable)
- [ ] Auto-restore prompt on first launch if cloud save exists
- [ ] All C# in `MetalPod.CloudSave` or `MetalPod.Editor` namespaces
- [ ] No modifications to existing files
- [ ] Compiles without errors on all platforms (native calls no-op on non-iOS)
