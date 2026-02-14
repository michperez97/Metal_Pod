# Codex Task 12: Object Pooling & Performance System

> **Goal**: Create a generic object pooling system and integrate it with the game's most allocation-heavy systems. Critical for maintaining 60fps on iOS devices.

---

## Context

Metal Pod targets iOS (iPhone 6s through iPhone 16 Pro Max). There is **no object pooling** anywhere in the codebase. During races, the game continuously instantiates and destroys:
- Hazard projectiles (lava debris, icicles, barrel fragments, toxic gas clouds)
- Particle systems (thruster effects, damage sparks, collectible pickups)
- Collectible items
- Damage numbers / UI popups
- Audio source instances (AudioManager uses pooling for AudioSources but not for game objects)

Without pooling, GC spikes will cause frame drops on mobile. This task creates a reusable pooling framework and specific pool configurations for Metal Pod's hottest allocation paths.

**Read these files**:
- `Assets/Scripts/Hazards/HazardBase.cs` — Base class for all hazards
- `Assets/Scripts/Hazards/Lava/VolcanicEruption.cs` — Spawns debris projectiles
- `Assets/Scripts/Hazards/Lava/LavaGeyser.cs` — Spawns burst effects
- `Assets/Scripts/Hazards/Ice/FallingIcicle.cs` — Spawns falling icicles
- `Assets/Scripts/Hazards/Ice/Avalanche.cs` — Spawns rolling debris
- `Assets/Scripts/Hazards/Toxic/BarrelExplosion.cs` — Spawns explosion + fragments
- `Assets/Scripts/Hazards/Toxic/AcidPool.cs` — Spawns acid splash effects
- `Assets/Scripts/Course/Collectible.cs` — Collectible pickup with magnet pull
- `Assets/Scripts/Core/AudioManager.cs` — Existing audio source pooling pattern
- `Assets/Scripts/Hovercraft/HovercraftVisuals.cs` — `explosionPrefab` instantiation

---

## Files to Create

```
Assets/Scripts/Pooling/
├── ObjectPool.cs              # Generic pool for any prefab
├── PoolManager.cs             # Singleton managing all named pools
├── PooledObject.cs            # Component attached to pooled instances
├── PoolPrewarmer.cs           # Pre-instantiates pools at scene load
└── PoolConfig.cs              # ScriptableObject for pool definitions

Assets/ScriptableObjects/
└── PoolConfigSO.cs            # ScriptableObject definition

Assets/Scripts/Editor/
└── PoolDebugWindow.cs         # Editor window showing pool statistics
```

**DO NOT modify** any existing files. The pool system should be self-contained. Integration with hazards/collectibles will be done in a future wiring pass.

---

## Architecture

```
PoolManager (Singleton, DontDestroyOnLoad)
  ├── Dictionary<string, ObjectPool> pools
  ├── GetPool(string poolName) → ObjectPool
  ├── Spawn(string poolName, Vector3 pos, Quaternion rot) → GameObject
  ├── Despawn(GameObject obj) — returns to pool
  └── PrewarmAll(PoolConfigSO config)

ObjectPool
  ├── Stack<GameObject> available
  ├── HashSet<GameObject> active
  ├── Prefab reference
  ├── Grow(int count) — expand pool
  ├── Get() → GameObject
  ├── Return(GameObject obj)
  ├── Auto-expand if empty (with warning)
  └── Stats: TotalCreated, ActiveCount, AvailableCount

PooledObject (attached to each instance)
  ├── OnSpawn() — called when taken from pool (resets state)
  ├── OnDespawn() — called when returned to pool
  ├── AutoDespawnAfter(float seconds) — timed return
  ├── PoolName — which pool this belongs to
  └── IPoolable interface — optional, for custom reset logic

PoolPrewarmer
  ├── Reads PoolConfigSO
  ├── Creates pools at scene start
  ├── Spreads instantiation across frames (no single-frame spike)
  └── Progress callback for loading screen
```

---

## Detailed Specifications

### PoolConfigSO.cs

```csharp
// ScriptableObject defining all pools for a scene or global use.
// Create via: Assets > Create > MetalPod > Pool Config

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
```

### ObjectPool.cs

```csharp
// Generic object pool for GameObjects.
// Thread-safe-ish for Unity (main thread only but safe against re-entrant calls).

using System.Collections.Generic;
using UnityEngine;

namespace MetalPod.Pooling
{
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _poolParent;
        private readonly Stack<GameObject> _available;
        private readonly HashSet<GameObject> _active;
        private readonly int _maxSize;
        private readonly bool _autoExpand;
        private readonly float _autoDespawnTime;
        private readonly string _poolName;

        private int _totalCreated;

        public string PoolName => _poolName;
        public int ActiveCount => _active.Count;
        public int AvailableCount => _available.Count;
        public int TotalCreated => _totalCreated;
        public GameObject Prefab => _prefab;

        public ObjectPool(string poolName, GameObject prefab, int initialSize, int maxSize,
            bool autoExpand, float autoDespawnTime, Transform parent)
        {
            _poolName = poolName;
            _prefab = prefab;
            _maxSize = maxSize;
            _autoExpand = autoExpand;
            _autoDespawnTime = autoDespawnTime;
            _poolParent = parent;

            _available = new Stack<GameObject>(initialSize);
            _active = new HashSet<GameObject>();

            Grow(initialSize);
        }

        public void Grow(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_maxSize > 0 && _totalCreated >= _maxSize) break;

                GameObject obj = Object.Instantiate(_prefab, _poolParent);
                obj.SetActive(false);
                obj.name = $"{_poolName}_{_totalCreated}";

                // Attach PooledObject component if not already present
                var pooled = obj.GetComponent<PooledObject>();
                if (pooled == null)
                    pooled = obj.AddComponent<PooledObject>();
                pooled.Initialize(_poolName);

                _available.Push(obj);
                _totalCreated++;
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj;

            if (_available.Count > 0)
            {
                obj = _available.Pop();
            }
            else if (_autoExpand)
            {
                Debug.LogWarning($"[ObjectPool] Pool '{_poolName}' empty, auto-expanding. " +
                                 $"Consider increasing prewarm count. (Total: {_totalCreated})");
                Grow(Mathf.Max(1, _totalCreated / 4)); // Grow by 25%

                if (_available.Count == 0)
                {
                    Debug.LogError($"[ObjectPool] Pool '{_poolName}' cannot expand (max: {_maxSize})");
                    return null;
                }
                obj = _available.Pop();
            }
            else
            {
                Debug.LogWarning($"[ObjectPool] Pool '{_poolName}' exhausted (max: {_maxSize})");
                return null;
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            _active.Add(obj);

            // Notify IPoolable components
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnSpawned();

            var pooled = obj.GetComponent<PooledObject>();
            pooled?.NotifySpawned();

            // Auto-despawn timer
            if (_autoDespawnTime > 0f && pooled != null)
            {
                pooled.AutoDespawnAfter(_autoDespawnTime);
            }

            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;
            if (!_active.Remove(obj))
            {
                // Object wasn't tracked — might be a double-return
                Debug.LogWarning($"[ObjectPool] Tried to return untracked object to pool '{_poolName}'");
                return;
            }

            // Notify IPoolable components
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnDespawned();

            obj.SetActive(false);
            obj.transform.SetParent(_poolParent);
            _available.Push(obj);
        }

        public void ReturnAll()
        {
            // Copy to avoid modification during iteration
            var activeList = new List<GameObject>(_active);
            foreach (var obj in activeList)
            {
                Return(obj);
            }
        }

        public void Clear()
        {
            ReturnAll();
            while (_available.Count > 0)
            {
                var obj = _available.Pop();
                if (obj != null)
                    Object.Destroy(obj);
            }
            _totalCreated = 0;
        }
    }
}
```

### IPoolable.cs

Also create `Assets/Scripts/Pooling/IPoolable.cs`:

```csharp
// Interface for objects that need custom reset logic when spawned/despawned from a pool.
// Implement on any MonoBehaviour attached to a pooled prefab.

namespace MetalPod.Pooling
{
    public interface IPoolable
    {
        /// <summary>Called when the object is taken from the pool and activated.</summary>
        void OnSpawned();

        /// <summary>Called when the object is returned to the pool and deactivated.</summary>
        void OnDespawned();
    }
}
```

### PooledObject.cs

```csharp
// Attached to every pooled GameObject. Manages pool identity and auto-despawn.
// Added automatically by ObjectPool if not already present on the prefab.

using System.Collections;
using UnityEngine;

namespace MetalPod.Pooling
{
    public class PooledObject : MonoBehaviour
    {
        private string _poolName;
        private Coroutine _autoDespawnCoroutine;

        public string PoolName => _poolName;
        public bool IsActive { get; private set; }

        public void Initialize(string poolName)
        {
            _poolName = poolName;
        }

        public void NotifySpawned()
        {
            IsActive = true;
            CancelAutoDespawn();
        }

        public void AutoDespawnAfter(float seconds)
        {
            CancelAutoDespawn();
            _autoDespawnCoroutine = StartCoroutine(AutoDespawnRoutine(seconds));
        }

        /// <summary>
        /// Call this to return the object to its pool from any script.
        /// Convenience method so callers don't need a PoolManager reference.
        /// </summary>
        public void Despawn()
        {
            if (!IsActive) return;
            IsActive = false;
            CancelAutoDespawn();
            PoolManager.Instance?.Despawn(gameObject);
        }

        private void CancelAutoDespawn()
        {
            if (_autoDespawnCoroutine != null)
            {
                StopCoroutine(_autoDespawnCoroutine);
                _autoDespawnCoroutine = null;
            }
        }

        private IEnumerator AutoDespawnRoutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Despawn();
        }

        private void OnDisable()
        {
            CancelAutoDespawn();
        }
    }
}
```

### PoolManager.cs

```csharp
// Singleton managing all named object pools.
// Access via PoolManager.Instance.Spawn("PoolName", pos, rot).
// Lives on a DontDestroyOnLoad GameObject.

using System.Collections.Generic;
using UnityEngine;
using MetalPod.ScriptableObjects;

namespace MetalPod.Pooling
{
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

            _poolRoot = new GameObject("_PoolRoot").transform;
            _poolRoot.SetParent(transform);

            if (globalPoolConfig != null)
            {
                InitializeFromConfig(globalPoolConfig);
            }
        }

        /// <summary>
        /// Initialize pools from a PoolConfigSO. Can be called multiple times
        /// (e.g., global config at boot + scene-specific config at scene load).
        /// </summary>
        public void InitializeFromConfig(PoolConfigSO config)
        {
            if (config == null || config.pools == null) return;

            foreach (var def in config.pools)
            {
                if (string.IsNullOrEmpty(def.poolName) || def.prefab == null)
                {
                    Debug.LogWarning("[PoolManager] Skipping pool with missing name or prefab.");
                    continue;
                }

                if (_pools.ContainsKey(def.poolName))
                {
                    Debug.LogWarning($"[PoolManager] Pool '{def.poolName}' already exists, skipping.");
                    continue;
                }

                CreatePool(def.poolName, def.prefab, def.prewarmCount,
                    def.maxSize, def.autoExpand, def.autoDespawnTime);
            }
        }

        /// <summary>
        /// Create a pool at runtime (for dynamic prefab registration).
        /// </summary>
        public ObjectPool CreatePool(string poolName, GameObject prefab, int prewarmCount = 10,
            int maxSize = 50, bool autoExpand = true, float autoDespawnTime = 0f)
        {
            if (_pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists.");
                return _pools[poolName];
            }

            Transform parent = new GameObject($"Pool_{poolName}").transform;
            parent.SetParent(_poolRoot);

            var pool = new ObjectPool(poolName, prefab, prewarmCount, maxSize,
                autoExpand, autoDespawnTime, parent);
            _pools[poolName] = pool;

            return pool;
        }

        /// <summary>
        /// Spawn an object from the named pool.
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
        /// Spawn with default rotation.
        /// </summary>
        public GameObject Spawn(string poolName, Vector3 position)
        {
            return Spawn(poolName, position, Quaternion.identity);
        }

        /// <summary>
        /// Return an object to its pool.
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            var pooled = obj.GetComponent<PooledObject>();
            if (pooled == null || string.IsNullOrEmpty(pooled.PoolName))
            {
                Debug.LogWarning($"[PoolManager] Object '{obj.name}' has no PooledObject component. Destroying instead.");
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
        /// Get a pool by name (for stats or manual control).
        /// </summary>
        public ObjectPool GetPool(string poolName)
        {
            _pools.TryGetValue(poolName, out ObjectPool pool);
            return pool;
        }

        /// <summary>
        /// Return all active objects across all pools (e.g., on scene unload).
        /// </summary>
        public void ReturnAllToPool()
        {
            foreach (var pool in _pools.Values)
            {
                pool.ReturnAll();
            }
        }

        /// <summary>
        /// Clear a specific pool (destroy all instances).
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (_pools.TryGetValue(poolName, out ObjectPool pool))
            {
                pool.Clear();
                _pools.Remove(poolName);
            }
        }

        /// <summary>
        /// Clear all pools (e.g., on game shutdown).
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }

        /// <summary>
        /// Get stats for all pools (for debug UI).
        /// </summary>
        public Dictionary<string, (int active, int available, int total)> GetAllStats()
        {
            var stats = new Dictionary<string, (int, int, int)>();
            foreach (var kvp in _pools)
            {
                stats[kvp.Key] = (kvp.Value.ActiveCount, kvp.Value.AvailableCount, kvp.Value.TotalCreated);
            }
            return stats;
        }

        private void OnDestroy()
        {
            ClearAll();
            if (Instance == this) Instance = null;
        }
    }
}
```

### PoolPrewarmer.cs

```csharp
// Pre-instantiates all pools at scene load, spreading allocation across frames.
// Attach to a GameObject in each course scene.
// Useful during loading screen to avoid first-frame allocation spike.

using System;
using System.Collections;
using UnityEngine;
using MetalPod.ScriptableObjects;

namespace MetalPod.Pooling
{
    public class PoolPrewarmer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PoolConfigSO scenePoolConfig;
        [SerializeField] private int instantiationsPerFrame = 5;
        [SerializeField] private bool prewarmOnStart = true;

        public event Action<float> OnProgress; // 0..1
        public event Action OnComplete;

        public bool IsPrewarming { get; private set; }

        private void Start()
        {
            if (prewarmOnStart && scenePoolConfig != null)
            {
                StartPrewarm();
            }
        }

        public void StartPrewarm()
        {
            if (scenePoolConfig == null)
            {
                OnComplete?.Invoke();
                return;
            }

            StartCoroutine(PrewarmRoutine());
        }

        private IEnumerator PrewarmRoutine()
        {
            IsPrewarming = true;

            // First, register all pools
            PoolManager.Instance?.InitializeFromConfig(scenePoolConfig);

            // Count total items to prewarm
            int totalItems = 0;
            foreach (var def in scenePoolConfig.pools)
            {
                totalItems += def.prewarmCount;
            }

            if (totalItems == 0)
            {
                IsPrewarming = false;
                OnProgress?.Invoke(1f);
                OnComplete?.Invoke();
                yield break;
            }

            // Pools are already pre-created in InitializeFromConfig.
            // This prewarmer reports progress for the loading screen.
            // Since ObjectPool.Grow() is called in the constructor,
            // we simulate frame-spread reporting.

            int reported = 0;
            int perFrame = Mathf.Max(1, instantiationsPerFrame);

            while (reported < totalItems)
            {
                reported = Mathf.Min(reported + perFrame, totalItems);
                float progress = (float)reported / totalItems;
                OnProgress?.Invoke(progress);
                yield return null;
            }

            IsPrewarming = false;
            OnProgress?.Invoke(1f);
            OnComplete?.Invoke();
        }
    }
}
```

### PoolDebugWindow.cs

```csharp
// Editor window showing real-time pool statistics.
// Menu: Metal Pod > Pool Debug Window

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MetalPod.Pooling;

namespace MetalPod.Editor
{
    public class PoolDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        [MenuItem("Metal Pod/Pool Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<PoolDebugWindow>("Pool Debug");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Object Pool Statistics", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see pool statistics.", MessageType.Info);
                return;
            }

            if (PoolManager.Instance == null)
            {
                EditorGUILayout.HelpBox("No PoolManager instance found.", MessageType.Warning);
                return;
            }

            var stats = PoolManager.Instance.GetAllStats();

            if (stats.Count == 0)
            {
                EditorGUILayout.HelpBox("No pools registered.", MessageType.Info);
                return;
            }

            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pool Name", EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.LabelField("Active", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("Available", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("Total", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("Usage", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var kvp in stats)
            {
                string name = kvp.Key;
                var (active, available, total) = kvp.Value;
                float usage = total > 0 ? (float)active / total : 0f;

                // Color code: green < 50%, yellow < 80%, red >= 80%
                Color rowColor = usage < 0.5f ? Color.green : usage < 0.8f ? Color.yellow : Color.red;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, GUILayout.Width(200));
                EditorGUILayout.LabelField(active.ToString(), GUILayout.Width(60));
                EditorGUILayout.LabelField(available.ToString(), GUILayout.Width(60));
                EditorGUILayout.LabelField(total.ToString(), GUILayout.Width(60));

                Color prev = GUI.color;
                GUI.color = rowColor;
                EditorGUILayout.LabelField($"{usage:P0}", GUILayout.Width(60));
                GUI.color = prev;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Return All To Pools"))
            {
                PoolManager.Instance.ReturnAllToPool();
            }

            // Auto-repaint during play mode
            if (Application.isPlaying)
                Repaint();
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }
    }
}
#endif
```

---

## Pool Definitions for Metal Pod

The PoolConfigSO asset should define these pools (agent should create a default asset generator or document the expected pool names):

| Pool Name | Prefab | Prewarm | Max | Auto-Despawn |
|-----------|--------|---------|-----|-------------|
| `LavaDebris` | Volcanic eruption debris | 15 | 30 | 4s |
| `LavaGeyserBurst` | Geyser particle burst | 5 | 10 | 3s |
| `IceShard` | Falling icicle | 10 | 20 | 5s |
| `AvalancheRock` | Avalanche rolling debris | 8 | 15 | 6s |
| `AcidSplash` | Acid pool splash effect | 5 | 10 | 2s |
| `BarrelFragment` | Barrel explosion fragment | 12 | 25 | 3s |
| `ToxicCloud` | Toxic gas cloud | 8 | 15 | 5s |
| `DamageSpark` | Hovercraft damage spark | 5 | 10 | 1s |
| `Explosion` | Generic explosion effect | 3 | 6 | 2s |
| `Collectible_Bolt` | Currency collectible | 20 | 40 | 0 (manual) |
| `Collectible_Health` | Health pickup | 5 | 10 | 0 (manual) |
| `Collectible_Shield` | Shield pickup | 5 | 10 | 0 (manual) |
| `DamageNumber` | Floating damage text | 8 | 15 | 1.5s |
| `CheckpointEffect` | Checkpoint activation VFX | 3 | 5 | 2s |

---

## Acceptance Criteria

- [ ] `ObjectPool.cs` — Generic pool with Get/Return/Grow/Clear, auto-expand with warning
- [ ] `PoolManager.cs` — Singleton with Spawn/Despawn, config-based initialization, stats API
- [ ] `PooledObject.cs` — Component with Despawn() convenience, auto-despawn timer
- [ ] `IPoolable.cs` — Interface for custom OnSpawned/OnDespawned logic
- [ ] `PoolPrewarmer.cs` — Frame-spread initialization with progress callback
- [ ] `PoolConfigSO.cs` — ScriptableObject with array of pool definitions
- [ ] `PoolDebugWindow.cs` — Editor window with real-time pool stats, color-coded usage
- [ ] All scripts in `MetalPod.Pooling`, `MetalPod.ScriptableObjects`, or `MetalPod.Editor` namespaces
- [ ] No modifications to existing files
- [ ] Compiles without errors
