using System.Collections;
using TMPro;
using UnityEngine;

namespace MetalPod.UI
{
    public class CourseUnlockedPopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private float visibleDuration = 2.0f;
        [SerializeField] private float fadeDuration = 0.25f;

        private Coroutine _routine;
        private System.Action<string> _courseUnlockedHandler;
        private bool _isSubscribed;

        private void OnEnable()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.gameObject.SetActive(false);
            }

            if (!_isSubscribed)
            {
                _courseUnlockedHandler = HandleCourseUnlocked;
                _isSubscribed = SharedContractsBridge.SubscribeEvent("OnCourseUnlocked", _courseUnlockedHandler);
            }
        }

        private void OnDisable()
        {
            if (_isSubscribed && _courseUnlockedHandler != null)
            {
                SharedContractsBridge.UnsubscribeEvent("OnCourseUnlocked", _courseUnlockedHandler);
            }

            _isSubscribed = false;
            _courseUnlockedHandler = null;
        }

        public void Show(string courseNameOrId)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(ShowRoutine(courseNameOrId));
        }

        private void HandleCourseUnlocked(string courseId)
        {
            Show(courseId);
        }

        private IEnumerator ShowRoutine(string courseNameOrId)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            canvasGroup.gameObject.SetActive(true);
            if (messageText != null)
            {
                messageText.text = $"UNLOCKED: {courseNameOrId}";
            }

            yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
            yield return new WaitForSecondsRealtime(visibleDuration);
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

            canvasGroup.gameObject.SetActive(false);
            _routine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
        }
    }
}
