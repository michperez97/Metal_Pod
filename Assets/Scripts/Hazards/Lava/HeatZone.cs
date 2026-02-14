using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    public class HeatZone : HazardBase
    {
        [Header("Heat Settings")]
        [SerializeField] private float heatIntensity = 1f;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem heatHaze;

        private int _playersInside;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Fire;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!IsPlayerCollider(other))
            {
                return;
            }

            _playersInside++;
            if (_playersInside == 1)
            {
                SetHeatVisualState(true);
            }

            TryApplyDamage(other, damagePerHit);
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (!isActive || !IsPlayerCollider(other))
            {
                return;
            }

            float scaledDamage = damagePerSecond * Mathf.Max(0f, heatIntensity) * Time.deltaTime;
            TryApplyDamage(other, scaledDamage);
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (!IsPlayerCollider(other))
            {
                return;
            }

            _playersInside = Mathf.Max(0, _playersInside - 1);
            if (_playersInside == 0)
            {
                SetHeatVisualState(false);
            }
        }

        private void OnDisable()
        {
            _playersInside = 0;
            SetHeatVisualState(false);
        }

        private void SetHeatVisualState(bool enabledState)
        {
            Shader.SetGlobalFloat("_MetalPodHeatDistortion", enabledState ? Mathf.Clamp01(heatIntensity) : 0f);

            if (heatHaze == null)
            {
                return;
            }

            if (enabledState)
            {
                heatHaze.Play();
            }
            else
            {
                heatHaze.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
