using MetalPod.Hovercraft;
using UnityEngine;

namespace MetalPod.Hazards
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HovercraftHealth))]
    public class HovercraftDamageReceiverAdapter : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private HovercraftHealth health;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<HovercraftHealth>();
            }
        }

        public void TakeDamage(float amount)
        {
            TakeDamage(amount, DamageType.Physical);
        }

        public void TakeDamage(float amount, DamageType type)
        {
            if (health == null || amount <= 0f)
            {
                return;
            }

            health.TakeDamage(amount, type switch
            {
                DamageType.Fire => MetalPod.Shared.DamageType.Fire,
                DamageType.Ice => MetalPod.Shared.DamageType.Ice,
                DamageType.Toxic => MetalPod.Shared.DamageType.Toxic,
                DamageType.Electric => MetalPod.Shared.DamageType.Electric,
                DamageType.Explosive => MetalPod.Shared.DamageType.Explosive,
                _ => MetalPod.Shared.DamageType.Physical
            });
        }

        public void RestoreHealth(float amount)
        {
            if (health == null || amount <= 0f)
            {
                return;
            }

            health.RestoreHealth(amount);
        }

        public void RestoreShield(float amount)
        {
            if (health == null || amount <= 0f)
            {
                return;
            }

            health.RestoreShield(amount);
        }
    }
}
