# Codex Task 19: Dynamic Music System

> **Goal**: Create a dynamic music system with environment-specific tracks, intensity layers that respond to gameplay (speed, danger, boost), and transition stingers. Builds on top of the existing AudioManager without modifying it.

---

## Context

The existing `AudioManager.cs` (`Assets/Scripts/Core/AudioManager.cs`) handles:
- Two music AudioSources with crossfade (`PlayMusic`, `CrossfadeMusicRoutine`)
- An ambient AudioSource
- SFX pool (8 pooled AudioSources)
- Volume control (master, music, sfx, ambient) with PlayerPrefs persistence

However, it plays flat music tracks with no dynamic behavior. For a racing game, the music should:
- Match the environment (heavy lava riffs, icy synth pads, industrial toxic beats)
- Intensify with speed (more layers/instruments at higher speed)
- Add a boost layer during boost
- Play a danger stinger when health is low
- Play victory/defeat stingers at course end
- Crossfade between environments during scene transitions

This task creates a **DynamicMusicManager** that sits on top of AudioManager and orchestrates layered music.

**Read these files**:
- `Assets/Scripts/Core/AudioManager.cs` — Existing audio system (entire file)
- `Assets/Scripts/Hovercraft/HovercraftController.cs` — CurrentSpeed, CurrentState, OnStateChanged
- `Assets/Scripts/Hovercraft/HovercraftHealth.cs` — HealthNormalized, OnDamage, OnDestroyed
- `Assets/Scripts/Course/CourseManager.cs` — CourseRunState, events
- `Assets/Scripts/Shared/EventBus.cs` — Global events
- `Assets/Scripts/Shared/GameConstants.cs` — Scene names

---

## Files to Create

```
Assets/Scripts/Audio/
├── DynamicMusicManager.cs     # Main controller orchestrating music layers
├── MusicLayer.cs              # Individual music layer (base, drums, lead, boost)
├── MusicStinger.cs            # One-shot stinger player (victory, defeat, danger)
└── EnvironmentMusicTrigger.cs # Component to trigger music profile on scene load

Assets/ScriptableObjects/
└── MusicProfileSO.cs          # ScriptableObject defining music for an environment

Assets/Scripts/Editor/
└── MusicDebugWindow.cs        # Editor window showing active layers and intensity
```

**DO NOT modify** any existing files, including AudioManager.cs. The DynamicMusicManager creates its own AudioSources for layered playback.

---

## Architecture

```
DynamicMusicManager (Singleton, DontDestroyOnLoad)
  ├── 4 AudioSources for music layers:
  │   ├── Layer 0: Base (always playing — rhythm/pad)
  │   ├── Layer 1: Drums (fades in at medium speed)
  │   ├── Layer 2: Lead (fades in at high speed)
  │   └── Layer 3: Boost (fades in only during boost)
  ├── 1 AudioSource for stingers (one-shot, ducks music)
  ├── Monitors hovercraft speed → adjusts layer volumes
  ├── Monitors hovercraft health → triggers danger stinger
  ├── Monitors CourseManager state → plays victory/defeat stingers
  ├── MusicProfileSO per environment defines which clips go in each layer
  └── Crossfades between profiles on scene change

MusicProfileSO
  ├── AudioClip baseLayer      (always plays, loop)
  ├── AudioClip drumsLayer     (fades in at drumSpeedThreshold)
  ├── AudioClip leadLayer      (fades in at leadSpeedThreshold)
  ├── AudioClip boostLayer     (fades in during boost state)
  ├── AudioClip ambientLoop    (environmental ambience)
  ├── AudioClip victoryStinger (on course complete)
  ├── AudioClip defeatStinger  (on course fail)
  ├── AudioClip dangerStinger  (on low health, once per threshold cross)
  ├── float bpm                (for beat-synced transitions)
  ├── float drumSpeedThreshold (normalized speed 0-1)
  ├── float leadSpeedThreshold (normalized speed 0-1)
  └── float layerFadeTime      (seconds to fade layers in/out)
```

---

## Detailed Specifications

### MusicProfileSO.cs

```csharp
using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MusicProfile", menuName = "MetalPod/Music Profile")]
    public class MusicProfileSO : ScriptableObject
    {
        [Header("Identity")]
        public string profileId;
        public string displayName; // e.g., "Lava Zone Music"

        [Header("Layers (all clips must be same length and BPM for sync)")]
        [Tooltip("Always playing — bass, rhythm, pads")]
        public AudioClip baseLayer;

        [Tooltip("Fades in at medium speed")]
        public AudioClip drumsLayer;

        [Tooltip("Fades in at high speed — lead guitar, synth")]
        public AudioClip leadLayer;

        [Tooltip("Fades in during boost — power riff")]
        public AudioClip boostLayer;

        [Header("Ambient")]
        [Tooltip("Environmental ambience loop (lava rumble, wind, industrial hum)")]
        public AudioClip ambientLoop;

        [Header("Stingers")]
        [Tooltip("Plays on course completion (short, triumphant)")]
        public AudioClip victoryStinger;

        [Tooltip("Plays on course failure (short, dramatic)")]
        public AudioClip defeatStinger;

        [Tooltip("Plays once when health drops below danger threshold")]
        public AudioClip dangerStinger;

        [Tooltip("Plays during countdown (optional — short hit per beat)")]
        public AudioClip countdownTick;

        [Tooltip("Plays on GO! (optional — impact hit)")]
        public AudioClip goStinger;

        [Header("Timing")]
        [Tooltip("BPM of all layer clips (for beat-synced transitions)")]
        public float bpm = 120f;

        [Tooltip("Speed layers begin at these normalized thresholds (0=stopped, 1=max speed)")]
        [Range(0f, 1f)] public float drumSpeedThreshold = 0.3f;
        [Range(0f, 1f)] public float leadSpeedThreshold = 0.6f;

        [Tooltip("How quickly layers fade in/out (seconds)")]
        [Range(0.1f, 3f)] public float layerFadeTime = 0.8f;

        [Tooltip("Volume ducking when stinger plays (0=full duck, 1=no duck)")]
        [Range(0f, 1f)] public float stingerDuckLevel = 0.3f;
        [Range(0.1f, 2f)] public float stingerDuckFadeTime = 0.3f;

        [Header("Health")]
        [Tooltip("Health threshold (normalized) to trigger danger stinger")]
        [Range(0f, 0.5f)] public float dangerHealthThreshold = 0.25f;

        /// <summary>Duration of one beat in seconds.</summary>
        public float BeatDuration => 60f / Mathf.Max(bpm, 1f);

        /// <summary>Duration of one bar (4 beats) in seconds.</summary>
        public float BarDuration => BeatDuration * 4f;
    }
}
```

### MusicLayer.cs

```csharp
// Represents a single music layer with fade control.
// Managed by DynamicMusicManager.

using UnityEngine;

namespace MetalPod.Audio
{
    public class MusicLayer
    {
        public string Name { get; }
        public AudioSource Source { get; }
        public float TargetVolume { get; private set; }
        public float CurrentVolume => Source.volume;
        public bool IsActive => TargetVolume > 0.01f;

        private float _fadeSpeed;
        private float _maxVolume;

        public MusicLayer(string name, AudioSource source, float maxVolume = 1f)
        {
            Name = name;
            Source = source;
            _maxVolume = maxVolume;
            TargetVolume = 0f;
            Source.volume = 0f;
            Source.loop = true;
            Source.playOnAwake = false;
        }

        public void SetClip(AudioClip clip)
        {
            bool wasPlaying = Source.isPlaying;
            Source.clip = clip;
            if (clip != null && wasPlaying)
                Source.Play();
        }

        public void Play(AudioClip clip, double scheduledTime = 0)
        {
            Source.clip = clip;
            if (clip == null) return;

            if (scheduledTime > 0)
                Source.PlayScheduled(scheduledTime);
            else
                Source.Play();
        }

        public void Stop()
        {
            Source.Stop();
            Source.volume = 0f;
            TargetVolume = 0f;
        }

        public void FadeIn(float fadeTime)
        {
            TargetVolume = _maxVolume;
            _fadeSpeed = _maxVolume / Mathf.Max(fadeTime, 0.01f);
        }

        public void FadeOut(float fadeTime)
        {
            TargetVolume = 0f;
            _fadeSpeed = _maxVolume / Mathf.Max(fadeTime, 0.01f);
        }

        public void SetVolumeImmediate(float volume)
        {
            TargetVolume = volume;
            Source.volume = volume;
        }

        public void SetMaxVolume(float maxVol)
        {
            _maxVolume = maxVol;
        }

        public void Update(float masterMusicVolume)
        {
            if (Source == null) return;

            float target = TargetVolume * masterMusicVolume;
            float current = Source.volume;

            if (Mathf.Abs(current - target) > 0.001f)
            {
                Source.volume = Mathf.MoveTowards(current, target,
                    _fadeSpeed * masterMusicVolume * Time.unscaledDeltaTime);
            }
        }

        /// <summary>Sync playback position to match another layer.</summary>
        public void SyncTo(MusicLayer other)
        {
            if (other.Source.isPlaying && Source.clip != null)
            {
                Source.timeSamples = other.Source.timeSamples;
                if (!Source.isPlaying)
                    Source.Play();
            }
        }
    }
}
```

### MusicStinger.cs

```csharp
// Plays one-shot stingers and manages music ducking during stinger playback.

using System.Collections;
using UnityEngine;

namespace MetalPod.Audio
{
    public class MusicStinger
    {
        private AudioSource _source;
        private float _duckLevel;
        private float _duckFadeTime;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        public MusicStinger(AudioSource source)
        {
            _source = source;
            _source.loop = false;
            _source.playOnAwake = false;
        }

        public void Configure(float duckLevel, float duckFadeTime)
        {
            _duckLevel = duckLevel;
            _duckFadeTime = duckFadeTime;
        }

        /// <summary>
        /// Play a stinger clip. Returns the duck multiplier over time.
        /// DynamicMusicManager should apply this multiplier to layer volumes.
        /// </summary>
        public void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            _source.PlayOneShot(clip, volume);
            _isPlaying = true;
        }

        /// <summary>
        /// Returns the current ducking multiplier (1.0 = no duck, duckLevel = full duck).
        /// Call this every frame while a stinger is playing.
        /// </summary>
        public float GetDuckMultiplier()
        {
            if (!_isPlaying) return 1f;
            if (!_source.isPlaying)
            {
                _isPlaying = false;
                return 1f;
            }
            return _duckLevel;
        }
    }
}
```

### DynamicMusicManager.cs

```csharp
// Orchestrates layered music playback responding to gameplay state.
// Creates its own AudioSources — does NOT modify AudioManager.
// Reads speed/health from hovercraft, course state from CourseManager.

using System;
using UnityEngine;
using MetalPod.Hovercraft;
using MetalPod.Course;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;

namespace MetalPod.Audio
{
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
        [SerializeField] private float profileCrossfadeTime = 1.5f;

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

        // Tracked game state
        private HovercraftController _hovercraft;
        private HovercraftHealth _hovercraftHealth;
        private Rigidbody _hovercraftRb;
        private CourseManager _courseManager;
        private float _maxSpeed = 40f; // Reference max speed
        private bool _dangerStingerPlayed;
        private bool _isRacing;

        public MusicProfileSO CurrentProfile => _currentProfile;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateAudioSources();
        }

        private void Start()
        {
            // Subscribe to scene changes to auto-switch profiles
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            // Start with menu music if available
            if (menuProfile != null)
                SetProfile(menuProfile);
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeFromGameplay();
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            float duckMultiplier = _stinger?.GetDuckMultiplier() ?? 1f;
            float effectiveVolume = musicVolume * duckMultiplier;

            _baseLayer?.Update(effectiveVolume);
            _drumsLayer?.Update(effectiveVolume);
            _leadLayer?.Update(effectiveVolume);
            _boostLayer?.Update(effectiveVolume);

            if (_isRacing)
                UpdateGameplayLayers();

            if (_isCrossfading)
                UpdateCrossfade();
        }

        // ── Audio Source Creation ────────────────────────────

        private void CreateAudioSources()
        {
            _baseLayer = new MusicLayer("Base", CreateSource("MusicLayer_Base"));
            _drumsLayer = new MusicLayer("Drums", CreateSource("MusicLayer_Drums"));
            _leadLayer = new MusicLayer("Lead", CreateSource("MusicLayer_Lead"));
            _boostLayer = new MusicLayer("Boost", CreateSource("MusicLayer_Boost"));

            var stingerSource = CreateSource("MusicStinger");
            _stinger = new MusicStinger(stingerSource);

            _ambientSource = CreateSource("MusicAmbient");
            _ambientSource.loop = true;
        }

        private AudioSource CreateSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D audio
            return source;
        }

        // ── Profile Management ──────────────────────────────

        public void SetProfile(MusicProfileSO profile, bool crossfade = true)
        {
            if (profile == null || profile == _currentProfile) return;

            if (crossfade && _currentProfile != null)
            {
                _pendingProfile = profile;
                _isCrossfading = true;
                _crossfadeTimer = 0f;

                // Fade out current layers
                float fadeTime = profileCrossfadeTime * 0.5f;
                _baseLayer.FadeOut(fadeTime);
                _drumsLayer.FadeOut(fadeTime);
                _leadLayer.FadeOut(fadeTime);
                _boostLayer.FadeOut(fadeTime);
            }
            else
            {
                ApplyProfile(profile);
            }
        }

        private void UpdateCrossfade()
        {
            _crossfadeTimer += Time.unscaledDeltaTime;

            if (_crossfadeTimer >= profileCrossfadeTime * 0.5f)
            {
                ApplyProfile(_pendingProfile);
                _pendingProfile = null;
                _isCrossfading = false;
            }
        }

        private void ApplyProfile(MusicProfileSO profile)
        {
            _currentProfile = profile;
            _stinger.Configure(profile.stingerDuckLevel, profile.stingerDuckFadeTime);

            // Sync all layers to play from the same position
            _baseLayer.Play(profile.baseLayer);
            _baseLayer.FadeIn(profile.layerFadeTime);

            _drumsLayer.SetClip(profile.drumsLayer);
            _drumsLayer.SyncTo(_baseLayer);
            _drumsLayer.SetVolumeImmediate(0f);

            _leadLayer.SetClip(profile.leadLayer);
            _leadLayer.SyncTo(_baseLayer);
            _leadLayer.SetVolumeImmediate(0f);

            _boostLayer.SetClip(profile.boostLayer);
            _boostLayer.SyncTo(_baseLayer);
            _boostLayer.SetVolumeImmediate(0f);

            // Ambient
            if (profile.ambientLoop != null)
            {
                _ambientSource.clip = profile.ambientLoop;
                _ambientSource.volume = musicVolume * 0.5f;
                _ambientSource.Play();
            }
            else
            {
                _ambientSource.Stop();
            }

            _dangerStingerPlayed = false;
        }

        // ── Gameplay Layer Control ──────────────────────────

        private void UpdateGameplayLayers()
        {
            if (_currentProfile == null || _hovercraftRb == null) return;

            float speed = _hovercraftRb.linearVelocity.magnitude;
            float normalizedSpeed = Mathf.Clamp01(speed / _maxSpeed);
            float fadeTime = _currentProfile.layerFadeTime;

            // Drums layer
            if (normalizedSpeed >= _currentProfile.drumSpeedThreshold)
                _drumsLayer.FadeIn(fadeTime);
            else
                _drumsLayer.FadeOut(fadeTime);

            // Lead layer
            if (normalizedSpeed >= _currentProfile.leadSpeedThreshold)
                _leadLayer.FadeIn(fadeTime);
            else
                _leadLayer.FadeOut(fadeTime);

            // Boost layer
            bool isBoosting = _hovercraft != null &&
                              _hovercraft.CurrentState == HovercraftState.Boosting;
            if (isBoosting)
                _boostLayer.FadeIn(fadeTime * 0.5f); // Quick fade in for boost
            else
                _boostLayer.FadeOut(fadeTime);

            // Danger stinger
            if (_hovercraftHealth != null && !_dangerStingerPlayed)
            {
                if (_hovercraftHealth.HealthNormalized <= _currentProfile.dangerHealthThreshold)
                {
                    _stinger.Play(_currentProfile.dangerStinger);
                    _dangerStingerPlayed = true;
                }
            }

            // Reset danger stinger if health recovers
            if (_hovercraftHealth != null &&
                _hovercraftHealth.HealthNormalized > _currentProfile.dangerHealthThreshold + 0.1f)
            {
                _dangerStingerPlayed = false;
            }
        }

        // ── Scene / Game State ──────────────────────────────

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // Auto-select music profile based on scene name
            string sceneName = scene.name;

            if (sceneName == GameConstants.SCENE_MAIN_MENU)
            {
                StopRacingMode();
                SetProfile(menuProfile);
            }
            else if (sceneName == GameConstants.SCENE_WORKSHOP)
            {
                StopRacingMode();
                SetProfile(workshopProfile);
            }
            else if (sceneName.Contains("Inferno") || sceneName.Contains("Molten") || sceneName.Contains("Magma"))
            {
                SetProfile(lavaProfile);
                StartRacingMode();
            }
            else if (sceneName.Contains("Frozen") || sceneName.Contains("Glacial") || sceneName.Contains("Arctic"))
            {
                SetProfile(iceProfile);
                StartRacingMode();
            }
            else if (sceneName.Contains("Rust") || sceneName.Contains("Chemical") || sceneName.Contains("Biohazard"))
            {
                SetProfile(toxicProfile);
                StartRacingMode();
            }
        }

        private void StartRacingMode()
        {
            _isRacing = true;
            FindHovercraft();
            FindCourseManager();
        }

        private void StopRacingMode()
        {
            _isRacing = false;
            UnsubscribeFromGameplay();
            _hovercraft = null;
            _hovercraftHealth = null;
            _hovercraftRb = null;
            _courseManager = null;
        }

        private void FindHovercraft()
        {
            var player = GameObject.FindGameObjectWithTag(GameConstants.TAG_PLAYER);
            if (player == null) return;

            _hovercraft = player.GetComponent<HovercraftController>();
            _hovercraftHealth = player.GetComponent<HovercraftHealth>();
            _hovercraftRb = player.GetComponent<Rigidbody>();

            if (_hovercraft != null)
            {
                // Get max speed from stats for normalization
                var stats = player.GetComponent<HovercraftStats>();
                if (stats != null)
                {
                    // Use a reference value; actual max speed depends on upgrades
                    _maxSpeed = 40f; // Reasonable default
                }
            }
        }

        private void FindCourseManager()
        {
            _courseManager = FindObjectOfType<CourseManager>();
            if (_courseManager != null)
            {
                _courseManager.OnRaceFinished += HandleRaceFinished;
                _courseManager.OnRaceFailed += HandleRaceFailed;
                _courseManager.OnCountdownTick += HandleCountdownTick;
                _courseManager.OnRaceStarted += HandleRaceStarted;
            }
        }

        private void UnsubscribeFromGameplay()
        {
            if (_courseManager != null)
            {
                _courseManager.OnRaceFinished -= HandleRaceFinished;
                _courseManager.OnRaceFailed -= HandleRaceFailed;
                _courseManager.OnCountdownTick -= HandleCountdownTick;
                _courseManager.OnRaceStarted -= HandleRaceStarted;
            }
        }

        // ── Event Handlers ──────────────────────────────────

        private void HandleCountdownTick(int count)
        {
            if (_currentProfile?.countdownTick != null)
                _stinger.Play(_currentProfile.countdownTick, 0.8f);
        }

        private void HandleRaceStarted()
        {
            if (_currentProfile?.goStinger != null)
                _stinger.Play(_currentProfile.goStinger);
        }

        private void HandleRaceFinished(float time)
        {
            if (_currentProfile?.victoryStinger != null)
            {
                // Fade out layers, play victory
                _drumsLayer.FadeOut(0.5f);
                _leadLayer.FadeOut(0.5f);
                _boostLayer.FadeOut(0.3f);
                _stinger.Play(_currentProfile.victoryStinger);
            }
        }

        private void HandleRaceFailed()
        {
            if (_currentProfile?.defeatStinger != null)
            {
                _baseLayer.FadeOut(0.5f);
                _drumsLayer.FadeOut(0.3f);
                _leadLayer.FadeOut(0.3f);
                _boostLayer.FadeOut(0.2f);
                _stinger.Play(_currentProfile.defeatStinger);
            }
        }

        // ── Public API ──────────────────────────────────────

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
        }

        public void PauseMusic()
        {
            _baseLayer.Source.Pause();
            _drumsLayer.Source.Pause();
            _leadLayer.Source.Pause();
            _boostLayer.Source.Pause();
            _ambientSource.Pause();
        }

        public void ResumeMusic()
        {
            _baseLayer.Source.UnPause();
            _drumsLayer.Source.UnPause();
            _leadLayer.Source.UnPause();
            _boostLayer.Source.UnPause();
            _ambientSource.UnPause();
        }

        public void StopAll()
        {
            _baseLayer.Stop();
            _drumsLayer.Stop();
            _leadLayer.Stop();
            _boostLayer.Stop();
            _ambientSource.Stop();
        }

        public void PlayStinger(AudioClip clip, float volume = 1f)
        {
            _stinger?.Play(clip, volume);
        }
    }
}
```

### EnvironmentMusicTrigger.cs

```csharp
// Attach to a GameObject in a course scene to override the auto-detected music profile.
// Useful for the Test Course or custom scenes.

using UnityEngine;
using MetalPod.ScriptableObjects;

namespace MetalPod.Audio
{
    public class EnvironmentMusicTrigger : MonoBehaviour
    {
        [SerializeField] private MusicProfileSO musicProfile;
        [SerializeField] private bool crossfade = true;

        private void Start()
        {
            if (musicProfile != null && DynamicMusicManager.Instance != null)
            {
                DynamicMusicManager.Instance.SetProfile(musicProfile, crossfade);
            }
        }
    }
}
```

### MusicDebugWindow.cs

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MetalPod.Editor
{
    public class MusicDebugWindow : EditorWindow
    {
        [MenuItem("Metal Pod/Music Debug")]
        public static void ShowWindow()
        {
            GetWindow<MusicDebugWindow>("Music Debug");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dynamic Music Debug", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see music state.", MessageType.Info);
                return;
            }

            var mgr = MetalPod.Audio.DynamicMusicManager.Instance;
            if (mgr == null)
            {
                EditorGUILayout.HelpBox("DynamicMusicManager not found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Current Profile",
                mgr.CurrentProfile != null ? mgr.CurrentProfile.displayName : "None");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);

            // The debug window would need to access layer data
            // For now, show profile info
            if (mgr.CurrentProfile != null)
            {
                var p = mgr.CurrentProfile;
                EditorGUILayout.LabelField($"BPM: {p.bpm}");
                EditorGUILayout.LabelField($"Drum Threshold: {p.drumSpeedThreshold:P0}");
                EditorGUILayout.LabelField($"Lead Threshold: {p.leadSpeedThreshold:P0}");
                EditorGUILayout.LabelField($"Layer Fade: {p.layerFadeTime:F1}s");
                EditorGUILayout.LabelField($"Danger Health: {p.dangerHealthThreshold:P0}");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clips:");
                EditorGUILayout.LabelField($"  Base: {(p.baseLayer != null ? p.baseLayer.name : "None")}");
                EditorGUILayout.LabelField($"  Drums: {(p.drumsLayer != null ? p.drumsLayer.name : "None")}");
                EditorGUILayout.LabelField($"  Lead: {(p.leadLayer != null ? p.leadLayer.name : "None")}");
                EditorGUILayout.LabelField($"  Boost: {(p.boostLayer != null ? p.boostLayer.name : "None")}");
            }

            if (Application.isPlaying)
                Repaint();
        }
    }
}
#endif
```

---

## Acceptance Criteria

- [ ] `DynamicMusicManager.cs` — Singleton, 4 music layers + stinger, speed/health monitoring, profile switching
- [ ] `MusicLayer.cs` — AudioSource wrapper with fade in/out, sync, volume control
- [ ] `MusicStinger.cs` — One-shot playback with duck multiplier
- [ ] `EnvironmentMusicTrigger.cs` — Scene-level profile override
- [ ] `MusicProfileSO.cs` — ScriptableObject with all layer clips, thresholds, stingers
- [ ] `MusicDebugWindow.cs` — Editor window showing active profile and layer state
- [ ] Auto-detects environment from scene name (Lava/Ice/Toxic/Menu/Workshop)
- [ ] Layers are synchronized (same playback position)
- [ ] Drums fade in at medium speed, Lead at high speed, Boost only during boost state
- [ ] Victory/defeat stingers play on course end with music ducking
- [ ] Danger stinger plays once when health drops below threshold
- [ ] Does NOT modify AudioManager.cs — creates its own AudioSources
- [ ] All scripts in `MetalPod.Audio`, `MetalPod.ScriptableObjects`, or `MetalPod.Editor` namespaces
- [ ] Compiles without errors
