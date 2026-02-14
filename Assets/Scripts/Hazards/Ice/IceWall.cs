using MetalPod.Hovercraft;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    [RequireComponent(typeof(Collider))]
    public class IceWall : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float breakSpeedThreshold = 25f;
        [SerializeField] private bool requiresBoost = true;
        [SerializeField] private float bounceForce = 12f;
        [SerializeField] private float bounceDamage = 5f;

        [Header("Visuals")]
        [SerializeField] private GameObject intactModel;
        [SerializeField] private GameObject shatteredPrefab;
        [SerializeField] private ParticleSystem shatterEffect;
        [SerializeField] private AudioClip shatterSound;
        [SerializeField] private AudioClip bounceSound;

        private bool _broken;

        private void OnCollisionEnter(Collision collision)
        {
            if (_broken)
            {
                return;
            }

            HovercraftController controller = collision.collider.GetComponentInParent<HovercraftController>();
            if (controller == null)
            {
                return;
            }

            Rigidbody playerRigidbody = controller.GetComponent<Rigidbody>();
            float playerSpeed = playerRigidbody != null
                ? playerRigidbody.velocity.magnitude
                : collision.relativeVelocity.magnitude;

            bool canBreakBySpeed = playerSpeed >= breakSpeedThreshold;
            bool isBoosting = controller.CurrentState == HovercraftState.Boosting;
            bool shouldBreak = requiresBoost ? (isBoosting || canBreakBySpeed) : canBreakBySpeed;

            if (shouldBreak)
            {
                BreakWall();
                return;
            }

            BouncePlayer(collision, playerRigidbody);
            TryApplyBounceDamage(collision.collider);
        }

        private void BreakWall()
        {
            _broken = true;

            if (intactModel != null)
            {
                intactModel.SetActive(false);
            }

            if (shatteredPrefab != null)
            {
                Instantiate(shatteredPrefab, transform.position, transform.rotation);
            }

            if (shatterEffect != null)
            {
                shatterEffect.Play();
            }

            if (shatterSound != null)
            {
                AudioSource.PlayClipAtPoint(shatterSound, transform.position);
            }

            Collider wallCollider = GetComponent<Collider>();
            if (wallCollider != null)
            {
                wallCollider.enabled = false;
            }
        }

        private void BouncePlayer(Collision collision, Rigidbody playerRigidbody)
        {
            if (playerRigidbody == null)
            {
                return;
            }

            Vector3 normal = collision.contactCount > 0
                ? collision.GetContact(0).normal
                : -transform.forward;

            Vector3 bounceDirection = normal.normalized;
            playerRigidbody.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

            if (bounceSound != null)
            {
                AudioSource.PlayClipAtPoint(bounceSound, transform.position);
            }
        }

        private void TryApplyBounceDamage(Collider collider)
        {
            IDamageReceiver receiver = collider.GetComponentInParent<IDamageReceiver>();
            if (receiver == null || bounceDamage <= 0f)
            {
                return;
            }

            receiver.TakeDamage(bounceDamage, DamageType.Physical);
        }
    }
}
