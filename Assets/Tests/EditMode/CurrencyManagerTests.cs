using NUnit.Framework;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class CurrencyManagerTests
    {
        private SaveFileIsolationScope _saveIsolation;
        private GameObject _saveObject;
        private SaveSystem _saveSystem;
        private CurrencyManager _currency;

        [SetUp]
        public void SetUp()
        {
            EventBus.Initialize();
            _saveIsolation = new SaveFileIsolationScope();
            _saveSystem = TestDataFactory.CreateInitializedSaveSystem(out _saveObject);
            _currency = new CurrencyManager();
            _currency.Initialize(_saveSystem);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Shutdown();
            TestDataFactory.DestroyAll(_saveObject);
            _saveIsolation.Dispose();
        }

        [Test]
        public void AddCurrency_IncreasesBalance()
        {
            _currency.AddCurrency(100);
            Assert.AreEqual(100, _currency.Currency);
        }

        [Test]
        public void SpendCurrency_DecreasesBalance()
        {
            _currency.AddCurrency(200);
            bool result = _currency.SpendCurrency(100);

            Assert.IsTrue(result);
            Assert.AreEqual(100, _currency.Currency);
        }

        [Test]
        public void SpendCurrency_InsufficientFunds_ReturnsFalse()
        {
            _currency.AddCurrency(50);
            bool result = _currency.SpendCurrency(100);

            Assert.IsFalse(result);
            Assert.AreEqual(50, _currency.Currency);
        }

        [Test]
        public void SpendCurrency_ExactAmount_ReturnsTrue_BalanceZero()
        {
            _currency.AddCurrency(100);
            bool result = _currency.SpendCurrency(100);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _currency.Currency);
        }

        [Test]
        public void CanAfford_SufficientFunds_ReturnsTrue()
        {
            _currency.AddCurrency(200);
            Assert.IsTrue(_currency.CanAfford(150));
        }

        [Test]
        public void CanAfford_InsufficientFunds_ReturnsFalse()
        {
            _currency.AddCurrency(50);
            Assert.IsFalse(_currency.CanAfford(100));
        }

        [Test]
        public void AwardCourseCompletion_GoldReward_IsHigherThanBronze()
        {
            CourseDataSO easyCourse = TestDataFactory.CreateCourse("lava_01", 50f, 65f, 80f, DifficultyLevel.Easy);

            int bronze = _currency.AwardCourseCompletion(easyCourse, 78f, 1, 0, true);
            int gold = _currency.AwardCourseCompletion(easyCourse, 49f, 3, 0, true);

            Assert.Greater(gold, bronze);
            TestDataFactory.DestroyAll(easyCourse);
        }

        [Test]
        public void AwardCourseCompletion_FirstCompletion_HasHigherRewardThanReplay()
        {
            CourseDataSO mediumCourse = TestDataFactory.CreateCourse("ice_01", 60f, 80f, 100f, DifficultyLevel.Medium);

            int first = _currency.AwardCourseCompletion(mediumCourse, 79f, 2, 0, true);
            int replay = _currency.AwardCourseCompletion(mediumCourse, 79f, 2, 0, false);

            Assert.Greater(first, replay);
            TestDataFactory.DestroyAll(mediumCourse);
        }

        [Test]
        public void AwardCourseCompletion_WithCollectibles_BonusIsAdded()
        {
            CourseDataSO easyCourse = TestDataFactory.CreateCourse("lava_02", 70f, 90f, 110f, DifficultyLevel.Easy);

            int withZeroCollectibles = _currency.AwardCourseCompletion(easyCourse, 88f, 0, 0, true);
            int withFiveCollectibles = _currency.AwardCourseCompletion(easyCourse, 88f, 0, 5, true);

            Assert.AreEqual(50, withFiveCollectibles - withZeroCollectibles);
            TestDataFactory.DestroyAll(easyCourse);
        }

        [Test]
        public void AwardCourseCompletion_NoMedal_BaseRewardOnly()
        {
            CourseDataSO mediumCourse = TestDataFactory.CreateCourse("ice_02", 80f, 105f, 130f, DifficultyLevel.Medium);

            int reward = _currency.AwardCourseCompletion(mediumCourse, 999f, 0, 0, true);

            Assert.AreEqual(150, reward);
            TestDataFactory.DestroyAll(mediumCourse);
        }

        [Test]
        public void AwardCourseCompletion_EasyVsHard_DifferentBaseRewards()
        {
            CourseDataSO easyCourse = TestDataFactory.CreateCourse("lava_03", 90f, 120f, 150f, DifficultyLevel.Easy);
            CourseDataSO hardCourse = TestDataFactory.CreateCourse("toxic_01", 65f, 85f, 105f, DifficultyLevel.Hard);

            int easyReward = _currency.AwardCourseCompletion(easyCourse, 140f, 0, 0, true);
            int hardReward = _currency.AwardCourseCompletion(hardCourse, 140f, 0, 0, true);

            Assert.AreEqual(100, easyReward);
            Assert.AreEqual(200, hardReward);

            TestDataFactory.DestroyAll(easyCourse, hardCourse);
        }

        [Test]
        public void AwardCourseCompletion_RaisesCurrencyEarnedEvent()
        {
            int received = -1;
            EventBus.OnCurrencyEarned += amount => received = amount;

            CourseDataSO easyCourse = TestDataFactory.CreateCourse("event_course", 50f, 60f, 70f, DifficultyLevel.Easy);
            int awarded = _currency.AwardCourseCompletion(easyCourse, 45f, 3, 0, true);

            Assert.AreEqual(awarded, received);
            TestDataFactory.DestroyAll(easyCourse);
        }

        [Test]
        public void AddCurrency_RaisesCurrencyChangedEvent()
        {
            int received = -1;
            EventBus.OnCurrencyChanged += total => received = total;

            _currency.AddCurrency(125);

            Assert.AreEqual(125, received);
        }

        [Test]
        public void SpendCurrency_WithNonPositiveAmount_ReturnsFalse()
        {
            _currency.AddCurrency(100);
            Assert.IsFalse(_currency.SpendCurrency(0));
            Assert.IsFalse(_currency.SpendCurrency(-50));
            Assert.AreEqual(100, _currency.Currency);
        }
    }
}
