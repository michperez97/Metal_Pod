using NUnit.Framework;
using MetalPod.Progression;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class SaveDataTests
    {
        [Test]
        public void CreateDefault_HasZeroCurrency()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.AreEqual(0, data.currency);
        }

        [Test]
        public void CreateDefault_OwnsDefaultCosmetic()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.Contains("default", data.ownedCosmetics);
        }

        [Test]
        public void CreateDefault_OwnsStarterDecal()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.Contains("decal_73", data.ownedCosmetics);
        }

        [Test]
        public void CreateDefault_VersionIsOne()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.AreEqual(1, data.version);
        }

        [Test]
        public void GetTotalMedals_NoMedals_ReturnsZero()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.AreEqual(0, data.GetTotalMedals());
        }

        [Test]
        public void GetTotalMedals_WithMixedMedals_CountsOnlyNonZeroEntries()
        {
            SaveData data = SaveData.CreateDefault();
            data.bestMedals.Set("lava_01", 3);
            data.bestMedals.Set("lava_02", 0);
            data.bestMedals.Set("lava_03", 1);
            data.bestMedals.Set("ice_01", 2);

            Assert.AreEqual(3, data.GetTotalMedals());
        }

        [Test]
        public void CreateDefault_NoCourseProgress()
        {
            SaveData data = SaveData.CreateDefault();
            Assert.AreEqual(0, data.totalCoursesCompleted);
            Assert.AreEqual(0, data.totalMedals);
        }

        [Test]
        public void SerializableIntDict_SetAndGet_Works()
        {
            SerializableIntDict dict = new SerializableIntDict();
            dict.Set("speed", 2);

            Assert.IsTrue(dict.ContainsKey("speed"));
            Assert.AreEqual(2, dict.GetValueOrDefault("speed"));
            Assert.AreEqual(2, dict["speed"]);
        }

        [Test]
        public void SerializableFloatDict_SetAndGet_Works()
        {
            SerializableFloatDict dict = new SerializableFloatDict();
            dict.Set("lava_01", 53.42f);

            Assert.IsTrue(dict.ContainsKey("lava_01"));
            Assert.AreEqual(53.42f, dict.GetValueOrDefault("lava_01"), 0.0001f);
            Assert.AreEqual(53.42f, dict["lava_01"], 0.0001f);
        }

        [Test]
        public void SerializableBoolDict_SetAndGet_Works()
        {
            SerializableBoolDict dict = new SerializableBoolDict();
            dict.Set("lava_01", true);

            Assert.IsTrue(dict.ContainsKey("lava_01"));
            Assert.IsTrue(dict.GetValueOrDefault("lava_01"));
            Assert.IsTrue(dict["lava_01"]);
        }

        [Test]
        public void SerializableDict_Remove_RemovesEntry()
        {
            SerializableIntDict dict = new SerializableIntDict();
            dict.Set("speed", 1);
            bool removed = dict.Remove("speed");

            Assert.IsTrue(removed);
            Assert.IsFalse(dict.ContainsKey("speed"));
            Assert.AreEqual(0, dict.GetValueOrDefault("speed"));
        }

        [Test]
        public void SerializableDict_ToDictionary_ContainsAllEntries()
        {
            SerializableIntDict dict = new SerializableIntDict();
            dict.Set("speed", 3);
            dict.Set("handling", 1);

            var runtime = dict.ToDictionary();

            Assert.AreEqual(2, runtime.Count);
            Assert.AreEqual(3, runtime["speed"]);
            Assert.AreEqual(1, runtime["handling"]);
        }
    }
}
