using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards.Ice
{
    public class BlizzardZone : HazardBase
    {
        [Header("Blizzard Settings")]
        [SerializeField] private float windForce = 5f;
        [SerializeField] private Vector3 windDirection = Vector3.right;
        [SerializeField] private float visibilityRange = 15f;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem snowParticles;
        [SerializeField] private AudioClip windLoop;

        private static int _activeZoneCount;
        private static bool _cachedFogEnabled;
        private static float _cachedFogStart;
        private static float _cachedFogEnd;

        private bool _playerInside;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Ice;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!IsPlayerCollider(other))
            {
                return;
            }

            if (!_playerInside)
            {
                _playerInside = true;
                ApplyFog();

                if (windLoop != null)
                {
                    AudioSource.PlayClipAtPoint(windLoop, transform.position);
                }
            }

            if (snowParticles != null && !snowParticles.isPlaying)
            {
                snowParticles.Play();
            }
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (!isActive)
            {
                return;
            }

            if (TryGetPlayerRigidbody(other, out Rigidbody playerRigidbody))
            {
                Vector3 worldDirection = transform.TransformDirection(windDirection.normalized);
                playerRigidbody.AddForce(worldDirection * windForce, ForceMode.Acceleration);
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (!IsPlayerCollider(other))
            {
                return;
            }

            _playerInside = false;
            ReleaseFog();

            if (snowParticles != null)
            {
                snowParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void OnDisable()
        {
            if (_playerInside)
            {
                _playerInside = false;
                ReleaseFog();
            }

            if (snowParticles != null)
            {
                snowParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void ApplyFog()
        {
            if (_activeZoneCount == 0)
            {
                _cachedFogEnabled = RenderSettings.fog;
                _cachedFogStart = RenderSettings.fogStartDistance;
                _cachedFogEnd = RenderSettings.fogEndDistance;
            }

            _activeZoneCount++;
            RenderSettings.fog = true;
            RenderSettings.fogEndDistance = visibilityRange;
            RenderSettings.fogStartDistance = Mathf.Max(0f, visibilityRange * 0.4f);
        }

        private void ReleaseFog()
        {
            if (_activeZoneCount <= 0)
            {
                return;
            }

            _activeZoneCount--;
            if (_activeZoneCount == 0)
            {
                RenderSettings.fog = _cachedFogEnabled;
                RenderSettings.fogStartDistance = _cachedFogStart;
                RenderSettings.fogEndDistance = _cachedFogEnd;
            }
        }
    }
}
