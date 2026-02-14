using System.Collections;
using System.IO;
using MetalPod.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MetalPod.Tests.PlayMode
{
    public class SaveLoadIntegrationTests : PlayModeTestBase
    {
        private SaveFileIsolationScope _saveIsolation;

        [SetUp]
        public void SaveSetUp()
        {
            _saveIsolation = new SaveFileIsolationScope();
        }

        [TearDown]
        public void SaveTearDown()
        {
            _saveIsolation?.Dispose();
        }

        [UnityTest]
        public IEnumerator SaveSystem_InitializeCreatesDefaultData()
        {
            SaveSystem saveSystem = CreateSaveSystem("SaveSystem_Default");
            yield return null;

            Assert.IsNotNull(saveSystem.CurrentData);
            Assert.Contains("default", saveSystem.CurrentData.ownedCosmetics);
            Assert.Contains("decal_73", saveSystem.CurrentData.ownedCosmetics);
        }

        [UnityTest]
        public IEnumerator SaveSystem_SaveAndLoad_PreservesCurrency()
        {
            SaveSystem saveSystem = CreateSaveSystem("SaveSystem_Currency_A");
            saveSystem.CurrentData.currency = 500;
            saveSystem.MarkDirty();
            saveSystem.Save();

            SaveSystem loadedSystem = CreateSaveSystem("SaveSystem_Currency_B");
            yield return null;

            Assert.AreEqual(500, loadedSystem.CurrentData.currency);
        }

        [UnityTest]
        public IEnumerator SaveSystem_SaveAndLoad_PreservesUpgradeLevels()
        {
            SaveSystem saveSystem = CreateSaveSystem("SaveSystem_Upgrade_A");
            saveSystem.CurrentData.upgradeLevels.Set("speed", 3);
            saveSystem.MarkDirty();
            saveSystem.Save();

            SaveSystem loadedSystem = CreateSaveSystem("SaveSystem_Upgrade_B");
            yield return null;

            Assert.AreEqual(3, loadedSystem.CurrentData.upgradeLevels.GetValueOrDefault("speed"));
        }

        [UnityTest]
        public IEnumerator SaveSystem_SaveAndLoad_PreservesBestTimes()
        {
            SaveSystem saveSystem = CreateSaveSystem("SaveSystem_Times_A");
            saveSystem.CurrentData.bestTimes.Set("test_course", 27.5f);
            saveSystem.MarkDirty();
            saveSystem.Save();

            SaveSystem loadedSystem = CreateSaveSystem("SaveSystem_Times_B");
            yield return null;

            Assert.AreEqual(27.5f, loadedSystem.CurrentData.bestTimes.GetValueOrDefault("test_course"), 0.001f);
        }

        [UnityTest]
        public IEnumerator SaveSystem_SaveAndLoad_PreservesCosmetics()
        {
            SaveSystem saveSystem = CreateSaveSystem("SaveSystem_Cosmetic_A");
            saveSystem.CurrentData.ownedCosmetics.Add("red_black");
            saveSystem.MarkDirty();
            saveSystem.Save();

            SaveSystem loadedSystem = CreateSaveSystem("SaveSystem_Cosmetic_B");
            yield return null;

            Assert.Contains("red_black", loadedSystem.CurrentData.ownedCosmetics);
        }

        [Test]
        public void SaveData_CreateDefault_HasDefaultCosmetics()
        {
            SaveData data = SaveData.CreateDefault();

            Assert.Contains("default", data.ownedCosmetics);
            Assert.Contains("decal_73", data.ownedCosmetics);
        }

        [UnityTest]
        public IEnumerator SaveSystem_CorruptFile_FallsBackToDefault()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "metalpod_save.json");
            string backupPath = Path.Combine(Application.persistentDataPath, "metalpod_save_backup.json");
            File.WriteAllText(savePath, "corrupt_json");

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            SaveSystem saveSystem = CreateSaveSystem("SaveSystem_Corrupt");
            yield return null;

            Assert.IsNotNull(saveSystem.CurrentData);
            Assert.AreEqual(0, saveSystem.CurrentData.currency);
            Assert.Contains("default", saveSystem.CurrentData.ownedCosmetics);
        }

        private SaveSystem CreateSaveSystem(string name)
        {
            GameObject saveObject = CreateTestObject(name);
            return saveObject.AddComponent<SaveSystem>();
        }
    }
}
