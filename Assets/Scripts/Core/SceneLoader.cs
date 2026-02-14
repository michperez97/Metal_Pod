using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public event Action OnLoadingScreenShown;
        public event Action OnLoadingScreenHidden;
        public event Action<string> OnScenePreloaded;

        private readonly Dictionary<string, AsyncOperation> preloadedScenes = new Dictionary<string, AsyncOperation>();
        private Coroutine activeLoadRoutine;
        private Action<bool> loadingScreenToggle;

        public bool IsLoading => activeLoadRoutine != null;

        public void BindLoadingScreen(Action<bool> toggleCallback)
        {
            loadingScreenToggle = toggleCallback;
        }

        public Coroutine LoadSceneAsync(
            string sceneName,
            Action<float> onProgress = null,
            Action onComplete = null,
            bool showLoadingScreen = true)
        {
            if (activeLoadRoutine != null)
            {
                StopCoroutine(activeLoadRoutine);
            }

            activeLoadRoutine = StartCoroutine(LoadSceneRoutine(sceneName, onProgress, onComplete, showLoadingScreen));
            return activeLoadRoutine;
        }

        public Coroutine PreloadSceneAsync(string sceneName, Action<float> onProgress = null, Action onReady = null)
        {
            if (preloadedScenes.ContainsKey(sceneName))
            {
                onReady?.Invoke();
                return null;
            }

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                onReady?.Invoke();
                return null;
            }

            return StartCoroutine(PreloadSceneRoutine(sceneName, onProgress, onReady));
        }

        public Coroutine ActivatePreloadedScene(
            string sceneName,
            Action<float> onProgress = null,
            Action onComplete = null,
            bool showLoadingScreen = true)
        {
            if (!preloadedScenes.ContainsKey(sceneName))
            {
                return LoadSceneAsync(sceneName, onProgress, onComplete, showLoadingScreen);
            }

            if (activeLoadRoutine != null)
            {
                StopCoroutine(activeLoadRoutine);
            }

            activeLoadRoutine = StartCoroutine(ActivatePreloadedRoutine(sceneName, onProgress, onComplete, showLoadingScreen));
            return activeLoadRoutine;
        }

        private IEnumerator LoadSceneRoutine(
            string sceneName,
            Action<float> onProgress,
            Action onComplete,
            bool showLoadingScreen)
        {
            if (showLoadingScreen)
            {
                ShowLoadingScreen();
            }

            AsyncOperation operation;
            if (preloadedScenes.TryGetValue(sceneName, out AsyncOperation preloadedOperation))
            {
                operation = preloadedOperation;
            }
            else
            {
                operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                if (operation == null)
                {
                    if (showLoadingScreen)
                    {
                        HideLoadingScreen();
                    }

                    activeLoadRoutine = null;
                    yield break;
                }
            }

            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                onProgress?.Invoke(progress);

                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }

            preloadedScenes.Remove(sceneName);
            onProgress?.Invoke(1f);

            if (showLoadingScreen)
            {
                HideLoadingScreen();
            }

            onComplete?.Invoke();
            activeLoadRoutine = null;
        }

        private IEnumerator PreloadSceneRoutine(string sceneName, Action<float> onProgress, Action onReady)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                yield break;
            }

            preloadedScenes[sceneName] = operation;
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                onProgress?.Invoke(progress);
                yield return null;
            }

            onProgress?.Invoke(1f);
            OnScenePreloaded?.Invoke(sceneName);
            onReady?.Invoke();
        }

        private IEnumerator ActivatePreloadedRoutine(
            string sceneName,
            Action<float> onProgress,
            Action onComplete,
            bool showLoadingScreen)
        {
            if (!preloadedScenes.TryGetValue(sceneName, out AsyncOperation operation))
            {
                activeLoadRoutine = null;
                yield break;
            }

            if (showLoadingScreen)
            {
                ShowLoadingScreen();
            }

            while (operation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                onProgress?.Invoke(progress);
                yield return null;
            }

            operation.allowSceneActivation = true;
            while (!operation.isDone)
            {
                float progress = Mathf.Lerp(0.9f, 1f, operation.progress);
                onProgress?.Invoke(progress);
                yield return null;
            }

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
            }

            preloadedScenes.Remove(sceneName);
            onProgress?.Invoke(1f);

            if (showLoadingScreen)
            {
                HideLoadingScreen();
            }

            onComplete?.Invoke();
            activeLoadRoutine = null;
        }

        private void ShowLoadingScreen()
        {
            loadingScreenToggle?.Invoke(true);
            OnLoadingScreenShown?.Invoke();
        }

        private void HideLoadingScreen()
        {
            loadingScreenToggle?.Invoke(false);
            OnLoadingScreenHidden?.Invoke();
        }
    }
}
