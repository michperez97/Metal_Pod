using System;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;

namespace MetalPod.Progression
{
    public class UpgradeManager
    {
        private SaveSystem _saveSystem;
        private CurrencyManager _currencyManager;
        private UpgradeDataSO[] _allUpgrades;

        public void Initialize(SaveSystem saveSystem, CurrencyManager currencyManager, UpgradeDataSO[] upgrades)
        {
            _saveSystem = saveSystem;
            _currencyManager = currencyManager;
            _allUpgrades = upgrades ?? Array.Empty<UpgradeDataSO>();
        }

        public int GetUpgradeLevel(string upgradeId)
        {
            if (_saveSystem == null || _saveSystem.CurrentData == null || string.IsNullOrWhiteSpace(upgradeId))
            {
                return 0;
            }

            return _saveSystem.CurrentData.upgradeLevels.GetValueOrDefault(upgradeId, 0);
        }

        public int GetMaxLevel(string upgradeId)
        {
            UpgradeDataSO upgrade = GetUpgradeData(upgradeId);
            return upgrade != null && upgrade.levels != null ? upgrade.levels.Length : 0;
        }

        public int GetNextLevelCost(string upgradeId)
        {
            UpgradeDataSO upgrade = GetUpgradeData(upgradeId);
            int currentLevel = GetUpgradeLevel(upgradeId);
            if (upgrade == null || upgrade.levels == null || currentLevel >= upgrade.levels.Length)
            {
                return -1;
            }

            return upgrade.levels[currentLevel].cost;
        }

        public bool CanPurchaseUpgrade(string upgradeId)
        {
            int cost = GetNextLevelCost(upgradeId);
            if (cost < 0)
            {
                return false;
            }

            return _currencyManager != null && _currencyManager.CanAfford(cost);
        }

        public bool TryPurchaseUpgrade(string upgradeId)
        {
            if (_saveSystem == null || _saveSystem.CurrentData == null)
            {
                return false;
            }

            if (!CanPurchaseUpgrade(upgradeId))
            {
                return false;
            }

            int cost = GetNextLevelCost(upgradeId);
            if (!_currencyManager.SpendCurrency(cost))
            {
                return false;
            }

            int newLevel = GetUpgradeLevel(upgradeId) + 1;
            _saveSystem.CurrentData.upgradeLevels.Set(upgradeId, newLevel);
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            EventBus.RaiseUpgradePurchased(upgradeId, newLevel);
            return true;
        }

        public (float speed, float handling, float shield, float boost) GetStatMultipliers()
        {
            float speed = 1f + (GetUpgradeLevel("speed") * 0.1f);
            float handling = 1f + (GetUpgradeLevel("handling") * 0.08f);
            float shield = 1f + (GetUpgradeLevel("shield") * 0.12f);
            float boost = 1f + (GetUpgradeLevel("boost") * 0.1f);
            return (speed, handling, shield, boost);
        }

        public UpgradeDataSO GetUpgradeData(string id)
        {
            if (_allUpgrades == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            for (int i = 0; i < _allUpgrades.Length; i++)
            {
                UpgradeDataSO candidate = _allUpgrades[i];
                if (candidate != null && candidate.upgradeId == id)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
