using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "MetalPod/UpgradeData")]
    public class UpgradeDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique upgrade ID used for progression persistence.")]
        public string upgradeId;
        [Tooltip("Display name shown in workshop UI.")]
        public string upgradeName;

        [Tooltip("Description shown in upgrade details panel.")]
        [TextArea]
        public string description;

        [Tooltip("Upgrade family used for grouping and filters.")]
        public UpgradeCategory category;
        [Tooltip("Icon displayed in upgrade buttons/cards.")]
        public Sprite icon;

        [Header("Levels")]
        [Tooltip("Per-level cost and scaling information.")]
        public UpgradeLevel[] levels;

        [Header("Stat Modifications")]
        [Tooltip("Stats affected by this upgrade.")]
        public StatModifier[] statModifiers;

        private void OnValidate()
        {
            if (levels != null)
            {
                foreach (UpgradeLevel level in levels)
                {
                    if (level == null)
                    {
                        continue;
                    }

                    level.cost = Mathf.Max(0, level.cost);
                    level.statMultiplier = Mathf.Max(0f, level.statMultiplier);
                }
            }

            if (statModifiers != null)
            {
                foreach (StatModifier modifier in statModifiers)
                {
                    if (modifier == null)
                    {
                        continue;
                    }

                    if (float.IsNaN(modifier.valuePerLevel) || float.IsInfinity(modifier.valuePerLevel))
                    {
                        modifier.valuePerLevel = 0f;
                    }
                }
            }
        }

        public bool IsValid(out string validationError)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                validationError = "Upgrade ID is required.";
                return false;
            }

            if (levels == null || levels.Length == 0)
            {
                validationError = "At least one upgrade level is required.";
                return false;
            }

            validationError = string.Empty;
            return true;
        }
    }

    public enum UpgradeCategory
    {
        Speed,
        Handling,
        Shield,
        Boost
    }

    [System.Serializable]
    public class UpgradeLevel
    {
        [Tooltip("Currency cost to purchase this level.")]
        [Min(0)]
        public int cost;
        [Tooltip("Stat multiplier applied at this level.")]
        [Min(0f)]
        public float statMultiplier = 1f;
        [Tooltip("Short text description for this level.")]
        public string description;
    }

    [System.Serializable]
    public class StatModifier
    {
        [Tooltip("Name of the target stat to modify.")]
        public string statName;
        [Tooltip("Per-level additive value applied to the target stat.")]
        public float valuePerLevel;
    }
}
