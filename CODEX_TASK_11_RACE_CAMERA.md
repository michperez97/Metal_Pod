# Codex Task 11: Race Camera System

> **Goal**: Create a complete race camera system with smooth chase, look-ahead, damage shake, speed FOV, and cinematic angles for countdown/finish. This is a critical missing piece — the game has no way to follow the hovercraft during races.

---

## Context

The project has a `WorkshopCameraController.cs` for the Workshop scene, but **zero race camera**. During courses, `CourseManager.cs` references `Camera.main` but never positions it. Without a chase camera, the game cannot be played.

The camera must handle:
- Smooth follow behind the hovercraft with damping
- Look-ahead based on velocity direction
- FOV scaling with speed (faster = wider)
- Screen shake on damage/collisions
- Cinematic intro flyover at course start
- Slow-motion finish cam when crossing the finish line
- Respawn camera (quick transition to checkpoint)

**Read these files**:
- `Assets/Scripts/Hovercraft/HovercraftController.cs` — States, speed, boost
- `Assets/Scripts/Hovercraft/HovercraftHealth.cs` — OnDamage, OnDestroyed events
- `Assets/Scripts/Hovercraft/HovercraftPhysics.cs` — Rigidbody velocity, transform
- `Assets/Scripts/Course/CourseManager.cs` — CourseRunState (Ready, Countdown, Racing, Respawning, Finished, Failed), events
- `Assets/Scripts/Hovercraft/HovercraftVisuals.cs` — Already has `screenShakeIntensity` and `screenShakeDuration` fields (lines 27-28) but the shake itself uses `Camera.main` — the race camera should own screen shake instead
- `Assets/Scripts/Shared/GameConstants.cs` — Tags and layer names
- `Assets/Scripts/Core/GameManager.cs` — `ActiveHovercraft` property

---

## Files to Create

```
Assets/Scripts/Camera/
├── RaceCameraController.cs    # Main chase camera with all modes
├── CameraShake.cs             # Reusable screen shake component
├── CameraProfile.cs           # ScriptableObject for camera tuning
└── CinematicCamera.cs         # Intro flyover and finish cam sequences

Assets/ScriptableObjects/
└── CameraProfileSO.cs         # ScriptableObject definition for camera settings
```

**DO NOT modify** any existing files. The race camera will be wired via inspector references when Unity opens.

---

## Architecture

```
RaceCameraController (on Main Camera in course scenes)
  ├── Follow Mode (default during Racing state)
  │   ├── Position: behind + above hovercraft, offset by velocity direction
  │   ├── Rotation: LookAt hovercraft + velocity look-ahead
  │   ├── FOV: base 60° → up to 75° at max speed
  │   └── Smooth damp on position + rotation
  ├── Cinematic Mode (during Countdown state)
  │   ├── Fly along a spline/path around the course start
  │   ├── Or: orbit around the hovercraft at the starting grid
  │   └── Transition to Follow Mode on race start
  ├── Finish Mode (on race finish)
  │   ├── Slow motion (Time.timeScale = 0.3)
  │   ├── Orbit around hovercraft
  │   └── Hold for 2 seconds then return control
  ├── Respawn Mode (during Respawning state)
  │   ├── Quick lerp to respawn checkpoint position
  │   └── Snap behind hovercraft after respawn
  └── CameraShake (attached as component)
      ├── Triggered on damage, collisions, explosions
      ├── Perlin noise for organic feel
      └── Intensity scales with damage amount

CameraProfileSO (tuning data)
  ├── Follow offset, damping, look-ahead distance
  ├── FOV range (min/max), speed FOV curve
  ├── Shake intensity/duration curves
  └── Cinematic orbit radius/speed/duration
```

---

## Detailed Specifications

### CameraProfileSO.cs

```csharp
// ScriptableObject holding all camera tuning parameters.
// Create via: Assets > Create > MetalPod > Camera Profile

using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CameraProfile", menuName = "MetalPod/Camera Profile")]
    public class CameraProfileSO : ScriptableObject
    {
        [Header("Follow Mode")]
        [Tooltip("Offset behind and above the hovercraft (local space)")]
        public Vector3 followOffset = new Vector3(0f, 4f, -8f);

        [Tooltip("How quickly camera catches up to target position (lower = smoother)")]
        [Range(0.01f, 1f)] public float positionSmoothTime = 0.15f;

        [Tooltip("How quickly camera rotation catches up (lower = smoother)")]
        [Range(0.01f, 1f)] public float rotationSmoothTime = 0.1f;

        [Tooltip("How far ahead of the hovercraft the camera looks, based on velocity")]
        [Range(0f, 20f)] public float lookAheadDistance = 6f;

        [Tooltip("Vertical offset for the look-at target above hovercraft center")]
        public float lookAtHeightOffset = 1.5f;

        [Header("Speed Effects")]
        [Tooltip("Camera FOV at zero speed")]
        [Range(40f, 90f)] public float baseFOV = 60f;

        [Tooltip("Camera FOV at maximum speed")]
        [Range(40f, 120f)] public float maxSpeedFOV = 78f;

        [Tooltip("Speed (m/s) at which FOV reaches maxSpeedFOV")]
        public float fovMaxSpeedReference = 40f;

        [Tooltip("How quickly FOV transitions")]
        [Range(0.01f, 1f)] public float fovSmoothTime = 0.25f;

        [Header("Boost Effects")]
        [Tooltip("Extra FOV added during boost")]
        public float boostFOVBonus = 8f;

        [Tooltip("Extra follow distance pulled back during boost")]
        public float boostExtraDistance = 2f;

        [Header("Cinematic Intro")]
        [Tooltip("Duration of intro flyover before countdown")]
        public float introDuration = 3f;

        [Tooltip("Orbit radius around start position during intro")]
        public float introOrbitRadius = 15f;

        [Tooltip("Orbit height during intro")]
        public float introOrbitHeight = 8f;

        [Tooltip("Orbit speed in degrees per second")]
        public float introOrbitSpeed = 60f;

        [Header("Finish Camera")]
        [Tooltip("Time scale during finish cam")]
        [Range(0.1f, 1f)] public float finishTimeScale = 0.3f;

        [Tooltip("Orbit radius during finish celebration")]
        public float finishOrbitRadius = 10f;

        [Tooltip("Duration of finish cam before results screen")]
        public float finishDuration = 2.5f;

        [Tooltip("Orbit speed during finish")]
        public float finishOrbitSpeed = 45f;

        [Header("Respawn")]
        [Tooltip("How fast camera snaps to new position on respawn")]
        [Range(0.01f, 0.5f)] public float respawnSnapTime = 0.3f;

        [Header("Shake")]
        [Tooltip("Default shake intensity for light hits")]
        public float lightShakeIntensity = 0.15f;

        [Tooltip("Default shake intensity for heavy hits")]
        public float heavyShakeIntensity = 0.5f;

        [Tooltip("Shake intensity for explosions")]
        public float explosionShakeIntensity = 0.8f;

        [Tooltip("Default shake duration")]
        public float shakeDecayTime = 0.3f;

        [Tooltip("Perlin noise frequency for shake")]
        public float shakeFrequency = 25f;
    }
}
```

### RaceCameraController.cs

```csharp
// Main race camera that follows the hovercraft with multiple modes.
// Attach to the Main Camera in every course scene.
// Requires CameraShake component and CameraProfileSO asset.

// Namespace: MetalPod.Camera (avoid collision with UnityEngine.Camera by using full qualifier)
// State machine: Intro → Countdown → Follow (Racing) → Finish / Respawn

using UnityEngine;
using MetalPod.Hovercraft;
using MetalPod.Course;
using MetalPod.ScriptableObjects;

namespace MetalPod.GameCamera
{
    public enum CameraMode
    {
        Intro,
        Follow,
        Finish,
        Respawn,
        Disabled
    }

    [RequireComponent(typeof(CameraShake))]
    public class RaceCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraProfileSO profile;
        [SerializeField] private CourseManager courseManager;

        [Header("Target")]
        [SerializeField] private Transform target; // Hovercraft transform — set via inspector or found at runtime

        private UnityEngine.Camera _camera;
        private CameraShake _shake;
        private HovercraftController _hovercraftController;
        private HovercraftHealth _hovercraftHealth;
        private Rigidbody _targetRigidbody;

        // State
        private CameraMode _currentMode = CameraMode.Disabled;
        private float _introTimer;
        private float _finishTimer;
        private float _respawnTimer;
        private float _introStartAngle;

        // Smoothing
        private Vector3 _currentVelocity;
        private float _currentFOV;
        private float _fovVelocity;
        private float _currentYaw;
        private float _yawVelocity;

        public CameraMode CurrentMode => _currentMode;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _shake = GetComponent<CameraShake>();
        }

        private void Start()
        {
            FindTarget();
            SubscribeToCourseEvents();

            if (profile == null)
            {
                Debug.LogError("[RaceCameraController] No CameraProfileSO assigned!");
                return;
            }

            _currentFOV = profile.baseFOV;

            // Start in intro mode if CourseManager is in Ready state
            if (courseManager != null && courseManager.CurrentState == CourseRunState.Ready)
            {
                EnterIntroMode();
            }
            else
            {
                EnterFollowMode();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromCourseEvents();
            UnsubscribeFromHovercraftEvents();
        }

        private void LateUpdate()
        {
            if (target == null || profile == null) return;

            switch (_currentMode)
            {
                case CameraMode.Intro:
                    UpdateIntroMode();
                    break;
                case CameraMode.Follow:
                    UpdateFollowMode();
                    break;
                case CameraMode.Finish:
                    UpdateFinishMode();
                    break;
                case CameraMode.Respawn:
                    UpdateRespawnMode();
                    break;
            }
        }

        // ── Finding Target ─────────────────────────────────────

        private void FindTarget()
        {
            if (target != null) SetupTarget();
            else
            {
                // Find player hovercraft by tag
                GameObject player = GameObject.FindGameObjectWithTag(
                    MetalPod.Shared.GameConstants.TAG_PLAYER);
                if (player != null)
                {
                    target = player.transform;
                    SetupTarget();
                }
            }
        }

        private void SetupTarget()
        {
            _hovercraftController = target.GetComponent<HovercraftController>();
            _hovercraftHealth = target.GetComponent<HovercraftHealth>();
            _targetRigidbody = target.GetComponent<Rigidbody>();
            SubscribeToHovercraftEvents();
        }

        // ── Intro Mode ────────────────────────────────────────

        private void EnterIntroMode()
        {
            _currentMode = CameraMode.Intro;
            _introTimer = 0f;
            _introStartAngle = Random.Range(0f, 360f);
        }

        private void UpdateIntroMode()
        {
            _introTimer += Time.deltaTime;
            float t = _introTimer / profile.introDuration;

            // Orbit around the hovercraft start position
            float angle = _introStartAngle + profile.introOrbitSpeed * _introTimer;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 center = target.position;
            Vector3 orbitPos = center + new Vector3(
                Mathf.Sin(rad) * profile.introOrbitRadius,
                profile.introOrbitHeight,
                Mathf.Cos(rad) * profile.introOrbitRadius
            );

            transform.position = orbitPos;
            transform.LookAt(center + Vector3.up * profile.lookAtHeightOffset);

            // Smoothly narrow FOV as intro ends
            float introFOV = Mathf.Lerp(profile.maxSpeedFOV, profile.baseFOV, t);
            _camera.fieldOfView = introFOV;

            if (_introTimer >= profile.introDuration)
            {
                EnterFollowMode();
            }
        }

        // ── Follow Mode ───────────────────────────────────────

        private void EnterFollowMode()
        {
            _currentMode = CameraMode.Follow;
        }

        private void UpdateFollowMode()
        {
            Vector3 velocity = _targetRigidbody != null ? _targetRigidbody.linearVelocity : Vector3.zero;
            float speed = velocity.magnitude;

            // Calculate desired position behind hovercraft
            Vector3 offset = profile.followOffset;

            // Extra pull-back during boost
            bool isBoosting = _hovercraftController != null &&
                              _hovercraftController.CurrentState == HovercraftState.Boosting;
            if (isBoosting)
            {
                offset.z -= profile.boostExtraDistance;
            }

            // Rotate offset by hovercraft's Y rotation
            Quaternion targetRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            Vector3 desiredPosition = target.position + targetRotation * offset;

            // Smooth position
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition,
                ref _currentVelocity, profile.positionSmoothTime);

            // Look-ahead target
            Vector3 lookAhead = Vector3.zero;
            if (speed > 0.5f)
            {
                lookAhead = velocity.normalized * profile.lookAheadDistance *
                            Mathf.Clamp01(speed / profile.fovMaxSpeedReference);
            }

            Vector3 lookTarget = target.position + Vector3.up * profile.lookAtHeightOffset + lookAhead;

            // Smooth rotation
            Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, desiredRot,
                1f - Mathf.Exp(-10f / Mathf.Max(profile.rotationSmoothTime, 0.001f) * Time.deltaTime));

            // FOV based on speed
            float targetFOV = Mathf.Lerp(
                profile.baseFOV, profile.maxSpeedFOV,
                Mathf.Clamp01(speed / profile.fovMaxSpeedReference));

            if (isBoosting)
                targetFOV += profile.boostFOVBonus;

            _currentFOV = Mathf.SmoothDamp(_currentFOV, targetFOV, ref _fovVelocity, profile.fovSmoothTime);
            _camera.fieldOfView = _currentFOV;
        }

        // ── Finish Mode ───────────────────────────────────────

        private void EnterFinishMode()
        {
            _currentMode = CameraMode.Finish;
            _finishTimer = 0f;
            Time.timeScale = profile.finishTimeScale;
            Time.fixedDeltaTime = 0.02f * profile.finishTimeScale;
        }

        private void UpdateFinishMode()
        {
            _finishTimer += Time.unscaledDeltaTime;
            float t = _finishTimer / profile.finishDuration;

            // Orbit around hovercraft during celebration
            float angle = profile.finishOrbitSpeed * _finishTimer;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 center = target.position;
            Vector3 orbitPos = center + new Vector3(
                Mathf.Sin(rad) * profile.finishOrbitRadius,
                profile.followOffset.y,
                Mathf.Cos(rad) * profile.finishOrbitRadius
            );

            transform.position = Vector3.Lerp(transform.position, orbitPos, Time.unscaledDeltaTime * 3f);
            transform.LookAt(center + Vector3.up * profile.lookAtHeightOffset);

            _camera.fieldOfView = Mathf.Lerp(_currentFOV, profile.baseFOV, t);

            if (_finishTimer >= profile.finishDuration)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
                _currentMode = CameraMode.Disabled;
            }
        }

        // ── Respawn Mode ──────────────────────────────────────

        private void EnterRespawnMode()
        {
            _currentMode = CameraMode.Respawn;
            _respawnTimer = 0f;
        }

        private void UpdateRespawnMode()
        {
            _respawnTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_respawnTimer / profile.respawnSnapTime);
            t = t * t * (3f - 2f * t); // Smoothstep

            // Lerp to position behind respawned hovercraft
            Quaternion targetRot = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            Vector3 desiredPosition = target.position + targetRot * profile.followOffset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);

            Vector3 lookTarget = target.position + Vector3.up * profile.lookAtHeightOffset;
            transform.LookAt(lookTarget);

            if (_respawnTimer >= profile.respawnSnapTime)
            {
                EnterFollowMode();
            }
        }

        // ── Event Subscriptions ───────────────────────────────

        private void SubscribeToCourseEvents()
        {
            if (courseManager == null) return;
            courseManager.OnCountdownStarted += HandleCountdownStarted;
            courseManager.OnRaceStarted += HandleRaceStarted;
            courseManager.OnRaceFinished += HandleRaceFinished;
            courseManager.OnPlayerRespawning += HandleRespawning;
            courseManager.OnPlayerRespawned += HandleRespawned;
        }

        private void UnsubscribeFromCourseEvents()
        {
            if (courseManager == null) return;
            courseManager.OnCountdownStarted -= HandleCountdownStarted;
            courseManager.OnRaceStarted -= HandleRaceStarted;
            courseManager.OnRaceFinished -= HandleRaceFinished;
            courseManager.OnPlayerRespawning -= HandleRespawning;
            courseManager.OnPlayerRespawned -= HandleRespawned;
        }

        private void SubscribeToHovercraftEvents()
        {
            if (_hovercraftHealth != null)
            {
                _hovercraftHealth.OnDamage += HandleDamage;
                _hovercraftHealth.OnDestroyed += HandleDestroyed;
            }
        }

        private void UnsubscribeFromHovercraftEvents()
        {
            if (_hovercraftHealth != null)
            {
                _hovercraftHealth.OnDamage -= HandleDamage;
                _hovercraftHealth.OnDestroyed -= HandleDestroyed;
            }
        }

        // ── Event Handlers ────────────────────────────────────

        private void HandleCountdownStarted() { /* Stay in current mode until race starts */ }
        private void HandleRaceStarted() => EnterFollowMode();
        private void HandleRaceFinished(float time) => EnterFinishMode();
        private void HandleRespawning() => EnterRespawnMode();
        private void HandleRespawned() => EnterFollowMode();

        private void HandleDamage(float damage, float remaining)
        {
            if (_shake == null || profile == null) return;
            float normalizedDamage = Mathf.Clamp01(damage / 30f); // 30 = heavy hit reference
            float intensity = Mathf.Lerp(profile.lightShakeIntensity, profile.heavyShakeIntensity, normalizedDamage);
            _shake.Shake(intensity, profile.shakeDecayTime);
        }

        private void HandleDestroyed()
        {
            if (_shake != null && profile != null)
                _shake.Shake(profile.explosionShakeIntensity, profile.shakeDecayTime * 2f);
        }
    }
}
```

### CameraShake.cs

```csharp
// Reusable Perlin noise-based camera shake.
// Attach alongside RaceCameraController on the camera.
// Call Shake(intensity, duration) to trigger.

using UnityEngine;

namespace MetalPod.GameCamera
{
    public class CameraShake : MonoBehaviour
    {
        private float _currentIntensity;
        private float _maxIntensity;
        private float _decayRate;
        private float _seed;
        private float _frequency = 25f;

        private Vector3 _shakeOffset;

        public bool IsShaking => _currentIntensity > 0.001f;
        public Vector3 ShakeOffset => _shakeOffset;

        public void Shake(float intensity, float duration)
        {
            // Stack shakes: take the higher intensity
            _maxIntensity = Mathf.Max(_currentIntensity, intensity);
            _currentIntensity = _maxIntensity;
            _decayRate = _maxIntensity / Mathf.Max(duration, 0.01f);
            _seed = Random.Range(0f, 1000f);
        }

        public void SetFrequency(float freq)
        {
            _frequency = freq;
        }

        public void StopShake()
        {
            _currentIntensity = 0f;
            _shakeOffset = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_currentIntensity <= 0.001f)
            {
                _shakeOffset = Vector3.zero;
                return;
            }

            float time = Time.unscaledTime * _frequency;

            _shakeOffset = new Vector3(
                (Mathf.PerlinNoise(_seed, time) - 0.5f) * 2f * _currentIntensity,
                (Mathf.PerlinNoise(_seed + 100f, time) - 0.5f) * 2f * _currentIntensity,
                0f // No Z shake to avoid nausea
            );

            // Apply shake offset to local position
            transform.localPosition += _shakeOffset;

            // Decay
            _currentIntensity -= _decayRate * Time.unscaledDeltaTime;
            if (_currentIntensity < 0f) _currentIntensity = 0f;
        }
    }
}
```

### CinematicCamera.cs

```csharp
// Optional cinematic camera for intro flyovers and replay angles.
// Can follow a set of waypoints or perform scripted moves.
// Used by CourseManager's introCamera field.

using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.GameCamera
{
    [System.Serializable]
    public struct CameraWaypoint
    {
        public Vector3 position;
        public Vector3 lookAtOffset;
        public float transitionTime;
        public AnimationCurve easeCurve; // Optional (null = linear)
    }

    public class CinematicCamera : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private CameraWaypoint[] waypoints;
        [SerializeField] private bool loop;

        [Header("Runtime")]
        [SerializeField] private Transform lookAtTarget;

        public event Action OnSequenceComplete;

        private UnityEngine.Camera _camera;
        private Coroutine _activeSequence;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        public void PlaySequence(Transform target = null)
        {
            if (target != null) lookAtTarget = target;
            if (_activeSequence != null) StopCoroutine(_activeSequence);
            _activeSequence = StartCoroutine(RunSequence());
        }

        public void Stop()
        {
            if (_activeSequence != null) StopCoroutine(_activeSequence);
            _isPlaying = false;
        }

        private IEnumerator RunSequence()
        {
            _isPlaying = true;

            do
            {
                for (int i = 0; i < waypoints.Length; i++)
                {
                    var wp = waypoints[i];
                    Vector3 startPos = transform.position;
                    Quaternion startRot = transform.rotation;
                    float elapsed = 0f;

                    while (elapsed < wp.transitionTime)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / wp.transitionTime);

                        if (wp.easeCurve != null && wp.easeCurve.length > 0)
                            t = wp.easeCurve.Evaluate(t);

                        transform.position = Vector3.Lerp(startPos, wp.position, t);

                        if (lookAtTarget != null)
                        {
                            Vector3 lookPos = lookAtTarget.position + wp.lookAtOffset;
                            Quaternion targetRot = Quaternion.LookRotation(lookPos - transform.position);
                            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                        }

                        yield return null;
                    }
                }
            } while (loop);

            _isPlaying = false;
            OnSequenceComplete?.Invoke();
            _activeSequence = null;
        }
    }
}
```

---

## Acceptance Criteria

- [ ] `CameraProfileSO.cs` — ScriptableObject with all tuning parameters, creates via asset menu
- [ ] `RaceCameraController.cs` — 4 modes (Intro/Follow/Finish/Respawn), smooth damping, speed FOV, boost effects
- [ ] `CameraShake.cs` — Perlin noise shake, intensity stacking, decay, no Z-axis shake
- [ ] `CinematicCamera.cs` — Waypoint sequence player with ease curves, look-at target
- [ ] All scripts in `MetalPod.GameCamera` or `MetalPod.ScriptableObjects` namespaces
- [ ] RaceCameraController subscribes to CourseManager events (countdown, race start, finish, respawn)
- [ ] RaceCameraController subscribes to HovercraftHealth events (damage, destroyed) for shake
- [ ] FOV scales from baseFOV to maxSpeedFOV based on Rigidbody velocity magnitude
- [ ] Finish mode sets Time.timeScale and restores it
- [ ] No modifications to existing files
- [ ] Compiles without errors (check all `using` statements)
