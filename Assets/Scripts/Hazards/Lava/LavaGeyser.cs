using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards.Lava
{
    public class LavaGeyser : HazardBase
    {
        private enum GeyserState
        {
            Dormant,
            Bubbling,
            Active
        }

        [Header("Geyser Settings")]
        [SerializeField] private float cycleTime = 5f;
        [SerializeField] private float activeDuration = 1.5f;
        [SerializeField] private float upwardForce = 30f;
        [SerializeField] private float geyserHeight = 8f;

        [Header("Warning")]
        [SerializeField] private ParticleSystem bubblingEffect;
        [SerializeField] private float bubbleDuration = 1.5f;

        [Header("Active")]
        [SerializeField] private ParticleSystem geyserEffect;
        [SerializeField] private AudioClip eruptSound;

        private GeyserState _state;
        private float _stateTimer;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Fire;
        }

        private void OnEnable()
        {
            EnterDormant();
        }

        private void Update()
        {
            if (!isActive)
            {
                EnterDormant();
                return;
            }

            _stateTimer -= Time.deltaTime;
            if (_stateTimer > 0f)
            {
                return;
            }

            switch (_state)
            {
                case GeyserState.Dormant:
                    EnterBubbling();
                    break;
                case GeyserState.Bubbling:
                    EnterActive();
                    break;
                default:
                    EnterDormant();
                    break;
            }
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (_state != GeyserState.Active)
            {
                return;
            }

            base.OnTriggerStay(other);

            if (TryGetPlayerRigidbody(other, out Rigidbody playerRigidbody))
            {
                playerRigidbody.AddForce(transform.up * upwardForce, ForceMode.Acceleration);
            }
        }

        private void EnterDormant()
        {
            _state = GeyserState.Dormant;
            float dormantDuration = Mathf.Max(0.1f, cycleTime - bubbleDuration - activeDuration);
            _stateTimer = dormantDuration;

            if (bubblingEffect != null)
            {
                bubblingEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (geyserEffect != null)
            {
                geyserEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void EnterBubbling()
        {
            _state = GeyserState.Bubbling;
            _stateTimer = Mathf.Max(0.1f, bubbleDuration);

            if (bubblingEffect != null)
            {
                bubblingEffect.Play();
            }
        }

        private void EnterActive()
        {
            _state = GeyserState.Active;
            _stateTimer = Mathf.Max(0.1f, activeDuration);

            if (bubblingEffect != null)
            {
                bubblingEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (geyserEffect != null)
            {
                geyserEffect.Play();
            }

            if (eruptSound != null)
            {
                AudioSource.PlayClipAtPoint(eruptSound, transform.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.35f);
            Gizmos.DrawWireCube(transform.position + (Vector3.up * (geyserHeight * 0.5f)), new Vector3(2f, geyserHeight, 2f));
        }
    }
}
