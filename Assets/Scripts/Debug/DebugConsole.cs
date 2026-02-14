#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MetalPod.Debugging
{
    /// <summary>
    /// In-game debug console for development and editor play mode.
    /// Toggle via triple-tap in top-left corner (or backquote key).
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class DebugConsole : MonoBehaviour
    {
        private const string InputControlName = "DebugConsole_Input";
        private const int WindowId = 54931;

        public static DebugConsole Instance { get; private set; }

        [Header("Behavior")]
        [SerializeField] private bool showOnStart;
        [SerializeField, Min(50)] private int maxLogLines = 250;
        [SerializeField, Min(32)] private int tapCornerSize = 100;
        [SerializeField, Min(2)] private int tapsRequired = 3;
        [SerializeField, Min(0.1f)] private float tapWindowSeconds = 1f;
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;

        [Header("Layout")]
        [SerializeField, Range(0.5f, 1f)] private float widthPercent = 0.92f;
        [SerializeField, Range(0.3f, 1f)] private float heightPercent = 0.6f;
        [SerializeField] private Vector2 minWindowSize = new Vector2(420f, 260f);

        private readonly Dictionary<string, DebugCommand> _commands =
            new Dictionary<string, DebugCommand>(StringComparer.OrdinalIgnoreCase);

        private readonly List<string> _logLines = new List<string>();
        private readonly List<string> _history = new List<string>();

        private Rect _windowRect;
        private Vector2 _scrollPosition;
        private string _input = string.Empty;
        private bool _isVisible;
        private bool _requestFocus;
        private int _historyIndex;
        private int _tapCount;
        private float _lastTapTime = -100f;
        private bool _commandsRegistered;

        private GUIStyle _logStyle;
        private GUIStyle _hintStyle;

        public bool IsVisible => _isVisible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            DebugConsole existing = FindObjectOfType<DebugConsole>();
            if (existing != null)
            {
                Instance = existing;
                return;
            }

            GameObject host = new GameObject("DebugConsole");
            DontDestroyOnLoad(host);

            host.AddComponent<DebugConsole>();
            host.AddComponent<DebugOverlay>();
            host.AddComponent<DebugCourseSkip>();
            host.AddComponent<DebugHazardTester>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            float width = Mathf.Max(minWindowSize.x, Screen.width * widthPercent);
            float height = Mathf.Max(minWindowSize.y, Screen.height * heightPercent);
            _windowRect = new Rect(12f, 12f, width, height);

            _isVisible = showOnStart;
            _historyIndex = 0;
        }

        private void Start()
        {
            if (_commandsRegistered)
            {
                return;
            }

            DebugCommands.RegisterAll(this);
            _commandsRegistered = true;
            Log("Debug console initialized. Type 'help' for available commands.");
        }

        private void Update()
        {
            DetectToggleTap();

            if (Input.GetKeyDown(toggleKey))
            {
                ToggleConsole();
            }

            if (!_isVisible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetVisible(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitInput();
            }
        }

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            EnsureStyles();
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "Debug Console");

            if (_requestFocus)
            {
                GUI.FocusWindow(WindowId);
                GUI.FocusControl(InputControlName);
                _requestFocus = false;
            }
        }

        public void ToggleConsole()
        {
            SetVisible(!_isVisible);
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            if (!_isVisible)
            {
                return;
            }

            _historyIndex = _history.Count;
            _requestFocus = true;
        }

        public void RegisterCommand(DebugCommand command)
        {
            if (command == null || string.IsNullOrWhiteSpace(command.Name))
            {
                return;
            }

            _commands[command.Name] = command;
        }

        public IEnumerable<DebugCommand> GetAllCommands()
        {
            return _commands.Values;
        }

        public void SubmitCommand(string commandText)
        {
            _input = commandText ?? string.Empty;
            SubmitInput();
        }

        public void ClearLog()
        {
            _logLines.Clear();
            _scrollPosition = Vector2.zero;
        }

        public void Log(string message)
        {
            if (message == null)
            {
                message = string.Empty;
            }

            _logLines.Add(message);
            while (_logLines.Count > maxLogLines)
            {
                _logLines.RemoveAt(0);
            }

            _scrollPosition.y = float.MaxValue;
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Triple-tap top-left corner or press ` to toggle.", _hintStyle);
            GUILayout.Space(4f);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            if (_logLines.Count == 0)
            {
                GUILayout.Label("No output.", _hintStyle);
            }
            else
            {
                for (int i = 0; i < _logLines.Count; i++)
                {
                    GUILayout.Label(_logLines[i], _logStyle);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUI.SetNextControlName(InputControlName);
            _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Run", GUILayout.Width(64f)))
            {
                SubmitInput();
            }

            if (GUILayout.Button("Clear", GUILayout.Width(64f)))
            {
                ClearLog();
            }

            if (GUILayout.Button("Close", GUILayout.Width(64f)))
            {
                SetVisible(false);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private void EnsureStyles()
        {
            if (_logStyle != null && _hintStyle != null)
            {
                return;
            }

            _logStyle = new GUIStyle(GUI.skin.label)
            {
                richText = false,
                wordWrap = true
            };

            _hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic
            };
            _hintStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);
        }

        private void SubmitInput()
        {
            string input = (_input ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                _requestFocus = true;
                return;
            }

            _history.Add(input);
            _historyIndex = _history.Count;
            Log($"> {input}");

            _input = string.Empty;
            _requestFocus = true;

            string[] tokens = Tokenize(input);
            if (tokens.Length == 0)
            {
                return;
            }

            string commandName = tokens[0].ToLowerInvariant();
            string[] args = tokens.Skip(1).ToArray();

            if (!_commands.TryGetValue(commandName, out DebugCommand command))
            {
                Log($"Unknown command '{commandName}'. Type 'help' for a command list.");
                return;
            }

            try
            {
                string result = command.Execute(args);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    Log(result);
                }
            }
            catch (Exception ex)
            {
                Log($"[ERROR] {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void NavigateHistory(int direction)
        {
            if (_history.Count == 0)
            {
                return;
            }

            _historyIndex = Mathf.Clamp(_historyIndex + direction, 0, _history.Count);
            _input = _historyIndex >= _history.Count ? string.Empty : _history[_historyIndex];
            _requestFocus = true;
        }

        private void DetectToggleTap()
        {
            if (!TryGetTapPosition(out Vector2 tapPosition))
            {
                return;
            }

            bool inCorner = tapPosition.x <= tapCornerSize &&
                            tapPosition.y >= Screen.height - tapCornerSize;
            if (!inCorner)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now - _lastTapTime <= tapWindowSeconds)
            {
                _tapCount++;
            }
            else
            {
                _tapCount = 1;
            }

            _lastTapTime = now;
            if (_tapCount >= tapsRequired)
            {
                _tapCount = 0;
                ToggleConsole();
            }
        }

        private static bool TryGetTapPosition(out Vector2 position)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began)
                {
                    continue;
                }

                position = touch.position;
                return true;
            }

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                position = Input.mousePosition;
                return true;
            }
#endif

            position = Vector2.zero;
            return false;
        }

        private static string[] Tokenize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Array.Empty<string>();
            }

            List<string> tokens = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }

                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }

            return tokens.ToArray();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
#endif
