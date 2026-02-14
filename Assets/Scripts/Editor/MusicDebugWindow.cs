#if UNITY_EDITOR
using MetalPod.Audio;
using MetalPod.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Editor
{
    public class MusicDebugWindow : EditorWindow
    {
        [MenuItem("Metal Pod/Music Debug")]
        public static void ShowWindow()
        {
            GetWindow<MusicDebugWindow>("Music Debug");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dynamic Music Debug", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect dynamic music state.", MessageType.Info);
                return;
            }

            DynamicMusicManager manager = DynamicMusicManager.Instance;
            if (manager == null)
            {
                EditorGUILayout.HelpBox("DynamicMusicManager is not active.", MessageType.Warning);
                return;
            }

            string profileName = manager.CurrentProfile != null ? manager.CurrentProfile.displayName : "None";
            EditorGUILayout.LabelField("Profile", profileName);
            EditorGUILayout.LabelField("Racing", manager.IsRacing ? "Yes" : "No");
            EditorGUILayout.LabelField("Crossfading", manager.IsCrossfading ? "Yes" : "No");
            EditorGUILayout.LabelField("Speed (Normalized)", manager.CurrentNormalizedSpeed.ToString("0.00"));
            EditorGUILayout.LabelField("Health (Normalized)", manager.CurrentHealthNormalized.ToString("0.00"));
            EditorGUILayout.LabelField("Boosting", manager.IsBoosting ? "Yes" : "No");
            EditorGUILayout.LabelField("Max Speed Ref", manager.MaxSpeedReference.ToString("0.0"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
            DrawLayerRow("Base", manager.BaseLayerVolume, manager.BaseLayerTargetVolume, manager.BaseLayerActive);
            DrawLayerRow("Drums", manager.DrumsLayerVolume, manager.DrumsLayerTargetVolume, manager.DrumsLayerActive);
            DrawLayerRow("Lead", manager.LeadLayerVolume, manager.LeadLayerTargetVolume, manager.LeadLayerActive);
            DrawLayerRow("Boost", manager.BoostLayerVolume, manager.BoostLayerTargetVolume, manager.BoostLayerActive);

            if (manager.CurrentProfile != null)
            {
                MusicProfileSO profile = manager.CurrentProfile;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Profile Settings", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("BPM", profile.bpm.ToString("0.0"));
                EditorGUILayout.LabelField("Drum Threshold", profile.drumSpeedThreshold.ToString("0.00"));
                EditorGUILayout.LabelField("Lead Threshold", profile.leadSpeedThreshold.ToString("0.00"));
                EditorGUILayout.LabelField("Fade Time", profile.layerFadeTime.ToString("0.00") + "s");
                EditorGUILayout.LabelField("Danger Threshold", profile.dangerHealthThreshold.ToString("0.00"));
            }

            Repaint();
        }

        private static void DrawLayerRow(string name, float currentVolume, float targetVolume, bool isActive)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(60f));

            Rect progressRect = GUILayoutUtility.GetRect(140f, 18f);
            EditorGUI.ProgressBar(
                progressRect,
                Mathf.Clamp01(currentVolume),
                string.Format("{0:0.00} / {1:0.00}", currentVolume, targetVolume));

            GUILayout.Label(isActive ? "On" : "Off", GUILayout.Width(24f));
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
