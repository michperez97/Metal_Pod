using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetalPod.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Animation")]
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private bool useUnscaledTime = true;

        private readonly Stack<GameObject> _panelHistory = new Stack<GameObject>();
        private GameObject _activePanel;
        private Coroutine _transitionRoutine;

        public GameObject ActivePanel => _activePanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            {
                _activePanel = mainMenuPanel;
            }
            else if (hudPanel != null && hudPanel.activeSelf)
            {
                _activePanel = hudPanel;
            }
            else if (pausePanel != null && pausePanel.activeSelf)
            {
                _activePanel = pausePanel;
            }
            else if (resultsPanel != null && resultsPanel.activeSelf)
            {
                _activePanel = resultsPanel;
            }
            else if (settingsPanel != null && settingsPanel.activeSelf)
            {
                _activePanel = settingsPanel;
            }
            else if (countdownPanel != null && countdownPanel.activeSelf)
            {
                _activePanel = countdownPanel;
            }
            else if (loadingPanel != null && loadingPanel.activeSelf)
            {
                _activePanel = loadingPanel;
            }
        }

        public void ShowPanel(GameObject panel)
        {
            ShowPanelInternal(panel, true);
        }

        public void GoBack()
        {
            if (_panelHistory.Count == 0)
            {
                return;
            }

            GameObject previous = _panelHistory.Pop();
            ShowPanelInternal(previous, false);
        }

        public void HideAll()
        {
            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
                _transitionRoutine = null;
            }

            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(hudPanel, false);
            SetPanelActive(pausePanel, false);
            SetPanelActive(resultsPanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(countdownPanel, false);
            SetPanelActive(loadingPanel, false);

            _activePanel = null;
            _panelHistory.Clear();
        }

        public void ShowMainMenu() => ShowPanel(mainMenuPanel);
        public void ShowHUD() => ShowPanel(hudPanel);
        public void ShowPause() => ShowPanel(pausePanel);
        public void ShowResultsPanel() => ShowPanel(resultsPanel);
        public void ShowSettingsPanel() => ShowPanel(settingsPanel);
        public void ShowCountdown() => ShowPanel(countdownPanel);
        public void ShowLoading() => ShowPanel(loadingPanel);

        // Backward compatibility with existing calls.
        public void ShowPauseMenu(bool visible)
        {
            if (visible)
            {
                ShowPause();
            }
            else if (_activePanel == pausePanel)
            {
                GoBack();
            }
            else
            {
                SetPanelActive(pausePanel, false);
            }
        }

        // Backward compatibility helper: show panel and populate basic result payload.
        public void ShowResults(float time, int medal)
        {
            ShowResultsPanel();
            if (resultsPanel == null)
            {
                return;
            }

            ResultsScreenUI results = resultsPanel.GetComponent<ResultsScreenUI>();
            if (results != null)
            {
                results.ShowResults(time, medal);
            }
        }

        public void ShowResults(float time, System.Enum medal)
        {
            int medalValue = medal != null ? System.Convert.ToInt32(medal) : 0;
            ShowResults(time, medalValue);
        }

        private void ShowPanelInternal(GameObject panel, bool rememberCurrent)
        {
            if (panel == null)
            {
                return;
            }

            if (panel == _activePanel)
            {
                SetPanelActive(panel, true);
                return;
            }

            if (rememberCurrent && _activePanel != null)
            {
                _panelHistory.Push(_activePanel);
            }

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
            }

            _transitionRoutine = StartCoroutine(TransitionRoutine(_activePanel, panel));
        }

        private IEnumerator TransitionRoutine(GameObject from, GameObject to)
        {
            if (from != null)
            {
                yield return AnimatePanel(from, false, transitionDuration);
                SetPanelActive(from, false);
            }

            SetPanelActive(to, true);
            yield return AnimatePanel(to, true, transitionDuration);
            _activePanel = to;
            _transitionRoutine = null;
        }

        private IEnumerator AnimatePanel(GameObject panel, bool show, float duration)
        {
            if (panel == null)
            {
                yield break;
            }

            RectTransform rect = panel.transform as RectTransform;
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }

            Vector2 visiblePos = Vector2.zero;
            Vector2 hiddenPos = new Vector2(show ? Screen.width : -Screen.width * 0.25f, 0f);

            if (rect != null && show)
            {
                rect.anchoredPosition = hiddenPos;
            }

            float startAlpha = show ? 0f : 1f;
            float endAlpha = show ? 1f : 0f;
            canvasGroup.alpha = startAlpha;
            canvasGroup.blocksRaycasts = show;
            canvasGroup.interactable = show;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.Lerp(show ? hiddenPos : visiblePos, show ? visiblePos : hiddenPos, smoothT);
                }

                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            if (rect != null)
            {
                rect.anchoredPosition = show ? visiblePos : hiddenPos;
            }
        }

        private static void SetPanelActive(GameObject panel, bool isActive)
        {
            if (panel != null)
            {
                panel.SetActive(isActive);
            }
        }
    }
}
