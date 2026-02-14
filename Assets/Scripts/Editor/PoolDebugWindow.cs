#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MetalPod.Pooling;

namespace MetalPod.Editor
{
    public class PoolDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        [MenuItem("Metal Pod/Pool Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<PoolDebugWindow>("Pool Debug");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Object Pool Statistics", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect pool stats.", MessageType.Info);
                return;
            }

            if (PoolManager.Instance == null)
            {
                EditorGUILayout.HelpBox("No PoolManager instance found.", MessageType.Warning);
                return;
            }

            var stats = PoolManager.Instance.GetAllStats();
            if (stats.Count == 0)
            {
                EditorGUILayout.HelpBox("No pools registered.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pool Name", EditorStyles.boldLabel, GUILayout.Width(220f));
            EditorGUILayout.LabelField("Active", EditorStyles.boldLabel, GUILayout.Width(60f));
            EditorGUILayout.LabelField("Available", EditorStyles.boldLabel, GUILayout.Width(70f));
            EditorGUILayout.LabelField("Total", EditorStyles.boldLabel, GUILayout.Width(60f));
            EditorGUILayout.LabelField("Usage", EditorStyles.boldLabel, GUILayout.Width(60f));
            EditorGUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var entry in stats)
            {
                string poolName = entry.Key;
                int active = entry.Value.active;
                int available = entry.Value.available;
                int total = entry.Value.total;
                float usage = total > 0 ? (float)active / total : 0f;

                Color usageColor = usage < 0.5f ? Color.green : usage < 0.8f ? Color.yellow : Color.red;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(poolName, GUILayout.Width(220f));
                EditorGUILayout.LabelField(active.ToString(), GUILayout.Width(60f));
                EditorGUILayout.LabelField(available.ToString(), GUILayout.Width(70f));
                EditorGUILayout.LabelField(total.ToString(), GUILayout.Width(60f));

                Color previousColor = GUI.color;
                GUI.color = usageColor;
                EditorGUILayout.LabelField($"{usage:P0}", GUILayout.Width(60f));
                GUI.color = previousColor;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            if (GUILayout.Button("Return All To Pools"))
            {
                PoolManager.Instance.ReturnAllToPool();
            }

            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif
