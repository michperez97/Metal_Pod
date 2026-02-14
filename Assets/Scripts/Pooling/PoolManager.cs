using System.Collections.Generic;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Pooling
{
    /// <summary>
    /// Singleton registry for all object pools.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PoolConfigSO globalPoolConfig;

        private readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();
        private Transform _poolRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsurePoolRoot();

            if (globalPoolConfig != null)
            {
                InitializeFromConfig(globalPoolConfig);
            }
        }

        /// <summary>
        /// Initialize pools from a ScriptableObject config.
        /// </summary>
        public void InitializeFromConfig(PoolConfigSO config)
        {
            if (config == null || config.pools == null)
            {
                return;
            }

            for (int i = 0; i < config.pools.Length; i++)
            {
                PoolConfigSO.PoolDefinition def = config.pools[i];
                if (def == null || string.IsNullOrWhiteSpace(def.poolName) || def.prefab == null)
                {
                    Debug.LogWarning("[PoolManager] Skipping pool with missing name or prefab.");
                    continue;
                }

                if (_pools.ContainsKey(def.poolName))
                {
                    Debug.LogWarning($"[PoolManager] Pool '{def.poolName}' already exists, skipping.");
                    continue;
                }

                CreatePool(
                    def.poolName,
                    def.prefab,
                    def.prewarmCount,
                    def.maxSize,
                    def.autoExpand,
                    def.autoDespawnTime);
            }
        }

        /// <summary>
        /// Backward-compatible alias for config-based prewarming.
        /// </summary>
        public void PrewarmAll(PoolConfigSO config)
        {
            InitializeFromConfig(config);
        }

        /// <summary>
        /// Create a pool at runtime.
        /// </summary>
        public ObjectPool CreatePool(string poolName, GameObject prefab, int prewarmCount = 10,
            int maxSize = 50, bool autoExpand = true, float autoDespawnTime = 0f)
        {
            EnsurePoolRoot();

            if (_pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists.");
                return _pools[poolName];
            }

            if (prefab == null)
            {
                Debug.LogError($"[PoolManager] Cannot create pool '{poolName}' without a prefab.");
                return null;
            }

            int clampedPrewarm = Mathf.Max(0, prewarmCount);
            int clampedMaxSize = Mathf.Max(0, maxSize);
            if (clampedMaxSize > 0)
            {
                clampedPrewarm = Mathf.Min(clampedPrewarm, clampedMaxSize);
            }

            Transform parent = new GameObject($"Pool_{poolName}").transform;
            parent.SetParent(_poolRoot, false);

            ObjectPool pool = new ObjectPool(
                poolName,
                prefab,
                clampedPrewarm,
                clampedMaxSize,
                autoExpand,
                autoDespawnTime,
                parent);

            _pools[poolName] = pool;
            return pool;
        }

        /// <summary>
        /// Spawn an object from a named pool.
        /// </summary>
        public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(poolName, out ObjectPool pool))
            {
                Debug.LogError($"[PoolManager] No pool named '{poolName}'. Did you register it?");
                return null;
            }

            return pool.Get(position, rotation);
        }

        /// <summary>
        /// Spawn using identity rotation.
        /// </summary>
        public GameObject Spawn(string poolName, Vector3 position)
        {
            return Spawn(poolName, position, Quaternion.identity);
        }

        /// <summary>
        /// Return an object to its owning pool.
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            PooledObject pooled = obj.GetComponent<PooledObject>();
            if (pooled == null || string.IsNullOrWhiteSpace(pooled.PoolName))
            {
                Debug.LogWarning($"[PoolManager] Object '{obj.name}' has no pool metadata. Destroying.");
                Destroy(obj);
                return;
            }

            if (_pools.TryGetValue(pooled.PoolName, out ObjectPool pool))
            {
                pool.Return(obj);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Pool '{pooled.PoolName}' not found. Destroying object.");
                Destroy(obj);
            }
        }

        /// <summary>
        /// Retrieve a pool by name.
        /// </summary>
        public ObjectPool GetPool(string poolName)
        {
            _pools.TryGetValue(poolName, out ObjectPool pool);
            return pool;
        }

        /// <summary>
        /// Return all active instances across all pools.
        /// </summary>
        public void ReturnAllToPool()
        {
            foreach (ObjectPool pool in _pools.Values)
            {
                pool.ReturnAll();
            }
        }

        /// <summary>
        /// Destroy all instances in one pool and remove it.
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (_pools.TryGetValue(poolName, out ObjectPool pool))
            {
                pool.Clear();

                if (pool.PoolParent != null)
                {
                    Destroy(pool.PoolParent.gameObject);
                }

                _pools.Remove(poolName);
            }
        }

        /// <summary>
        /// Destroy all pools and all pooled instances.
        /// </summary>
        public void ClearAll()
        {
            foreach (ObjectPool pool in _pools.Values)
            {
                pool.Clear();
                if (pool.PoolParent != null)
                {
                    Destroy(pool.PoolParent.gameObject);
                }
            }

            _pools.Clear();
        }

        /// <summary>
        /// Collect runtime pool statistics for debug UIs.
        /// </summary>
        public Dictionary<string, (int active, int available, int total)> GetAllStats()
        {
            Dictionary<string, (int active, int available, int total)> stats =
                new Dictionary<string, (int active, int available, int total)>();

            foreach (KeyValuePair<string, ObjectPool> kvp in _pools)
            {
                stats[kvp.Key] = (kvp.Value.ActiveCount, kvp.Value.AvailableCount, kvp.Value.TotalCreated);
            }

            return stats;
        }

        private void EnsurePoolRoot()
        {
            if (_poolRoot != null)
            {
                return;
            }

            GameObject poolRootObject = new GameObject("_PoolRoot");
            _poolRoot = poolRootObject.transform;
            _poolRoot.SetParent(transform, false);
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            ClearAll();
            Instance = null;
        }
    }
}
