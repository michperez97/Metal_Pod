using System.Collections.Generic;
using UnityEngine;

namespace MetalPod.Pooling
{
    /// <summary>
    /// Generic object pool for GameObject prefabs.
    /// </summary>
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
        internal Transform PoolParent => _poolParent;

        public ObjectPool(string poolName, GameObject prefab, int initialSize, int maxSize,
            bool autoExpand, float autoDespawnTime, Transform parent)
        {
            _poolName = poolName;
            _prefab = prefab;
            _maxSize = maxSize;
            _autoExpand = autoExpand;
            _autoDespawnTime = autoDespawnTime;
            _poolParent = parent;

            int capacity = Mathf.Max(0, initialSize);
            _available = new Stack<GameObject>(capacity);
            _active = new HashSet<GameObject>();

            Grow(capacity);
        }

        public void Grow(int count)
        {
            int growCount = Mathf.Max(0, count);
            for (int i = 0; i < growCount; i++)
            {
                if (_maxSize > 0 && _totalCreated >= _maxSize)
                {
                    break;
                }

                GameObject obj = Object.Instantiate(_prefab, _poolParent);
                obj.SetActive(false);
                obj.name = $"{_poolName}_{_totalCreated}";

                PooledObject pooled = obj.GetComponent<PooledObject>();
                if (pooled == null)
                {
                    pooled = obj.AddComponent<PooledObject>();
                }

                pooled.Initialize(_poolName);
                pooled.OnDespawn();

                _available.Push(obj);
                _totalCreated++;
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj = TryTakeAvailable();

            if (obj == null)
            {
                if (_autoExpand)
                {
                    Debug.LogWarning($"[ObjectPool] Pool '{_poolName}' empty, auto-expanding. " +
                                     $"Consider increasing prewarm count. (Total: {_totalCreated})");
                    Grow(Mathf.Max(1, _totalCreated / 4));
                    obj = TryTakeAvailable();

                    if (obj == null)
                    {
                        Debug.LogError($"[ObjectPool] Pool '{_poolName}' cannot expand (max: {_maxSize})");
                        return null;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ObjectPool] Pool '{_poolName}' exhausted (max: {_maxSize})");
                    return null;
                }
            }

            obj.transform.SetParent(null, false);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            _active.Add(obj);

            IPoolable[] poolableComponents = obj.GetComponents<IPoolable>();
            for (int i = 0; i < poolableComponents.Length; i++)
            {
                poolableComponents[i].OnSpawned();
            }

            PooledObject pooled = obj.GetComponent<PooledObject>();
            if (pooled != null)
            {
                pooled.OnSpawn();

                if (_autoDespawnTime > 0f)
                {
                    pooled.AutoDespawnAfter(_autoDespawnTime);
                }
            }

            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!_active.Remove(obj))
            {
                Debug.LogWarning($"[ObjectPool] Tried to return untracked object to pool '{_poolName}'");
                return;
            }

            IPoolable[] poolableComponents = obj.GetComponents<IPoolable>();
            for (int i = 0; i < poolableComponents.Length; i++)
            {
                poolableComponents[i].OnDespawned();
            }

            PooledObject pooled = obj.GetComponent<PooledObject>();
            pooled?.OnDespawn();

            obj.SetActive(false);
            obj.transform.SetParent(_poolParent, false);
            _available.Push(obj);
        }

        public void ReturnAll()
        {
            _active.RemoveWhere(item => item == null);
            List<GameObject> activeList = new List<GameObject>(_active);
            for (int i = 0; i < activeList.Count; i++)
            {
                Return(activeList[i]);
            }
        }

        public void Clear()
        {
            ReturnAll();

            while (_available.Count > 0)
            {
                GameObject obj = _available.Pop();
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }

            _active.Clear();
            _totalCreated = 0;
        }

        private GameObject TryTakeAvailable()
        {
            while (_available.Count > 0)
            {
                GameObject obj = _available.Pop();
                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }
    }
}
