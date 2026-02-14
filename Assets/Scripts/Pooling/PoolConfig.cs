namespace MetalPod.Pooling
{
    /// <summary>
    /// Recommended pool definitions for Metal Pod's highest-frequency spawned objects.
    /// Prefabs are assigned via PoolConfigSO assets.
    /// </summary>
    public static class PoolConfig
    {
        public readonly struct PoolDefaults
        {
            public PoolDefaults(string poolName, int prewarmCount, int maxSize, float autoDespawnTime)
            {
                PoolName = poolName;
                PrewarmCount = prewarmCount;
                MaxSize = maxSize;
                AutoDespawnTime = autoDespawnTime;
            }

            public string PoolName { get; }
            public int PrewarmCount { get; }
            public int MaxSize { get; }
            public float AutoDespawnTime { get; }
        }

        public static readonly PoolDefaults[] RecommendedPools =
        {
            new PoolDefaults("LavaDebris", 15, 30, 4f),
            new PoolDefaults("LavaGeyserBurst", 5, 10, 3f),
            new PoolDefaults("IceShard", 10, 20, 5f),
            new PoolDefaults("AvalancheRock", 8, 15, 6f),
            new PoolDefaults("AcidSplash", 5, 10, 2f),
            new PoolDefaults("BarrelFragment", 12, 25, 3f),
            new PoolDefaults("ToxicCloud", 8, 15, 5f),
            new PoolDefaults("DamageSpark", 5, 10, 1f),
            new PoolDefaults("Explosion", 3, 6, 2f),
            new PoolDefaults("Collectible_Bolt", 20, 40, 0f),
            new PoolDefaults("Collectible_Health", 5, 10, 0f),
            new PoolDefaults("Collectible_Shield", 5, 10, 0f),
            new PoolDefaults("DamageNumber", 8, 15, 1.5f),
            new PoolDefaults("CheckpointEffect", 3, 5, 2f)
        };
    }
}
