#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using MetalPod.Achievements;
using MetalPod.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Editor
{
    /// <summary>
    /// Play-mode debug window for viewing and manipulating achievements.
    /// </summary>
    public class AchievementEditorWindow : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("Metal Pod/Achievements")]
        public static void ShowWindow()
        {
            AchievementEditorWindow window = GetWindow<AchievementEditorWindow>("Achievements");
            window.minSize = new Vector2(440f, 320f);
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Achievement Debugger", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            if (GUILayout.Button("Generate/Update Achievement Assets"))
            {
                AchievementDefinition.GenerateAll();
            }

            EditorGUILayout.Space(8f);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to inspect unlock state and force unlock/lock achievements.",
                    MessageType.Info);
                return;
            }

            AchievementManager manager = AchievementManager.Instance;
            if (manager == null)
            {
                EditorGUILayout.HelpBox("AchievementManager not found in the active scene.", MessageType.Warning);
                return;
            }

            DrawSummary(manager);

            EditorGUILayout.Space(8f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            Array categories = Enum.GetValues(typeof(AchievementCategory));
            for (int i = 0; i < categories.Length; i++)
            {
                AchievementCategory category = (AchievementCategory)categories.GetValue(i);
                List<Achievement> achievements = manager.GetByCategory(category);
                if (achievements.Count == 0)
                {
                    continue;
                }

                EditorGUILayout.LabelField(category.ToString(), EditorStyles.boldLabel);
                for (int j = 0; j < achievements.Count; j++)
                {
                    DrawAchievementRow(manager, achievements[j]);
                }

                EditorGUILayout.Space(6f);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawSummary(AchievementManager manager)
        {
            EditorGUILayout.HelpBox(
                $"Unlocked: {manager.UnlockedCount}/{manager.TotalCount}",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reevaluate"))
            {
                manager.ReevaluateNow();
            }

            if (GUILayout.Button("Reset All"))
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Reset Achievements",
                    "Reset all achievement progress and unlock flags?",
                    "Reset",
                    "Cancel");
                if (confirm)
                {
                    manager.ResetAll();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAchievementRow(AchievementManager manager, Achievement achievement)
        {
            if (achievement == null || achievement.Definition == null)
            {
                return;
            }

            AchievementDataSO definition = achievement.Definition;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            string title = achievement.IsUnlocked
                ? $"{definition.title}  (Unlocked)"
                : definition.title;
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (!achievement.IsUnlocked)
            {
                if (GUILayout.Button("Unlock", GUILayout.Width(64f)))
                {
                    manager.ForceUnlock(definition.achievementId);
                }
            }
            else
            {
                if (GUILayout.Button("Lock", GUILayout.Width(64f)))
                {
                    manager.ForceLock(definition.achievementId);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"ID: {definition.achievementId}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(definition.description, EditorStyles.wordWrappedMiniLabel);

            int target = Mathf.Max(1, definition.targetValue);
            int current = Mathf.Clamp(achievement.CurrentProgress, 0, target);
            float normalized = target > 0 ? (float)current / target : 0f;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField($"Progress: {current}/{target} ({normalized:P0})", EditorStyles.miniLabel);
            Rect barRect = GUILayoutUtility.GetRect(10f, 10f, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(barRect, normalized, string.Empty);

            EditorGUILayout.EndVertical();
        }
    }
}
#endif
