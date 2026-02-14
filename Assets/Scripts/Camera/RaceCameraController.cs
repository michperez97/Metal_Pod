using MetalPod.Core;
using MetalPod.Course;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

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
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindTarget = true;

        private UnityEngine.Camera cameraComponent;
        private CameraShake shake;
        private HovercraftController hovercraftController;
        private HovercraftHealth hovercraftHealth;
        private Rigidbody targetRigidbody;

        private CameraMode currentMode = CameraMode.Disabled;
        private float introTimer;
        private float finishTimer;
        private float respawnTimer;
        private float introStartAngle;

        private Vector3 positionVelocity;
        private float currentFov;
        private float fovVelocity;

        private bool courseEventsSubscribed;
        private bool hovercraftEventsSubscribed;

        public CameraMode CurrentMode => currentMode;

        private void Awake()
        {
            cameraComponent = GetComponent<UnityEngine.Camera>();
            shake = GetComponent<CameraShake>();
        }

        private void OnEnable()
        {
            ResolveCourseManager();
            SubscribeToCourseEvents();
        }

        private void Start()
        {
            FindTarget();

            if (profile == null)
            {
                Debug.LogError("[RaceCameraController] No CameraProfileSO assigned.");
                currentMode = CameraMode.Disabled;
                return;
            }

            if (shake != null)
            {
                shake.SetFrequency(profile.shakeFrequency);
            }

            currentFov = profile.baseFOV;
            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = currentFov;
            }

            if (courseManager != null)
            {
                if (courseManager.CurrentState == CourseRunState.Ready || courseManager.CurrentState == CourseRunState.Countdown)
                {
                    EnterIntroMode();
                }
                else if (courseManager.CurrentState == CourseRunState.Respawning)
                {
                    EnterRespawnMode();
                }
                else
                {
                    EnterFollowMode();
                }
            }
            else
            {
                EnterFollowMode();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromCourseEvents();
            UnsubscribeFromHovercraftEvents();
            RestoreTimeScale();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCourseEvents();
            UnsubscribeFromHovercraftEvents();
            RestoreTimeScale();
        }

        private void LateUpdate()
        {
            if (profile == null)
            {
                return;
            }

            if (target == null && autoFindTarget)
            {
                FindTarget();
            }

            if (target == null)
            {
                return;
            }

            if (shake != null)
            {
                shake.SetFrequency(profile.shakeFrequency);
            }

            switch (currentMode)
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

            ApplyShakeOffset();
        }

        private void ResolveCourseManager()
        {
            if (courseManager == null)
            {
                courseManager = FindObjectOfType<CourseManager>();
            }
        }

        private void FindTarget()
        {
            if (target != null)
            {
                SetupTarget();
                return;
            }

            HovercraftController fromManager = GameManager.Instance != null
                ? GameManager.Instance.ActiveHovercraft
                : null;

            if (fromManager != null)
            {
                target = fromManager.transform;
                SetupTarget();
                return;
            }

            GameObject playerByTag = GameObject.FindGameObjectWithTag(GameConstants.TAG_PLAYER);
            if (playerByTag != null)
            {
                target = playerByTag.transform;
                SetupTarget();
                return;
            }

            HovercraftController fallback = FindObjectOfType<HovercraftController>();
            if (fallback != null)
            {
                target = fallback.transform;
                SetupTarget();
            }
        }

        private void SetupTarget()
        {
            UnsubscribeFromHovercraftEvents();

            if (target == null)
            {
                hovercraftController = null;
                hovercraftHealth = null;
                targetRigidbody = null;
                return;
            }

            hovercraftController = target.GetComponent<HovercraftController>();
            if (hovercraftController == null)
            {
                hovercraftController = target.GetComponentInParent<HovercraftController>();
            }

            hovercraftHealth = target.GetComponent<HovercraftHealth>();
            if (hovercraftHealth == null && hovercraftController != null)
            {
                hovercraftHealth = hovercraftController.GetComponent<HovercraftHealth>();
            }

            targetRigidbody = target.GetComponent<Rigidbody>();
            if (targetRigidbody == null && hovercraftController != null)
            {
                targetRigidbody = hovercraftController.GetComponent<Rigidbody>();
            }

            SubscribeToHovercraftEvents();
        }

        private void EnterIntroMode()
        {
            currentMode = CameraMode.Intro;
            introTimer = 0f;
            introStartAngle = Random.Range(0f, 360f);
            RestoreTimeScale();
        }

        private void UpdateIntroMode()
        {
            introTimer += Time.deltaTime;
            float duration = Mathf.Max(0.01f, profile.introDuration);
            float t = Mathf.Clamp01(introTimer / duration);

            float angle = introStartAngle + (profile.introOrbitSpeed * introTimer);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 center = target.position;
            Vector3 orbitPosition = center + new Vector3(
                Mathf.Sin(rad) * profile.introOrbitRadius,
                profile.introOrbitHeight,
                Mathf.Cos(rad) * profile.introOrbitRadius);

            transform.position = orbitPosition;
            transform.LookAt(center + Vector3.up * profile.lookAtHeightOffset);

            float introFov = Mathf.Lerp(profile.maxSpeedFOV, profile.baseFOV, t);
            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = introFov;
            }

            if (introTimer >= duration)
            {
                EnterFollowMode();
            }
        }

        private void EnterFollowMode()
        {
            currentMode = CameraMode.Follow;
            RestoreTimeScale();
        }

        private void UpdateFollowMode()
        {
            Vector3 velocity = targetRigidbody != null ? targetRigidbody.velocity : Vector3.zero;
            Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float speed = velocity.magnitude;

            bool isBoosting = hovercraftController != null && hovercraftController.CurrentState == HovercraftState.Boosting;
            Vector3 localOffset = CameraProfile.GetFollowOffset(profile, isBoosting);

            Vector3 forward = target.forward;
            if (planarVelocity.sqrMagnitude > 0.25f)
            {
                Vector3 velocityForward = planarVelocity.normalized;
                forward = Vector3.Slerp(target.forward, velocityForward, 0.55f);
            }

            Quaternion yawRotation = Quaternion.LookRotation(forward, Vector3.up);
            Vector3 desiredPosition = target.position
                + (yawRotation * new Vector3(localOffset.x, 0f, localOffset.z))
                + (Vector3.up * localOffset.y);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                profile.positionSmoothTime);

            Vector3 lookAhead = Vector3.zero;
            if (planarVelocity.sqrMagnitude > 0.25f)
            {
                float speed01 = CameraProfile.GetSpeedNormalized(profile, speed);
                lookAhead = planarVelocity.normalized * (profile.lookAheadDistance * speed01);
            }

            Vector3 lookTarget = target.position + Vector3.up * profile.lookAtHeightOffset + lookAhead;
            Vector3 toLookTarget = lookTarget - transform.position;
            if (toLookTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(toLookTarget.normalized, Vector3.up);
                float rotationLerp = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(profile.rotationSmoothTime, 0.001f));
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationLerp);
            }

            float targetFov = CameraProfile.GetTargetFov(profile, speed, isBoosting);
            currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, profile.fovSmoothTime);
            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = currentFov;
            }
        }

        private void EnterFinishMode()
        {
            currentMode = CameraMode.Finish;
            finishTimer = 0f;

            Time.timeScale = profile.finishTimeScale;
            Time.fixedDeltaTime = 0.02f * profile.finishTimeScale;
        }

        private void UpdateFinishMode()
        {
            finishTimer += Time.unscaledDeltaTime;
            float duration = Mathf.Max(0.01f, profile.finishDuration);
            float t = Mathf.Clamp01(finishTimer / duration);

            float angle = profile.finishOrbitSpeed * finishTimer;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 center = target.position;
            Vector3 orbitPosition = center + new Vector3(
                Mathf.Sin(rad) * profile.finishOrbitRadius,
                profile.followOffset.y,
                Mathf.Cos(rad) * profile.finishOrbitRadius);

            transform.position = Vector3.Lerp(transform.position, orbitPosition, Time.unscaledDeltaTime * 3f);
            transform.LookAt(center + Vector3.up * profile.lookAtHeightOffset);

            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = Mathf.Lerp(currentFov, profile.baseFOV, t);
            }

            if (finishTimer >= duration)
            {
                RestoreTimeScale();
                currentMode = CameraMode.Disabled;
            }
        }

        private void EnterRespawnMode()
        {
            currentMode = CameraMode.Respawn;
            respawnTimer = 0f;
            RestoreTimeScale();
        }

        private void UpdateRespawnMode()
        {
            respawnTimer += Time.deltaTime;
            float t = Mathf.Clamp01(respawnTimer / Mathf.Max(0.01f, profile.respawnSnapTime));
            t = t * t * (3f - (2f * t));

            Quaternion yawRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            Vector3 desiredPosition = target.position + yawRotation * profile.followOffset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
            Vector3 lookTarget = target.position + Vector3.up * profile.lookAtHeightOffset;
            transform.LookAt(lookTarget);

            if (respawnTimer >= profile.respawnSnapTime)
            {
                EnterFollowMode();
            }
        }

        private void SubscribeToCourseEvents()
        {
            if (courseManager == null || courseEventsSubscribed)
            {
                return;
            }

            courseManager.OnCountdownStarted += HandleCountdownStarted;
            courseManager.OnRaceStarted += HandleRaceStarted;
            courseManager.OnRaceFinished += HandleRaceFinished;
            courseManager.OnPlayerRespawning += HandleRespawning;
            courseManager.OnPlayerRespawned += HandleRespawned;
            courseEventsSubscribed = true;
        }

        private void UnsubscribeFromCourseEvents()
        {
            if (courseManager == null || !courseEventsSubscribed)
            {
                return;
            }

            courseManager.OnCountdownStarted -= HandleCountdownStarted;
            courseManager.OnRaceStarted -= HandleRaceStarted;
            courseManager.OnRaceFinished -= HandleRaceFinished;
            courseManager.OnPlayerRespawning -= HandleRespawning;
            courseManager.OnPlayerRespawned -= HandleRespawned;
            courseEventsSubscribed = false;
        }

        private void SubscribeToHovercraftEvents()
        {
            if (hovercraftHealth == null || hovercraftEventsSubscribed)
            {
                return;
            }

            hovercraftHealth.OnDamage += HandleDamage;
            hovercraftHealth.OnDestroyed += HandleDestroyed;
            hovercraftEventsSubscribed = true;
        }

        private void UnsubscribeFromHovercraftEvents()
        {
            if (hovercraftHealth == null || !hovercraftEventsSubscribed)
            {
                return;
            }

            hovercraftHealth.OnDamage -= HandleDamage;
            hovercraftHealth.OnDestroyed -= HandleDestroyed;
            hovercraftEventsSubscribed = false;
        }

        private void HandleCountdownStarted()
        {
            if (currentMode == CameraMode.Disabled || currentMode == CameraMode.Follow)
            {
                EnterIntroMode();
            }
        }

        private void HandleRaceStarted()
        {
            EnterFollowMode();
        }

        private void HandleRaceFinished(float _)
        {
            EnterFinishMode();
        }

        private void HandleRespawning()
        {
            EnterRespawnMode();
        }

        private void HandleRespawned()
        {
            EnterFollowMode();
        }

        private void HandleDamage(float damage, float _)
        {
            if (shake == null || profile == null)
            {
                return;
            }

            float normalizedDamage = Mathf.Clamp01(damage / 30f);
            float intensity = Mathf.Lerp(profile.lightShakeIntensity, profile.heavyShakeIntensity, normalizedDamage);
            shake.Shake(intensity, profile.shakeDecayTime);
        }

        private void HandleDestroyed()
        {
            if (shake == null || profile == null)
            {
                return;
            }

            shake.Shake(profile.explosionShakeIntensity, profile.shakeDecayTime * 2f);
        }

        private void ApplyShakeOffset()
        {
            if (shake == null)
            {
                return;
            }

            Vector3 offset = shake.ShakeOffset;
            if (offset.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.position += (transform.right * offset.x) + (transform.up * offset.y);
        }

        private static void RestoreTimeScale()
        {
            if (Mathf.Abs(Time.timeScale - 1f) > 0.001f)
            {
                Time.timeScale = 1f;
            }

            if (Mathf.Abs(Time.fixedDeltaTime - 0.02f) > 0.0001f)
            {
                Time.fixedDeltaTime = 0.02f;
            }
        }
    }
}
