using System.Collections;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    [RequireComponent(typeof(HovercraftController))]
    [RequireComponent(typeof(HovercraftHealth))]
    public class HovercraftAudio : MonoBehaviour
    {
        [Header("Engine")]
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioClip engineLoop;
        [SerializeField] private float minPitch = 0.8f;
        [SerializeField] private float maxPitch = 1.5f;
        [SerializeField] private float minVolume = 0.3f;
        [SerializeField] private float maxVolume = 1f;
        [SerializeField] private float engineLerpSpeed = 6f;
        [SerializeField] private float boostPitchBonus = 0.2f;

        [Header("Actions")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip boostClip;
        [SerializeField] private AudioClip brakeClip;
        [SerializeField] private AudioClip collisionClip;
        [SerializeField] private AudioClip shieldHitClip;
        [SerializeField] private AudioClip healthHitClip;
        [SerializeField] private AudioClip shieldBreakClip;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip respawnClip;
        [SerializeField] private float collisionVelocityThreshold = 7f;

        private HovercraftController _controller;
        private HovercraftHealth _health;
        private HovercraftStats _stats;
        private Coroutine _engineFadeRoutine;
        private float _dynamicPitchOffset;

        private void Awake()
        {
            _controller = GetComponent<HovercraftController>();
            _health = GetComponent<HovercraftHealth>();
            _stats = GetComponent<HovercraftStats>();

            if (engineSource == null)
            {
                engineSource = gameObject.AddComponent<AudioSource>();
                engineSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.OnStateChanged += HandleStateChanged;
            }

            if (_health != null)
            {
                _health.OnDamage += HandleDamage;
                _health.OnShieldBreak += HandleShieldBreak;
                _health.OnDestroyed += HandleDestroyed;
                _health.OnRestored += HandleRespawned;
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnStateChanged -= HandleStateChanged;
            }

            if (_health != null)
            {
                _health.OnDamage -= HandleDamage;
                _health.OnShieldBreak -= HandleShieldBreak;
                _health.OnDestroyed -= HandleDestroyed;
                _health.OnRestored -= HandleRespawned;
            }
        }

        private void Start()
        {
            StartEngineLoop();
        }

        private void Update()
        {
            if (_controller == null || _stats == null || engineSource == null || _controller.CurrentState == HovercraftState.Destroyed)
            {
                return;
            }

            float maxSpeed = _stats.MaxSpeed;
            float speedNormalized = maxSpeed > 0f ? Mathf.Clamp01(_controller.CurrentSpeed / maxSpeed) : 0f;
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedNormalized) + _dynamicPitchOffset;
            float targetVolume = Mathf.Lerp(minVolume, maxVolume, speedNormalized);

            engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, Time.deltaTime * engineLerpSpeed);
            engineSource.volume = Mathf.Lerp(engineSource.volume, targetVolume, Time.deltaTime * engineLerpSpeed);
            _dynamicPitchOffset = Mathf.Lerp(_dynamicPitchOffset, 0f, Time.deltaTime * 3f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_controller == null || _controller.CurrentState == HovercraftState.Destroyed)
            {
                return;
            }

            if (collision.relativeVelocity.magnitude >= collisionVelocityThreshold)
            {
                PlayOneShot(collisionClip);
            }
        }

        private void HandleStateChanged(HovercraftState previous, HovercraftState current)
        {
            if (current == HovercraftState.Boosting)
            {
                PlayOneShot(boostClip);
                _dynamicPitchOffset = Mathf.Max(_dynamicPitchOffset, boostPitchBonus);
            }

            if (current == HovercraftState.Braking && previous != HovercraftState.Braking)
            {
                PlayOneShot(brakeClip);
            }
        }

        private void HandleDamage(float _, float healthDamage)
        {
            PlayOneShot(healthDamage > 0f ? healthHitClip : shieldHitClip);
        }

        private void HandleShieldBreak()
        {
            PlayOneShot(shieldBreakClip);
        }

        private void HandleDestroyed()
        {
            PlayOneShot(explosionClip);
            FadeOutEngineAndStop();
        }

        private void HandleRespawned()
        {
            PlayOneShot(respawnClip);
            StartEngineLoop();
        }

        private void StartEngineLoop()
        {
            if (engineSource == null)
            {
                return;
            }

            engineSource.clip = engineLoop;
            engineSource.loop = true;

            if (engineSource.clip != null && !engineSource.isPlaying)
            {
                engineSource.volume = minVolume;
                engineSource.pitch = minPitch;
                engineSource.Play();
            }
        }

        private void FadeOutEngineAndStop()
        {
            if (engineSource == null)
            {
                return;
            }

            if (_engineFadeRoutine != null)
            {
                StopCoroutine(_engineFadeRoutine);
            }

            _engineFadeRoutine = StartCoroutine(FadeEngineRoutine(0f, 0.25f, true));
        }

        private IEnumerator FadeEngineRoutine(float targetVolume, float duration, bool stopAfterFade)
        {
            float elapsed = 0f;
            float startVolume = engineSource.volume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                engineSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            engineSource.volume = targetVolume;
            if (stopAfterFade)
            {
                engineSource.Stop();
            }

            _engineFadeRoutine = null;
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
            else if (engineSource != null)
            {
                engineSource.PlayOneShot(clip);
            }
        }
    }
}
