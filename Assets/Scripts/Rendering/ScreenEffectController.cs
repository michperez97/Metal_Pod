using System.Collections;
using UnityEngine;

namespace MetalPod.Rendering
{
    public class ScreenEffectController : MonoBehaviour
    {
        [Header("Smoothing")]
        [SerializeField] private float transitionSpeed = 6f;
        [SerializeField] private float damageFlashDuration = 0.18f;

        public static ScreenEffectController Instance { get; private set; }

        private static readonly int HeatDistortionId = Shader.PropertyToID("_HeatDistortionIntensity");
        private static readonly int HeatDistortionLegacyId = Shader.PropertyToID("_MetalPodHeatDistortion");
        private static readonly int FrostIntensityId = Shader.PropertyToID("_FrostIntensity");
        private static readonly int ToxicIntensityId = Shader.PropertyToID("_ToxicIntensity");
        private static readonly int DamageFlashIntensityId = Shader.PropertyToID("_DamageFlashIntensity");
        private static readonly int DamageVignetteIntensityId = Shader.PropertyToID("_DamageVignetteIntensity");

        private float _currentHeat;
        private float _targetHeat;
        private float _currentFrost;
        private float _targetFrost;
        private float _currentToxic;
        private float _targetToxic;
        private float _currentDamageVignette;
        private float _targetDamageVignette;

        private Coroutine _damageFlashRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyGlobals(0f, 0f, 0f, 0f, 0f);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            float lerpStep = Mathf.Max(0.01f, transitionSpeed) * Time.deltaTime;

            _currentHeat = Mathf.MoveTowards(_currentHeat, _targetHeat, lerpStep);
            _currentFrost = Mathf.MoveTowards(_currentFrost, _targetFrost, lerpStep);
            _currentToxic = Mathf.MoveTowards(_currentToxic, _targetToxic, lerpStep);
            _currentDamageVignette = Mathf.MoveTowards(_currentDamageVignette, _targetDamageVignette, lerpStep);

            Shader.SetGlobalFloat(HeatDistortionId, _currentHeat);
            Shader.SetGlobalFloat(HeatDistortionLegacyId, _currentHeat);
            Shader.SetGlobalFloat(FrostIntensityId, _currentFrost);
            Shader.SetGlobalFloat(ToxicIntensityId, _currentToxic);
            Shader.SetGlobalFloat(DamageVignetteIntensityId, _currentDamageVignette);
        }

        public void SetHeatDistortion(float intensity)
        {
            _targetHeat = Mathf.Clamp01(intensity);
        }

        public void SetFrostOverlay(float intensity)
        {
            _targetFrost = Mathf.Clamp01(intensity);
        }

        public void SetToxicEffect(float intensity)
        {
            _targetToxic = Mathf.Clamp01(intensity);
        }

        public void TriggerDamageFlash()
        {
            TriggerDamageFlash(1f);
        }

        public void TriggerDamageFlash(float peakIntensity)
        {
            if (_damageFlashRoutine != null)
            {
                StopCoroutine(_damageFlashRoutine);
            }

            _damageFlashRoutine = StartCoroutine(DamageFlashRoutine(Mathf.Clamp01(peakIntensity)));
        }

        public void SetLowHealthVignette(float healthNormalized)
        {
            float clampedHealth = Mathf.Clamp01(healthNormalized);
            _targetDamageVignette = 1f - clampedHealth;
        }

        public void SetDamageVignette(float intensity)
        {
            _targetDamageVignette = Mathf.Clamp01(intensity);
        }

        public void ClearAllEffects()
        {
            _targetHeat = 0f;
            _targetFrost = 0f;
            _targetToxic = 0f;
            _targetDamageVignette = 0f;

            if (_damageFlashRoutine != null)
            {
                StopCoroutine(_damageFlashRoutine);
                _damageFlashRoutine = null;
            }

            ApplyGlobals(0f, 0f, 0f, 0f, 0f);
        }

        private IEnumerator DamageFlashRoutine(float peak)
        {
            float duration = Mathf.Max(0.05f, damageFlashDuration);
            float elapsed = 0f;

            Shader.SetGlobalFloat(DamageFlashIntensityId, peak);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Shader.SetGlobalFloat(DamageFlashIntensityId, Mathf.Lerp(peak, 0f, t));
                yield return null;
            }

            Shader.SetGlobalFloat(DamageFlashIntensityId, 0f);
            _damageFlashRoutine = null;
        }

        private static void ApplyGlobals(float heat, float frost, float toxic, float damageFlash, float damageVignette)
        {
            Shader.SetGlobalFloat(HeatDistortionId, heat);
            Shader.SetGlobalFloat(HeatDistortionLegacyId, heat);
            Shader.SetGlobalFloat(FrostIntensityId, frost);
            Shader.SetGlobalFloat(ToxicIntensityId, toxic);
            Shader.SetGlobalFloat(DamageFlashIntensityId, damageFlash);
            Shader.SetGlobalFloat(DamageVignetteIntensityId, damageVignette);
        }
    }
}
