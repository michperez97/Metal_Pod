using NUnit.Framework;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class CosmeticManagerTests
    {
        private SaveFileIsolationScope _saveIsolation;
        private GameObject _saveObject;
        private SaveSystem _saveSystem;
        private CurrencyManager _currency;
        private CosmeticManager _cosmetics;
        private CosmeticDataSO[] _cosmeticAssets;

        [SetUp]
        public void SetUp()
        {
            EventBus.Initialize();
            _saveIsolation = new SaveFileIsolationScope();
            _saveSystem = TestDataFactory.CreateInitializedSaveSystem(out _saveObject);
            _currency = new CurrencyManager();
            _currency.Initialize(_saveSystem);
            _currency.AddCurrency(2000);

            _cosmeticAssets = TestDataFactory.CreateDefaultCosmetics();
            _cosmetics = new CosmeticManager();
            _cosmetics.Initialize(_saveSystem, _currency, _cosmeticAssets);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Shutdown();
            if (_cosmeticAssets != null)
            {
                for (int i = 0; i < _cosmeticAssets.Length; i++)
                {
                    TestDataFactory.DestroyAll(_cosmeticAssets[i]);
                }
            }

            TestDataFactory.DestroyAll(_saveObject);
            _saveIsolation.Dispose();
        }

        [Test]
        public void OwnsCosmetic_Default_ReturnsTrue()
        {
            Assert.IsTrue(_cosmetics.OwnsCosmetic("default"));
        }

        [Test]
        public void OwnsCosmetic_Unpurchased_ReturnsFalse()
        {
            Assert.IsFalse(_cosmetics.OwnsCosmetic("red_black"));
        }

        [Test]
        public void TryPurchaseCosmetic_SufficientFunds_Succeeds()
        {
            bool result = _cosmetics.TryPurchaseCosmetic("red_black");

            Assert.IsTrue(result);
            Assert.IsTrue(_cosmetics.OwnsCosmetic("red_black"));
        }

        [Test]
        public void TryPurchaseCosmetic_AlreadyOwned_ReturnsFalse()
        {
            Assert.IsTrue(_cosmetics.TryPurchaseCosmetic("red_black"));
            Assert.IsFalse(_cosmetics.TryPurchaseCosmetic("red_black"));
        }

        [Test]
        public void TryPurchaseCosmetic_InsufficientFunds_Fails()
        {
            _saveSystem.CurrentData.currency = 0;
            _saveSystem.Save();

            bool result = _cosmetics.TryPurchaseCosmetic("chrome");

            Assert.IsFalse(result);
            Assert.IsFalse(_cosmetics.OwnsCosmetic("chrome"));
        }

        [Test]
        public void CanPurchaseCosmetic_RequiresMedals()
        {
            _saveSystem.CurrentData.totalMedals = 3;
            Assert.IsFalse(_cosmetics.CanPurchaseCosmetic("chrome"));

            _saveSystem.CurrentData.totalMedals = 5;
            Assert.IsTrue(_cosmetics.CanPurchaseCosmetic("chrome"));
        }

        [Test]
        public void EquipCosmetic_Owned_ChangesEquipped()
        {
            _cosmetics.TryPurchaseCosmetic("red_black");
            _cosmetics.EquipCosmetic("red_black");

            Assert.AreEqual("red_black", _cosmetics.EquippedColorScheme);
        }

        [Test]
        public void EquipCosmetic_NotOwned_NoChange()
        {
            string before = _cosmetics.EquippedColorScheme;
            _cosmetics.EquipCosmetic("red_black");

            Assert.AreEqual(before, _cosmetics.EquippedColorScheme);
        }

        [Test]
        public void EquipCosmetic_RaisesEvent()
        {
            _cosmetics.TryPurchaseCosmetic("red_black");

            string received = null;
            EventBus.OnCosmeticEquipped += id => received = id;

            _cosmetics.EquipCosmetic("red_black");

            Assert.AreEqual("red_black", received);
        }
    }
}
