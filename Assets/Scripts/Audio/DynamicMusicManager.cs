using MetalPod.Course;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Audio
{
    /// <summary>
    /// Orchestrates layered music playback responding to gameplay state.
    /// Creates its own AudioSources and does not depend on AudioManager internals.
    /// </summary>
    public class DynamicMusicManager : MonoBehaviour
    {
        public static DynamicMusicManager Instance { get; private set; }

        [Header("Profiles")]
        [SerializeField] private MusicProfileSO menuProfile;
        [SerializeField] private MusicProfileSO workshopProfile;
        [SerializeField] private MusicProfileSO lavaProfile;
        [SerializeField] private MusicProfileSO iceProfile;
        [SerializeField] private MusicProfileSO toxicProfile;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField] [Range(0.1f, 5f)] private float profileCrossfadeTime = 1.5f;
        [SerializeField] [Range(0f, 1f)] private float ambientVolumeMultiplier = 0.5f;
        [SerializeField] [Min(1f)] private float fallbackMaxSpeed = 40f;

        private MusicLayer _baseLayer;
        private MusicLayer _drumsLayer;
        private MusicLayer _leadLayer;
        private MusicLayer _boostLayer;
        private MusicStinger _stinger;
        private AudioSource _ambientSource;

        private MusicProfileSO _currentProfile;
        private MusicProfileSO _pendingProfile;
        private float _crossfadeTimer;
        private bool _isCrossfading;

        private HovercraftController _hovercraft;
        private HovercraftHealth _hovercraftHealth;
        private HovercraftStats _hovercraftStats;
        private CourseManager _courseManager;

        private float _referenceRefreshTimer;
        private float _maxSpeed;
        private float _currentNormalizedSpeed;
        private float _currentHealthNormalized = 1f;
        private bool _isBoosting;
        private bool _isRacing;
        private bool _dangerWasBelowThreshold;

        public MusicProfileSO CurrentProfile => _currentProfile;
        public bool IsCrossfading => _isCrossfading;
        public bool IsRacing => _isRacing;
        public float CurrentNormalizedSpeed => _currentNormalizedSpeed;
        public float CurrentHealthNormalized => _currentHealthNormalized;
        public bool IsBoosting => _isBoosting;
        public float MaxSpeedReference => _maxSpeed;

        public float BaseLayerVolume => _baseLayer != null ? _baseLayer.CurrentVolume : 0f;
        public float DrumsLayerVolume => _drumsLayer != null ? _drumsLayer.CurrentVolume : 0f;
        public float LeadLayerVolume => _leadLayer != null ? _leadLayer.CurrentVolume : 0f;
        public float BoostLayerVolume => _boostLayer != null ? _boostLayer.CurrentVolume : 0f;

        public float BaseLayerTargetVolume => _baseLayer != null ? _baseLayer.TargetVolume : 0f;
        public float DrumsLayerTargetVolume => _drumsLayer != null ? _drumsLayer.TargetVolume : 0f;
        public float LeadLayerTargetVolume => _leadLayer != null ? _leadLayer.TargetVolume : 0f;
        public float BoostLayerTargetVolume => _boostLayer != null ? _boostLayer.TargetVolume : 0f;

        public bool BaseLayerActive => _baseLayer != null && _baseLayer.IsActive;
        public bool DrumsLayerActive => _drumsLayer != null && _drumsLayer.IsActive;
        public bool LeadLayerActive => _leadLayer != null && _leadLayer.IsActive;
        public bool BoostLayerActive => _boostLayer != null && _boostLayer.IsActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _maxSpeed = Mathf.Max(1f, fallbackMaxSpeed);
            CreateAudioSources();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            SelectProfileForScene(SceneManager.GetActiveScene().name, false);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeFromCourseManager();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (_isCrossfading)
            {
                UpdateCrossfade();
            }

            if (_isRacing)
            {
                _referenceRefreshTimer -= Time.unscaledDeltaTime;
                if (_referenceRefreshTimer <= 0f)
                {
                    _referenceRefreshTimer = 0.5f;
                    RefreshGameplayReferences();
                }

                UpdateGameplayLayers();
            }

            float duckMultiplier = _stinger != null ? _stinger.GetDuckMultiplier() : 1f;
            float effectiveMusicVolume = musicVolume * duckMultiplier;

            _baseLayer?.Update(effectiveMusicVolume);
            _drumsLayer?.Update(effectiveMusicVolume);
            _leadLayer?.Update(effectiveMusicVolume);
            _boostLayer?.Update(effectiveMusicVolume);

            if (_ambientSource != null && _ambientSource.isPlaying)
            {
                _ambientSource.volume = musicVolume * ambientVolumeMultiplier;
            }
        }

        private void CreateAudioSources()
        {
            _baseLayer = new MusicLayer("Base", CreateSource("MusicLayer_Base"));
            _drumsLayer = new MusicLayer("Drums", CreateSource("MusicLayer_Drums"));
            _leadLayer = new MusicLayer("Lead", CreateSource("MusicLayer_Lead"));
            _boostLayer = new MusicLayer("Boost", CreateSource("MusicLayer_Boost"));

            _stinger = new MusicStinger(CreateSource("MusicStinger"));

            _ambientSource = CreateSource("MusicAmbient");
            if (_ambientSource != null)
            {
                _ambientSource.loop = true;
                _ambientSource.playOnAwake = false;
            }
        }

        private AudioSource CreateSource(string sourceName)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        public void SetProfile(MusicProfileSO profile, bool crossfade = true)
        {
            if (profile == null)
            {
                return;
            }

            if (!_isCrossfading && profile == _currentProfile)
            {
                return;
            }

            if (crossfade && _currentProfile != null)
            {
                _pendingProfile = profile;
                _crossfadeTimer = 0f;
                _isCrossfading = true;

                float fadeOutTime = Mathf.Max(0.05f, profileCrossfadeTime * 0.5f);
                _baseLayer?.FadeOut(fadeOutTime);
                _drumsLayer?.FadeOut(fadeOutTime);
                _leadLayer?.FadeOut(fadeOutTime);
                _boostLayer?.FadeOut(fadeOutTime);
                return;
            }

            _isCrossfading = false;
            _pendingProfile = null;
            ApplyProfile(profile);
        }

        private void UpdateCrossfade()
        {
            _crossfadeTimer += Time.unscaledDeltaTime;
            float halfCrossfade = Mathf.Max(0.05f, profileCrossfadeTime * 0.5f);

            if (_crossfadeTimer >= halfCrossfade)
            {
                ApplyProfile(_pendingProfile);
                _pendingProfile = null;
                _isCrossfading = false;
            }
        }

        private void ApplyProfile(MusicProfileSO profile)
        {
            if (profile == null)
            {
                return;
            }

            _currentProfile = profile;
            _stinger?.Configure(profile.stingerDuckLevel, profile.stingerDuckFadeTime);

            double startTime = AudioSettings.dspTime + 0.05d;

            _baseLayer?.Play(profile.baseLayer, startTime);
            if (profile.baseLayer != null)
            {
                _baseLayer?.FadeIn(profile.layerFadeTime);
            }
            else
            {
                _baseLayer?.Stop();
            }

            PrepareSyncedLayer(_drumsLayer, profile.drumsLayer, startTime);
            PrepareSyncedLayer(_leadLayer, profile.leadLayer, startTime);
            PrepareSyncedLayer(_boostLayer, profile.boostLayer, startTime);

            if (_ambientSource != null)
            {
                if (profile.ambientLoop != null)
                {
                    if (_ambientSource.clip != profile.ambientLoop)
                    {
                        _ambientSource.Stop();
                        _ambientSource.clip = profile.ambientLoop;
                    }

                    if (!_ambientSource.isPlaying)
                    {
                        _ambientSource.Play();
                    }

                    _ambientSource.volume = musicVolume * ambientVolumeMultiplier;
                }
                else
                {
                    _ambientSource.Stop();
                    _ambientSource.clip = null;
                }
            }

            _dangerWasBelowThreshold = _hovercraftHealth != null &&
                _hovercraftHealth.HealthNormalized <= profile.dangerHealthThreshold;
        }

        private static void PrepareSyncedLayer(MusicLayer layer, AudioClip clip, double startTime)
        {
            if (layer == null)
            {
                return;
            }

            layer.Play(clip, startTime);
            layer.SetVolumeImmediate(0f);
        }

        private void UpdateGameplayLayers()
        {
            if (_currentProfile == null || _hovercraft == null)
            {
                _currentNormalizedSpeed = 0f;
                _isBoosting = false;
                return;
            }

            if (_hovercraftStats != null)
            {
                _maxSpeed = Mathf.Max(1f, _hovercraftStats.MaxSpeed);
            }

            float speed = _hovercraft.CurrentSpeed;
            _currentNormalizedSpeed = Mathf.Clamp01(speed / Mathf.Max(1f, _maxSpeed));
            float fadeTime = Mathf.Max(0.05f, _currentProfile.layerFadeTime);

            if (_currentProfile.drumsLayer != null && _currentNormalizedSpeed >= _currentProfile.drumSpeedThreshold)
            {
                _drumsLayer?.FadeIn(fadeTime);
            }
            else
            {
                _drumsLayer?.FadeOut(fadeTime);
            }

            if (_currentProfile.leadLayer != null && _currentNormalizedSpeed >= _currentProfile.leadSpeedThreshold)
            {
                _leadLayer?.FadeIn(fadeTime);
            }
            else
            {
                _leadLayer?.FadeOut(fadeTime);
            }

            _isBoosting = _hovercraft.CurrentState == HovercraftState.Boosting;
            if (_currentProfile.boostLayer != null && _isBoosting)
            {
                _boostLayer?.FadeIn(Mathf.Max(0.05f, fadeTime * 0.5f));
            }
            else
            {
                _boostLayer?.FadeOut(fadeTime);
            }

            if (_hovercraftHealth != null)
            {
                _currentHealthNormalized = _hovercraftHealth.HealthNormalized;
                bool isBelowDanger = _currentHealthNormalized <= _currentProfile.dangerHealthThreshold;

                if (isBelowDanger && !_dangerWasBelowThreshold)
                {
                    _stinger?.Play(_currentProfile.dangerStinger);
                }

                _dangerWasBelowThreshold = isBelowDanger;
            }
            else
            {
                _currentHealthNormalized = 1f;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SelectProfileForScene(scene.name, true);
        }

        private void SelectProfileForScene(string sceneName, bool allowCrossfade)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || sceneName == GameConstants.SCENE_PERSISTENT)
            {
                return;
            }

            if (sceneName == GameConstants.SCENE_MAIN_MENU)
            {
                StopRacingMode();
                SetProfile(menuProfile, allowCrossfade);
                return;
            }

            if (sceneName == GameConstants.SCENE_WORKSHOP)
            {
                StopRacingMode();
                SetProfile(workshopProfile, allowCrossfade);
                return;
            }

            if (ContainsAny(sceneName, "inferno", "molten", "magma", "lava"))
            {
                StartRacingMode();
                SetProfile(lavaProfile, allowCrossfade);
                return;
            }

            if (ContainsAny(sceneName, "frozen", "glacial", "arctic", "ice"))
            {
                StartRacingMode();
                SetProfile(iceProfile, allowCrossfade);
                return;
            }

            if (ContainsAny(sceneName, "rust", "chemical", "biohazard", "toxic"))
            {
                StartRacingMode();
                SetProfile(toxicProfile, allowCrossfade);
                return;
            }

            StopRacingMode();
        }

        private static bool ContainsAny(string value, params string[] terms)
        {
            string lower = value.ToLowerInvariant();
            for (int i = 0; i < terms.Length; i++)
            {
                if (lower.Contains(terms[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void StartRacingMode()
        {
            _isRacing = true;
            _referenceRefreshTimer = 0f;
            RefreshGameplayReferences();
        }

        private void StopRacingMode()
        {
            _isRacing = false;
            _currentNormalizedSpeed = 0f;
            _currentHealthNormalized = 1f;
            _isBoosting = false;
            _maxSpeed = Mathf.Max(1f, fallbackMaxSpeed);

            _hovercraft = null;
            _hovercraftHealth = null;
            _hovercraftStats = null;
            UnsubscribeFromCourseManager();

            _drumsLayer?.FadeOut(0.2f);
            _leadLayer?.FadeOut(0.2f);
            _boostLayer?.FadeOut(0.2f);
        }

        private void RefreshGameplayReferences()
        {
            GameObject player = GameObject.FindGameObjectWithTag(GameConstants.TAG_PLAYER);
            if (player != null)
            {
                _hovercraft = player.GetComponent<HovercraftController>();
                _hovercraftHealth = player.GetComponent<HovercraftHealth>();
                _hovercraftStats = player.GetComponent<HovercraftStats>();

                if (_hovercraftStats != null)
                {
                    _maxSpeed = Mathf.Max(1f, _hovercraftStats.MaxSpeed);
                }
            }

            if (_courseManager == null)
            {
                BindCourseManager(FindObjectOfType<CourseManager>());
            }
        }

        private void BindCourseManager(CourseManager courseManager)
        {
            if (courseManager == _courseManager)
            {
                return;
            }

            UnsubscribeFromCourseManager();
            _courseManager = courseManager;

            if (_courseManager == null)
            {
                return;
            }

            _courseManager.OnRaceFinished += HandleRaceFinished;
            _courseManager.OnRaceFailed += HandleRaceFailed;
            _courseManager.OnCountdownTick += HandleCountdownTick;
            _courseManager.OnRaceStarted += HandleRaceStarted;
        }

        private void UnsubscribeFromCourseManager()
        {
            if (_courseManager == null)
            {
                return;
            }

            _courseManager.OnRaceFinished -= HandleRaceFinished;
            _courseManager.OnRaceFailed -= HandleRaceFailed;
            _courseManager.OnCountdownTick -= HandleCountdownTick;
            _courseManager.OnRaceStarted -= HandleRaceStarted;
            _courseManager = null;
        }

        private void HandleCountdownTick(int count)
        {
            if (count <= 0 || _currentProfile == null)
            {
                return;
            }

            _stinger?.Play(_currentProfile.countdownTick, 0.8f);
        }

        private void HandleRaceStarted()
        {
            if (_currentProfile == null)
            {
                return;
            }

            _stinger?.Play(_currentProfile.goStinger);
        }

        private void HandleRaceFinished(float _)
        {
            if (_currentProfile == null)
            {
                return;
            }

            _drumsLayer?.FadeOut(0.5f);
            _leadLayer?.FadeOut(0.5f);
            _boostLayer?.FadeOut(0.3f);
            _stinger?.Play(_currentProfile.victoryStinger);
        }

        private void HandleRaceFailed()
        {
            if (_currentProfile == null)
            {
                return;
            }

            _baseLayer?.FadeOut(0.5f);
            _drumsLayer?.FadeOut(0.3f);
            _leadLayer?.FadeOut(0.3f);
            _boostLayer?.FadeOut(0.2f);
            _stinger?.Play(_currentProfile.defeatStinger);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
        }

        public void PauseMusic()
        {
            PauseLayer(_baseLayer);
            PauseLayer(_drumsLayer);
            PauseLayer(_leadLayer);
            PauseLayer(_boostLayer);
            _ambientSource?.Pause();
        }

        public void ResumeMusic()
        {
            ResumeLayer(_baseLayer);
            ResumeLayer(_drumsLayer);
            ResumeLayer(_leadLayer);
            ResumeLayer(_boostLayer);
            _ambientSource?.UnPause();
        }

        public void StopAll()
        {
            _baseLayer?.Stop();
            _drumsLayer?.Stop();
            _leadLayer?.Stop();
            _boostLayer?.Stop();

            if (_ambientSource != null)
            {
                _ambientSource.Stop();
            }
        }

        public void PlayStinger(AudioClip clip, float volume = 1f)
        {
            _stinger?.Play(clip, volume);
        }

        private static void PauseLayer(MusicLayer layer)
        {
            if (layer?.Source != null)
            {
                layer.Source.Pause();
            }
        }

        private static void ResumeLayer(MusicLayer layer)
        {
            if (layer?.Source != null)
            {
                layer.Source.UnPause();
            }
        }
    }
}
