using System.Collections;
using MetalPod.Hovercraft;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class FallingIcicle : HazardBase
    {
        private enum IcicleState
        {
            Intact,
            Cracking,
            Falling,
            Shattered
        }

        [Header("Icicle Settings")]
        [SerializeField] private float triggerRadius = 8f;
        [SerializeField] private float crackDuration = 1f;
        [SerializeField] private bool respawns = true;
        [SerializeField] private float respawnDelay = 10f;

        [Header("Visuals")]
        [SerializeField] private Renderer icicleRenderer;
        [SerializeField] private ParticleSystem crackParticles;
        [SerializeField] private ParticleSystem shatterEffect;
        [SerializeField] private AudioClip crackSound;
        [SerializeField] private AudioClip shatterSound;

        private Rigidbody _rb;
        private Collider _collider;
        private Transform _player;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private IcicleState _state;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Physical;

            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _startPosition = transform.position;
            _startRotation = transform.rotation;

            _rb.isKinematic = true;
            _rb.useGravity = false;
            _state = IcicleState.Intact;
        }

        private void Start()
        {
            HovercraftController playerController = FindObjectOfType<HovercraftController>();
            if (playerController != null)
            {
                _player = playerController.transform;
            }
        }

        private void Update()
        {
            if (!isActive || _state != IcicleState.Intact || _player == null)
            {
                return;
            }

            float distance = Vector3.Distance(_player.position, transform.position);
            if (distance <= triggerRadius)
            {
                StartCoroutine(CrackAndDropRoutine());
            }
        }

        private IEnumerator CrackAndDropRoutine()
        {
            if (_state != IcicleState.Intact)
            {
                yield break;
            }

            _state = IcicleState.Cracking;

            if (crackParticles != null)
            {
                crackParticles.Play();
            }

            if (crackSound != null)
            {
                AudioSource.PlayClipAtPoint(crackSound, transform.position);
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, crackDuration));

            _state = IcicleState.Falling;
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_state != IcicleState.Falling)
            {
                return;
            }

            TryApplyDamage(collision.collider, damagePerHit);
            Shatter();
        }

        private void Shatter()
        {
            _state = IcicleState.Shattered;
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _collider.enabled = false;

            if (icicleRenderer != null)
            {
                icicleRenderer.enabled = false;
            }

            if (shatterEffect != null)
            {
                shatterEffect.Play();
            }

            if (shatterSound != null)
            {
                AudioSource.PlayClipAtPoint(shatterSound, transform.position);
            }

            if (respawns)
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, respawnDelay));

            transform.SetPositionAndRotation(_startPosition, _startRotation);
            _state = IcicleState.Intact;
            _collider.enabled = true;

            if (icicleRenderer != null)
            {
                icicleRenderer.enabled = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
