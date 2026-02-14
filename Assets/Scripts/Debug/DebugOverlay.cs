#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Text;
using MetalPod.Course;
using MetalPod.Hovercraft;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace MetalPod.Debugging
{
    /// <summary>
    /// Lightweight runtime overlay for FPS, memory, and game state information.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private bool showOnStart;
        [SerializeField] private int fontSize = 13;
        [SerializeField] private Vector2 offset = new Vector2(10f, 10f);
        [SerializeField] private Color textColor = new Color(0.92f, 0.98f, 0.92f, 1f);
        [SerializeField] private Color warningColor = new Color(0.95f, 0.85f, 0.25f, 1f);
        [SerializeField] private Color criticalColor = new Color(1f, 0.45f, 0.35f, 1f);
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.72f);

        private bool _isVisible;
        private float _fpsCurrent;
        private float _fpsMin;
        private float _fpsMax;
        private double _fpsAccumulator;
        private int _fpsFrames;

        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private Texture2D _backgroundTexture;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            _isVisible = showOnStart;
            ResetStats();
        }

        private void Update()
        {
            if (!_isVisible)
            {
                return;
            }

            float delta = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            _fpsCurrent = 1f / delta;
            _fpsMin = Mathf.Min(_fpsMin, _fpsCurrent);
            _fpsMax = Mathf.Max(_fpsMax, _fpsCurrent);
            _fpsAccumulator += _fpsCurrent;
            _fpsFrames++;
        }

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            EnsureStyles();

            string text = BuildOverlayText(out Color dynamicFpsColor);
            int lineCount = CountLines(text);
            float width = 430f;
            float height = Mathf.Max(130f, (lineCount * (fontSize + 4f)) + 14f);

            Rect rect = new Rect(Screen.width - width - offset.x, offset.y, width, height);
            GUI.Box(rect, GUIContent.none, _boxStyle);

            _labelStyle.normal.textColor = dynamicFpsColor;
            GUI.Label(rect, text, _labelStyle);
            _labelStyle.normal.textColor = textColor;
        }

        public void Toggle()
        {
            if (_isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            _isVisible = true;
            ResetStats();
        }

        public void Hide()
        {
            _isVisible = false;
        }

        private void ResetStats()
        {
            _fpsCurrent = 0f;
            _fpsMin = float.MaxValue;
            _fpsMax = 0f;
            _fpsAccumulator = 0d;
            _fpsFrames = 0;
        }

        private string BuildOverlayText(out Color fpsColor)
        {
            float fpsAverage = _fpsFrames > 0 ? (float)(_fpsAccumulator / _fpsFrames) : 0f;
            float min = _fpsFrames > 0 ? _fpsMin : 0f;
            float max = _fpsFrames > 0 ? _fpsMax : 0f;

            fpsColor = _fpsCurrent >= 55f ? textColor :
                       _fpsCurrent >= 30f ? warningColor :
                       criticalColor;

            long allocatedBytes = Profiler.GetTotalAllocatedMemoryLong();
            long reservedBytes = Profiler.GetTotalReservedMemoryLong();
            long monoBytes = Profiler.GetMonoUsedSizeLong();
            long gcBytes = System.GC.GetTotalMemory(false);

            StringBuilder builder = new StringBuilder(384);
            builder.AppendLine($"FPS: {_fpsCurrent:F1}  (min {min:F1} / max {max:F1} / avg {fpsAverage:F1})");
            builder.AppendLine(
                $"MEM: Alloc {ToMb(allocatedBytes):F1}MB | Reserved {ToMb(reservedBytes):F1}MB | Mono {ToMb(monoBytes):F1}MB | GC {ToMb(gcBytes):F1}MB");
            builder.AppendLine($"Scene: {SceneManager.GetActiveScene().name}");
            builder.AppendLine($"TimeScale: {Time.timeScale:F2}");

            CourseManager courseManager = FindObjectOfType<CourseManager>();
            if (courseManager != null)
            {
                builder.AppendLine($"Course State: {courseManager.CurrentState}");
            }

            HovercraftController controller = FindObjectOfType<HovercraftController>();
            if (controller != null)
            {
                builder.Append($"Hovercraft: {controller.CurrentState}, Speed {controller.CurrentSpeed:F1}m/s");

                HovercraftHealth health = controller.GetComponent<HovercraftHealth>();
                if (health != null)
                {
                    builder.AppendLine();
                    builder.Append(
                        $"Health {health.CurrentHealth:F1}/{health.MaxHealth:F1} | Shield {health.CurrentShield:F1}/{health.MaxShield:F1}");
                }
            }

            return builder.ToString();
        }

        private void EnsureStyles()
        {
            if (_boxStyle != null && _labelStyle != null)
            {
                return;
            }

            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, backgroundColor);
            _backgroundTexture.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = _backgroundTexture;
            _boxStyle.border = new RectOffset(4, 4, 4, 4);

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(8, 8, 6, 6)
            };
            _labelStyle.normal.textColor = textColor;
        }

        private static float ToMb(long bytes)
        {
            return bytes / (1024f * 1024f);
        }

        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 1;
            }

            int count = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    count++;
                }
            }

            return count;
        }

        private void OnDestroy()
        {
            if (_backgroundTexture != null)
            {
                Destroy(_backgroundTexture);
            }
        }
    }
}
#endif
