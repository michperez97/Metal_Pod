using MetalPod.ScriptableObjects;
using MetalPod.UI;
using UnityEngine;

namespace MetalPod.Workshop
{
    public class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour currencyManagerSource;

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

            int cost = upgradeData.levels[targetLevel].cost;
            if (currencyManagerSource == null)
            {
                currencyManagerSource = FindSourceWithMember("SpendCurrency");
            }

            object response = ReflectionValueReader.Invoke(currencyManagerSource, "SpendCurrency", cost);
            return response is bool result && result;
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
