using UnityEngine;

namespace MetalPod.Tutorial
{
    public static class TutorialSaveData
    {
        private const string Prefix = "Tutorial_";

        public static bool IsTutorialCompleted(string tutorialId)
        {
            if (string.IsNullOrWhiteSpace(tutorialId))
            {
                return false;
            }

            return PlayerPrefs.GetInt(Prefix + tutorialId, 0) == 1;
        }

        public static void SetTutorialCompleted(string tutorialId)
        {
            if (string.IsNullOrWhiteSpace(tutorialId))
            {
                return;
            }

            PlayerPrefs.SetInt(Prefix + tutorialId, 1);
            PlayerPrefs.Save();
        }

        public static void ResetTutorial(string tutorialId)
        {
            if (string.IsNullOrWhiteSpace(tutorialId))
            {
                return;
            }

            PlayerPrefs.DeleteKey(Prefix + tutorialId);
            PlayerPrefs.Save();
        }

        public static void ResetAllTutorials()
        {
            ResetTutorial("first_play");
            ResetTutorial("workshop_intro");
        }
    }
}
