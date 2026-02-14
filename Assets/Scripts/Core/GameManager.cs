using System;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Core
{
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(GameStateManager))]
    [RequireComponent(typeof(SceneLoader))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Global runtime config used for boot values and performance defaults.")]
        [SerializeField] private GameConfigSO gameConfig;
        [Tooltip("When running from the persistent bootstrap scene, auto-load MainMenu on start.")]
        [SerializeField] private bool loadMainMenuOnStart = true;

        [Header("Core Services")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private AudioManager audioManager;

        [Header("Runtime Data")]
        [SerializeField] private CourseDataSO currentCourse;
        [SerializeField] private HovercraftController activeHovercraft;

        public event Action OnInitialized;

        public bool IsInitialized { get; private set; }
        public GameConfigSO GameConfig => gameConfig;
        public GameStateManager GameStateManager => gameStateManager;
        public SceneLoader SceneLoader => sceneLoader;
        public AudioManager AudioManager => audioManager;
        public CourseDataSO CurrentCourse => currentCourse;
        public HovercraftController ActiveHovercraft => activeHovercraft;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCoreSystems();
        }

        private void Start()
        {
            if (!loadMainMenuOnStart || sceneLoader == null)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != GameConstants.SCENE_PERSISTENT)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(GameConstants.SCENE_MAIN_MENU))
            {
                Debug.LogWarning($"Cannot auto-load '{GameConstants.SCENE_MAIN_MENU}'. Add it to Build Settings first.");
                return;
            }

            gameStateManager?.SetState(GameState.Loading);
            sceneLoader.LoadSceneAsync(
                GameConstants.SCENE_MAIN_MENU,
                null,
                () => gameStateManager?.SetState(GameState.MainMenu));
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }

            EventBus.Shutdown();
            IsInitialized = false;
            Instance = null;
        }

        public void SetCurrentCourse(CourseDataSO courseData)
        {
            currentCourse = courseData;
        }

        public void RegisterHovercraft(HovercraftController controller)
        {
            activeHovercraft = controller;
        }

        private void OnValidate()
        {
            if (gameStateManager == null)
            {
                gameStateManager = GetComponent<GameStateManager>();
            }

            if (sceneLoader == null)
            {
                sceneLoader = GetComponent<SceneLoader>();
            }

            if (audioManager == null)
            {
                audioManager = GetComponent<AudioManager>();
            }
        }

        private void InitializeCoreSystems()
        {
            if (IsInitialized)
            {
                return;
            }

            if (gameStateManager == null)
            {
                gameStateManager = GetComponent<GameStateManager>();
            }

            if (sceneLoader == null)
            {
                sceneLoader = GetComponent<SceneLoader>();
            }

            if (audioManager == null)
            {
                audioManager = GetComponent<AudioManager>();
            }

            EventBus.Initialize();
            ApplyRuntimeConfigDefaults();

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        private void ApplyRuntimeConfigDefaults()
        {
            if (gameConfig == null)
            {
                return;
            }

            if (gameConfig.targetFrameRate > 0)
            {
                Application.targetFrameRate = gameConfig.targetFrameRate;
            }

            SetFloatPrefIfMissing(GameConstants.PREF_TILT_SENSITIVITY, gameConfig.defaultTiltSensitivity);
            SetFloatPrefIfMissing(GameConstants.PREF_MASTER_VOLUME, 1f);
            SetFloatPrefIfMissing(GameConstants.PREF_MUSIC_VOLUME, 0.7f);
            SetFloatPrefIfMissing(GameConstants.PREF_SFX_VOLUME, 0.9f);
            SetIntPrefIfMissing(GameConstants.PREF_HAPTICS_ENABLED, 1);
            SetIntPrefIfMissing(GameConstants.PREF_QUALITY_LEVEL, QualitySettings.GetQualityLevel());
        }

        private static void SetFloatPrefIfMissing(string key, float value)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetFloat(key, value);
            }
        }

        private static void SetIntPrefIfMissing(string key, int value)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, value);
            }
        }
    }
}
