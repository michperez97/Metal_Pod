using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetalPod.UI
{
    [System.Serializable]
    public class CourseResultData
    {
        public float completionTime;
        public int medal;
        public bool isNewBestTime;
        public int baseCurrencyEarned;
        public int bonusCurrencyEarned;
        public int collectiblesFound;
        public int collectiblesTotal;
        public float healthRemaining;
        public bool nextCourseUnlocked;
        public string nextCourseId;
    }

    public class ResultsScreenUI : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text bestTimeText;
        [SerializeField] private Image medalImage;
        [SerializeField] private TMP_Text medalText;

        [Header("Currency")]
        [SerializeField] private TMP_Text baseCurrencyText;
        [SerializeField] private TMP_Text bonusCurrencyText;
        [SerializeField] private TMP_Text totalCurrencyText;

        [Header("Details")]
        [SerializeField] private TMP_Text collectiblesText;
        [SerializeField] private TMP_Text healthRemainingText;
        [SerializeField] private TMP_Text newRecordBadge;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextCourseButton;
        [SerializeField] private Button workshopButton;

        [Header("Medal Sprites")]
        [SerializeField] private Sprite goldMedalSprite;
        [SerializeField] private Sprite silverMedalSprite;
        [SerializeField] private Sprite bronzeMedalSprite;
        [SerializeField] private Sprite noMedalSprite;

        [Header("Animation")]
        [SerializeField] private float timeCountDuration = 1.5f;
        [SerializeField] private float medalRevealDelay = 1.5f;
        [SerializeField] private float medalBounceDuration = 0.3f;
        [SerializeField] private string workshopSceneNameOverride = "";

        private CourseResultData _currentResult;
        private Coroutine _timeRoutine;
        private Coroutine _medalRoutine;
        private Coroutine _currencyRoutine;

        private void Awake()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetry);
            }

            if (nextCourseButton != null)
            {
                nextCourseButton.onClick.AddListener(OnNextCourse);
            }

            if (workshopButton != null)
            {
                workshopButton.onClick.AddListener(OnWorkshop);
            }
        }

        public void Show(CourseResultData result)
        {
            _currentResult = result ?? new CourseResultData();
            gameObject.SetActive(true);
            Time.timeScale = 0f;

            StopAnimations();
            SetStaticFields(_currentResult);

            _timeRoutine = StartCoroutine(AnimateTimeCountUp(0f, _currentResult.completionTime, timeCountDuration));
            _medalRoutine = StartCoroutine(ShowMedalDelayed(_currentResult.medal, medalRevealDelay));
            _currencyRoutine = StartCoroutine(AnimateCurrencyCountUp(_currentResult));
        }

        public void Hide()
        {
            StopAnimations();
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        // Compatibility overload for older callers.
        public void ShowResults(float time, int medal)
        {
            Show(new CourseResultData
            {
                completionTime = time,
                medal = medal
            });
        }

        public void ShowResults(float time, System.Enum medal)
        {
            int medalValue = medal != null ? System.Convert.ToInt32(medal) : 0;
            ShowResults(time, medalValue);
        }

        private void SetStaticFields(CourseResultData result)
        {
            if (bestTimeText != null)
            {
                bestTimeText.text = FormatTime(result.completionTime);
            }

            if (collectiblesText != null)
            {
                collectiblesText.text = $"{result.collectiblesFound}/{result.collectiblesTotal}";
            }

            if (healthRemainingText != null)
            {
                healthRemainingText.text = $"{Mathf.RoundToInt(result.healthRemaining * 100f)}%";
            }

            if (newRecordBadge != null)
            {
                newRecordBadge.gameObject.SetActive(result.isNewBestTime);
            }

            if (nextCourseButton != null)
            {
                nextCourseButton.interactable = result.nextCourseUnlocked;
            }

            if (medalImage != null)
            {
                medalImage.transform.localScale = Vector3.zero;
            }

            if (baseCurrencyText != null)
            {
                baseCurrencyText.text = "0";
            }

            if (bonusCurrencyText != null)
            {
                bonusCurrencyText.text = "0";
            }

            if (totalCurrencyText != null)
            {
                totalCurrencyText.text = "0";
            }

            if (timeText != null)
            {
                timeText.text = "00:00.00";
            }
        }

        private IEnumerator AnimateTimeCountUp(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float current = Mathf.Lerp(from, to, t);
                if (timeText != null)
                {
                    timeText.text = FormatTime(current);
                }

                yield return null;
            }

            if (timeText != null)
            {
                timeText.text = FormatTime(to);
            }
        }

        private IEnumerator ShowMedalDelayed(int medal, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            if (medalImage != null)
            {
                medalImage.sprite = GetMedalSprite(medal);
            }

            if (medalText != null)
            {
                medalText.text = MedalToText(medal);
            }

            yield return StartCoroutine(AnimateMedalBounce());
        }

        private IEnumerator AnimateMedalBounce()
        {
            if (medalImage == null)
            {
                yield break;
            }

            float elapsed = 0f;
            Vector3 start = Vector3.zero;
            Vector3 peak = Vector3.one * 1.2f;
            Vector3 end = Vector3.one;

            while (elapsed < medalBounceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = medalBounceDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / medalBounceDuration);

                if (t < 0.6f)
                {
                    medalImage.transform.localScale = Vector3.Lerp(start, peak, t / 0.6f);
                }
                else
                {
                    medalImage.transform.localScale = Vector3.Lerp(peak, end, (t - 0.6f) / 0.4f);
                }

                yield return null;
            }

            medalImage.transform.localScale = end;
        }

        private IEnumerator AnimateCurrencyCountUp(CourseResultData result)
        {
            float duration = 1.0f;
            float elapsed = 0f;
            int total = result.baseCurrencyEarned + result.bonusCurrencyEarned;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);

                int baseValue = Mathf.RoundToInt(Mathf.Lerp(0f, result.baseCurrencyEarned, t));
                int bonusValue = Mathf.RoundToInt(Mathf.Lerp(0f, result.bonusCurrencyEarned, t));
                int totalValue = Mathf.RoundToInt(Mathf.Lerp(0f, total, t));

                if (baseCurrencyText != null)
                {
                    baseCurrencyText.text = baseValue.ToString();
                }

                if (bonusCurrencyText != null)
                {
                    bonusCurrencyText.text = bonusValue.ToString();
                }

                if (totalCurrencyText != null)
                {
                    totalCurrencyText.text = totalValue.ToString();
                }

                yield return null;
            }

            if (baseCurrencyText != null)
            {
                baseCurrencyText.text = result.baseCurrencyEarned.ToString();
            }

            if (bonusCurrencyText != null)
            {
                bonusCurrencyText.text = result.bonusCurrencyEarned.ToString();
            }

            if (totalCurrencyText != null)
            {
                totalCurrencyText.text = total.ToString();
            }
        }

        private void OnRetry()
        {
            Time.timeScale = 1f;
            Scene current = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(current.name))
            {
                SceneManager.LoadScene(current.name);
            }
        }

        private void OnNextCourse()
        {
            if (_currentResult == null || !_currentResult.nextCourseUnlocked)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_currentResult.nextCourseId))
            {
                SharedContractsBridge.Raise("RaiseCourseSelected", _currentResult.nextCourseId);
            }

            Time.timeScale = 1f;
        }

        private void OnWorkshop()
        {
            string workshopScene = string.IsNullOrEmpty(workshopSceneNameOverride)
                ? SharedContractsBridge.SceneWorkshop
                : workshopSceneNameOverride;

            Time.timeScale = 1f;
            SceneManager.LoadScene(workshopScene);
        }

        private void StopAnimations()
        {
            if (_timeRoutine != null)
            {
                StopCoroutine(_timeRoutine);
                _timeRoutine = null;
            }

            if (_medalRoutine != null)
            {
                StopCoroutine(_medalRoutine);
                _medalRoutine = null;
            }

            if (_currencyRoutine != null)
            {
                StopCoroutine(_currencyRoutine);
                _currencyRoutine = null;
            }
        }

        private static string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int centiseconds = Mathf.FloorToInt((time * 100f) % 100f);
            return $"{minutes:00}:{seconds:00}.{centiseconds:00}";
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

        private static string MedalToText(int medal)
        {
            switch (medal)
            {
                case 3:
                    return "GOLD";
                case 2:
                    return "SILVER";
                case 1:
                    return "BRONZE";
                default:
                    return "NONE";
            }
        }
    }
}
