using NUnit.Framework;
using MetalPod.Shared;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class EventBusTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Shutdown();
        }

        [Test]
        public void Initialize_SetsIsInitializedTrue()
        {
            Assert.IsTrue(EventBus.IsInitialized);
        }

        [Test]
        public void Shutdown_SetsIsInitializedFalse()
        {
            EventBus.Shutdown();
            Assert.IsFalse(EventBus.IsInitialized);
        }

        [Test]
        public void OnCurrencyChanged_FiresWithCorrectValue()
        {
            int received = -1;
            EventBus.OnCurrencyChanged += value => received = value;

            EventBus.RaiseCurrencyChanged(500);

            Assert.AreEqual(500, received);
        }

        [Test]
        public void OnCourseCompleted_FiresWithAllParams()
        {
            string receivedId = null;
            float receivedTime = -1f;
            int receivedMedal = -1;

            EventBus.OnCourseCompleted += (id, time, medal) =>
            {
                receivedId = id;
                receivedTime = time;
                receivedMedal = medal;
            };

            EventBus.RaiseCourseCompleted("lava_01", 52.5f, 3);

            Assert.AreEqual("lava_01", receivedId);
            Assert.AreEqual(52.5f, receivedTime, 0.001f);
            Assert.AreEqual(3, receivedMedal);
        }

        [Test]
        public void OnUpgradePurchased_FiresWithIdAndLevel()
        {
            string receivedId = null;
            int receivedLevel = -1;

            EventBus.OnUpgradePurchased += (id, level) =>
            {
                receivedId = id;
                receivedLevel = level;
            };

            EventBus.RaiseUpgradePurchased("speed", 3);

            Assert.AreEqual("speed", receivedId);
            Assert.AreEqual(3, receivedLevel);
        }

        [Test]
        public void MultipleListeners_AllReceiveEvent()
        {
            int count = 0;
            EventBus.OnCurrencyChanged += _ => count++;
            EventBus.OnCurrencyChanged += _ => count++;

            EventBus.RaiseCurrencyChanged(100);

            Assert.AreEqual(2, count);
        }

        [Test]
        public void NoListeners_RaiseDoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus.RaiseCurrencyChanged(100));
            Assert.DoesNotThrow(() => EventBus.RaiseCourseCompleted("test", 0f, 0));
            Assert.DoesNotThrow(() => EventBus.RaiseCosmeticEquipped("default"));
        }

        [Test]
        public void Shutdown_ClearsListeners()
        {
            int count = 0;
            EventBus.OnCurrencyChanged += _ => count++;

            EventBus.Shutdown();
            EventBus.RaiseCurrencyChanged(123);

            Assert.AreEqual(0, count);
        }
    }
}
