using System.Collections;
using UnityEngine;

namespace MetalPod.Pooling
{
    /// <summary>
    /// Tracks pooled instance metadata and provides timed/manual despawn helpers.
    /// </summary>
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

        public void OnSpawn()
        {
            IsActive = true;
            CancelAutoDespawn();
        }

        public void OnDespawn()
        {
            IsActive = false;
            CancelAutoDespawn();
        }

        public void NotifySpawned()
        {
            OnSpawn();
        }

        public void NotifyDespawned()
        {
            OnDespawn();
        }

        public void AutoDespawnAfter(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            CancelAutoDespawn();
            _autoDespawnCoroutine = StartCoroutine(AutoDespawnRoutine(seconds));
        }

        /// <summary>
        /// Convenience method for returning this object to its pool.
        /// </summary>
        public void Despawn()
        {
            if (!IsActive)
            {
                return;
            }

            PoolManager manager = PoolManager.Instance;
            if (manager != null)
            {
                manager.Despawn(gameObject);
            }
            else
            {
                OnDespawn();
                gameObject.SetActive(false);
            }
        }

        private IEnumerator AutoDespawnRoutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Despawn();
        }

        private void CancelAutoDespawn()
        {
            if (_autoDespawnCoroutine != null)
            {
                StopCoroutine(_autoDespawnCoroutine);
                _autoDespawnCoroutine = null;
            }
        }

        private void OnDisable()
        {
            IsActive = false;
            CancelAutoDespawn();
        }
    }
}
