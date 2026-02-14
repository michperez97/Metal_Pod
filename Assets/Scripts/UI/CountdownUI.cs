using System.Collections;
using MetalPod.Accessibility;
using TMPro;
using UnityEngine;

namespace MetalPod.UI
{
    public class CountdownUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private Animator countdownAnimator;
        [SerializeField] private float goDisplayDuration = 0.6f;
        [SerializeField] private MonoBehaviour courseEventsSource;

        private Coroutine _autoHideRoutine;
        private System.Action<int> _countdownHandler;
        private bool _countdownSubscribed;

        private void OnEnable()
        {
            if (courseEventsSource == null)
            {
                courseEventsSource = FindSourceWithEvent("OnCountdownTick");
            }

            if (!_countdownSubscribed && courseEventsSource != null)
            {
                _countdownHandler = HandleCountdownTick;
                _countdownSubscribed = ReflectionValueReader.SubscribeEvent(courseEventsSource, "OnCountdownTick", _countdownHandler);
            }
        }

        private void OnDisable()
        {
            if (_countdownSubscribed && courseEventsSource != null && _countdownHandler != null)
            {
                ReflectionValueReader.UnsubscribeEvent(courseEventsSource, "OnCountdownTick", _countdownHandler);
            }

            _countdownSubscribed = false;
            _countdownHandler = null;
        }

        public void ShowCountdown(int number)
        {
            gameObject.SetActive(true);

            if (countdownText != null)
            {
                countdownText.text = number > 0 ? number.ToString() : "GO!";
            }

            if (countdownAnimator != null)
            {
                countdownAnimator.SetTrigger("Pop");
            }

            if (number <= 0)
            {
                if (_autoHideRoutine != null)
                {
                    StopCoroutine(_autoHideRoutine);
                }

                _autoHideRoutine = StartCoroutine(AutoHideRoutine());
            }
        }

        public void Hide()
        {
            if (_autoHideRoutine != null)
            {
                StopCoroutine(_autoHideRoutine);
                _autoHideRoutine = null;
            }

            gameObject.SetActive(false);
        }

        private void HandleCountdownTick(int value)
        {
            ShowCountdown(value);
            string countdownValue = value > 0 ? value.ToString() : "GO";
            AccessibilityManager.Instance?.Announce(string.Format(AccessibilityLabels.CountdownAnnounce, countdownValue));
        }

        private static MonoBehaviour FindSourceWithEvent(string eventName)
        {
            MonoBehaviour[] sources = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < sources.Length; i++)
            {
                MonoBehaviour source = sources[i];
                if (source != null && ReflectionValueReader.HasEvent(source, eventName))
                {
                    return source;
                }
            }

            return null;
        }

        private IEnumerator AutoHideRoutine()
        {
            yield return new WaitForSecondsRealtime(goDisplayDuration);
            Hide();
        }
    }
}
