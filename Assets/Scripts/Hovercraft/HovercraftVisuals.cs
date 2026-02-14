using System.Collections;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    public class HovercraftVisuals : MonoBehaviour
    {
        [Header("Thrusters")]
        [SerializeField] private ParticleSystem[] mainThrusters;
        [SerializeField] private ParticleSystem[] sideThrusters;
        [SerializeField] private ParticleSystem boostThruster;
        [SerializeField] private float mainThrusterMinEmission = 50f;
        [SerializeField] private float mainThrusterMaxEmission = 200f;
        [SerializeField] private float sideThrusterMinEmission = 30f;
        [SerializeField] private float sideThrusterMaxEmission = 100f;
        [SerializeField] private float boostThrusterEmission = 300f;

        [Header("Damage")]
        [SerializeField] private ParticleSystem sparksEffect;
        [SerializeField] private ParticleSystem smokeEffect;
        [SerializeField] private ParticleSystem fireEffect;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private Renderer[] modelRenderers;

        [Header("Screen Effects")]
        [SerializeField] private float screenShakeIntensity = 0.3f;
        [SerializeField] private float screenShakeDuration = 0.2f;
        [SerializeField] private float flashDuration = 0.12f;

        private HovercraftController _controller;
        private HovercraftHealth _health;
        private HovercraftInput _input;
        private HovercraftStats _stats;
        private MaterialPropertyBlock _propertyBlock;
        private Color[] _baseColors;
        private bool _isExploded;
        private Coroutine _flashRoutine;
        private Coroutine _screenShakeRoutine;
        private Coroutine _invincibilityRoutine;

        private void Awake()
        {
            _controller = GetComponent<HovercraftController>();
            _health = GetComponent<HovercraftHealth>();
            _input = GetComponent<HovercraftInput>();
            _stats = GetComponent<HovercraftStats>();

            if (modelRenderers == null || modelRenderers.Length == 0)
            {
                modelRenderers = GetComponentsInChildren<Renderer>(true);
            }

            _propertyBlock = new MaterialPropertyBlock();
            _baseColors = new Color[modelRenderers.Length];
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                Renderer modelRenderer = modelRenderers[i];
                _baseColors[i] = modelRenderer != null && modelRenderer.sharedMaterial != null
                    ? modelRenderer.sharedMaterial.color
                    : Color.white;
            }
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamageTyped += HandleDamageTyped;
                _health.OnDestroyed += PlayExplosion;
                _health.OnRestored += PlayRespawnEffect;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamageTyped -= HandleDamageTyped;
                _health.OnDestroyed -= PlayExplosion;
                _health.OnRestored -= PlayRespawnEffect;
            }
        }

        private void Update()
        {
            if (_isExploded)
            {
                return;
            }

            float maxSpeed = _stats != null ? _stats.MaxSpeed : 0f;
            float currentSpeed = _controller != null ? _controller.CurrentSpeed : 0f;
            float speedNormalized = maxSpeed > 0f ? Mathf.Clamp01(currentSpeed / maxSpeed) : 0f;
            bool isBoosting = _controller != null && _controller.CurrentState == HovercraftState.Boosting;

            UpdateThrusters(speedNormalized, isBoosting);

            if (_health != null)
            {
                UpdateDamageState(_health.HealthNormalized);

                if (_health.IsInvincible && _invincibilityRoutine == null)
                {
                    _invincibilityRoutine = StartCoroutine(InvincibilityBlinkRoutine());
                }
            }
        }

        public void UpdateThrusters(float speedNormalized, bool isBoosting)
        {
            float mainEmission = Mathf.Lerp(mainThrusterMinEmission, mainThrusterMaxEmission, speedNormalized);
            for (int i = 0; i < mainThrusters.Length; i++)
            {
                SetThrusterEmission(mainThrusters[i], mainEmission);
            }

            float steeringStrength = Mathf.Abs(_input != null ? _input.SteeringInput : 0f);
            float sideEmission = steeringStrength <= 0.05f
                ? 0f
                : Mathf.Lerp(sideThrusterMinEmission, sideThrusterMaxEmission, steeringStrength);

            for (int i = 0; i < sideThrusters.Length; i++)
            {
                SetThrusterEmission(sideThrusters[i], sideEmission);
            }

            SetThrusterEmission(boostThruster, isBoosting ? boostThrusterEmission : 0f);
        }

        public void PlayDamageEffect(DamageType type)
        {
            if (sparksEffect != null)
            {
                sparksEffect.Play();
            }

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashRoutine(GetDamageFlashColor(type), flashDuration));

            if (_screenShakeRoutine != null)
            {
                StopCoroutine(_screenShakeRoutine);
            }

            _screenShakeRoutine = StartCoroutine(ScreenShakeRoutine(screenShakeIntensity, screenShakeDuration));
        }

        public void UpdateDamageState(float healthNormalized)
        {
            bool showSmoke = healthNormalized < 0.5f;
            bool showFire = healthNormalized < 0.25f;

            SetLoopingParticleState(smokeEffect, showSmoke);
            SetLoopingParticleState(fireEffect, showFire);

            float damageSeverity = 1f - Mathf.Clamp01(healthNormalized);
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                Renderer modelRenderer = modelRenderers[i];
                if (modelRenderer == null)
                {
                    continue;
                }

                modelRenderer.GetPropertyBlock(_propertyBlock);
                Color darkened = Color.Lerp(_baseColors[i], Color.black, damageSeverity * 0.5f);
                _propertyBlock.SetColor("_Color", darkened);
                modelRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void PlayExplosion()
        {
            if (_isExploded)
            {
                return;
            }

            _isExploded = true;

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            SetAllParticlesStopped();
            SetRenderersVisible(false);
        }

        public void PlayRespawnEffect()
        {
            _isExploded = false;
            SetRenderersVisible(true);
            SetLoopingParticleState(smokeEffect, false);
            SetLoopingParticleState(fireEffect, false);

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashRoutine(new Color(0.4f, 0.8f, 1f), flashDuration * 1.75f));
        }

        private void HandleDamageTyped(float _, float __, DamageType type)
        {
            PlayDamageEffect(type);
        }

        private void SetThrusterEmission(ParticleSystem thruster, float rate)
        {
            if (thruster == null)
            {
                return;
            }

            ParticleSystem.EmissionModule emission = thruster.emission;
            emission.rateOverTime = rate;

            if (rate > 0.1f)
            {
                if (!thruster.isPlaying)
                {
                    thruster.Play();
                }
            }
            else if (thruster.isPlaying)
            {
                thruster.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private static void SetLoopingParticleState(ParticleSystem particleSystem, bool active)
        {
            if (particleSystem == null)
            {
                return;
            }

            if (active)
            {
                if (!particleSystem.isPlaying)
                {
                    particleSystem.Play();
                }
            }
            else if (particleSystem.isPlaying)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void SetAllParticlesStopped()
        {
            for (int i = 0; i < mainThrusters.Length; i++)
            {
                SetThrusterEmission(mainThrusters[i], 0f);
            }

            for (int i = 0; i < sideThrusters.Length; i++)
            {
                SetThrusterEmission(sideThrusters[i], 0f);
            }

            SetThrusterEmission(boostThruster, 0f);
            SetLoopingParticleState(smokeEffect, false);
            SetLoopingParticleState(fireEffect, false);

            if (sparksEffect != null)
            {
                sparksEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void SetRenderersVisible(bool visible)
        {
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                if (modelRenderers[i] != null)
                {
                    modelRenderers[i].enabled = visible;
                }
            }
        }

        private IEnumerator FlashRoutine(Color flashColor, float duration)
        {
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                Renderer modelRenderer = modelRenderers[i];
                if (modelRenderer == null)
                {
                    continue;
                }

                modelRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_Color", flashColor);
                modelRenderer.SetPropertyBlock(_propertyBlock);
            }

            yield return new WaitForSeconds(duration);

            if (_health != null)
            {
                UpdateDamageState(_health.HealthNormalized);
            }
        }

        private IEnumerator ScreenShakeRoutine(float intensity, float duration)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                yield break;
            }

            Transform cameraTransform = mainCamera.transform;
            Vector3 originalPosition = cameraTransform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cameraTransform.localPosition = originalPosition + (Random.insideUnitSphere * intensity);
                yield return null;
            }

            cameraTransform.localPosition = originalPosition;
        }

        private IEnumerator InvincibilityBlinkRoutine()
        {
            bool visible = true;
            while (_health != null && _health.IsInvincible)
            {
                visible = !visible;
                if (!_isExploded)
                {
                    SetRenderersVisible(visible);
                }

                yield return new WaitForSeconds(0.08f);
            }

            if (!_isExploded)
            {
                SetRenderersVisible(true);
            }

            _invincibilityRoutine = null;
        }

        private static Color GetDamageFlashColor(DamageType type)
        {
            return type switch
            {
                DamageType.Fire => new Color(1f, 0.45f, 0.1f),
                DamageType.Ice => new Color(0.35f, 0.75f, 1f),
                DamageType.Toxic => new Color(0.25f, 1f, 0.35f),
                DamageType.Electric => new Color(0.8f, 0.85f, 1f),
                _ => new Color(1f, 0.25f, 0.25f)
            };
        }
    }
}
