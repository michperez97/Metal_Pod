using NUnit.Framework;
using MetalPod.Course;
using MetalPod.ScriptableObjects;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class MedalSystemTests
    {
        [Test]
        public void EvaluatePerformance_UnderGoldTime_ReturnsGold()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("test", 50f, 65f, 80f);
            Assert.AreEqual(Medal.Gold, MedalSystem.EvaluatePerformance(45f, course));
            TestDataFactory.DestroyAll(course);
        }

        [Test]
        public void EvaluatePerformance_ExactGoldTime_ReturnsGold()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("test", 50f, 65f, 80f);
            Assert.AreEqual(Medal.Gold, MedalSystem.EvaluatePerformance(50f, course));
            TestDataFactory.DestroyAll(course);
        }

        [Test]
        public void EvaluatePerformance_BetweenGoldAndSilver_ReturnsSilver()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("test", 50f, 65f, 80f);
            Assert.AreEqual(Medal.Silver, MedalSystem.EvaluatePerformance(55f, course));
            TestDataFactory.DestroyAll(course);
        }

        [Test]
        public void EvaluatePerformance_ExactSilverTime_ReturnsSilver()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("test", 50f, 65f, 80f);
            Assert.AreEqual(Medal.Silver, MedalSystem.EvaluatePerformance(65f, course));
            TestDataFactory.DestroyAll(course);
        }

        [Test]
        public void EvaluatePerformance_BetweenSilverAndBronze_ReturnsBronze()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("test", 50f, 65f, 80f);
            Assert.AreEqual(Medal.Bronze, MedalSystem.EvaluatePerformance(70f, course));
            TestDataFactory.DestroyAll(course);
        }

        [Test]
        public void EvaluatePerformance_ExactBronzeTime_ReturnsBronze()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("test", 50f, 65f, 80f);
            Assert.AreEqual(Medal.Bronze, MedalSystem.EvaluatePerformance(80f, course));
            TestDataFactory.DestroyAll(course);
        }

        [Test]
        public void EvaluatePerformance_AboveBronze_ReturnsNone()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("test", 50f, 65f, 80f);
            Assert.AreEqual(Medal.None, MedalSystem.EvaluatePerformance(120f, course));
            TestDataFactory.DestroyAll(course);
        }

        [Test]
        public void EvaluatePerformance_NullCourse_ReturnsNone()
        {
            Assert.AreEqual(Medal.None, MedalSystem.EvaluatePerformance(42f, null));
        }

        [Test]
        public void EvaluatePerformance_AllLavaCourses_ValidThresholds()
        {
            AssertThresholdOrder(50f, 65f, 80f);
            AssertThresholdOrder(70f, 90f, 110f);
            AssertThresholdOrder(90f, 120f, 150f);
        }

        [Test]
        public void EvaluatePerformance_AllIceCourses_ValidThresholds()
        {
            AssertThresholdOrder(60f, 80f, 100f);
            AssertThresholdOrder(80f, 105f, 130f);
            AssertThresholdOrder(100f, 130f, 160f);
        }

        [Test]
        public void EvaluatePerformance_AllToxicCourses_ValidThresholds()
        {
            AssertThresholdOrder(65f, 85f, 105f);
            AssertThresholdOrder(85f, 110f, 140f);
            AssertThresholdOrder(110f, 145f, 180f);
        }

        [Test]
        public void EvaluatePerformance_BoundaryValues_ClassifiedCorrectly()
        {
            CourseDataSO course = TestDataFactory.CreateCourse("boundary", 100f, 130f, 160f);

            Assert.AreEqual(Medal.Gold, MedalSystem.EvaluatePerformance(99.99f, course));
            Assert.AreEqual(Medal.Silver, MedalSystem.EvaluatePerformance(100.01f, course));
            Assert.AreEqual(Medal.Bronze, MedalSystem.EvaluatePerformance(130.01f, course));
            Assert.AreEqual(Medal.None, MedalSystem.EvaluatePerformance(160.01f, course));

            TestDataFactory.DestroyAll(course);
        }

        private static void AssertThresholdOrder(float gold, float silver, float bronze)
        {
            Assert.Less(gold, silver);
            Assert.Less(silver, bronze);
        }
    }
}
