namespace MetalPod.Shared
{
    public interface IHovercraftData
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentShield { get; }
        float MaxShield { get; }
        float CurrentSpeed { get; }
        float MaxSpeed { get; }
        float HealthNormalized { get; }
        float ShieldNormalized { get; }
        float BoostCooldownNormalized { get; }
        bool IsBoosting { get; }
        bool IsDestroyed { get; }
    }
}
