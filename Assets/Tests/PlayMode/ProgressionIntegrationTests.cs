using System.Collections;
using MetalPod.Course;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MetalPod.Tests.PlayMode
{
    public class ProgressionIntegrationTests : PlayModeTestBase
    {
        private SaveFileIsolationScope _saveIsolation;
        private ProgressionManager _progression;
        private SaveSystem _saveSystem;
        private CourseDataSO[] _courses;
        private UpgradeDataSO[] _upgrades;
        private CosmeticDataSO[] _cosmetics;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            _saveIsolation = new SaveFileIsolationScope();

            _courses = new[]
            {
                TrackAsset(PlayModeTestFactory.CreateCourseData(
                    "lava_01",
                    50f,
                    65f,
                    80f,
                    DifficultyLevel.Easy,
                    EnvironmentType.Lava,
                    0)),
                TrackAsset(PlayModeTestFactory.CreateCourseData(
                    "lava_02",
                    70f,
                    90f,
                    110f,
                    DifficultyLevel.Medium,
                    EnvironmentType.Lava,
                    1))
            };

            _upgrades = new[]
            {
                TrackAsset(PlayModeTestFactory.CreateUpgradeData("speed", UpgradeCategory.Speed, 100, 250, 500)),
                TrackAsset(PlayModeTestFactory.CreateUpgradeData("handling", UpgradeCategory.Handling, 100, 250, 500)),
                TrackAsset(PlayModeTestFactory.CreateUpgradeData("shield", UpgradeCategory.Shield, 150, 350, 600)),
                TrackAsset(PlayModeTestFactory.CreateUpgradeData("boost", UpgradeCategory.Boost, 100, 250, 500))
            };

            _cosmetics = new[]
            {
                TrackAsset(PlayModeTestFactory.CreateCosmeticData("default", CosmeticType.ColorScheme, 0)),
                TrackAsset(PlayModeTestFactory.CreateCosmeticData("decal_73", CosmeticType.Decal, 0)),
                TrackAsset(PlayModeTestFactory.CreateCosmeticData("red_black", CosmeticType.ColorScheme, 200))
            };

            GameObject progressionObject = CreateTestObject("ProgressionManager_PlayMode");
            _saveSystem = progressionObject.AddComponent<SaveSystem>();
            _progression = progressionObject.AddComponent<ProgressionManager>();
            yield return null;

            SetPrivateField(_progression, "allCourses", _courses);
            SetPrivateField(_progression, "allUpgrades", _upgrades);
            SetPrivateField(_progression, "allCosmetics", _cosmetics);

            CurrencyManager currencyManager = new CurrencyManager();
            currencyManager.Initialize(_saveSystem);

            UpgradeManager upgradeManager = new UpgradeManager();
            upgradeManager.Initialize(_saveSystem, currencyManager, _upgrades);

            CosmeticManager cosmeticManager = new CosmeticManager();
            cosmeticManager.Initialize(_saveSystem, currencyManager, _cosmetics);

            SetPrivateField(_progression, "_currencyManager", currencyManager);
            SetPrivateField(_progression, "_upgradeManager", upgradeManager);
            SetPrivateField(_progression, "_cosmeticManager", cosmeticManager);
            InvokeNonPublicMethod(_progression, "EnsureDefaults");
            InvokeNonPublicMethod(_progression, "CheckAndUnlockCourses", false);

            EventBus.OnCourseCompleted += HandleCourseCompletedFromEvent;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            EventBus.OnCourseCompleted -= HandleCourseCompletedFromEvent;
            _saveIsolation?.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProgressionManager_InitializesAllSubsystems()
        {
            yield return null;

            Assert.IsNotNull(_progression);
            Assert.IsNotNull(_progression.CurrencyMgr);
            Assert.IsNotNull(_progression.Upgrades);
            Assert.IsNotNull(_progression.Cosmetics);
        }

        [UnityTest]
        public IEnumerator CurrencyFlow_EarnAndSpend()
        {
            _progression.CurrencyMgr.AddCurrency(100);
            bool spent = _progression.CurrencyMgr.SpendCurrency(30);
            yield return null;

            Assert.IsTrue(spent);
            Assert.AreEqual(70, _progression.Currency);
        }

        [UnityTest]
        public IEnumerator UpgradeFlow_PurchaseIncreasesLevel()
        {
            _progression.CurrencyMgr.AddCurrency(500);
            bool purchased = _progression.Upgrades.TryPurchaseUpgrade("speed");
            yield return null;

            Assert.IsTrue(purchased);
            Assert.AreEqual(1, _progression.Upgrades.GetUpgradeLevel("speed"));
        }

        [UnityTest]
        public IEnumerator UpgradeFlow_InsufficientFunds_Fails()
        {
            bool purchased = _progression.Upgrades.TryPurchaseUpgrade("speed");
            yield return null;

            Assert.IsFalse(purchased);
            Assert.AreEqual(0, _progression.Upgrades.GetUpgradeLevel("speed"));
        }

        [UnityTest]
        public IEnumerator CosmeticFlow_PurchaseAndEquip()
        {
            _progression.CurrencyMgr.AddCurrency(500);
            bool purchased = _progression.Cosmetics.TryPurchaseCosmetic("red_black");
            _progression.Cosmetics.EquipCosmetic("red_black");
            yield return null;

            Assert.IsTrue(purchased);
            Assert.IsTrue(_progression.Cosmetics.OwnsCosmetic("red_black"));
            Assert.AreEqual("red_black", _progression.Cosmetics.EquippedColorScheme);
        }

        [UnityTest]
        public IEnumerator CourseCompletion_AwardsCurrency()
        {
            int before = _progression.Currency;
            EventBus.RaiseCourseCompleted("lava_01", 49f, (int)Medal.Gold);
            yield return null;

            Assert.Greater(_progression.Currency, before);
        }

        [UnityTest]
        public IEnumerator MedalImprovement_AwardsBonus()
        {
            _progression.RecordCourseCompletion("lava_01", 79f, (int)Medal.Bronze);
            int afterBronze = _progression.Currency;

            _progression.RecordCourseCompletion("lava_01", 49f, (int)Medal.Gold);
            int afterGold = _progression.Currency;
            yield return null;

            Assert.Greater(afterGold, afterBronze);
            Assert.AreEqual((int)Medal.Gold, _progression.GetBestMedal("lava_01"));
        }

        private void HandleCourseCompletedFromEvent(string courseId, float completionTime, int medal)
        {
            _progression.RecordCourseCompletion(courseId, completionTime, medal, 0);
        }
    }
}
