using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "HazardData", menuName = "MetalPod/HazardData")]
    public class HazardDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique hazard ID for telemetry and balancing.")]
        public string hazardId;
        [Tooltip("Display name shown in tooltips and debug tools.")]
        public string hazardName;

        [Tooltip("Description used in course previews.")]
        [TextArea]
        public string description;

        [Tooltip("Damage applied per second while inside persistent hazard zones.")]
        [Min(0f)]
        public float damagePerSecond = 5f;
        [Tooltip("Instant damage applied when entering/impacting the hazard.")]
        [Min(0f)]
        public float damagePerHit = 10f;
        [Tooltip("Warning radius used for UI indicators and audio cues.")]
        [Min(0f)]
        public float warningRadius = 10f;
        [Tooltip("Relative danger tier used for encounter balancing.")]
        [Range(1, 5)]
        public int threatLevel = 1;

        private void OnValidate()
        {
            damagePerSecond = Mathf.Max(0f, damagePerSecond);
            damagePerHit = Mathf.Max(0f, damagePerHit);
            warningRadius = Mathf.Max(0f, warningRadius);
            threatLevel = Mathf.Clamp(threatLevel, 1, 5);
        }

        public bool IsValid(out string validationError)
        {
            if (string.IsNullOrWhiteSpace(hazardId))
            {
                validationError = "Hazard ID is required.";
                return false;
            }

            if (damagePerSecond <= 0f && damagePerHit <= 0f)
            {
                validationError = "Hazard should apply either hit damage or damage over time.";
                return false;
            }

            validationError = string.Empty;
            return true;
        }
    }
}
