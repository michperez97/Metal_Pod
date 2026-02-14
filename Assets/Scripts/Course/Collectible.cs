using System.Collections;
using System;
using MetalPod.Hovercraft;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Course
{
    public enum CollectibleType
    {
        Currency,
        Health,
        Shield
    }

    [RequireComponent(typeof(Collider))]
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private CollectibleType type;
        [SerializeField] private int currencyAmount = 10;
        [SerializeField] private float restoreAmount = 10f;
        [SerializeField] private float spinSpeed = 90f;

        [Header("Magnet")]
        [SerializeField] private float magnetRange = 5f;
        [SerializeField] private float magnetSpeed = 12f;

        [Header("VFX")]
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private GameObject collectionBurstPrefab;
        [SerializeField] private float deactivateDelay = 0.05f;

        public event Action<Collectible, HovercraftController> OnCollected;

        public CollectibleType Type => type;
        public int CurrencyAmount => currencyAmount;
        public float RestoreAmount => restoreAmount;

        private HovercraftController _player;
        private Collider _trigger;
        private bool _collected;

        private void Awake()
        {
            _trigger = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _collected = false;

            if (_trigger != null)
            {
                _trigger.enabled = true;
            }

            if (glowParticles != null)
            {
                glowParticles.Play();
            }
        }

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Update()
        {
            if (_collected)
            {
                return;
            }

            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
            ApplyMagnetPull();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected)
            {
                return;
            }

            HovercraftController controller = other.GetComponentInParent<HovercraftController>();
            if (controller == null)
            {
                return;
            }

            _collected = true;
            OnCollected?.Invoke(this, controller);
            ApplyOptionalRestore(controller);
            PlayCollectionVfx();
            StartCoroutine(HideCollectibleRoutine());
        }

        private void ApplyMagnetPull()
        {
            HovercraftController player = ResolvePlayer();
            if (player == null)
            {
                return;
            }

            Vector3 toPlayer = player.transform.position - transform.position;
            if (toPlayer.sqrMagnitude > magnetRange * magnetRange)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.transform.position,
                magnetSpeed * Time.deltaTime);
        }

        private HovercraftController ResolvePlayer()
        {
            if (_player == null)
            {
                _player = FindObjectOfType<HovercraftController>();
            }

            return _player;
        }

        private void ApplyOptionalRestore(HovercraftController controller)
        {
            if (type == CollectibleType.Currency)
            {
                return;
            }

            IDamageReceiver receiver = controller.GetComponent<IDamageReceiver>();
            if (receiver == null)
            {
                receiver = controller.GetComponentInChildren<IDamageReceiver>();
            }

            if (receiver == null)
            {
                return;
            }

            if (type == CollectibleType.Health)
            {
                receiver.RestoreHealth(restoreAmount);
            }
            else if (type == CollectibleType.Shield)
            {
                receiver.RestoreShield(restoreAmount);
            }
        }

        private void PlayCollectionVfx()
        {
            if (collectionBurstPrefab != null)
            {
                Instantiate(collectionBurstPrefab, transform.position, Quaternion.identity);
            }

            if (glowParticles != null)
            {
                glowParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private IEnumerator HideCollectibleRoutine()
        {
            if (_trigger != null)
            {
                _trigger.enabled = false;
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }

            yield return new WaitForSeconds(Mathf.Max(0f, deactivateDelay));
            gameObject.SetActive(false);
        }
    }
}
