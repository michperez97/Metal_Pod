using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.UI
{
    public class HUD : MonoBehaviour
    {
        [Header("Data Sources (Optional)")]
        [SerializeField] private MonoBehaviour unifiedHovercraftDataSource;
        [SerializeField] private MonoBehaviour healthDataSource;
        [SerializeField] private MonoBehaviour speedDataSource;
        [SerializeField] private MonoBehaviour boostDataSource;
        [SerializeField] private MonoBehaviour timerDataSource;
        [SerializeField] private MonoBehaviour courseEventsSource;

        [Header("Health")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image shieldFill;
        [SerializeField] private Image healthDamageFlash;

        [Header("Speed")]
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private Image speedNeedle;

        [Header("Timer")]
        [SerializeField] private TMP_Text timerText;

        [Header("Boost")]
        [SerializeField] private Image boostFill;
        [SerializeField] private Image boostIcon;
        [SerializeField] private Color boostReadyColor = Color.cyan;
        [SerializeField] private Color boostCooldownColor = Color.gray;

        [Header("Hazard Warning")]
        [SerializeField] private Image warningArrowLeft;
        [SerializeField] private Image warningArrowRight;
        [SerializeField] private Image warningArrowForward;
        [SerializeField] private CanvasGroup warningGroup;

        [Header("Behavior")]
        [SerializeField] private float barLerpSpeed = 8f;
        [SerializeField] private float warningFadeSpeed = 5f;

        private Coroutine _damageFlashRoutine;
        private float _warningVisibilityTarget;
        private float _warningUntilTime;
        private bool _damageSubscribed;
        private bool _countdownSubscribed;
        private bool _raceStartedSubscribed;
        private Action<float, float> _damageHandler;
        private Action<int> _countdownHandler;
        private Action _raceStartedHandler;

        private void OnEnable()
        {
            TryAutoBindSources();
            BindEvents();
            HideAllWarnings();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void Update()
        {
            UpdateHealthAndShield();
            UpdateSpeed();
            UpdateBoost();
            UpdateTimer();
            UpdateHazardWarning();
        }

        public void OnDamageReceived()
        {
            if (_damageFlashRoutine != null)
            {
                StopCoroutine(_damageFlashRoutine);
            }

            _damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
        }

        public void UpdateTimer(float elapsed)
        {
            if (timerText == null)
            {
                return;
            }

            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            int centiseconds = Mathf.FloorToInt((elapsed * 100f) % 100f);
            timerText.text = $"{minutes:00}:{seconds:00}.{centiseconds:00}";
        }

        public void ShowHazardWarning(Vector3 worldDirection, float durationSeconds = 0.6f)
        {
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            Vector3 localDirection = cameraTransform != null
                ? cameraTransform.InverseTransformDirection(worldDirection.normalized)
                : transform.InverseTransformDirection(worldDirection.normalized);

            HideAllWarnings();
            if (Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.z))
            {
                if (localDirection.x < 0f && warningArrowLeft != null)
                {
                    warningArrowLeft.gameObject.SetActive(true);
                }
                else if (warningArrowRight != null)
                {
                    warningArrowRight.gameObject.SetActive(true);
                }
            }
            else if (warningArrowForward != null)
            {
                warningArrowForward.gameObject.SetActive(true);
            }

            _warningVisibilityTarget = 1f;
            _warningUntilTime = Time.unscaledTime + Mathf.Max(0.1f, durationSeconds);
        }

        public void HideHazardWarning()
        {
            _warningVisibilityTarget = 0f;
            _warningUntilTime = 0f;
            HideAllWarnings();
        }

        private void UpdateHealthAndShield()
        {
            object healthSource = ChooseSourceWithMember("HealthNormalized", unifiedHovercraftDataSource, healthDataSource);
            object shieldSource = ChooseSourceWithMember("ShieldNormalized", unifiedHovercraftDataSource, healthDataSource);

            float healthNormalized = Mathf.Clamp01(ReflectionValueReader.GetFloat(healthSource, "HealthNormalized", 0f));
            float shieldNormalized = Mathf.Clamp01(ReflectionValueReader.GetFloat(shieldSource, "ShieldNormalized", 0f));

            if (healthFill != null)
            {
                healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, healthNormalized, Time.deltaTime * barLerpSpeed);
                healthFill.color = Color.Lerp(Color.red, Color.green, healthNormalized);
            }

            if (shieldFill != null)
            {
                shieldFill.fillAmount = Mathf.Lerp(shieldFill.fillAmount, shieldNormalized, Time.deltaTime * barLerpSpeed);
            }
        }

        private void UpdateSpeed()
        {
            object source = ChooseSourceWithMember("CurrentSpeed", unifiedHovercraftDataSource, speedDataSource);
            float currentSpeed = Mathf.Max(0f, ReflectionValueReader.GetFloat(source, "CurrentSpeed", 0f));
            float maxSpeed = Mathf.Max(currentSpeed, ReflectionValueReader.GetFloat(source, "MaxSpeed", 0f));

            if (speedText != null)
            {
                speedText.text = $"{Mathf.RoundToInt(currentSpeed):000}";
            }

            if (speedNeedle != null && maxSpeed > 0f)
            {
                float angle = Mathf.Lerp(-120f, 120f, Mathf.Clamp01(currentSpeed / maxSpeed));
                speedNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            }
        }

        private void UpdateBoost()
        {
            object source = ChooseSourceWithMember("BoostCooldownNormalized", unifiedHovercraftDataSource, boostDataSource);
            float boostCooldownNormalized = Mathf.Clamp01(ReflectionValueReader.GetFloat(source, "BoostCooldownNormalized", 0f));

            if (boostFill != null)
            {
                boostFill.fillAmount = boostCooldownNormalized;
            }

            if (boostIcon != null)
            {
                bool ready = boostCooldownNormalized >= 1f || boostCooldownNormalized <= 0.001f;
                boostIcon.color = ready ? boostReadyColor : boostCooldownColor;
            }
        }

        private void UpdateTimer()
        {
            if (timerText == null)
            {
                return;
            }

            object source = ChooseSourceWithMember("ElapsedTime", timerDataSource, courseEventsSource);
            if (source != null)
            {
                float elapsed = ReflectionValueReader.GetFloat(source, "ElapsedTime", -1f);
                if (elapsed >= 0f)
                {
                    UpdateTimer(elapsed);
                    return;
                }

                object formatted = ReflectionValueReader.Invoke(source, "GetFormattedTime");
                if (formatted is string formattedString && !string.IsNullOrEmpty(formattedString))
                {
                    timerText.text = formattedString;
                }
            }
        }

        private void UpdateHazardWarning()
        {
            if (warningGroup == null)
            {
                return;
            }

            if (_warningUntilTime > 0f && Time.unscaledTime >= _warningUntilTime)
            {
                _warningVisibilityTarget = 0f;
                _warningUntilTime = 0f;
                HideAllWarnings();
            }

            warningGroup.alpha = Mathf.MoveTowards(warningGroup.alpha, _warningVisibilityTarget, Time.unscaledDeltaTime * warningFadeSpeed);
            warningGroup.gameObject.SetActive(warningGroup.alpha > 0.001f);
        }

        private void TryAutoBindSources()
        {
            if (unifiedHovercraftDataSource == null)
            {
                unifiedHovercraftDataSource = FindSourceWithMembers("HealthNormalized", "ShieldNormalized", "CurrentSpeed", "BoostCooldownNormalized");
            }

            if (healthDataSource == null)
            {
                healthDataSource = FindSourceWithMembers("HealthNormalized", "ShieldNormalized");
            }

            if (speedDataSource == null)
            {
                speedDataSource = FindSourceWithMembers("CurrentSpeed");
            }

            if (boostDataSource == null)
            {
                boostDataSource = FindSourceWithMembers("BoostCooldownNormalized");
            }

            if (timerDataSource == null)
            {
                timerDataSource = FindSourceWithMembers("ElapsedTime");
                if (timerDataSource == null)
                {
                    timerDataSource = FindSourceWithMembers("GetFormattedTime");
                }
            }

            if (courseEventsSource == null)
            {
                courseEventsSource = FindSourceWithEvents("OnCountdownTick", "OnRaceStarted");
            }
        }

        private void BindEvents()
        {
            if (!_damageSubscribed)
            {
                object damageSource = ChooseSourceWithEvent("OnDamage", healthDataSource, unifiedHovercraftDataSource);
                if (damageSource != null)
                {
                    _damageHandler = (_, _) => OnDamageReceived();
                    _damageSubscribed = ReflectionValueReader.SubscribeEvent(damageSource, "OnDamage", _damageHandler);
                }
            }

            if (!_countdownSubscribed)
            {
                object countdownSource = ChooseSourceWithEvent("OnCountdownTick", courseEventsSource, timerDataSource);
                if (countdownSource != null)
                {
                    _countdownHandler = HandleCountdownTick;
                    _countdownSubscribed = ReflectionValueReader.SubscribeEvent(countdownSource, "OnCountdownTick", _countdownHandler);
                }
            }

            if (!_raceStartedSubscribed)
            {
                object raceSource = ChooseSourceWithEvent("OnRaceStarted", courseEventsSource, timerDataSource);
                if (raceSource != null)
                {
                    _raceStartedHandler = HandleRaceStarted;
                    _raceStartedSubscribed = ReflectionValueReader.SubscribeEvent(raceSource, "OnRaceStarted", _raceStartedHandler);
                }
            }
        }

        private void UnbindEvents()
        {
            object damageSource = ChooseSourceWithEvent("OnDamage", healthDataSource, unifiedHovercraftDataSource);
            if (_damageSubscribed && damageSource != null && _damageHandler != null)
            {
                ReflectionValueReader.UnsubscribeEvent(damageSource, "OnDamage", _damageHandler);
            }

            object countdownSource = ChooseSourceWithEvent("OnCountdownTick", courseEventsSource, timerDataSource);
            if (_countdownSubscribed && countdownSource != null && _countdownHandler != null)
            {
                ReflectionValueReader.UnsubscribeEvent(countdownSource, "OnCountdownTick", _countdownHandler);
            }

            object raceSource = ChooseSourceWithEvent("OnRaceStarted", courseEventsSource, timerDataSource);
            if (_raceStartedSubscribed && raceSource != null && _raceStartedHandler != null)
            {
                ReflectionValueReader.UnsubscribeEvent(raceSource, "OnRaceStarted", _raceStartedHandler);
            }

            _damageSubscribed = false;
            _countdownSubscribed = false;
            _raceStartedSubscribed = false;
            _damageHandler = null;
            _countdownHandler = null;
            _raceStartedHandler = null;
        }

        private void HandleCountdownTick(int secondsRemaining)
        {
            if (timerText != null)
            {
                timerText.text = secondsRemaining > 0 ? secondsRemaining.ToString() : "GO!";
            }
        }

        private void HandleRaceStarted()
        {
            if (timerText != null)
            {
                timerText.text = "00:00.00";
            }
        }

        private IEnumerator DamageFlashRoutine()
        {
            if (healthDamageFlash == null)
            {
                yield break;
            }

            healthDamageFlash.gameObject.SetActive(true);
            Color color = healthDamageFlash.color;
            color.a = 0.3f;
            healthDamageFlash.color = color;

            float elapsed = 0f;
            const float duration = 0.15f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(0.3f, 0f, elapsed / duration);
                healthDamageFlash.color = color;
                yield return null;
            }

            color.a = 0f;
            healthDamageFlash.color = color;
            healthDamageFlash.gameObject.SetActive(false);
            _damageFlashRoutine = null;
        }

        private void HideAllWarnings()
        {
            if (warningArrowLeft != null)
            {
                warningArrowLeft.gameObject.SetActive(false);
            }

            if (warningArrowRight != null)
            {
                warningArrowRight.gameObject.SetActive(false);
            }

            if (warningArrowForward != null)
            {
                warningArrowForward.gameObject.SetActive(false);
            }

            if (warningGroup != null && _warningVisibilityTarget <= 0f)
            {
                warningGroup.alpha = 0f;
                warningGroup.gameObject.SetActive(false);
            }
        }

        private static MonoBehaviour FindSourceWithMembers(params string[] members)
        {
            MonoBehaviour[] candidates = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                MonoBehaviour candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                bool hasAll = true;
                for (int m = 0; m < members.Length; m++)
                {
                    if (!ReflectionValueReader.HasMember(candidate, members[m]))
                    {
                        hasAll = false;
                        break;
                    }
                }

                if (hasAll)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static MonoBehaviour FindSourceWithEvents(params string[] events)
        {
            MonoBehaviour[] candidates = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                MonoBehaviour candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                bool hasAll = true;
                for (int e = 0; e < events.Length; e++)
                {
                    if (!ReflectionValueReader.HasEvent(candidate, events[e]))
                    {
                        hasAll = false;
                        break;
                    }
                }

                if (hasAll)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static object ChooseSourceWithMember(string memberName, params MonoBehaviour[] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                MonoBehaviour candidate = candidates[i];
                if (candidate != null && ReflectionValueReader.HasMember(candidate, memberName))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static object ChooseSourceWithEvent(string eventName, params MonoBehaviour[] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                MonoBehaviour candidate = candidates[i];
                if (candidate != null && ReflectionValueReader.HasEvent(candidate, eventName))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
