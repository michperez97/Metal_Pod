using MetalPod.Progression;
using UnityEngine;

namespace MetalPod.Tutorial
{
    public static class TutorialSaveData
    {
        private const string Prefix = "Tutorial_";
        private const string FirstPlayTutorialId = "first_play";

        public static bool IsTutorialCompleted(string tutorialId)
        {
            if (string.IsNullOrWhiteSpace(tutorialId))
            {
                return false;
            }

            if (PlayerPrefs.GetInt(Prefix + tutorialId, 0) == 1)
            {
                return true;
            }

            return ReadCompletionFromSaveData(tutorialId);
        }

        public static void SetTutorialCompleted(string tutorialId)
        {
            if (string.IsNullOrWhiteSpace(tutorialId))
            {
                return;
            }

            PlayerPrefs.SetInt(Prefix + tutorialId, 1);
            PlayerPrefs.Save();
            SyncCompletionToSaveData(tutorialId, true);
        }

        public static void ResetTutorial(string tutorialId)
        {
            if (string.IsNullOrWhiteSpace(tutorialId))
            {
                return;
            }

            PlayerPrefs.DeleteKey(Prefix + tutorialId);
            PlayerPrefs.Save();
            SyncCompletionToSaveData(tutorialId, false);
        }

        public static void ResetAllTutorials()
        {
            ResetTutorial("first_play");
            ResetTutorial("workshop_intro");
        }

        private static bool ReadCompletionFromSaveData(string tutorialId)
        {
            SaveSystem saveSystem = ResolveSaveSystem();
            if (saveSystem?.CurrentData == null)
            {
                return false;
            }

            SaveData currentData = saveSystem.CurrentData;
            if (tutorialId == FirstPlayTutorialId && currentData.tutorialCompleted)
            {
                return true;
            }

            return currentData.completedTutorials.GetValueOrDefault(tutorialId, false);
        }

        private static void SyncCompletionToSaveData(string tutorialId, bool completed)
        {
            SaveSystem saveSystem = ResolveSaveSystem();
            if (saveSystem?.CurrentData == null)
            {
                return;
            }

            SaveData currentData = saveSystem.CurrentData;
            currentData.completedTutorials.Set(tutorialId, completed);
            if (tutorialId == FirstPlayTutorialId)
            {
                currentData.tutorialCompleted = completed;
            }

            saveSystem.MarkDirty();
            saveSystem.Save();
        }

        private static SaveSystem ResolveSaveSystem()
        {
            SaveSystem saveSystem = Object.FindFirstObjectByType<SaveSystem>(FindObjectsInactive.Include);
            if (saveSystem == null)
            {
                return null;
            }

            saveSystem.Initialize();
            return saveSystem;
        }
    }
}
