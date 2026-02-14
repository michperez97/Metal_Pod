using System.IO;
using NUnit.Framework;
using MetalPod.Progression;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class SaveSystemTests
    {
        private SaveFileIsolationScope _saveIsolation;
        private GameObject _saveObject;
        private SaveSystem _saveSystem;

        [SetUp]
        public void SetUp()
        {
            _saveIsolation = new SaveFileIsolationScope();
            _saveSystem = TestDataFactory.CreateInitializedSaveSystem(out _saveObject);
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(_saveObject);
            _saveIsolation.Dispose();
        }

        [Test]
        public void Load_NoSaveFile_ReturnsNull()
        {
            SaveData loaded = _saveSystem.Load();
            Assert.IsNull(loaded);
        }

        [Test]
        public void Save_ThenLoad_DataPreserved()
        {
            _saveSystem.CurrentData.currency = 777;
            _saveSystem.CurrentData.upgradeLevels.Set("speed", 4);
            _saveSystem.CurrentData.bestTimes.Set("lava_01", 48.23f);
            _saveSystem.CurrentData.ownedCosmetics.Add("red_black");
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            GameObject secondObject;
            SaveSystem second = TestDataFactory.CreateInitializedSaveSystem(out secondObject);
            SaveData loaded = second.Load();

            Assert.NotNull(loaded);
            Assert.AreEqual(777, loaded.currency);
            Assert.AreEqual(4, loaded.upgradeLevels.GetValueOrDefault("speed"));
            Assert.AreEqual(48.23f, loaded.bestTimes.GetValueOrDefault("lava_01"), 0.001f);
            Assert.Contains("red_black", loaded.ownedCosmetics);

            TestDataFactory.DestroyAll(secondObject);
        }

        [Test]
        public void Load_CorruptedMainFile_UsesBackup()
        {
            SaveData backupData = SaveData.CreateDefault();
            backupData.currency = 321;
            string backupJson = JsonUtility.ToJson(backupData, true);
            File.WriteAllText(_saveSystem.BackupSavePath, backupJson);
            File.WriteAllText(_saveSystem.SavePath, "this is not json");

            SaveData loaded = _saveSystem.Load();

            Assert.NotNull(loaded);
            Assert.AreEqual(321, loaded.currency);
        }

        [Test]
        public void Save_CreatesBackup_OnSecondSave()
        {
            _saveSystem.CurrentData.currency = 10;
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            _saveSystem.CurrentData.currency = 20;
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            Assert.IsTrue(File.Exists(_saveSystem.BackupSavePath));
        }

        [Test]
        public void Save_PreservesCurrency()
        {
            _saveSystem.CurrentData.currency = 500;
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            SaveData loaded = _saveSystem.Load();
            Assert.AreEqual(500, loaded.currency);
        }

        [Test]
        public void Save_PreservesUpgrades()
        {
            _saveSystem.CurrentData.upgradeLevels.Set("boost", 3);
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            SaveData loaded = _saveSystem.Load();
            Assert.AreEqual(3, loaded.upgradeLevels.GetValueOrDefault("boost"));
        }

        [Test]
        public void Save_PreservesBestTimes()
        {
            _saveSystem.CurrentData.bestTimes.Set("toxic_03", 132.45f);
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            SaveData loaded = _saveSystem.Load();
            Assert.AreEqual(132.45f, loaded.bestTimes.GetValueOrDefault("toxic_03"), 0.001f);
        }

        [Test]
        public void Save_PreservesOwnedCosmetics()
        {
            _saveSystem.CurrentData.ownedCosmetics.Add("chrome");
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            SaveData loaded = _saveSystem.Load();
            Assert.Contains("chrome", loaded.ownedCosmetics);
        }

        [Test]
        public void ResetSave_RestoresDefaults()
        {
            _saveSystem.CurrentData.currency = 999;
            _saveSystem.CurrentData.ownedCosmetics.Add("chrome");
            _saveSystem.MarkDirty();
            _saveSystem.Save();

            _saveSystem.ResetSave();

            Assert.AreEqual(0, _saveSystem.CurrentData.currency);
            Assert.Contains("default", _saveSystem.CurrentData.ownedCosmetics);
            Assert.Contains("decal_73", _saveSystem.CurrentData.ownedCosmetics);
        }
    }
}
