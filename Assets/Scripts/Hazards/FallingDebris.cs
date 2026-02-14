using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    [RequireComponent(typeof(Rigidbody))]
    public class FallingDebris : HazardBase
    {
        [SerializeField] private bool destroyOnImpact = true;
        [SerializeField] private float lifetimeSeconds = 8f;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Physical;
        }

        private void Start()
        {
            if (lifetimeSeconds > 0f)
            {
                Destroy(gameObject, lifetimeSeconds);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActive)
            {
                return;
            }

            TryApplyDamage(collision.collider, damagePerHit);

            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
    }
}
