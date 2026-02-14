using NUnit.Framework;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class ProgressionManagerTests
    {
        private SaveFileIsolationScope _saveIsolation;
        private GameObject _go;
        private SaveSystem _saveSystem;
        private ProgressionManager _progression;
        private CourseDataSO[] _courses;
        private UpgradeDataSO[] _upgrades;
        private CosmeticDataSO[] _cosmetics;

        [SetUp]
        public void SetUp()
        {
            EventBus.Initialize();
            _saveIsolation = new SaveFileIsolationScope();

            _go = new GameObject("Test_ProgressionManager");
            _saveSystem = _go.AddComponent<SaveSystem>();
            _saveSystem.Initialize();

            _progression = _go.AddComponent<ProgressionManager>();

            var map = TestDataFactory.CreateProgressionCourseMap();
            _courses = new CourseDataSO[]
            {
                map["lava_01"], map["lava_02"], map["lava_03"],
                map["ice_01"], map["ice_02"], map["ice_03"],
                map["toxic_01"], map["toxic_02"], map["toxic_03"]
            };

            _upgrades = TestDataFactory.CreateDefaultUpgrades();
            _cosmetics = TestDataFactory.CreateDefaultCosmetics();

            TestDataFactory.SetPrivateField(_progression, "allCourses", _courses);
            TestDataFactory.SetPrivateField(_progression, "allUpgrades", _upgrades);
            TestDataFactory.SetPrivateField(_progression, "allCosmetics", _cosmetics);

            TestDataFactory.InvokePrivateMethod(_progression, "CheckAndUnlockCourses", false);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Shutdown();

            if (_courses != null)
            {
                for (int i = 0; i < _courses.Length; i++)
                {
                    TestDataFactory.DestroyAll(_courses[i]);
                }
            }

            if (_upgrades != null)
            {
                for (int i = 0; i < _upgrades.Length; i++)
                {
                    TestDataFactory.DestroyAll(_upgrades[i]);
                }
            }

            if (_cosmetics != null)
            {
                for (int i = 0; i < _cosmetics.Length; i++)
                {
                    TestDataFactory.DestroyAll(_cosmetics[i]);
                }
            }

            TestDataFactory.DestroyAll(_go);
            _saveIsolation.Dispose();
        }

        [Test]
        public void ImplementsIProgressionData()
        {
            Assert.IsTrue(_progression is IProgressionData);
        }

        [Test]
        public void InitialCurrency_MatchesSaveData()
        {
            Assert.AreEqual(_saveSystem.CurrentData.currency, _progression.Currency);
        }

        [Test]
        public void RecordCourseCompletion_SetsBestTimeAndBestMedal()
        {
            _progression.RecordCourseCompletion("lava_01", 49.5f, 3);

            Assert.AreEqual(49.5f, _progression.GetBestTime("lava_01"), 0.001f);
            Assert.AreEqual(3, _progression.GetBestMedal("lava_01"));
        }

        [Test]
        public void RecordCourseCompletion_ReplayWorseTime_DoesNotOverwriteBest()
        {
            _progression.RecordCourseCompletion("lava_01", 50f, 2);
            _progression.RecordCourseCompletion("lava_01", 55f, 2);

            Assert.AreEqual(50f, _progression.GetBestTime("lava_01"), 0.001f);
        }

        [Test]
        public void RecordCourseCompletion_BetterTime_OverwritesBest()
        {
            _progression.RecordCourseCompletion("lava_01", 60f, 1);
            _progression.RecordCourseCompletion("lava_01", 52f, 2);

            Assert.AreEqual(52f, _progression.GetBestTime("lava_01"), 0.001f);
            Assert.AreEqual(2, _progression.GetBestMedal("lava_01"));
        }

        [Test]
        public void IsCourseUnlocked_FirstLavaCourse_AlwaysTrue()
        {
            Assert.IsTrue(_progression.IsCourseUnlocked("lava_01"));
        }

        [Test]
        public void RecordCourseCompletion_UnlocksSecondCourse_WhenPrerequisiteMet()
        {
            Assert.IsFalse(_progression.IsCourseUnlocked("lava_02"));

            _progression.RecordCourseCompletion("lava_01", 49f, 3);

            Assert.IsTrue(_progression.IsCourseUnlocked("lava_02"));
        }

        [Test]
        public void EventBusCourseCompleted_TriggersProgressionRecord()
        {
            EventBus.RaiseCourseCompleted("lava_01", 51.5f, 2);

            Assert.AreEqual(51.5f, _progression.GetBestTime("lava_01"), 0.001f);
            Assert.AreEqual(2, _progression.GetBestMedal("lava_01"));
        }

        [Test]
        public void TotalMedals_TracksCompletedCoursesWithAnyMedal()
        {
            _progression.RecordCourseCompletion("lava_01", 55f, 1);
            _progression.RecordCourseCompletion("lava_02", 89f, 2);
            _progression.RecordCourseCompletion("lava_03", 130f, 0);

            Assert.AreEqual(2, _progression.TotalMedals);
        }
    }
}
