using System.Collections;
using MetalPod.ScriptableObjects;
using MetalPod.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Workshop
{
    public class WorkshopManager : MonoBehaviour
    {
        public enum WorkshopPanel
        {
            None,
            Upgrades,
            Customization,
            CourseSelection
        }

        [Header("Panels")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private GameObject customizationPanel;
        [SerializeField] private GameObject courseSelectionPanel;

        [Header("References")]
        [SerializeField] private Transform hovercraftDisplay;
        [SerializeField] private Transform protagonistPosition;
        [SerializeField] private WorkshopCameraController workshopCamera;
        [SerializeField] private CurrencyDisplay currencyDisplay;
        [SerializeField] private ProtagonistController protagonistController;

        [Header("Courses")]
        [SerializeField] private CourseDataSO[] availableCourses;
        [SerializeField] private string workshopSceneNameOverride = "";

        [Header("Transitions")]
        [SerializeField] private float panelFadeDuration = 0.25f;

        private WorkshopPanel _activePanel = WorkshopPanel.None;
        private Coroutine _panelTransitionRoutine;

        public WorkshopPanel ActivePanel => _activePanel;

        private void Start()
        {
            if (currencyDisplay != null)
            {
                currencyDisplay.gameObject.SetActive(true);
            }

            OpenUpgrades();
        }

        public void OpenUpgrades()
        {
            SetPanel(WorkshopPanel.Upgrades);
            if (workshopCamera != null)
            {
                workshopCamera.FocusOnHovercraft();
            }

            if (protagonistController != null)
            {
                protagonistController.SetWorking();
            }
        }

        public void OpenCustomization()
        {
            SetPanel(WorkshopPanel.Customization);
            if (workshopCamera != null)
            {
                workshopCamera.FocusOnHovercraft();
            }

            if (protagonistController != null)
            {
                protagonistController.SetIdle();
            }
        }

        public void OpenCourseSelection()
        {
            SetPanel(WorkshopPanel.CourseSelection);
            if (workshopCamera != null)
            {
                workshopCamera.FocusOnMap();
            }

            if (protagonistController != null)
            {
                protagonistController.SetIdle();
            }
        }

        // Backward compatibility methods.
        public void ShowUpgrades() => OpenUpgrades();
        public void ShowCustomization() => OpenCustomization();
        public void ShowCourseSelection() => OpenCourseSelection();

        public void LaunchCourse(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                return;
            }

            SharedContractsBridge.Raise("RaiseCourseSelected", courseId);

            if (protagonistController != null)
            {
                protagonistController.SetCelebrating();
            }

            string sceneToLoad = ResolveSceneFromCourseId(courseId);
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                sceneToLoad = courseId;
            }

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                sceneToLoad = string.IsNullOrEmpty(workshopSceneNameOverride)
                    ? SharedContractsBridge.SceneWorkshop
                    : workshopSceneNameOverride;
            }

            SceneManager.LoadScene(sceneToLoad);
        }

        private void SetPanel(WorkshopPanel panel)
        {
            if (_activePanel == panel)
            {
                return;
            }

            if (_panelTransitionRoutine != null)
            {
                StopCoroutine(_panelTransitionRoutine);
            }

            _panelTransitionRoutine = StartCoroutine(SetPanelRoutine(panel));
        }

        private IEnumerator SetPanelRoutine(WorkshopPanel panel)
        {
            GameObject targetPanel = GetPanelObject(panel);
            GameObject currentPanel = GetPanelObject(_activePanel);

            if (currentPanel != null)
            {
                yield return FadePanel(currentPanel, false);
                currentPanel.SetActive(false);
            }

            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
                yield return FadePanel(targetPanel, true);
            }

            _activePanel = panel;
            _panelTransitionRoutine = null;
        }

        private IEnumerator FadePanel(GameObject panel, bool show)
        {
            if (panel == null)
            {
                yield break;
            }

            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = panel.AddComponent<CanvasGroup>();
            }

            float start = show ? 0f : 1f;
            float end = show ? 1f : 0f;
            group.alpha = start;
            group.interactable = show;
            group.blocksRaycasts = show;

            float elapsed = 0f;
            while (elapsed < panelFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = panelFadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / panelFadeDuration);
                group.alpha = Mathf.Lerp(start, end, t);
                yield return null;
            }

            group.alpha = end;
        }

        private GameObject GetPanelObject(WorkshopPanel panel)
        {
            switch (panel)
            {
                case WorkshopPanel.Upgrades:
                    return upgradePanel;
                case WorkshopPanel.Customization:
                    return customizationPanel;
                case WorkshopPanel.CourseSelection:
                    return courseSelectionPanel;
                default:
                    return null;
            }
        }

        private string ResolveSceneFromCourseId(string courseId)
        {
            if (availableCourses == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < availableCourses.Length; i++)
            {
                CourseDataSO course = availableCourses[i];
                if (course == null)
                {
                    continue;
                }

                if (course.courseId == courseId)
                {
                    return course.sceneName;
                }
            }

            return string.Empty;
        }
    }
}
