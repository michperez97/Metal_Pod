using UnityEngine;

namespace MetalPod.Hazards
{
    public class AcidPool : HazardBase
    {
        [Header("Pool Settings")]
        [SerializeField] private bool hasRisingLevel;
        [SerializeField] private float minLevel;
        [SerializeField] private float maxLevel = 2f;
        [SerializeField] private float riseSpeed = 0.5f;

        [Header("Visuals")]
        [SerializeField] private Renderer acidSurface;
        [SerializeField] private ParticleSystem bubbles;
        [SerializeField] private ParticleSystem splashEffect;
        [SerializeField] private AudioClip sizzleSound;
        [SerializeField] private float uvScrollSpeed = 0.15f;

        private float _baseSurfaceY;
        private Vector2 _surfaceUvOffset;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Toxic;
        }

        private void Start()
        {
            if (acidSurface != null)
            {
                _baseSurfaceY = acidSurface.transform.localPosition.y;
            }

            if (bubbles != null)
            {
                bubbles.Play();
            }
        }

        private void Update()
        {
            UpdateRisingLevel();
            AnimateSurface();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!IsPlayerCollider(other))
            {
                return;
            }

            base.OnTriggerEnter(other);

            if (splashEffect != null)
            {
                splashEffect.Play();
            }

            if (sizzleSound != null)
            {
                AudioSource.PlayClipAtPoint(sizzleSound, transform.position);
            }
        }

        private void UpdateRisingLevel()
        {
            if (!hasRisingLevel || acidSurface == null)
            {
                return;
            }

            float cycleValue = Mathf.PingPong(Time.time * Mathf.Max(0.01f, riseSpeed), 1f);
            float dynamicOffset = Mathf.Lerp(minLevel, maxLevel, cycleValue);
            Vector3 localPosition = acidSurface.transform.localPosition;
            localPosition.y = _baseSurfaceY + dynamicOffset;
            acidSurface.transform.localPosition = localPosition;
        }

        private void AnimateSurface()
        {
            if (acidSurface == null || acidSurface.material == null)
            {
                return;
            }

            _surfaceUvOffset.x += uvScrollSpeed * Time.deltaTime;
            acidSurface.material.SetTextureOffset("_MainTex", _surfaceUvOffset);
        }
    }
}
