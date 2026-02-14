#if DEVELOPMENT_BUILD || UNITY_EDITOR
using MetalPod.Core;
using MetalPod.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Debugging
{
    /// <summary>
    /// GUI utility to jump directly to course and utility scenes.
    /// </summary>
    public class DebugCourseSkip : MonoBehaviour
    {
        [Header("Scene Lists")]
        [SerializeField] private string[] courseScenes =
        {
            "TestCourse",
            "InfernoGate",
            "MoltenRidge",
            "MagmaCanyon",
            "FrozenPass",
            "GlacialRavine",
            "ArcticStorm",
            "RustValley",
            "ChemicalPlant",
            "BiohazardCore"
        };

        [SerializeField] private string[] utilityScenes =
        {
            GameConstants.SCENE_MAIN_MENU,
            GameConstants.SCENE_WORKSHOP,
            GameConstants.SCENE_PERSISTENT
        };

        private bool _isVisible;
        private Vector2 _scrollPosition;
        private Rect _windowRect = new Rect(12f, 12f, 280f, 440f);

        public bool IsVisible => _isVisible;

        public void Toggle() => _isVisible = !_isVisible;
        public void Show() => _isVisible = true;
        public void Hide() => _isVisible = false;

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            _windowRect = GUI.Window(8121, _windowRect, DrawWindow, "Course Skip");
        }

        private void DrawWindow(int _)
        {
            GUILayout.BeginVertical();
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            GUILayout.Label("Courses");
            DrawSceneButtons(courseScenes);

            GUILayout.Space(8f);
            GUILayout.Label("Utility");
            DrawSceneButtons(utilityScenes);

            GUILayout.EndScrollView();
            GUILayout.Space(4f);

            if (GUILayout.Button("Close"))
            {
                Hide();
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 22f));
        }

        private void DrawSceneButtons(string[] scenes)
        {
            if (scenes == null)
            {
                return;
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                string sceneName = scenes[i];
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    continue;
                }

                bool inBuild = Application.CanStreamedLevelBeLoaded(sceneName);
                GUI.enabled = inBuild;
                if (GUILayout.Button(inBuild ? sceneName : $"{sceneName} (missing)"))
                {
                    LoadScene(sceneName);
                    Hide();
                }

                GUI.enabled = true;
            }
        }

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                DebugConsole.Instance?.Log($"Scene '{sceneName}' is not in Build Settings.");
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.SceneLoader != null)
            {
                GameManager.Instance.SceneLoader.LoadSceneAsync(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }

            DebugConsole.Instance?.Log($"Loading scene '{sceneName}'...");
        }
    }
}
#endif
