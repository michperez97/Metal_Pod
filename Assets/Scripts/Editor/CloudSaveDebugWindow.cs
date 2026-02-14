#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

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

            MetalPod.CloudSave.CloudSaveManager manager = MetalPod.CloudSave.CloudSaveManager.Instance;
            if (manager == null)
            {
                EditorGUILayout.HelpBox("CloudSaveManager not found in scene.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Status");
            EditorGUILayout.LabelField($"  Available: {manager.IsCloudAvailable}");
            EditorGUILayout.LabelField($"  Has Cloud Save: {manager.HasCloudSave}");
            EditorGUILayout.LabelField($"  Cloud Timestamp: {manager.CloudTimestamp}");
            EditorGUILayout.LabelField($"  Local Timestamp: {manager.LocalTimestamp}");

            EditorGUILayout.Space();

            if (GUILayout.Button("Backup to Cloud"))
            {
                manager.BackupToCloud();
            }

            if (GUILayout.Button("Restore from Cloud"))
            {
                manager.RestoreFromCloud();
            }

            if (GUILayout.Button("Delete Cloud Save"))
            {
                manager.DeleteCloudSave();
            }

            if (GUILayout.Button("Get Cloud Info"))
            {
                MetalPod.CloudSave.CloudSaveInfo info = manager.GetCloudSaveInfo();
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

