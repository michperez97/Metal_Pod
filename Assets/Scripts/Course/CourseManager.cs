using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Course
{
    public enum CourseRunState
    {
        Ready,
        Countdown,
        Racing,
        Respawning,
        Finished,
        Failed
    }

    public class CourseManager : MonoBehaviour, ICourseEvents
    {
        [Header("References")]
        [SerializeField] private CourseDataSO courseData;
        [SerializeField] private CourseTimer courseTimer;
        [SerializeField] private FinishLine finishLine;
        [SerializeField] private Transform defaultSpawnPoint;

        [Header("Flow")]
        [SerializeField] private float countdownSeconds = 3f;
        [SerializeField] private float respawnDelaySeconds = 1.5f;

        [Header("Intro Camera")]
        [SerializeField] private Camera introCamera;
        [SerializeField] private float introDurationSeconds = 2f;
        [SerializeField] private bool lockPlayerInputDuringIntro = true;

        public event Action OnCountdownStarted;
        public event Action<int> OnCountdownTick;
        public event Action OnRaceStarted;
        public event Action<float> OnRaceFinished;
        public event Action OnRaceFailed;
        public event Action<int> OnCheckpointReached;
        public event Action OnPlayerRespawning;

        public event Action OnPlayerRespawned;
        public event Action<Medal, float> OnCourseCompleted;

        public CourseRunState CurrentState { get; private set; } = CourseRunState.Ready;

        private Checkpoint[] _checkpoints;
        private Checkpoint _lastCheckpoint;
        private int _nextCheckpointOrder;

        private HovercraftController _playerController;
        private HovercraftHealth _playerHealth;
        private HovercraftInput _playerInput;
        private Rigidbody _playerRigidbody;

        private void Awake()
        {
            _checkpoints = FindObjectsOfType<Checkpoint>(true)
                .OrderBy(cp => cp.CheckpointIndex)
                .ToArray();
        }

        private void OnEnable()
        {
            for (int i = 0; i < _checkpoints.Length; i++)
            {
                _checkpoints[i].OnActivated += HandleCheckpointActivated;
            }

            if (finishLine != null)
            {
                finishLine.OnPlayerFinished += HandlePlayerFinished;
            }
        }

        private void OnDisable()
        {
            if (finishLine != null)
            {
                finishLine.OnPlayerFinished -= HandlePlayerFinished;
            }

            for (int i = 0; i < _checkpoints.Length; i++)
            {
                _checkpoints[i].OnActivated -= HandleCheckpointActivated;
            }

            UnbindPlayerEvents();
        }

        private void Start()
        {
            if (_playerController == null)
            {
                HovercraftController discoveredPlayer = FindObjectOfType<HovercraftController>();
                if (discoveredPlayer != null)
                {
                    RegisterPlayer(discoveredPlayer);
                }
            }

            BeginCourse();
        }

        public void RegisterPlayer(HovercraftController playerController)
        {
            UnbindPlayerEvents();

            _playerController = playerController;
            _playerHealth = playerController.GetComponent<HovercraftHealth>();
            _playerInput = playerController.GetComponent<HovercraftInput>();
            _playerRigidbody = playerController.GetComponent<Rigidbody>();

            if (_playerHealth != null)
            {
                _playerHealth.OnDestroyed += HandlePlayerDestroyed;
            }
        }

        public void BeginCourse()
        {
            StopAllCoroutines();
            ResetCourseProgress();
            StartCoroutine(CourseStartRoutine());
        }

        public void FailCourse()
        {
            if (CurrentState == CourseRunState.Finished || CurrentState == CourseRunState.Failed)
            {
                return;
            }

            CurrentState = CourseRunState.Failed;
            courseTimer?.StopTimer();
            OnRaceFailed?.Invoke();
        }

        private IEnumerator CourseStartRoutine()
        {
            yield return PlayIntroCameraRoutine();

            CurrentState = CourseRunState.Countdown;
            courseTimer?.ResetTimer();
            OnCountdownStarted?.Invoke();

            int countdown = Mathf.CeilToInt(countdownSeconds);
            while (countdown > 0)
            {
                OnCountdownTick?.Invoke(countdown);
                yield return new WaitForSeconds(1f);
                countdown--;
            }

            CurrentState = CourseRunState.Racing;
            courseTimer?.StartTimer();
            OnRaceStarted?.Invoke();
        }

        private IEnumerator PlayIntroCameraRoutine()
        {
            if (introCamera == null || introDurationSeconds <= 0f)
            {
                yield break;
            }

            Camera mainCamera = Camera.main;

            if (mainCamera != null && mainCamera != introCamera)
            {
                mainCamera.enabled = false;
            }

            introCamera.enabled = true;

            if (lockPlayerInputDuringIntro && _playerInput != null)
            {
                _playerInput.enabled = false;
            }

            yield return new WaitForSeconds(introDurationSeconds);

            introCamera.enabled = false;

            if (mainCamera != null && mainCamera != introCamera)
            {
                mainCamera.enabled = true;
            }

            if (lockPlayerInputDuringIntro && _playerInput != null)
            {
                _playerInput.enabled = true;
            }
        }

        private void HandleCheckpointActivated(Checkpoint checkpoint, HovercraftController controller)
        {
            if (controller != _playerController || CurrentState != CourseRunState.Racing)
            {
                return;
            }

            if (_nextCheckpointOrder >= _checkpoints.Length)
            {
                checkpoint.ResetCheckpoint();
                return;
            }

            Checkpoint expectedCheckpoint = _checkpoints[_nextCheckpointOrder];
            if (checkpoint != expectedCheckpoint)
            {
                checkpoint.ResetCheckpoint();
                return;
            }

            _lastCheckpoint = checkpoint;
            _nextCheckpointOrder++;
            OnCheckpointReached?.Invoke(checkpoint.CheckpointIndex);
        }

        private void HandlePlayerDestroyed()
        {
            if (CurrentState != CourseRunState.Racing)
            {
                return;
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            CurrentState = CourseRunState.Respawning;
            courseTimer?.PauseTimer();
            OnPlayerRespawning?.Invoke();

            yield return new WaitForSeconds(respawnDelaySeconds);

            Transform spawn = _lastCheckpoint != null ? _lastCheckpoint.SpawnPoint : defaultSpawnPoint;
            if (spawn != null && _playerController != null)
            {
                _playerController.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            }

            if (_playerRigidbody != null)
            {
                _playerRigidbody.velocity = Vector3.zero;
                _playerRigidbody.angularVelocity = Vector3.zero;
            }

            _playerHealth?.RestoreToFull();
            _playerController?.NotifyRespawned();

            CurrentState = CourseRunState.Racing;
            courseTimer?.ResumeTimer();
            OnPlayerRespawned?.Invoke();
        }

        private void HandlePlayerFinished(HovercraftController controller)
        {
            if (CurrentState != CourseRunState.Racing || controller != _playerController)
            {
                return;
            }

            CurrentState = CourseRunState.Finished;
            courseTimer?.StopTimer();

            float completionTime = courseTimer != null ? courseTimer.ElapsedTime : 0f;
            Medal medal = MedalSystem.EvaluatePerformance(completionTime, courseData);

            OnRaceFinished?.Invoke(completionTime);
            OnCourseCompleted?.Invoke(medal, completionTime);

            string courseId = !string.IsNullOrWhiteSpace(courseData?.courseId)
                ? courseData.courseId
                : SceneManager.GetActiveScene().name;

            CourseEventBus.RaiseCourseCompleted(courseId, completionTime, (int)medal);
            RaiseSharedEventBusCourseCompleted(courseId, completionTime, (int)medal);
        }

        private void RaiseSharedEventBusCourseCompleted(string courseId, float completionTime, int medal)
        {
            // Bridge call for Agent 1 EventBus when it exists, without hard dependency.
            Type eventBusType = Type.GetType("MetalPod.Shared.EventBus, Assembly-CSharp");
            if (eventBusType == null)
            {
                return;
            }

            MethodInfo raiseMethod = eventBusType.GetMethod(
                "RaiseCourseCompleted",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(float), typeof(int) },
                null);

            raiseMethod?.Invoke(null, new object[] { courseId, completionTime, medal });
        }

        private void ResetCourseProgress()
        {
            _lastCheckpoint = null;
            _nextCheckpointOrder = 0;

            for (int i = 0; i < _checkpoints.Length; i++)
            {
                _checkpoints[i].ResetCheckpoint();
            }
        }

        private void UnbindPlayerEvents()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDestroyed -= HandlePlayerDestroyed;
            }

            _playerController = null;
            _playerHealth = null;
            _playerInput = null;
            _playerRigidbody = null;
        }
    }
}
