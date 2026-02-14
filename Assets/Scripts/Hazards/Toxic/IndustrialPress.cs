using System.Collections;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards.Toxic
{
    public class IndustrialPress : MonoBehaviour
    {
        [Header("Press Settings")]
        [SerializeField] private float openDuration = 3f;
        [SerializeField] private float closeDuration = 1f;
        [SerializeField] private float closeSpeed = 10f;
        [SerializeField] private float openSpeed = 2f;

        [Header("Components")]
        [SerializeField] private Transform pressHead;
        [SerializeField] private Vector3 openPosition;
        [SerializeField] private Vector3 closedPosition;
        [SerializeField] private BoxCollider crushZone;

        [Header("Audio/Visual")]
        [SerializeField] private AudioClip slamSound;
        [SerializeField] private AudioClip hydraulicSound;
        [SerializeField] private ParticleSystem steamEffect;
        [SerializeField] private float crushDamage = 9999f;

        private Coroutine _cycleRoutine;

        private void OnEnable()
        {
            if (pressHead != null)
            {
                pressHead.localPosition = openPosition;
            }

            _cycleRoutine = StartCoroutine(CycleRoutine());
        }

        private void OnDisable()
        {
            if (_cycleRoutine != null)
            {
                StopCoroutine(_cycleRoutine);
                _cycleRoutine = null;
            }
        }

        private IEnumerator CycleRoutine()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(Mathf.Max(0.1f, openDuration));
                yield return MoveHead(closedPosition, closeSpeed, true);
                yield return new WaitForSeconds(Mathf.Max(0.05f, closeDuration));
                yield return MoveHead(openPosition, openSpeed, false);
            }
        }

        private IEnumerator MoveHead(Vector3 targetPosition, float speed, bool crushing)
        {
            if (pressHead == null)
            {
                yield break;
            }

            if (hydraulicSound != null)
            {
                AudioSource.PlayClipAtPoint(hydraulicSound, pressHead.position);
            }

            while ((pressHead.localPosition - targetPosition).sqrMagnitude > 0.0001f)
            {
                pressHead.localPosition = Vector3.MoveTowards(
                    pressHead.localPosition,
                    targetPosition,
                    Mathf.Max(0.05f, speed) * Time.deltaTime);

                if (crushing)
                {
                    EvaluateCrushZone();
                }

                yield return null;
            }

            if (crushing)
            {
                if (slamSound != null)
                {
                    AudioSource.PlayClipAtPoint(slamSound, pressHead.position);
                }

                if (steamEffect != null)
                {
                    steamEffect.Play();
                }

                EvaluateCrushZone();
            }
        }

        private void EvaluateCrushZone()
        {
            if (crushZone == null)
            {
                return;
            }

            Collider[] hits = Physics.OverlapBox(
                crushZone.bounds.center,
                crushZone.bounds.extents,
                crushZone.transform.rotation,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                IDamageReceiver receiver = hits[i].GetComponentInParent<IDamageReceiver>();
                if (receiver != null)
                {
                    receiver.TakeDamage(crushDamage, DamageType.Physical);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (crushZone == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Gizmos.matrix = Matrix4x4.TRS(crushZone.bounds.center, crushZone.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, crushZone.bounds.size);
        }
    }
}
