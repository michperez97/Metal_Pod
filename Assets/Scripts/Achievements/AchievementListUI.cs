using System;
using System.Collections.Generic;
using MetalPod.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Achievements
{
    /// <summary>
    /// UI-ready scroll list showing categorized achievements and progress.
    /// </summary>
    public class AchievementListUI : MonoBehaviour
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private bool hideHiddenUntilUnlocked = true;
        [SerializeField] private float refreshIntervalSeconds = 0.5f;

        private class RowRefs
        {
            public Achievement Achievement;
            public GameObject Root;
            public CanvasGroup CanvasGroup;
            public Image Icon;
            public TMP_Text Title;
            public TMP_Text Description;
            public GameObject ProgressContainer;
            public RectTransform ProgressFill;
            public TMP_Text ProgressText;
        }

        private readonly Dictionary<string, RowRefs> _rowsById = new Dictionary<string, RowRefs>(StringComparer.Ordinal);
        private readonly List<GameObject> _generatedObjects = new List<GameObject>();

        private AchievementManager _manager;
        private bool _subscribed;
        private float _refreshTimer;

        private void Awake()
        {
            if (contentRoot == null)
            {
                contentRoot = transform as RectTransform;
            }
        }

        private void OnEnable()
        {
            TrySubscribe();
            Rebuild();
        }

        private void Update()
        {
            if (!_subscribed)
            {
                TrySubscribe();
                if (_subscribed)
                {
                    Rebuild();
                }
            }

            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer >= refreshIntervalSeconds)
            {
                _refreshTimer = 0f;
                RefreshRows();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
            ClearGeneratedObjects();
            _rowsById.Clear();
        }

        private void TrySubscribe()
        {
            if (_subscribed)
            {
                return;
            }

            _manager = AchievementManager.Instance ?? FindFirstObjectByType<AchievementManager>(FindObjectsInactive.Include);
            if (_manager == null)
            {
                return;
            }

            _manager.OnAchievementUnlocked += HandleAchievementUnlocked;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (_subscribed && _manager != null)
            {
                _manager.OnAchievementUnlocked -= HandleAchievementUnlocked;
            }

            _subscribed = false;
            _manager = null;
        }

        private void HandleAchievementUnlocked(Achievement _)
        {
            RefreshRows();
        }

        private void Rebuild()
        {
            _rowsById.Clear();
            ClearGeneratedObjects();

            if (contentRoot == null || _manager == null)
            {
                return;
            }

            Array categories = Enum.GetValues(typeof(AchievementCategory));
            for (int i = 0; i < categories.Length; i++)
            {
                AchievementCategory category = (AchievementCategory)categories.GetValue(i);
                List<Achievement> items = _manager.GetByCategory(category);
                if (items.Count == 0)
                {
                    continue;
                }

                CreateCategoryHeader(category);
                for (int j = 0; j < items.Count; j++)
                {
                    Achievement achievement = items[j];
                    if (achievement == null || achievement.Definition == null)
                    {
                        continue;
                    }

                    RowRefs row = CreateRow(achievement);
                    if (row != null)
                    {
                        _rowsById[achievement.Definition.achievementId] = row;
                    }
                }
            }

            RefreshRows();
        }

        private void RefreshRows()
        {
            if (_manager == null)
            {
                return;
            }

            foreach (KeyValuePair<string, RowRefs> entry in _rowsById)
            {
                RowRefs row = entry.Value;
                if (row == null || row.Achievement == null || row.Achievement.Definition == null)
                {
                    continue;
                }

                AchievementDataSO definition = row.Achievement.Definition;
                bool hiddenLocked = definition.isHidden && !row.Achievement.IsUnlocked && hideHiddenUntilUnlocked;

                if (row.Title != null)
                {
                    row.Title.text = hiddenLocked ? "Hidden Achievement" : definition.title;
                }

                if (row.Description != null)
                {
                    row.Description.text = hiddenLocked ? "Unlock to reveal details." : definition.description;
                }

                if (row.Icon != null)
                {
                    row.Icon.sprite = hiddenLocked ? null : definition.icon;
                    row.Icon.enabled = row.Icon.sprite != null;
                }

                if (row.CanvasGroup != null)
                {
                    row.CanvasGroup.alpha = row.Achievement.IsUnlocked ? 1f : 0.92f;
                }

                int target = Mathf.Max(1, definition.targetValue);
                int clampedProgress = Mathf.Clamp(row.Achievement.CurrentProgress, 0, target);
                float normalized = target > 0 ? (float)clampedProgress / target : (row.Achievement.IsUnlocked ? 1f : 0f);

                if (row.ProgressFill != null)
                {
                    row.ProgressFill.anchorMax = new Vector2(normalized, 1f);
                }

                if (row.ProgressText != null)
                {
                    row.ProgressText.text = row.Achievement.IsUnlocked
                        ? "Unlocked"
                        : $"{clampedProgress} / {target}";
                }

                if (row.ProgressContainer != null)
                {
                    row.ProgressContainer.SetActive(!row.Achievement.IsUnlocked);
                }
            }
        }

        private void CreateCategoryHeader(AchievementCategory category)
        {
            GameObject headerObject = new GameObject($"Header_{category}", typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            RegisterGeneratedObject(headerObject);
            headerObject.transform.SetParent(contentRoot, false);

            LayoutElement layoutElement = headerObject.GetComponent<LayoutElement>();
            layoutElement.minHeight = 28f;

            TextMeshProUGUI text = headerObject.GetComponent<TextMeshProUGUI>();
            text.text = category.ToString().ToUpperInvariant();
            text.fontSize = 24f;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 0.78f, 0.2f, 1f);
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = false;
        }

        private RowRefs CreateRow(Achievement achievement)
        {
            if (contentRoot == null || achievement == null || achievement.Definition == null)
            {
                return null;
            }

            GameObject root = new GameObject(
                $"Achievement_{achievement.Definition.achievementId}",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(LayoutElement));
            RegisterGeneratedObject(root);
            root.transform.SetParent(contentRoot, false);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.65f);

            LayoutElement rowLayout = root.GetComponent<LayoutElement>();
            rowLayout.minHeight = 104f;

            VerticalLayoutGroup verticalLayout = root.GetComponent<VerticalLayoutGroup>();
            verticalLayout.padding = new RectOffset(10, 10, 8, 8);
            verticalLayout.spacing = 6f;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.childControlHeight = false;
            verticalLayout.childControlWidth = true;
            verticalLayout.childForceExpandHeight = false;

            GameObject topRow = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            RegisterGeneratedObject(topRow);
            topRow.transform.SetParent(root.transform, false);

            LayoutElement topLayoutElement = topRow.GetComponent<LayoutElement>();
            topLayoutElement.minHeight = 42f;

            HorizontalLayoutGroup topLayout = topRow.GetComponent<HorizontalLayoutGroup>();
            topLayout.spacing = 8f;
            topLayout.childAlignment = TextAnchor.MiddleLeft;
            topLayout.childControlHeight = false;
            topLayout.childControlWidth = false;
            topLayout.childForceExpandWidth = false;

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            RegisterGeneratedObject(iconObject);
            iconObject.transform.SetParent(topRow.transform, false);
            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.minWidth = 36f;
            iconLayout.preferredWidth = 36f;
            iconLayout.minHeight = 36f;
            iconLayout.preferredHeight = 36f;

            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.preserveAspect = true;

            TMP_Text titleText = CreateText(topRow.transform, "Title", 22f, FontStyles.Bold, Color.white);
            if (titleText != null)
            {
                titleText.enableWordWrapping = true;
            }

            TMP_Text descriptionText = CreateText(root.transform, "Description", 18f, FontStyles.Normal, new Color(0.86f, 0.86f, 0.86f, 1f));

            GameObject progressContainer = new GameObject("Progress", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            RegisterGeneratedObject(progressContainer);
            progressContainer.transform.SetParent(root.transform, false);

            LayoutElement progressLayout = progressContainer.GetComponent<LayoutElement>();
            progressLayout.minHeight = 28f;

            VerticalLayoutGroup progressVertical = progressContainer.GetComponent<VerticalLayoutGroup>();
            progressVertical.spacing = 4f;
            progressVertical.childControlWidth = true;
            progressVertical.childControlHeight = false;
            progressVertical.childForceExpandHeight = false;

            TMP_Text progressText = CreateText(progressContainer.transform, "ProgressText", 16f, FontStyles.Normal, new Color(1f, 0.9f, 0.45f, 1f));

            GameObject trackObject = new GameObject("Track", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            RegisterGeneratedObject(trackObject);
            trackObject.transform.SetParent(progressContainer.transform, false);

            LayoutElement trackLayout = trackObject.GetComponent<LayoutElement>();
            trackLayout.minHeight = 10f;
            trackLayout.preferredHeight = 10f;

            Image trackImage = trackObject.GetComponent<Image>();
            trackImage.color = new Color(0.22f, 0.22f, 0.22f, 1f);

            RectTransform trackRect = trackObject.GetComponent<RectTransform>();

            GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            RegisterGeneratedObject(fillObject);
            fillObject.transform.SetParent(trackObject.transform, false);

            Image fillImage = fillObject.GetComponent<Image>();
            fillImage.color = new Color(1f, 0.72f, 0.15f, 1f);

            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            trackRect.anchorMin = new Vector2(0f, 0.5f);
            trackRect.anchorMax = new Vector2(1f, 0.5f);
            trackRect.offsetMin = new Vector2(0f, 0f);
            trackRect.offsetMax = new Vector2(0f, 0f);

            return new RowRefs
            {
                Achievement = achievement,
                Root = root,
                CanvasGroup = root.GetComponent<CanvasGroup>(),
                Icon = iconImage,
                Title = titleText,
                Description = descriptionText,
                ProgressContainer = progressContainer,
                ProgressFill = fillRect,
                ProgressText = progressText
            };
        }

        private static TMP_Text CreateText(Transform parent, string name, float size, FontStyles style, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            textObject.transform.SetParent(parent, false);

            LayoutElement layoutElement = textObject.GetComponent<LayoutElement>();
            layoutElement.minHeight = size + 4f;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = true;
            return text;
        }

        private void RegisterGeneratedObject(GameObject gameObject)
        {
            if (gameObject != null)
            {
                _generatedObjects.Add(gameObject);
            }
        }

        private void ClearGeneratedObjects()
        {
            for (int i = 0; i < _generatedObjects.Count; i++)
            {
                if (_generatedObjects[i] != null)
                {
                    Destroy(_generatedObjects[i]);
                }
            }

            _generatedObjects.Clear();
        }
    }
}
