using System.Reflection;
using MetalPod.ScriptableObjects;
using MetalPod.UI;
using UnityEngine;

namespace MetalPod.Workshop
{
    public class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour currencyManagerSource;
        [SerializeField] private MonoBehaviour progressionManagerSource;

        private object _upgradeManagerSource;

        public bool TryPurchaseUpgrade(UpgradeDataSO upgradeData, int targetLevel)
        {
            if (upgradeData == null || upgradeData.levels == null)
            {
                return false;
            }

            if (targetLevel < 0 || targetLevel >= upgradeData.levels.Length)
            {
                return false;
            }

            object upgradeManager = ResolveUpgradeManager();
            if (upgradeManager != null)
            {
                return TryPurchaseViaUpgradeManager(upgradeData.upgradeId);
            }

            int cost = upgradeData.levels[targetLevel].cost;
            if (currencyManagerSource == null)
            {
                currencyManagerSource = FindSourceWithMember("SpendCurrency");
            }

            object response = ReflectionValueReader.Invoke(currencyManagerSource, "SpendCurrency", cost);
            bool purchased = response is bool result && result;
            if (purchased)
            {
                SharedContractsBridge.Raise("RaiseUpgradePurchased", upgradeData.upgradeId, targetLevel + 1);
            }

            return purchased;
        }

        private bool TryPurchaseViaUpgradeManager(string upgradeId)
        {
            object upgradeManager = ResolveUpgradeManager();
            if (upgradeManager == null || string.IsNullOrEmpty(upgradeId))
            {
                return false;
            }

            object response = ReflectionValueReader.Invoke(upgradeManager, "TryPurchaseUpgrade", upgradeId);
            return response is bool purchased && purchased;
        }

        private object ResolveUpgradeManager()
        {
            if (_upgradeManagerSource != null)
            {
                return _upgradeManagerSource;
            }

            if (progressionManagerSource == null)
            {
                progressionManagerSource = FindSourceWithMember("Upgrades");
            }

            if (progressionManagerSource == null)
            {
                return null;
            }

            PropertyInfo upgradesProperty = progressionManagerSource.GetType().GetProperty(
                "Upgrades",
                BindingFlags.Public | BindingFlags.Instance);

            if (upgradesProperty == null)
            {
                return null;
            }

            _upgradeManagerSource = upgradesProperty.GetValue(progressionManagerSource);
            return _upgradeManagerSource;
        }

        private static MonoBehaviour FindSourceWithMember(string memberName)
        {
            MonoBehaviour[] candidates = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                MonoBehaviour candidate = candidates[i];
                if (candidate != null && ReflectionValueReader.HasMember(candidate, memberName))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
