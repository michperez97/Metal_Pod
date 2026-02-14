using System.Collections.Generic;
using TMPro;
using MetalPod.ScriptableObjects;
using MetalPod.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Workshop
{
    public class CourseSelectionUI : MonoBehaviour
    {
        [Header("Environment Tabs")]
        [SerializeField] private Button lavaTab;
        [SerializeField] private Button iceTab;
        [SerializeField] private Button toxicTab;
        [SerializeField] private Image lavaTabHighlight;
        [SerializeField] private Image iceTabHighlight;
        [SerializeField] private Image toxicTabHighlight;

        [Header("Course List")]
        [SerializeField] private Transform courseListContainer;
        [SerializeField] private GameObject courseCardPrefab;

        [Header("Course Detail")]
        [SerializeField] private TMP_Text courseNameText;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_Text bestTimeText;
        [SerializeField] private Image bestMedalImage;
        [SerializeField] private TMP_Text hazardListText;
        [SerializeField] private Button launchButton;
        [SerializeField] private TMP_Text lockedText;

        [Header("Data")]
        [SerializeField] private CourseDataSO[] allCourses;
        [SerializeField] private WorkshopManager workshopManager;
        [SerializeField] private MonoBehaviour progressionDataSource;
        [SerializeField] private Sprite goldMedalSprite;
        [SerializeField] private Sprite silverMedalSprite;
        [SerializeField] private Sprite bronzeMedalSprite;
        [SerializeField] private Sprite noMedalSprite;

        private readonly List<GameObject> _spawnedCards = new List<GameObject>();
        private CourseDataSO _selectedCourse;
        private EnvironmentType _activeEnvironment = EnvironmentType.Lava;

        public CourseDataSO SelectedCourse => _selectedCourse;

        private void Awake()
        {
            if (lavaTab != null)
            {
                lavaTab.onClick.AddListener(() => SelectEnvironment(EnvironmentType.Lava));
            }

            if (iceTab != null)
            {
                iceTab.onClick.AddListener(() => SelectEnvironment(EnvironmentType.Ice));
            }

            if (toxicTab != null)
            {
                toxicTab.onClick.AddListener(() => SelectEnvironment(EnvironmentType.Toxic));
            }

            if (launchButton != null)
            {
                launchButton.onClick.AddListener(OnLaunchSelectedCourse);
            }
        }

        private void Start()
        {
            if (progressionDataSource == null)
            {
                progressionDataSource = FindSourceWithMember("IsCourseUnlocked");
            }

            SelectEnvironment(_activeEnvironment);
        }

        public void SelectCourse(CourseDataSO course)
        {
            _selectedCourse = course;
            RefreshDetailPanel();
            RefreshCardSelectionVisuals();
        }

        private void SelectEnvironment(EnvironmentType environmentType)
        {
            _activeEnvironment = environmentType;
            RefreshTabVisuals();
            RebuildCourseCards();
        }

        private void RefreshTabVisuals()
        {
            SetHighlight(lavaTabHighlight, _activeEnvironment == EnvironmentType.Lava);
            SetHighlight(iceTabHighlight, _activeEnvironment == EnvironmentType.Ice);
            SetHighlight(toxicTabHighlight, _activeEnvironment == EnvironmentType.Toxic);
        }

        private void RebuildCourseCards()
        {
            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                if (_spawnedCards[i] != null)
                {
                    Destroy(_spawnedCards[i]);
                }
            }

            _spawnedCards.Clear();

            CourseDataSO firstCourse = null;
            if (allCourses != null)
            {
                for (int i = 0; i < allCourses.Length; i++)
                {
                    CourseDataSO course = allCourses[i];
                    if (course == null || course.environmentType != _activeEnvironment)
                    {
                        continue;
                    }

                    GameObject card = CreateCourseCard(course);
                    _spawnedCards.Add(card);
                    if (firstCourse == null)
                    {
                        firstCourse = course;
                    }
                }
            }

            if (_selectedCourse == null || _selectedCourse.environmentType != _activeEnvironment)
            {
                _selectedCourse = firstCourse;
            }

            RefreshDetailPanel();
            RefreshCardSelectionVisuals();
        }

        private GameObject CreateCourseCard(CourseDataSO course)
        {
            GameObject card = courseCardPrefab != null
                ? Instantiate(courseCardPrefab, courseListContainer)
                : CreateFallbackCardObject(course.courseId);
            card.name = $"CourseCard_{course.courseId}";

            Button button = card.GetComponentInChildren<Button>();
            if (button == null)
            {
                button = card.AddComponent<Button>();
            }

            button.onClick.AddListener(() => SelectCourse(course));

            TMP_Text[] texts = card.GetComponentsInChildren<TMP_Text>(true);
            TMP_Text nameLabel = FindTextWithFallback(texts, "Name");
            TMP_Text difficultyLabel = FindTextWithFallback(texts, "Difficulty");
            TMP_Text bestLabel = FindTextWithFallback(texts, "Best");
            TMP_Text lockLabel = FindTextWithFallback(texts, "Lock");

            if (nameLabel != null)
            {
                nameLabel.text = course.courseName;
            }

            if (difficultyLabel != null)
            {
                difficultyLabel.text = course.difficulty.ToString();
            }

            if (bestLabel != null)
            {
                float best = GetBestTime(course.courseId);
                bestLabel.text = best > 0f ? $"Best: {FormatTime(best)}" : "Best: --:--.--";
            }

            bool unlocked = IsCourseUnlocked(course);
            if (lockLabel != null)
            {
                lockLabel.gameObject.SetActive(!unlocked);
                lockLabel.text = unlocked ? string.Empty : $"Requires {course.requiredMedals} medals";
            }

            Image medalImage = FindMedalImage(card);
            if (medalImage != null)
            {
                medalImage.sprite = GetMedalSprite(GetBestMedal(course.courseId));
            }

            return card;
        }

        private GameObject CreateFallbackCardObject(string courseId)
        {
            GameObject root = new GameObject($"CourseCard_{courseId}", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(courseListContainer, false);
            return root;
        }

        private void RefreshDetailPanel()
        {
            if (_selectedCourse == null)
            {
                if (courseNameText != null) courseNameText.text = "No Course";
                if (difficultyText != null) difficultyText.text = string.Empty;
                if (bestTimeText != null) bestTimeText.text = "--:--.--";
                if (hazardListText != null) hazardListText.text = string.Empty;
                if (lockedText != null) lockedText.text = string.Empty;
                if (launchButton != null) launchButton.interactable = false;
                return;
            }

            bool unlocked = IsCourseUnlocked(_selectedCourse);
            float bestTime = GetBestTime(_selectedCourse.courseId);
            int bestMedal = GetBestMedal(_selectedCourse.courseId);

            if (courseNameText != null)
            {
                courseNameText.text = _selectedCourse.courseName;
            }

            if (difficultyText != null)
            {
                difficultyText.text = _selectedCourse.difficulty.ToString();
            }

            if (bestTimeText != null)
            {
                bestTimeText.text = bestTime > 0f ? FormatTime(bestTime) : "--:--.--";
            }

            if (bestMedalImage != null)
            {
                bestMedalImage.sprite = GetMedalSprite(bestMedal);
            }

            if (hazardListText != null)
            {
                hazardListText.text = BuildHazardText(_selectedCourse.hazardDescriptions);
            }

            if (lockedText != null)
            {
                lockedText.text = unlocked ? string.Empty : $"Requires {_selectedCourse.requiredMedals} medals";
                lockedText.gameObject.SetActive(!unlocked);
            }

            if (launchButton != null)
            {
                launchButton.interactable = unlocked;
            }
        }

        private void RefreshCardSelectionVisuals()
        {
            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                GameObject card = _spawnedCards[i];
                if (card == null)
                {
                    continue;
                }

                bool selected = _selectedCourse != null && card.name.Contains(_selectedCourse.courseId);
                Image image = card.GetComponent<Image>();
                if (image != null)
                {
                    image.color = selected ? new Color(1f, 0.53f, 0f, 0.35f) : new Color(0.2f, 0.2f, 0.2f, 0.65f);
                }
            }
        }

        private void OnLaunchSelectedCourse()
        {
            if (_selectedCourse == null || !IsCourseUnlocked(_selectedCourse))
            {
                return;
            }

            if (workshopManager != null)
            {
                workshopManager.LaunchCourse(_selectedCourse.courseId);
            }
        }

        private bool IsCourseUnlocked(CourseDataSO course)
        {
            if (course == null)
            {
                return false;
            }

            if (progressionDataSource != null)
            {
                object response = ReflectionValueReader.Invoke(progressionDataSource, "IsCourseUnlocked", course.courseId);
                if (response is bool unlockedResult)
                {
                    return unlockedResult;
                }
            }

            return GetTotalMedals() >= course.requiredMedals;
        }

        private float GetBestTime(string courseId)
        {
            if (progressionDataSource != null)
            {
                object response = ReflectionValueReader.Invoke(progressionDataSource, "GetBestTime", courseId);
                if (response is float best)
                {
                    return best;
                }
            }

            return 0f;
        }

        private int GetBestMedal(string courseId)
        {
            if (progressionDataSource != null)
            {
                object response = ReflectionValueReader.Invoke(progressionDataSource, "GetBestMedal", courseId);
                if (response is int medal)
                {
                    return medal;
                }
            }

            return 0;
        }

        private int GetTotalMedals()
        {
            if (progressionDataSource != null)
            {
                int total = ReflectionValueReader.GetInt(progressionDataSource, "TotalMedals", int.MinValue);
                if (total != int.MinValue)
                {
                    return total;
                }
            }

            return 0;
        }

        private Sprite GetMedalSprite(int medal)
        {
            switch (medal)
            {
                case 3:
                    return goldMedalSprite != null ? goldMedalSprite : noMedalSprite;
                case 2:
                    return silverMedalSprite != null ? silverMedalSprite : noMedalSprite;
                case 1:
                    return bronzeMedalSprite != null ? bronzeMedalSprite : noMedalSprite;
                default:
                    return noMedalSprite;
            }
        }

        private static TMP_Text FindTextWithFallback(TMP_Text[] texts, string key)
        {
            if (texts == null || texts.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text != null && text.name.ToLowerInvariant().Contains(key.ToLowerInvariant()))
                {
                    return text;
                }
            }

            return texts[0];
        }

        private static Image FindMedalImage(GameObject root)
        {
            Image[] images = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image != null && image.name.ToLowerInvariant().Contains("medal"))
                {
                    return image;
                }
            }

            return null;
        }

        private static void SetHighlight(Image highlight, bool active)
        {
            if (highlight != null)
            {
                highlight.enabled = active;
            }
        }

        private static string BuildHazardText(string[] hazards)
        {
            if (hazards == null || hazards.Length == 0)
            {
                return "No hazards listed.";
            }

            string result = string.Empty;
            for (int i = 0; i < hazards.Length; i++)
            {
                string hazard = hazards[i];
                if (string.IsNullOrWhiteSpace(hazard))
                {
                    continue;
                }

                result += $"• {hazard}\n";
            }

            return result.TrimEnd('\n');
        }

        private static string FormatTime(float secondsValue)
        {
            int minutes = Mathf.FloorToInt(secondsValue / 60f);
            int seconds = Mathf.FloorToInt(secondsValue % 60f);
            int centiseconds = Mathf.FloorToInt((secondsValue * 100f) % 100f);
            return $"{minutes:00}:{seconds:00}.{centiseconds:00}";
        }

        private static MonoBehaviour FindSourceWithMember(string memberName)
        {
            MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (ReflectionValueReader.HasMember(behaviour, memberName))
                {
                    return behaviour;
                }
            }

            return null;
        }
    }
}
