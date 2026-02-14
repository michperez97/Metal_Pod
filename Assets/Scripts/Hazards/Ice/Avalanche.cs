using System.Collections;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    public class Avalanche : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 18f;
        [SerializeField] private float startDelay = 3f;
        [SerializeField] private float duration = 15f;

        [Header("Kill Zone")]
        [SerializeField] private BoxCollider killZone;
        [SerializeField] private float instantKillDamage = 9999f;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem avalancheWall;
        [SerializeField] private AudioClip rumbleLoop;

        private bool _sequenceRunning;
        private bool _active;

        private void OnTriggerEnter(Collider other)
        {
            if (_sequenceRunning)
            {
                return;
            }

            if (other.GetComponentInParent<IDamageReceiver>() == null)
            {
                return;
            }

            StartCoroutine(AvalancheRoutine());
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            transform.position += transform.forward * (speed * Time.deltaTime);
            EvaluateKillZone();
        }

        private IEnumerator AvalancheRoutine()
        {
            _sequenceRunning = true;

            if (startDelay > 0f)
            {
                yield return new WaitForSeconds(startDelay);
            }

            _active = true;

            if (avalancheWall != null)
            {
                avalancheWall.Play();
            }

            if (rumbleLoop != null)
            {
                AudioSource.PlayClipAtPoint(rumbleLoop, transform.position);
            }

            yield return new WaitForSeconds(Mathf.Max(0.1f, duration));
            StopAvalanche();
        }

        private void EvaluateKillZone()
        {
            if (killZone == null)
            {
                return;
            }

            Vector3 center = killZone.bounds.center;
            Vector3 halfExtents = killZone.bounds.extents;

            Collider[] hits = Physics.OverlapBox(
                center,
                halfExtents,
                killZone.transform.rotation,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                IDamageReceiver receiver = hits[i].GetComponentInParent<IDamageReceiver>();
                if (receiver == null)
                {
                    continue;
                }

                receiver.TakeDamage(instantKillDamage, DamageType.Ice);
                StopAvalanche();
                return;
            }
        }

        private void StopAvalanche()
        {
            _active = false;

            if (avalancheWall != null)
            {
                avalancheWall.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (killZone == null)
            {
                return;
            }

            Gizmos.color = new Color(0.7f, 0.9f, 1f, 0.35f);
            Gizmos.matrix = Matrix4x4.TRS(killZone.bounds.center, killZone.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, killZone.bounds.size);
        }
    }
}
