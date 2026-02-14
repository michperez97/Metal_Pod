using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    public abstract class HazardBase : MonoBehaviour
    {
        [Header("Hazard Data")]
        [SerializeField] protected HazardDataSO hazardData;
        [SerializeField] protected float damagePerSecond = 5f;
        [SerializeField] protected float damagePerHit = 10f;
        [SerializeField] protected DamageType damageType = DamageType.Physical;
        [SerializeField] protected bool isActive = true;

        public DamageType HazardDamageType => damageType;
        public bool IsActive => isActive;

        protected virtual void Awake()
        {
            if (hazardData == null)
            {
                return;
            }

            damagePerSecond = hazardData.damagePerSecond;
            damagePerHit = hazardData.damagePerHit;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!isActive || damagePerHit <= 0f)
            {
                return;
            }

            TryApplyDamage(other, damagePerHit);
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (!isActive || damagePerSecond <= 0f)
            {
                return;
            }

            TryApplyDamage(other, damagePerSecond * Time.deltaTime);
        }

        protected virtual void OnTriggerExit(Collider _)
        {
        }

        public virtual void Activate()
        {
            isActive = true;
        }

        public virtual void Deactivate()
        {
            isActive = false;
        }

        protected bool IsPlayerCollider(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.GetComponentInParent<IDamageReceiver>() != null)
            {
                return true;
            }

            return other.CompareTag(GameConstants.TAG_PLAYER);
        }

        protected bool TryApplyDamage(Collider other, float amount)
        {
            if (amount <= 0f)
            {
                return false;
            }

            if (!TryGetDamageReceiver(other, out IDamageReceiver receiver))
            {
                return false;
            }

            receiver.TakeDamage(amount, damageType);
            return true;
        }

        protected bool TryGetDamageReceiver(Collider other, out IDamageReceiver receiver)
        {
            receiver = null;

            if (other == null)
            {
                return false;
            }

            receiver = other.GetComponentInParent<IDamageReceiver>();
            return receiver != null;
        }

        protected bool TryGetPlayerRigidbody(Collider other, out Rigidbody rigidbody)
        {
            rigidbody = null;

            if (!IsPlayerCollider(other))
            {
                return false;
            }

            rigidbody = other.attachedRigidbody;
            if (rigidbody == null)
            {
                rigidbody = other.GetComponentInParent<Rigidbody>();
            }

            return rigidbody != null;
        }
    }
}
