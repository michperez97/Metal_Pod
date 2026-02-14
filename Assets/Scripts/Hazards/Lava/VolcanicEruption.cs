using System.Collections;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards.Lava
{
    public class VolcanicEruption : HazardBase
    {
        [Header("Eruption Settings")]
        [SerializeField] private float eruptionInterval = 8f;
        [SerializeField] private float warningDuration = 2f;
        [SerializeField] private int debrisCount = 5;
        [SerializeField] private float debrisSpreadRadius = 10f;
        [SerializeField] private float debrisSpawnHeight = 18f;
        [SerializeField] private GameObject debrisPrefab;

        [Header("Warning")]
        [SerializeField] private ParticleSystem rumbleParticles;
        [SerializeField] private AudioClip warningRumble;
        [SerializeField] private AudioClip eruptionSound;

        private Coroutine _eruptionRoutine;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Fire;
        }

        private void OnEnable()
        {
            _eruptionRoutine = StartCoroutine(EruptionRoutine());
        }

        private void OnDisable()
        {
            if (_eruptionRoutine != null)
            {
                StopCoroutine(_eruptionRoutine);
                _eruptionRoutine = null;
            }

            if (rumbleParticles != null)
            {
                rumbleParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private IEnumerator EruptionRoutine()
        {
            WaitForSeconds intervalWait = new WaitForSeconds(Mathf.Max(0.1f, eruptionInterval));

            while (enabled)
            {
                yield return intervalWait;

                if (!isActive)
                {
                    continue;
                }

                if (rumbleParticles != null)
                {
                    rumbleParticles.Play();
                }

                if (warningRumble != null)
                {
                    AudioSource.PlayClipAtPoint(warningRumble, transform.position);
                }

                if (warningDuration > 0f)
                {
                    yield return new WaitForSeconds(warningDuration);
                }

                if (rumbleParticles != null)
                {
                    rumbleParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

                TriggerEruption();
            }
        }

        private void TriggerEruption()
        {
            if (eruptionSound != null)
            {
                AudioSource.PlayClipAtPoint(eruptionSound, transform.position);
            }

            if (debrisPrefab == null || debrisCount <= 0)
            {
                return;
            }

            for (int i = 0; i < debrisCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * debrisSpreadRadius;
                Vector3 spawnPosition = transform.position
                    + new Vector3(randomCircle.x, debrisSpawnHeight, randomCircle.y);

                GameObject debrisObject = Instantiate(debrisPrefab, spawnPosition, Random.rotation);
                if (debrisObject.TryGetComponent(out Rigidbody debrisRigidbody))
                {
                    Vector3 lateral = new Vector3(
                        Random.Range(-2f, 2f),
                        0f,
                        Random.Range(-2f, 2f));
                    debrisRigidbody.velocity = lateral;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, debrisSpreadRadius);
        }
    }
}
