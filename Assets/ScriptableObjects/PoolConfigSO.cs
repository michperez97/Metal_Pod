using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PoolConfig", menuName = "MetalPod/Pool Config")]
    public class PoolConfigSO : ScriptableObject
    {
        [System.Serializable]
        public class PoolDefinition
        {
            [Tooltip("Unique name for this pool (e.g., 'LavaDebris', 'IceShard')")]
            public string poolName;

            [Tooltip("Prefab to pool")]
            public GameObject prefab;

            [Tooltip("Number of instances to pre-create")]
            public int prewarmCount = 10;

            [Tooltip("Maximum pool size (0 = unlimited)")]
            public int maxSize = 50;

            [Tooltip("Auto-expand if pool is empty")]
            public bool autoExpand = true;

            [Tooltip("If > 0, pooled objects auto-return after this many seconds")]
            public float autoDespawnTime = 0f;
        }

        public PoolDefinition[] pools;
    }
}
