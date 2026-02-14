using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MetalPod.Shared;

namespace MetalPod.Transitions
{
    // Singleton managing scene transitions. Lives on _Persistent scene.
    // Call: SceneTransitionManager.Instance.TransitionToScene("SceneName")
    // Automatically selects transition type based on scene name.
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas transitionCanvas;
        [SerializeField] private RawImage transitionImage;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI tipText;

        [Header("Materials")]
        [SerializeField] private Material fadeMaterial;
        [SerializeField] private Material wipeMaterial;
        [SerializeField] private Material dissolveMaterial;

        [Header("Timing")]
        [SerializeField] private float defaultTransitionInDuration = 0.5f;
        [SerializeField] private float defaultTransitionOutDuration = 0.5f;
        [SerializeField] private float minimumLoadingDisplayTime = 1f;

        private Coroutine _activeTransition;
        private bool _isTransitioning;

        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (transitionCanvas != null)
            {
                transitionCanvas.sortingOrder = 9999;
            }
            if (transitionImage != null)
                transitionImage.enabled = false;
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        /// <summary>
        /// Full scene transition: transition in -> load scene -> transition out.
        /// Uses SceneLoader from GameManager if available, else falls back to direct load.
        /// </summary>
        public void TransitionToScene(string sceneName, Action onComplete = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Transition already in progress.");
                return;
            }

            TransitionBase transition = GetTransitionForScene(sceneName);
            if (_activeTransition != null) StopCoroutine(_activeTransition);
            _activeTransition = StartCoroutine(TransitionSequence(sceneName, transition, onComplete));
        }

        /// <summary>
        /// Play only a transition in (cover screen), then invoke callback.
        /// Useful for custom flows.
        /// </summary>
        public void PlayTransitionIn(TransitionBase transition, float duration, Action onComplete = null)
        {
            transition.SetTarget(transitionImage, GetMaterialForTransition(transition));
            StartCoroutine(transition.PlayIn(duration, onComplete));
        }

        /// <summary>
        /// Play only a transition out (reveal screen), then invoke callback.
        /// </summary>
        public void PlayTransitionOut(TransitionBase transition, float duration, Action onComplete = null)
        {
            transition.SetTarget(transitionImage, GetMaterialForTransition(transition));
            StartCoroutine(transition.PlayOut(duration, onComplete));
        }

        private IEnumerator TransitionSequence(string sceneName, TransitionBase transition, Action onComplete)
        {
            _isTransitioning = true;

            Material mat = GetMaterialForTransition(transition);
            transition.SetTarget(transitionImage, mat);

            // Phase 1: Transition In (cover screen)
            yield return transition.PlayIn(defaultTransitionInDuration, null);

            // Phase 2: Show loading panel and load scene
            if (loadingPanel != null) loadingPanel.SetActive(true);
            if (progressBar != null) progressBar.value = 0f;

            // Pick a random loading tip
            if (tipText != null)
                tipText.text = LoadingTips.GetRandomTip();

            float loadStart = Time.unscaledTime;

            // Load via SceneLoader if available
            var sceneLoader = MetalPod.Core.GameManager.Instance?.SceneLoader;
            bool loadComplete = false;

            if (sceneLoader != null)
            {
                sceneLoader.LoadSceneAsync(sceneName,
                    onProgress: p => { if (progressBar != null) progressBar.value = p; },
                    onComplete: () => loadComplete = true,
                    showLoadingScreen: false);
            }
            else
            {
                // Fallback direct load
                var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
                while (op != null && !op.isDone)
                {
                    if (progressBar != null) progressBar.value = op.progress;
                    yield return null;
                }
                loadComplete = true;
            }

            // Wait for load
            while (!loadComplete) yield return null;

            // Ensure minimum display time for loading screen
            float elapsed = Time.unscaledTime - loadStart;
            if (elapsed < minimumLoadingDisplayTime)
            {
                yield return new WaitForSecondsRealtime(minimumLoadingDisplayTime - elapsed);
            }

            if (progressBar != null) progressBar.value = 1f;

            // Phase 3: Hide loading panel, Transition Out (reveal screen)
            if (loadingPanel != null) loadingPanel.SetActive(false);

            yield return transition.PlayOut(defaultTransitionOutDuration, null);

            _isTransitioning = false;
            _activeTransition = null;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Map scene names to transition types.
        /// </summary>
        private TransitionBase GetTransitionForScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return new FadeTransition(Color.black);
            }

            // Environment-themed transitions for courses
            if (sceneName.Contains("Inferno") || sceneName.Contains("Molten") || sceneName.Contains("Magma"))
                return new EnvironmentTransition(EnvironmentTheme.Lava);

            if (sceneName.Contains("Frozen") || sceneName.Contains("Glacial") || sceneName.Contains("Arctic"))
                return new EnvironmentTransition(EnvironmentTheme.Ice);

            if (sceneName.Contains("Rust") || sceneName.Contains("Chemical") || sceneName.Contains("Biohazard"))
                return new EnvironmentTransition(EnvironmentTheme.Toxic);

            if (sceneName == GameConstants.SCENE_WORKSHOP)
                return new WipeTransition(WipeDirection.Right);

            if (sceneName == GameConstants.SCENE_MAIN_MENU)
                return new FadeTransition(Color.black);

            // Default
            return new FadeTransition(Color.black);
        }

        private Material GetMaterialForTransition(TransitionBase transition)
        {
            if (transition is FadeTransition) return fadeMaterial;
            if (transition is WipeTransition) return wipeMaterial;
            if (transition is EnvironmentTransition) return dissolveMaterial;
            return fadeMaterial;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
