using System.Collections.Generic;
using NUnit.Framework;
using MetalPod.Progression;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class CourseUnlockTests
    {
        private CourseUnlockData _unlockData;
        private Dictionary<string, CourseDataSO> _courses;

        [SetUp]
        public void SetUp()
        {
            _unlockData = ScriptableObject.CreateInstance<CourseUnlockData>();
            _courses = TestDataFactory.CreateProgressionCourseMap();

            _unlockData.courseOrder = new CourseDataSO[]
            {
                _courses["lava_01"], _courses["lava_02"], _courses["lava_03"],
                _courses["ice_01"], _courses["ice_02"], _courses["ice_03"],
                _courses["toxic_01"], _courses["toxic_02"], _courses["toxic_03"]
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (_courses != null)
            {
                foreach (CourseDataSO course in _courses.Values)
                {
                    TestDataFactory.DestroyAll(course);
                }
            }

            TestDataFactory.DestroyAll(_unlockData);
        }

        [Test]
        public void FirstCourse_AlwaysUnlocked()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["lava_01"], data));
        }

        [Test]
        public void SecondCourse_RequiresFirstCompleted()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.IsFalse(_unlockData.IsCourseUnlocked(_courses["lava_02"], data));

            data.completedCourses.Set("lava_01", true);
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["lava_02"], data));
        }

        [Test]
        public void IceFirstCourse_RequiresAllLavaAndFiveMedals()
        {
            SaveData data = SaveData.CreateDefault();
            data.completedCourses.Set("lava_03", true);
            data.totalMedals = 4;
            Assert.IsFalse(_unlockData.IsCourseUnlocked(_courses["ice_01"], data));

            data.totalMedals = 5;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["ice_01"], data));
        }

        [Test]
        public void ToxicFirstCourse_RequiresTwelveMedals()
        {
            SaveData data = SaveData.CreateDefault();
            data.completedCourses.Set("ice_03", true);
            data.totalMedals = 11;
            Assert.IsFalse(_unlockData.IsCourseUnlocked(_courses["toxic_01"], data));

            data.totalMedals = 12;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["toxic_01"], data));
        }

        [Test]
        public void FinalCourse_RequiresEighteenMedals()
        {
            SaveData data = SaveData.CreateDefault();
            data.completedCourses.Set("toxic_02", true);
            data.totalMedals = 17;
            Assert.IsFalse(_unlockData.IsCourseUnlocked(_courses["toxic_03"], data));

            data.totalMedals = 18;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["toxic_03"], data));
        }

        [Test]
        public void CourseNotUnlocked_PrerequisiteNotMet()
        {
            SaveData data = SaveData.CreateDefault();
            data.totalMedals = 99;

            Assert.IsFalse(_unlockData.IsCourseUnlocked(_courses["lava_02"], data));
        }

        [Test]
        public void CourseNotUnlocked_MedalRequirementNotMet()
        {
            SaveData data = SaveData.CreateDefault();
            data.completedCourses.Set("lava_03", true);
            data.totalMedals = 3;

            Assert.IsFalse(_unlockData.IsCourseUnlocked(_courses["ice_01"], data));
        }

        [Test]
        public void FullProgression_AllUnlocksInOrder()
        {
            SaveData data = SaveData.CreateDefault();

            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["lava_01"], data));

            data.completedCourses.Set("lava_01", true);
            data.totalMedals = 1;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["lava_02"], data));

            data.completedCourses.Set("lava_02", true);
            data.totalMedals = 2;
            Assert.IsFalse(_unlockData.IsCourseUnlocked(_courses["lava_03"], data));

            data.totalMedals = 3;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["lava_03"], data));

            data.completedCourses.Set("lava_03", true);
            data.totalMedals = 5;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["ice_01"], data));

            data.completedCourses.Set("ice_01", true);
            data.totalMedals = 7;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["ice_02"], data));

            data.completedCourses.Set("ice_02", true);
            data.totalMedals = 9;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["ice_03"], data));

            data.completedCourses.Set("ice_03", true);
            data.totalMedals = 12;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["toxic_01"], data));

            data.completedCourses.Set("toxic_01", true);
            data.totalMedals = 15;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["toxic_02"], data));

            data.completedCourses.Set("toxic_02", true);
            data.totalMedals = 18;
            Assert.IsTrue(_unlockData.IsCourseUnlocked(_courses["toxic_03"], data));
        }

        [Test]
        public void IsCourseUnlocked_ById_Works()
        {
            SaveData data = SaveData.CreateDefault();
            data.completedCourses.Set("lava_01", true);

            Assert.IsTrue(_unlockData.IsCourseUnlocked("lava_02", data));
        }

        [Test]
        public void GetCourseById_Unknown_ReturnsNull()
        {
            Assert.IsNull(_unlockData.GetCourseById("missing_course"));
        }
    }
}
