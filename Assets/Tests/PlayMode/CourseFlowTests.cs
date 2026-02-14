using System.Collections;
using MetalPod.Course;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MetalPod.Tests.PlayMode
{
    public class CourseFlowTests : PlayModeTestBase
    {
        [Test]
        public void CourseManager_StartsInReadyState()
        {
            CourseScenario scenario = CreateCourseScenario();
            Assert.AreEqual(CourseRunState.Ready, scenario.Manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator CourseManager_CountdownToRacing()
        {
            CourseScenario scenario = CreateCourseScenario();

            yield return null;

            Assert.AreEqual(CourseRunState.Racing, scenario.Manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator Checkpoint_ActivatesOnPlayerTrigger()
        {
            CourseScenario scenario = CreateCourseScenario();
            yield return null;

            bool fired = false;
            int receivedIndex = -1;
            scenario.Manager.OnCheckpointReached += index =>
            {
                fired = true;
                receivedIndex = index;
            };

            Collider playerCollider = scenario.Player.GetComponent<Collider>();
            scenario.Checkpoint.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);

            yield return null;

            Assert.IsTrue(fired);
            Assert.AreEqual(0, receivedIndex);
            Assert.IsTrue(scenario.Checkpoint.IsActive);
        }

        [UnityTest]
        public IEnumerator CourseFinish_TransitionsToFinishedState()
        {
            CourseScenario scenario = CreateCourseScenario();
            yield return null;

            Collider playerCollider = scenario.Player.GetComponent<Collider>();
            scenario.FinishLine.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);

            yield return null;

            Assert.AreEqual(CourseRunState.Finished, scenario.Manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator CourseFinish_BroadcastsEventBus()
        {
            CourseScenario scenario = CreateCourseScenario();
            yield return null;

            string courseId = null;
            float time = -1f;
            int medal = -1;
            EventBus.OnCourseCompleted += (id, completionTime, completionMedal) =>
            {
                courseId = id;
                time = completionTime;
                medal = completionMedal;
            };

            SetAutoProperty(scenario.Timer, "ElapsedTime", 29f);
            Collider playerCollider = scenario.Player.GetComponent<Collider>();
            scenario.FinishLine.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);

            yield return null;

            Assert.AreEqual("test_course", courseId);
            Assert.AreEqual(29f, time, 0.001f);
            Assert.AreEqual((int)Medal.Gold, medal);
        }

        [UnityTest]
        public IEnumerator MedalSystem_AwardsGoldForFastTime()
        {
            CourseScenario scenario = CreateCourseScenario();
            yield return null;

            Medal receivedMedal = Medal.None;
            scenario.Manager.OnCourseCompleted += (medal, _) => receivedMedal = medal;

            SetAutoProperty(scenario.Timer, "ElapsedTime", 25f);
            Collider playerCollider = scenario.Player.GetComponent<Collider>();
            scenario.FinishLine.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);

            yield return null;

            Assert.AreEqual(Medal.Gold, receivedMedal);
        }

        [UnityTest]
        public IEnumerator MedalSystem_AwardsBronzeForSlowTime()
        {
            CourseScenario scenario = CreateCourseScenario();
            yield return null;

            Medal receivedMedal = Medal.None;
            scenario.Manager.OnCourseCompleted += (medal, _) => receivedMedal = medal;

            SetAutoProperty(scenario.Timer, "ElapsedTime", 58f);
            Collider playerCollider = scenario.Player.GetComponent<Collider>();
            scenario.FinishLine.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);

            yield return null;

            Assert.AreEqual(Medal.Bronze, receivedMedal);
        }

        private CourseScenario CreateCourseScenario()
        {
            HovercraftStatsSO hovercraftStats = TrackAsset(PlayModeTestFactory.CreateHovercraftStats());
            CourseDataSO courseData = TrackAsset(PlayModeTestFactory.CreateCourseData(
                "test_course",
                30f,
                45f,
                60f,
                DifficultyLevel.Easy,
                EnvironmentType.Ice,
                1));

            GameObject hovercraftObject = TrackObject(PlayModeTestFactory.CreateHovercraft(hovercraftStats));
            HovercraftController player = hovercraftObject.GetComponent<HovercraftController>();

            GameObject checkpointObject = TrackObject(PlayModeTestFactory.CreateCheckpoint(Vector3.forward * 10f, 0));
            Checkpoint checkpoint = checkpointObject.GetComponent<Checkpoint>();

            GameObject finishObject = TrackObject(PlayModeTestFactory.CreateFinishLine(Vector3.forward * 20f));
            FinishLine finishLine = finishObject.GetComponent<FinishLine>();

            GameObject timerObject = CreateTestObject("CourseTimer");
            CourseTimer timer = timerObject.AddComponent<CourseTimer>();

            GameObject managerObject = CreateTestObject("CourseManager");
            CourseManager manager = managerObject.AddComponent<CourseManager>();

            SetPrivateField(manager, "courseData", courseData);
            SetPrivateField(manager, "courseTimer", timer);
            SetPrivateField(manager, "finishLine", finishLine);
            SetPrivateField(manager, "defaultSpawnPoint", hovercraftObject.transform);
            SetPrivateField(manager, "countdownSeconds", 0f);
            SetPrivateField(manager, "introDurationSeconds", 0f);
            SetPrivateField(manager, "lockPlayerInputDuringIntro", false);
            SetPrivateField(manager, "respawnDelaySeconds", 0f);

            manager.RegisterPlayer(player);

            return new CourseScenario
            {
                Manager = manager,
                Player = player,
                Checkpoint = checkpoint,
                FinishLine = finishLine,
                Timer = timer
            };
        }

        private sealed class CourseScenario
        {
            public CourseManager Manager;
            public HovercraftController Player;
            public Checkpoint Checkpoint;
            public FinishLine FinishLine;
            public CourseTimer Timer;
        }
    }
}
