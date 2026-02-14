using System;
using System.Collections;
using System.Collections.Generic;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Pooling
{
    /// <summary>
    /// Pre-instantiates configured pools across frames to reduce startup spikes.
    /// </summary>
    public class PoolPrewarmer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PoolConfigSO scenePoolConfig;
        [SerializeField] private int instantiationsPerFrame = 5;
        [SerializeField] private bool prewarmOnStart = true;

        public event Action<float> OnProgress;
        public event Action OnComplete;

        public bool IsPrewarming { get; private set; }

        private Coroutine _prewarmRoutine;

        private struct PrewarmTask
        {
            public ObjectPool pool;
            public int count;
        }

        private void Start()
        {
            if (prewarmOnStart)
            {
                StartPrewarm();
            }
        }

        public void StartPrewarm()
        {
            if (IsPrewarming)
            {
                return;
            }

            if (scenePoolConfig == null || scenePoolConfig.pools == null)
            {
                OnProgress?.Invoke(1f);
                OnComplete?.Invoke();
                return;
            }

            _prewarmRoutine = StartCoroutine(PrewarmRoutine());
        }

        private IEnumerator PrewarmRoutine()
        {
            IsPrewarming = true;

            PoolManager manager = PoolManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[PoolPrewarmer] No PoolManager instance found. Skipping prewarm.");
                IsPrewarming = false;
                OnProgress?.Invoke(1f);
                OnComplete?.Invoke();
                yield break;
            }

            List<PrewarmTask> tasks = new List<PrewarmTask>(scenePoolConfig.pools.Length);
            int totalToCreate = 0;

            for (int i = 0; i < scenePoolConfig.pools.Length; i++)
            {
                PoolConfigSO.PoolDefinition def = scenePoolConfig.pools[i];
                if (def == null || string.IsNullOrWhiteSpace(def.poolName) || def.prefab == null)
                {
                    Debug.LogWarning("[PoolPrewarmer] Skipping pool with missing name or prefab.");
                    continue;
                }

                ObjectPool pool = manager.GetPool(def.poolName);
                if (pool == null)
                {
                    pool = manager.CreatePool(
                        def.poolName,
                        def.prefab,
                        prewarmCount: 0,
                        maxSize: def.maxSize,
                        autoExpand: def.autoExpand,
                        autoDespawnTime: def.autoDespawnTime);
                }

                if (pool == null)
                {
                    continue;
                }

                int targetCount = Mathf.Max(0, def.prewarmCount);
                int neededCount = Mathf.Max(0, targetCount - pool.TotalCreated);

                if (neededCount > 0)
                {
                    tasks.Add(new PrewarmTask
                    {
                        pool = pool,
                        count = neededCount
                    });
                    totalToCreate += neededCount;
                }
            }

            if (totalToCreate == 0)
            {
                IsPrewarming = false;
                OnProgress?.Invoke(1f);
                OnComplete?.Invoke();
                _prewarmRoutine = null;
                yield break;
            }

            int created = 0;
            int perFrame = Mathf.Max(1, instantiationsPerFrame);
            int frameBudget = 0;

            for (int taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
            {
                PrewarmTask task = tasks[taskIndex];
                for (int i = 0; i < task.count; i++)
                {
                    task.pool.Grow(1);
                    created++;
                    frameBudget++;

                    if (frameBudget >= perFrame)
                    {
                        frameBudget = 0;
                        OnProgress?.Invoke((float)created / totalToCreate);
                        yield return null;
                    }
                }
            }

            IsPrewarming = false;
            OnProgress?.Invoke(1f);
            OnComplete?.Invoke();
            _prewarmRoutine = null;
        }

        private void OnDisable()
        {
            if (_prewarmRoutine != null)
            {
                StopCoroutine(_prewarmRoutine);
                _prewarmRoutine = null;
                IsPrewarming = false;
            }
        }
    }
}
