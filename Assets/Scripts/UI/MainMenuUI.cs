using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetalPod.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Title")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Animator titleAnimator;

        [Header("Background")]
        [SerializeField] private RawImage backgroundImage;

        [Header("New Game Confirmation")]
        [SerializeField] private GameObject confirmNewGamePanel;
        [SerializeField] private Button confirmNewGameYesButton;
        [SerializeField] private Button confirmNewGameNoButton;

        [Header("Data")]
        [SerializeField] private MonoBehaviour saveSystemSource;
        [SerializeField] private string fallbackSaveFileName = "metal_pod_save.json";
        [SerializeField] private string workshopSceneNameOverride = "";

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinue);
            }

            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGamePressed);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettings);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuit);
            }

            if (confirmNewGameYesButton != null)
            {
                confirmNewGameYesButton.onClick.AddListener(ConfirmNewGame);
            }

            if (confirmNewGameNoButton != null)
            {
                confirmNewGameNoButton.onClick.AddListener(CancelNewGame);
            }
        }

        private void Start()
        {
            if (titleText != null && string.IsNullOrEmpty(titleText.text))
            {
                titleText.text = "METAL POD";
            }

            if (titleAnimator != null)
            {
                titleAnimator.SetTrigger("Intro");
            }

            if (confirmNewGamePanel != null)
            {
                confirmNewGamePanel.SetActive(false);
            }

            SetContinueVisible(HasSaveData());
        }

        public void SetContinueVisible(bool isVisible)
        {
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(isVisible);
            }
        }

        public void OnContinue()
        {
            LoadWorkshopScene();
        }

        public void OnNewGamePressed()
        {
            if (confirmNewGamePanel != null)
            {
                confirmNewGamePanel.SetActive(true);
                return;
            }

            ConfirmNewGame();
        }

        public void ConfirmNewGame()
        {
            ResetSaveData();
            if (confirmNewGamePanel != null)
            {
                confirmNewGamePanel.SetActive(false);
            }

            LoadWorkshopScene();
        }

        public void CancelNewGame()
        {
            if (confirmNewGamePanel != null)
            {
                confirmNewGamePanel.SetActive(false);
            }
        }

        public void OnSettings()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSettingsPanel();
            }
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private bool HasSaveData()
        {
            if (saveSystemSource != null)
            {
                string savePath = ReflectionValueReader.GetString(saveSystemSource, "SavePath", string.Empty);
                if (!string.IsNullOrEmpty(savePath))
                {
                    return File.Exists(savePath);
                }
            }

            string fallbackPath = Path.Combine(Application.persistentDataPath, fallbackSaveFileName);
            return File.Exists(fallbackPath);
        }

        private void ResetSaveData()
        {
            if (saveSystemSource == null)
            {
                saveSystemSource = FindSourceWithMethod("ResetSave");
            }

            if (saveSystemSource != null)
            {
                ReflectionValueReader.Invoke(saveSystemSource, "ResetSave");
            }
        }

        private void LoadWorkshopScene()
        {
            string targetScene = string.IsNullOrEmpty(workshopSceneNameOverride)
                ? SharedContractsBridge.SceneWorkshop
                : workshopSceneNameOverride;

            MonoBehaviour sceneLoader = FindSourceWithMethod("LoadSceneAsync");
            if (sceneLoader != null)
            {
                ReflectionValueReader.Invoke(sceneLoader, "LoadSceneAsync", targetScene, null, null);
            }
            else
            {
                SceneManager.LoadScene(targetScene);
            }
        }

        private static MonoBehaviour FindSourceWithMethod(string methodName)
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                MethodInfo method = behaviour.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    return behaviour;
                }
            }

            return null;
        }
    }
}
