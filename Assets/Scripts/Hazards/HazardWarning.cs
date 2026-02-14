using System;
using MetalPod.Hovercraft;
using UnityEngine;

namespace MetalPod.Hazards
{
    public class HazardWarning : MonoBehaviour
    {
        [SerializeField] private HovercraftController player;
        [SerializeField] private float detectionRadius = 40f;
        [SerializeField] private float refreshInterval = 0.1f;

        public event Action<Vector3, int> OnWarningUpdated;

        private HazardBase[] _hazards;
        private float _timer;

        private void Start()
        {
            _hazards = FindObjectsOfType<HazardBase>();

            if (player == null)
            {
                player = FindObjectOfType<HovercraftController>();
            }
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < refreshInterval)
            {
                return;
            }

            _timer = 0f;
            EvaluateThreats();
        }

        private void EvaluateThreats()
        {
            HazardBase mostRelevantHazard = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < _hazards.Length; i++)
            {
                HazardBase hazard = _hazards[i];
                if (hazard == null || !hazard.gameObject.activeInHierarchy)
                {
                    continue;
                }

                float distance = Vector3.Distance(player.transform.position, hazard.transform.position);
                if (distance <= detectionRadius && distance < bestDistance)
                {
                    bestDistance = distance;
                    mostRelevantHazard = hazard;
                }
            }

            if (mostRelevantHazard == null)
            {
                OnWarningUpdated?.Invoke(Vector3.zero, 0);
                return;
            }

            Vector3 direction = (mostRelevantHazard.transform.position - player.transform.position).normalized;
            int threatLevel = bestDistance < detectionRadius * 0.35f ? 2 : 1;
            OnWarningUpdated?.Invoke(direction, threatLevel);
        }
    }
}
