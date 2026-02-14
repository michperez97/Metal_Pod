using UnityEngine;

namespace MetalPod.Hazards
{
    public class LavaFlow : HazardBase
    {
        [Header("Flow")]
        [SerializeField] private bool useIntermittentFlow;
        [SerializeField] private float activeDuration = 2f;
        [SerializeField] private float inactiveDuration = 2f;

        [Header("Visual")]
        [SerializeField] private Renderer lavaRenderer;
        [SerializeField] private string textureOffsetProperty = "_MainTex";
        [SerializeField] private Vector2 uvScrollSpeed = new Vector2(0.2f, 0f);

        private float _cycleTimer;
        private Vector2 _currentOffset;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Fire;
        }

        private void Update()
        {
            AnimateSurface();
            UpdateFlowCycle();
        }

        private void AnimateSurface()
        {
            if (lavaRenderer == null || lavaRenderer.material == null)
            {
                return;
            }

            _currentOffset += uvScrollSpeed * Time.deltaTime;
            lavaRenderer.material.SetTextureOffset(textureOffsetProperty, _currentOffset);
        }

        private void UpdateFlowCycle()
        {
            if (!useIntermittentFlow)
            {
                return;
            }

            _cycleTimer += Time.deltaTime;
            float duration = isActive ? activeDuration : inactiveDuration;
            if (_cycleTimer >= Mathf.Max(0.01f, duration))
            {
                _cycleTimer = 0f;
                isActive = !isActive;
            }
        }
    }
}
