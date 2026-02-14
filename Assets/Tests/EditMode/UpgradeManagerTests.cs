using NUnit.Framework;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class UpgradeManagerTests
    {
        private SaveFileIsolationScope _saveIsolation;
        private GameObject _saveObject;
        private SaveSystem _saveSystem;
        private CurrencyManager _currency;
        private UpgradeManager _upgrades;
        private UpgradeDataSO[] _upgradeAssets;

        [SetUp]
        public void SetUp()
        {
            EventBus.Initialize();
            _saveIsolation = new SaveFileIsolationScope();
            _saveSystem = TestDataFactory.CreateInitializedSaveSystem(out _saveObject);

            _currency = new CurrencyManager();
            _currency.Initialize(_saveSystem);
            _currency.AddCurrency(30000);

            _upgradeAssets = TestDataFactory.CreateDefaultUpgrades();
            _upgrades = new UpgradeManager();
            _upgrades.Initialize(_saveSystem, _currency, _upgradeAssets);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Shutdown();
            TestDataFactory.DestroyAll(_saveObject);
            if (_upgradeAssets != null)
            {
                for (int i = 0; i < _upgradeAssets.Length; i++)
                {
                    TestDataFactory.DestroyAll(_upgradeAssets[i]);
                }
            }

            _saveIsolation.Dispose();
        }

        [Test]
        public void GetUpgradeLevel_NoPurchases_ReturnsZero()
        {
            Assert.AreEqual(0, _upgrades.GetUpgradeLevel("speed"));
            Assert.AreEqual(0, _upgrades.GetUpgradeLevel("handling"));
            Assert.AreEqual(0, _upgrades.GetUpgradeLevel("shield"));
            Assert.AreEqual(0, _upgrades.GetUpgradeLevel("boost"));
        }

        [Test]
        public void TryPurchaseUpgrade_SufficientFunds_ReturnsTrue()
        {
            bool result = _upgrades.TryPurchaseUpgrade("speed");
            Assert.IsTrue(result);
            Assert.AreEqual(1, _upgrades.GetUpgradeLevel("speed"));
        }

        [Test]
        public void TryPurchaseUpgrade_InsufficientFunds_ReturnsFalse()
        {
            _saveSystem.CurrentData.currency = 0;
            _saveSystem.Save();

            bool result = _upgrades.TryPurchaseUpgrade("speed");

            Assert.IsFalse(result);
            Assert.AreEqual(0, _upgrades.GetUpgradeLevel("speed"));
        }

        [Test]
        public void TryPurchaseUpgrade_MaxLevel_ReturnsFalse()
        {
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(_upgrades.TryPurchaseUpgrade("speed"));
            }

            bool result = _upgrades.TryPurchaseUpgrade("speed");
            Assert.IsFalse(result);
            Assert.AreEqual(5, _upgrades.GetUpgradeLevel("speed"));
        }

        [Test]
        public void TryPurchaseUpgrade_DeductsCurrency()
        {
            int before = _currency.Currency;
            bool success = _upgrades.TryPurchaseUpgrade("speed");

            Assert.IsTrue(success);
            Assert.AreEqual(before - 100, _currency.Currency);
        }

        [Test]
        public void GetNextLevelCost_IncreasesPerLevel_ForSpeed()
        {
            Assert.AreEqual(100, _upgrades.GetNextLevelCost("speed"));
            _upgrades.TryPurchaseUpgrade("speed");
            Assert.AreEqual(250, _upgrades.GetNextLevelCost("speed"));
            _upgrades.TryPurchaseUpgrade("speed");
            Assert.AreEqual(500, _upgrades.GetNextLevelCost("speed"));
        }

        [Test]
        public void GetStatMultipliers_NoUpgrades_AllOnes()
        {
            var multipliers = _upgrades.GetStatMultipliers();
            Assert.AreEqual(1f, multipliers.speed, 0.0001f);
            Assert.AreEqual(1f, multipliers.handling, 0.0001f);
            Assert.AreEqual(1f, multipliers.shield, 0.0001f);
            Assert.AreEqual(1f, multipliers.boost, 0.0001f);
        }

        [Test]
        public void GetStatMultipliers_SpeedLevel3_Correct()
        {
            _upgrades.TryPurchaseUpgrade("speed");
            _upgrades.TryPurchaseUpgrade("speed");
            _upgrades.TryPurchaseUpgrade("speed");

            var multipliers = _upgrades.GetStatMultipliers();
            Assert.AreEqual(1.3f, multipliers.speed, 0.0001f);
        }

        [Test]
        public void GetStatMultipliers_AllMaxLevel_Correct()
        {
            MaxUpgrade("speed");
            MaxUpgrade("handling");
            MaxUpgrade("shield");
            MaxUpgrade("boost");

            var multipliers = _upgrades.GetStatMultipliers();
            Assert.AreEqual(1.5f, multipliers.speed, 0.0001f);
            Assert.AreEqual(1.4f, multipliers.handling, 0.0001f);
            Assert.AreEqual(1.6f, multipliers.shield, 0.0001f);
            Assert.AreEqual(1.5f, multipliers.boost, 0.0001f);
        }

        [Test]
        public void UpgradeCosts_MatchBalanceSheet()
        {
            CollectionAssert.AreEqual(new[] { 100, 250, 500, 1000, 2000 }, GetCosts("speed"));
            CollectionAssert.AreEqual(new[] { 100, 250, 500, 1000, 2000 }, GetCosts("handling"));
            CollectionAssert.AreEqual(new[] { 150, 350, 600, 1200, 2500 }, GetCosts("shield"));
            CollectionAssert.AreEqual(new[] { 100, 250, 500, 1000, 2000 }, GetCosts("boost"));
        }

        [Test]
        public void TotalCostToMaxAllUpgrades_IsExpected16350()
        {
            int total = Sum(GetCosts("speed")) + Sum(GetCosts("handling")) + Sum(GetCosts("shield")) + Sum(GetCosts("boost"));
            Assert.AreEqual(16350, total);
        }

        [Test]
        public void TryPurchaseUpgrade_RaisesUpgradePurchasedEvent()
        {
            string receivedId = null;
            int receivedLevel = -1;
            EventBus.OnUpgradePurchased += (id, level) =>
            {
                receivedId = id;
                receivedLevel = level;
            };

            _upgrades.TryPurchaseUpgrade("speed");

            Assert.AreEqual("speed", receivedId);
            Assert.AreEqual(1, receivedLevel);
        }

        private void MaxUpgrade(string id)
        {
            while (_upgrades.TryPurchaseUpgrade(id))
            {
            }
        }

        private int[] GetCosts(string upgradeId)
        {
            UpgradeDataSO data = _upgrades.GetUpgradeData(upgradeId);
            int[] costs = new int[data.levels.Length];
            for (int i = 0; i < data.levels.Length; i++)
            {
                costs[i] = data.levels[i].cost;
            }

            return costs;
        }

        private static int Sum(int[] values)
        {
            int total = 0;
            for (int i = 0; i < values.Length; i++)
            {
                total += values[i];
            }

            return total;
        }
    }
}
