using System.Collections;
using System.Collections.Generic;
using MetalPod.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Achievements
{
    /// <summary>
    /// Toast-style popup for newly unlocked achievements.
    /// </summary>
    public class AchievementPopup : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupRoot;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Animation")]
        [SerializeField] private float showDuration = 2.25f;
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float slideDistance = 28f;

        private readonly Queue<Achievement> _queue = new Queue<Achievement>();

        private AchievementManager _manager;
        private Coroutine _displayRoutine;
        private Vector2 _restingPosition;
        private bool _subscribed;
        private bool _hasRestingPosition;

        private void Awake()
        {
            if (popupRoot != null)
            {
                _restingPosition = popupRoot.anchoredPosition;
                _hasRestingPosition = true;
            }
        }

        private void OnEnable()
        {
            ResetVisualState();
            TrySubscribe();
        }

        private void Update()
        {
            if (!_subscribed)
            {
                TrySubscribe();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();

            if (_displayRoutine != null)
            {
                StopCoroutine(_displayRoutine);
                _displayRoutine = null;
            }

            _queue.Clear();
            ResetVisualState();
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
            if (!_subscribed || _manager == null)
            {
                _subscribed = false;
                _manager = null;
                return;
            }

            _manager.OnAchievementUnlocked -= HandleAchievementUnlocked;
            _subscribed = false;
            _manager = null;
        }

        private void HandleAchievementUnlocked(Achievement achievement)
        {
            if (achievement == null || achievement.Definition == null)
            {
                return;
            }

            _queue.Enqueue(achievement);
            if (_displayRoutine == null)
            {
                _displayRoutine = StartCoroutine(DisplayQueueRoutine());
            }
        }

        private IEnumerator DisplayQueueRoutine()
        {
            while (_queue.Count > 0)
            {
                Achievement achievement = _queue.Dequeue();
                ApplyAchievementData(achievement);

                yield return AnimatePopup(0f, 1f, _restingPosition + Vector2.up * slideDistance, _restingPosition);
                yield return new WaitForSecondsRealtime(showDuration);
                yield return AnimatePopup(1f, 0f, _restingPosition, _restingPosition + Vector2.up * slideDistance);
            }

            _displayRoutine = null;
        }

        private void ApplyAchievementData(Achievement achievement)
        {
            AchievementDataSO definition = achievement.Definition;
            if (titleText != null)
            {
                titleText.text = definition != null ? definition.title : "Achievement";
            }

            if (descriptionText != null)
            {
                descriptionText.text = definition != null ? definition.description : string.Empty;
            }

            if (iconImage != null)
            {
                iconImage.sprite = definition != null ? definition.icon : null;
                iconImage.enabled = iconImage.sprite != null;
            }
        }

        private IEnumerator AnimatePopup(float fromAlpha, float toAlpha, Vector2 fromPos, Vector2 toPos)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            canvasGroup.gameObject.SetActive(true);

            if (popupRoot != null)
            {
                popupRoot.anchoredPosition = fromPos;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

                if (popupRoot != null)
                {
                    popupRoot.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
                }

                yield return null;
            }

            canvasGroup.alpha = toAlpha;
            if (popupRoot != null)
            {
                popupRoot.anchoredPosition = toPos;
            }

            if (Mathf.Approximately(toAlpha, 0f))
            {
                canvasGroup.gameObject.SetActive(false);
            }
        }

        private void ResetVisualState()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.gameObject.SetActive(false);
            }

            if (popupRoot != null && _hasRestingPosition)
            {
                popupRoot.anchoredPosition = _restingPosition;
            }
        }
    }
}
