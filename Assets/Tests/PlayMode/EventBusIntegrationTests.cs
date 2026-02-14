using System.Collections;
using MetalPod.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MetalPod.Tests.PlayMode
{
    public class EventBusIntegrationTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator EventBus_CurrencyChanged_PropagatesAcrossFrames()
        {
            RuntimeCurrencyListener listener = CreateTestObject("Listener").AddComponent<RuntimeCurrencyListener>();
            RuntimeCurrencyEmitter emitter = CreateTestObject("Emitter").AddComponent<RuntimeCurrencyEmitter>();
            yield return null;

            emitter.Emit(125);
            yield return null;

            Assert.AreEqual(125, listener.LastTotal);
            Assert.AreEqual(1, listener.ReceiveCount);
        }

        [UnityTest]
        public IEnumerator EventBus_CourseCompleted_MultipleSubscribers()
        {
            int count = 0;
            EventBus.OnCourseCompleted += (_, _, _) => count++;
            EventBus.OnCourseCompleted += (_, _, _) => count++;
            EventBus.OnCourseCompleted += (_, _, _) => count++;

            EventBus.RaiseCourseCompleted("course_a", 42f, 3);
            yield return null;

            Assert.AreEqual(3, count);
        }

        [UnityTest]
        public IEnumerator EventBus_Initialize_ClearsStaleListeners()
        {
            int count = 0;
            EventBus.OnCurrencyChanged += _ => count++;

            EventBus.Initialize();
            EventBus.RaiseCurrencyChanged(100);
            yield return null;

            Assert.AreEqual(0, count);
        }

        [UnityTest]
        public IEnumerator EventBus_Shutdown_ClearsAllListeners()
        {
            int count = 0;
            EventBus.OnCurrencyChanged += _ => count++;

            EventBus.Shutdown();
            EventBus.RaiseCurrencyChanged(100);
            yield return null;

            Assert.AreEqual(0, count);
        }

        [UnityTest]
        public IEnumerator EventBus_UpgradePurchased_CarriesCorrectData()
        {
            string receivedId = null;
            int receivedLevel = -1;
            EventBus.OnUpgradePurchased += (id, level) =>
            {
                receivedId = id;
                receivedLevel = level;
            };

            EventBus.RaiseUpgradePurchased("speed", 3);
            yield return null;

            Assert.AreEqual("speed", receivedId);
            Assert.AreEqual(3, receivedLevel);
        }

        private sealed class RuntimeCurrencyListener : MonoBehaviour
        {
            public int LastTotal { get; private set; } = -1;
            public int ReceiveCount { get; private set; }

            private void OnEnable()
            {
                EventBus.OnCurrencyChanged += HandleCurrencyChanged;
            }

            private void OnDisable()
            {
                EventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            }

            private void HandleCurrencyChanged(int total)
            {
                LastTotal = total;
                ReceiveCount++;
            }
        }

        private sealed class RuntimeCurrencyEmitter : MonoBehaviour
        {
            public void Emit(int total)
            {
                EventBus.RaiseCurrencyChanged(total);
            }
        }
    }
}
