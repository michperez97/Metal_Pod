using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.CloudSave
{
    /// <summary>
    /// Optional UI component for manual backup/restore.
    /// Attach to a settings screen.
    /// </summary>
    public class CloudSaveUI : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI cloudInfoText;

        [Header("Buttons")]
        [SerializeField] private Button backupButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Button deleteButton;

        [Header("Conflict Panel")]
        [SerializeField] private GameObject conflictPanel;
        [SerializeField] private TextMeshProUGUI conflictLocalText;
        [SerializeField] private TextMeshProUGUI conflictCloudText;
        [SerializeField] private Button useLocalButton;
        [SerializeField] private Button useCloudButton;

        private void OnEnable()
        {
            if (backupButton != null) backupButton.onClick.AddListener(OnBackupPressed);
            if (restoreButton != null) restoreButton.onClick.AddListener(OnRestorePressed);
            if (deleteButton != null) deleteButton.onClick.AddListener(OnDeletePressed);
            if (useLocalButton != null) useLocalButton.onClick.AddListener(OnUseLocalPressed);
            if (useCloudButton != null) useCloudButton.onClick.AddListener(OnUseCloudPressed);

            CloudSaveManager manager = CloudSaveManager.Instance;
            if (manager != null)
            {
                manager.OnBackupComplete += UpdateDisplay;
                manager.OnRestoreComplete += UpdateDisplay;
                manager.OnConflictDetected += ShowConflictPanel;
            }

            UpdateDisplay();
        }

        private void OnDisable()
        {
            if (backupButton != null) backupButton.onClick.RemoveListener(OnBackupPressed);
            if (restoreButton != null) restoreButton.onClick.RemoveListener(OnRestorePressed);
            if (deleteButton != null) deleteButton.onClick.RemoveListener(OnDeletePressed);
            if (useLocalButton != null) useLocalButton.onClick.RemoveListener(OnUseLocalPressed);
            if (useCloudButton != null) useCloudButton.onClick.RemoveListener(OnUseCloudPressed);

            CloudSaveManager manager = CloudSaveManager.Instance;
            if (manager != null)
            {
                manager.OnBackupComplete -= UpdateDisplay;
                manager.OnRestoreComplete -= UpdateDisplay;
                manager.OnConflictDetected -= ShowConflictPanel;
            }
        }

        private void UpdateDisplay()
        {
            CloudSaveManager manager = CloudSaveManager.Instance;
            if (manager == null)
            {
                if (statusText != null)
                {
                    statusText.text = "Cloud Save: Unavailable";
                }
                return;
            }

            if (statusText != null)
            {
                statusText.text = manager.IsCloudAvailable
                    ? "iCloud: Connected"
                    : "iCloud: Not Available";
            }

            if (cloudInfoText != null)
            {
                if (manager.HasCloudSave)
                {
                    CloudSaveInfo info = manager.GetCloudSaveInfo();
                    cloudInfoText.text = $"Last backup: {info.FormattedTimestamp}\n" +
                                         $"Medals: {info.totalMedals} | Bolts: {info.currency}";
                }
                else
                {
                    cloudInfoText.text = "No cloud backup found.";
                }
            }

            if (restoreButton != null) restoreButton.interactable = manager.HasCloudSave;
            if (deleteButton != null) deleteButton.interactable = manager.HasCloudSave;
            if (backupButton != null) backupButton.interactable = manager.IsCloudAvailable;

            if (conflictPanel != null) conflictPanel.SetActive(false);
        }

        private void ShowConflictPanel(CloudSaveConflict conflict)
        {
            if (conflictPanel == null)
            {
                return;
            }

            conflictPanel.SetActive(true);

            if (conflictLocalText != null)
            {
                conflictLocalText.text =
                    "LOCAL SAVE\n" +
                    $"Medals: {conflict.localInfo.totalMedals}\n" +
                    $"Courses: {conflict.localInfo.totalCoursesCompleted}\n" +
                    $"Bolts: {conflict.localInfo.currency}\n" +
                    $"Saved: {conflict.localInfo.FormattedTimestamp}";
            }

            if (conflictCloudText != null)
            {
                conflictCloudText.text =
                    "CLOUD SAVE\n" +
                    $"Medals: {conflict.cloudInfo.totalMedals}\n" +
                    $"Courses: {conflict.cloudInfo.totalCoursesCompleted}\n" +
                    $"Bolts: {conflict.cloudInfo.currency}\n" +
                    $"Saved: {conflict.cloudInfo.FormattedTimestamp}";
            }
        }

        private void OnBackupPressed()
        {
            CloudSaveManager.Instance?.BackupToCloud();
        }

        private void OnRestorePressed()
        {
            CloudSaveManager.Instance?.RestoreFromCloud();
        }

        private void OnDeletePressed()
        {
            CloudSaveManager.Instance?.DeleteCloudSave();
            UpdateDisplay();
        }

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

