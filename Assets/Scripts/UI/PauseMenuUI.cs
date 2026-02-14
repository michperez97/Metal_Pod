using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetalPod.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private string workshopSceneNameOverride = "";

        private Coroutine _fadeRoutine;

        private void Awake()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResume);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestart);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettings);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuit);
            }
        }

        private void OnEnable()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;
            StartFade(1f, true);
        }

        public void Hide()
        {
            Time.timeScale = 1f;
            StartFade(0f, false);
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void OnResume()
        {
            Hide();
        }

        private void OnRestart()
        {
            Time.timeScale = 1f;
            Scene current = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(current.name))
            {
                SceneManager.LoadScene(current.name);
            }
        }

        private void OnSettings()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSettingsPanel();
            }
        }

        private void OnQuit()
        {
            Time.timeScale = 1f;
            string workshopScene = string.IsNullOrEmpty(workshopSceneNameOverride)
                ? SharedContractsBridge.SceneWorkshop
                : workshopSceneNameOverride;

            MonoBehaviour sceneLoader = FindSourceWithMethod("LoadSceneAsync");
            if (sceneLoader != null)
            {
                ReflectionValueReader.Invoke(sceneLoader, "LoadSceneAsync", workshopScene, null, null);
            }
            else
            {
                SceneManager.LoadScene(workshopScene);
            }
        }

        private void StartFade(float targetAlpha, bool show)
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(show);
                return;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, show));
        }

        private IEnumerator FadeRoutine(float targetAlpha, bool show)
        {
            if (show)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;

            if (!show)
            {
                gameObject.SetActive(false);
            }

            _fadeRoutine = null;
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
