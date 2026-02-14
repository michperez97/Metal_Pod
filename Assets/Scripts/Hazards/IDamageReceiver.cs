namespace MetalPod.Hazards
{
    public interface IDamageReceiver
    {
        void TakeDamage(float amount);
        void TakeDamage(float amount, DamageType type);
        void RestoreHealth(float amount);
        void RestoreShield(float amount);
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Toxic,
        Electric,
        Explosive
    }
}
