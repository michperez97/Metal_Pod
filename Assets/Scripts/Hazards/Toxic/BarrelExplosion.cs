using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetalPod.Hazards
{
    public class BarrelExplosion : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float explosionDamage = 40f;
        [SerializeField] private float explosionForce = 20f;
        [SerializeField] private float chainReactionDelay = 0.2f;

        [Header("Trigger")]
        [SerializeField] private float triggerSpeed = 15f;
        [SerializeField] private bool triggerOnAnyContact;

        [Header("Visuals")]
        [SerializeField] private GameObject barrelModel;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private AudioClip explosionSound;

        private bool _exploded;
        private Coroutine _delayedExplodeRoutine;

        private void OnCollisionEnter(Collision collision)
        {
            if (_exploded)
            {
                return;
            }

            if (!ShouldTrigger(collision))
            {
                return;
            }

            Explode();
        }

        public void ExplodeDelayed(float delay)
        {
            if (_exploded || _delayedExplodeRoutine != null)
            {
                return;
            }

            _delayedExplodeRoutine = StartCoroutine(DelayedExplodeRoutine(delay));
        }

        private IEnumerator DelayedExplodeRoutine(float delay)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delay));
            Explode();
        }

        private bool ShouldTrigger(Collision collision)
        {
            if (triggerOnAnyContact)
            {
                return true;
            }

            Rigidbody sourceRigidbody = collision.rigidbody != null
                ? collision.rigidbody
                : collision.collider.GetComponentInParent<Rigidbody>();

            float speed = sourceRigidbody != null
                ? sourceRigidbody.velocity.magnitude
                : collision.relativeVelocity.magnitude;

            return speed >= triggerSpeed;
        }

        private void Explode()
        {
            if (_exploded)
            {
                return;
            }

            _exploded = true;

            if (barrelModel != null)
            {
                barrelModel.SetActive(false);
            }

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }

            ApplyExplosionEffects();

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            Destroy(gameObject, 2f);
        }

        private void ApplyExplosionEffects()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, ~0, QueryTriggerInteraction.Collide);

            var damagedReceivers = new HashSet<IDamageReceiver>();
            var chainedBarrels = new HashSet<BarrelExplosion>();

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];

                IDamageReceiver receiver = hit.GetComponentInParent<IDamageReceiver>();
                if (receiver != null && !damagedReceivers.Contains(receiver))
                {
                    damagedReceivers.Add(receiver);
                    receiver.TakeDamage(explosionDamage, DamageType.Explosive);
                }

                Rigidbody rb = hit.attachedRigidbody != null
                    ? hit.attachedRigidbody
                    : hit.GetComponentInParent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
                }

                BarrelExplosion otherBarrel = hit.GetComponentInParent<BarrelExplosion>();
                if (otherBarrel != null && otherBarrel != this && !chainedBarrels.Contains(otherBarrel))
                {
                    chainedBarrels.Add(otherBarrel);
                    otherBarrel.ExplodeDelayed(chainReactionDelay);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.15f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
