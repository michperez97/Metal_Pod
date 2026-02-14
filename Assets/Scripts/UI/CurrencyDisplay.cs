using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MetalPod.UI
{
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour progressionDataSource;
        [SerializeField] private MonoBehaviour currencyManagerSource;
        [SerializeField] private TMP_Text currencyText;
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float animationDuration = 0.2f;

        private int _displayedCurrency;
        private int _targetCurrency;
        private Coroutine _animateRoutine;

        private Action<int> _currencyChangedHandler;
        private Action<int> _currencyEarnedHandler;
        private bool _eventBusCurrencyChangedSubscribed;
        private bool _eventBusCurrencyEarnedSubscribed;

        private Action<int> _currencyManagerChangedHandler;
        private bool _currencyManagerSubscribed;

        private void OnEnable()
        {
            TryAutoBindSources();
            SubscribeToEvents();

            int initialCurrency = ReadCurrencyFromSources();
            _displayedCurrency = initialCurrency;
            _targetCurrency = initialCurrency;
            UpdateText(initialCurrency);
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        public void SetCurrency(int amount)
        {
            _targetCurrency = Mathf.Max(0, amount);
            if (!animateChanges)
            {
                _displayedCurrency = _targetCurrency;
                UpdateText(_displayedCurrency);
                return;
            }

            if (_animateRoutine != null)
            {
                StopCoroutine(_animateRoutine);
            }

            _animateRoutine = StartCoroutine(AnimateCurrency(_displayedCurrency, _targetCurrency, animationDuration));
        }

        private void HandleCurrencyChanged(int total)
        {
            SetCurrency(total);
        }

        private void HandleCurrencyEarned(int amount)
        {
            int sourceValue = ReadCurrencyFromSources();
            if (sourceValue > 0)
            {
                SetCurrency(sourceValue);
                return;
            }

            SetCurrency(_targetCurrency + Mathf.Max(0, amount));
        }

        private void TryAutoBindSources()
        {
            if (progressionDataSource == null)
            {
                progressionDataSource = FindSourceWithMember("Currency");
            }

            if (currencyManagerSource == null)
            {
                currencyManagerSource = FindSourceWithEvent("OnCurrencyChanged");
            }
        }

        private void SubscribeToEvents()
        {
            if (!_eventBusCurrencyChangedSubscribed)
            {
                _currencyChangedHandler = HandleCurrencyChanged;
                _eventBusCurrencyChangedSubscribed = SharedContractsBridge.SubscribeEvent("OnCurrencyChanged", _currencyChangedHandler);
            }

            if (!_eventBusCurrencyEarnedSubscribed)
            {
                _currencyEarnedHandler = HandleCurrencyEarned;
                _eventBusCurrencyEarnedSubscribed = SharedContractsBridge.SubscribeEvent("OnCurrencyEarned", _currencyEarnedHandler);
            }

            if (currencyManagerSource != null && !_currencyManagerSubscribed)
            {
                _currencyManagerChangedHandler = HandleCurrencyChanged;
                _currencyManagerSubscribed = ReflectionValueReader.SubscribeEvent(currencyManagerSource, "OnCurrencyChanged", _currencyManagerChangedHandler);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventBusCurrencyChangedSubscribed && _currencyChangedHandler != null)
            {
                SharedContractsBridge.UnsubscribeEvent("OnCurrencyChanged", _currencyChangedHandler);
            }

            if (_eventBusCurrencyEarnedSubscribed && _currencyEarnedHandler != null)
            {
                SharedContractsBridge.UnsubscribeEvent("OnCurrencyEarned", _currencyEarnedHandler);
            }

            if (_currencyManagerSubscribed && currencyManagerSource != null && _currencyManagerChangedHandler != null)
            {
                ReflectionValueReader.UnsubscribeEvent(currencyManagerSource, "OnCurrencyChanged", _currencyManagerChangedHandler);
            }

            _eventBusCurrencyChangedSubscribed = false;
            _eventBusCurrencyEarnedSubscribed = false;
            _currencyManagerSubscribed = false;
            _currencyChangedHandler = null;
            _currencyEarnedHandler = null;
            _currencyManagerChangedHandler = null;
        }

        private int ReadCurrencyFromSources()
        {
            if (progressionDataSource != null)
            {
                int fromProgression = ReflectionValueReader.GetInt(progressionDataSource, "Currency", int.MinValue);
                if (fromProgression != int.MinValue)
                {
                    return Mathf.Max(0, fromProgression);
                }
            }

            if (currencyManagerSource != null)
            {
                int fromManager = ReflectionValueReader.GetInt(currencyManagerSource, "CurrentCurrency", int.MinValue);
                if (fromManager != int.MinValue)
                {
                    return Mathf.Max(0, fromManager);
                }
            }

            return 0;
        }

        private IEnumerator AnimateCurrency(int from, int to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                _displayedCurrency = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
                UpdateText(_displayedCurrency);
                yield return null;
            }

            _displayedCurrency = to;
            UpdateText(_displayedCurrency);
            _animateRoutine = null;
        }

        private void UpdateText(int value)
        {
            if (currencyText != null)
            {
                currencyText.text = value.ToString();
            }
        }

        private static MonoBehaviour FindSourceWithMember(string memberName)
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
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

        private static MonoBehaviour FindSourceWithEvent(string eventName)
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                EventInfo info = behaviour.GetType().GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
                if (info != null)
                {
                    return behaviour;
                }
            }

            return null;
        }
    }
}
