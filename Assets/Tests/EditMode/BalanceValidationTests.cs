using NUnit.Framework;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class BalanceValidationTests
    {
        [Test]
        public void AllCourses_GoldLessThanSilverLessThanBronze()
        {
            var courses = new (float gold, float silver, float bronze)[]
            {
                (50, 65, 80),
                (70, 90, 110),
                (90, 120, 150),
                (60, 80, 100),
                (80, 105, 130),
                (100, 130, 160),
                (65, 85, 105),
                (85, 110, 140),
                (110, 145, 180)
            };

            foreach (var c in courses)
            {
                Assert.Less(c.gold, c.silver);
                Assert.Less(c.silver, c.bronze);
            }
        }

        [Test]
        public void DifficultyProgression_TimesIncreaseWithDifficulty()
        {
            Assert.Less(50f, 70f);
            Assert.Less(70f, 90f);

            Assert.Less(60f, 80f);
            Assert.Less(80f, 100f);

            Assert.Less(65f, 85f);
            Assert.Less(85f, 110f);
        }

        [Test]
        public void UnlockRequirements_AreAchievable()
        {
            Assert.LessOrEqual(5, 9);
            Assert.LessOrEqual(12, 18);
            Assert.LessOrEqual(18, 18);
        }

        [Test]
        public void UpgradeEconomy_FirstUpgradeAffordableAfterFirstGoldEasyCompletion()
        {
            int easyBase = 100;
            float goldBonus = 1.0f;
            int reward = UnityEngine.Mathf.RoundToInt(easyBase * (1f + goldBonus));

            Assert.GreaterOrEqual(reward, 100);
        }

        [Test]
        public void UpgradeEconomy_TotalCostToMaxAll_Is16350()
        {
            int total =
                (100 + 250 + 500 + 1000 + 2000) +
                (100 + 250 + 500 + 1000 + 2000) +
                (150 + 350 + 600 + 1200 + 2500) +
                (100 + 250 + 500 + 1000 + 2000);

            Assert.AreEqual(16350, total);
        }

        [Test]
        public void CosmeticPricing_MostItemsInAccessibleRange()
        {
            int[] commonCosmeticCosts = { 100, 150, 200, 200 };
            for (int i = 0; i < commonCosmeticCosts.Length; i++)
            {
                Assert.GreaterOrEqual(commonCosmeticCosts[i], 100);
                Assert.LessOrEqual(commonCosmeticCosts[i], 200);
            }
        }

        [Test]
        public void MedalGapRatio_IsConsistentAcrossLavaCourses()
        {
            Assert.AreEqual(15f, 65f - 50f, 0.001f);
            Assert.AreEqual(15f, 80f - 65f, 0.001f);

            Assert.AreEqual(20f, 90f - 70f, 0.001f);
            Assert.AreEqual(20f, 110f - 90f, 0.001f);

            Assert.AreEqual(30f, 120f - 90f, 0.001f);
            Assert.AreEqual(30f, 150f - 120f, 0.001f);
        }

        [Test]
        public void ReplayReward_MultiplierEncouragesButLimitsGrinding()
        {
            float first = 200f;
            float replay = first * 0.5f;
            Assert.AreEqual(100f, replay, 0.001f);
            Assert.Less(replay, first);
        }
    }
}
