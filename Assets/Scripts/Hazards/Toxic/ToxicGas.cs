using System.Collections;
using UnityEngine;

namespace MetalPod.Hazards
{
    public class ToxicGas : HazardBase
    {
        [Header("Gas Settings")]
        [SerializeField] private bool isPeriodic;
        [SerializeField] private float ventInterval = 4f;
        [SerializeField] private float ventDuration = 2f;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem gasCloud;
        [SerializeField] private Color fogColor = new Color(0.2f, 0.8f, 0.1f, 0.5f);
        [SerializeField] private AudioClip hissSound;

        private static int _activeFogUsers;
        private static Color _previousFogColor;

        private Coroutine _ventRoutine;
        private bool _isVenting;
        private int _playersInside;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Toxic;
        }

        private void OnEnable()
        {
            if (isPeriodic)
            {
                _ventRoutine = StartCoroutine(PeriodicVentRoutine());
            }
            else
            {
                SetVentState(true);
            }
        }

        private void OnDisable()
        {
            if (_ventRoutine != null)
            {
                StopCoroutine(_ventRoutine);
                _ventRoutine = null;
            }

            if (_playersInside > 0)
            {
                for (int i = 0; i < _playersInside; i++)
                {
                    ReleaseFogOverride();
                }
            }

            _playersInside = 0;
            SetVentState(false);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!IsPlayerCollider(other))
            {
                return;
            }

            _playersInside++;
            if (_isVenting)
            {
                ApplyFogOverride();
            }

            base.OnTriggerEnter(other);
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (!IsPlayerCollider(other))
            {
                return;
            }

            _playersInside = Mathf.Max(0, _playersInside - 1);
            ReleaseFogOverride();
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (!_isVenting)
            {
                return;
            }

            base.OnTriggerStay(other);
        }

        private IEnumerator PeriodicVentRoutine()
        {
            WaitForSeconds ventIntervalWait = new WaitForSeconds(Mathf.Max(0.1f, ventInterval));
            WaitForSeconds ventDurationWait = new WaitForSeconds(Mathf.Max(0.1f, ventDuration));

            SetVentState(false);

            while (enabled)
            {
                yield return ventIntervalWait;
                SetVentState(true);
                yield return ventDurationWait;
                SetVentState(false);
            }
        }

        private void SetVentState(bool venting)
        {
            bool shouldVent = venting && isActive;
            if (_isVenting == shouldVent)
            {
                return;
            }

            _isVenting = shouldVent;

            if (gasCloud != null)
            {
                if (_isVenting)
                {
                    gasCloud.Play();
                }
                else
                {
                    gasCloud.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            if (_isVenting)
            {
                if (hissSound != null)
                {
                    AudioSource.PlayClipAtPoint(hissSound, transform.position);
                }

                for (int i = 0; i < _playersInside; i++)
                {
                    ApplyFogOverride();
                }
            }
            else
            {
                for (int i = 0; i < _playersInside; i++)
                {
                    ReleaseFogOverride();
                }
            }
        }

        private void ApplyFogOverride()
        {
            if (_activeFogUsers == 0)
            {
                _previousFogColor = RenderSettings.fogColor;
            }

            _activeFogUsers++;
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
        }

        private void ReleaseFogOverride()
        {
            if (_activeFogUsers <= 0)
            {
                return;
            }

            _activeFogUsers--;
            if (_activeFogUsers == 0)
            {
                RenderSettings.fogColor = _previousFogColor;
            }
        }
    }
}
