namespace MetalPod.Pooling
{
    /// <summary>
    /// Implement on components that require custom reset logic when pooled objects spawn/despawn.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called when the object is taken from the pool and activated.</summary>
        void OnSpawned();

        /// <summary>Called when the object is returned to the pool and deactivated.</summary>
        void OnDespawned();
    }
}
