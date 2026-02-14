using System;
using System.Collections.Generic;
using MetalPod.Course;
using MetalPod.Hovercraft;
using MetalPod.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private TutorialUI tutorialUI;
        [SerializeField] private MonoBehaviour hovercraftDataSource;
        [SerializeField] private MonoBehaviour courseEventsSource;
        [SerializeField] private HovercraftInput hovercraftInput;
        [SerializeField] private HovercraftHealth hovercraftHealth;

        [Header("Behavior")]
        [SerializeField] private bool forceShowTutorial;
        [SerializeField] private string[] firstPlayCourseScenes = { "Lava_Course_01", "TestCourse" };
        [SerializeField] private float delayAfterConditionMet = 0.2f;

        private TutorialSequence _currentSequence;
        private int _currentStepIndex = -1;
        private TutorialStep _currentStep;
        private bool _isActive;
        private bool _conditionMet;
        private float _stepTimer;
        private float _conditionMetAt;

        private IHovercraftData _hovercraftData;
        private ICourseEvents _courseEvents;

        private readonly List<Collectible> _collectibleSources = new List<Collectible>();

        private bool _tookDamageFlag;
        private bool _shieldUsedFlag;
        private bool _checkpointReachedFlag;
        private bool _collectiblePickedFlag;
        private bool _courseFinishedFlag;

        private bool _timeScaleOverridden;
        private float _previousTimeScale = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ResolveReferences();

            if (tutorialUI == null)
            {
                enabled = false;
                return;
            }

            _currentSequence = ResolveSequenceForCurrentScene();
            if (_currentSequence == null)
            {
                enabled = false;
                return;
            }

            if (!forceShowTutorial && TutorialSaveData.IsTutorialCompleted(_currentSequence.sequenceId))
            {
                enabled = false;
                return;
            }

            _isActive = true;
            SubscribeToEvents();
            AdvanceStep();
        }

        private void Update()
        {
            if (!_isActive || _currentStep == null)
            {
                return;
            }

            _stepTimer += Time.unscaledDeltaTime;

            if (!_conditionMet)
            {
                _conditionMet = CheckCondition(_currentStep);
                if (_conditionMet)
                {
                    _conditionMetAt = _stepTimer;
                }
            }

            if (!_conditionMet && _currentStep.autoAdvanceDelay > 0f && _stepTimer >= _currentStep.autoAdvanceDelay)
            {
                _conditionMet = true;
                _conditionMetAt = _stepTimer;
            }

            if (!_conditionMet)
            {
                return;
            }

            if (_currentStep.requireTapToContinue)
            {
                tutorialUI.ShowTapToContinue();
                if (WasTapPressedThisFrame())
                {
                    AdvanceStep();
                }

                return;
            }

            if (_stepTimer - _conditionMetAt >= Mathf.Max(0.01f, delayAfterConditionMet))
            {
                AdvanceStep();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            RestoreTimeScale();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void TriggerContextualStep(string stepId)
        {
            if (!_isActive || _currentSequence?.steps == null || string.IsNullOrWhiteSpace(stepId))
            {
                return;
            }

            for (int i = _currentStepIndex + 1; i < _currentSequence.steps.Length; i++)
            {
                if (_currentSequence.steps[i].stepId != stepId)
                {
                    continue;
                }

                _currentStepIndex = i - 1;
                AdvanceStep();
                return;
            }
        }

        private void AdvanceStep()
        {
            RestoreTimeScale();

            _currentStepIndex++;
            ResetStepFlags();

            _conditionMet = false;
            _stepTimer = 0f;
            _conditionMetAt = 0f;

            if (_currentSequence?.steps == null || _currentStepIndex >= _currentSequence.steps.Length)
            {
                CompleteTutorial();
                return;
            }

            _currentStep = _currentSequence.steps[_currentStepIndex];

            ApplyStepTimeScale(_currentStep);
            tutorialUI.ShowStep(_currentStep);

            if (_currentStep.completionCondition == TutorialCondition.None)
            {
                _conditionMet = !_currentStep.requireTapToContinue;
                _conditionMetAt = 0f;
            }
        }

        private bool CheckCondition(TutorialStep step)
        {
            switch (step.completionCondition)
            {
                case TutorialCondition.None:
                    return true;
                case TutorialCondition.TiltDevice:
                    if (hovercraftInput != null && Mathf.Abs(hovercraftInput.SteeringInput) > 0.25f)
                    {
                        return true;
                    }

                    return Mathf.Abs(Input.acceleration.x) > 0.15f;
                case TutorialCondition.TapBoost:
                    return hovercraftInput != null && hovercraftInput.BoostPressedThisFrame;
                case TutorialCondition.TapBrake:
                    return hovercraftInput != null && hovercraftInput.BrakeHeld;
                case TutorialCondition.ReachSpeed:
                    return _hovercraftData != null && _hovercraftData.CurrentSpeed >= step.conditionValue;
                case TutorialCondition.TakeDamage:
                    return _tookDamageFlag;
                case TutorialCondition.UseShield:
                    return _shieldUsedFlag;
                case TutorialCondition.ReachCheckpoint:
                    return _checkpointReachedFlag;
                case TutorialCondition.CollectItem:
                    return _collectiblePickedFlag;
                case TutorialCondition.FinishCourse:
                    return _courseFinishedFlag;
                case TutorialCondition.WaitSeconds:
                    return _stepTimer >= Mathf.Max(0f, step.conditionValue);
                default:
                    return false;
            }
        }

        private void SubscribeToEvents()
        {
            if (_courseEvents != null)
            {
                _courseEvents.OnCheckpointReached += HandleCheckpointReached;
                _courseEvents.OnRaceFinished += HandleRaceFinished;
            }

            if (hovercraftHealth != null)
            {
                hovercraftHealth.OnDamage += HandleDamageTaken;
            }

            Collectible[] collectibles = FindObjectsByType<Collectible>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < collectibles.Length; i++)
            {
                Collectible collectible = collectibles[i];
                if (collectible == null)
                {
                    continue;
                }

                collectible.OnCollected += HandleCollectiblePicked;
                _collectibleSources.Add(collectible);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_courseEvents != null)
            {
                _courseEvents.OnCheckpointReached -= HandleCheckpointReached;
                _courseEvents.OnRaceFinished -= HandleRaceFinished;
            }

            if (hovercraftHealth != null)
            {
                hovercraftHealth.OnDamage -= HandleDamageTaken;
            }

            for (int i = 0; i < _collectibleSources.Count; i++)
            {
                Collectible collectible = _collectibleSources[i];
                if (collectible != null)
                {
                    collectible.OnCollected -= HandleCollectiblePicked;
                }
            }

            _collectibleSources.Clear();
        }

        private void HandleCheckpointReached(int _)
        {
            _checkpointReachedFlag = true;
        }

        private void HandleRaceFinished(float _)
        {
            _courseFinishedFlag = true;
        }

        private void HandleCollectiblePicked(Collectible _, HovercraftController __)
        {
            _collectiblePickedFlag = true;
        }

        private void HandleDamageTaken(float totalDamage, float remainingDamage)
        {
            if (totalDamage <= 0f)
            {
                return;
            }

            _tookDamageFlag = true;
            if (remainingDamage < totalDamage)
            {
                _shieldUsedFlag = true;
            }
        }

        private void ApplyStepTimeScale(TutorialStep step)
        {
            if (step == null)
            {
                return;
            }

            if (step.pauseGame)
            {
                OverrideTimeScale(0f);
                return;
            }

            if (step.slowMotion)
            {
                OverrideTimeScale(0.3f);
            }
        }

        private void OverrideTimeScale(float target)
        {
            if (!_timeScaleOverridden)
            {
                _previousTimeScale = Time.timeScale;
            }

            _timeScaleOverridden = true;
            Time.timeScale = target;
        }

        private void RestoreTimeScale()
        {
            if (!_timeScaleOverridden)
            {
                return;
            }

            Time.timeScale = _previousTimeScale;
            _timeScaleOverridden = false;
            _previousTimeScale = 1f;
        }

        private void CompleteTutorial()
        {
            _isActive = false;
            RestoreTimeScale();
            tutorialUI.Hide();

            if (_currentSequence != null)
            {
                TutorialSaveData.SetTutorialCompleted(_currentSequence.sequenceId);
            }

            UnsubscribeFromEvents();
        }

        private void ResetStepFlags()
        {
            _tookDamageFlag = false;
            _shieldUsedFlag = false;
            _checkpointReachedFlag = false;
            _collectiblePickedFlag = false;
            _courseFinishedFlag = false;
        }

        private void ResolveReferences()
        {
            if (tutorialUI == null)
            {
                tutorialUI = FindFirstObjectByType<TutorialUI>(FindObjectsInactive.Include);
            }

            if (hovercraftDataSource == null)
            {
                hovercraftDataSource = FindMonoBehaviourImplementing<IHovercraftData>();
            }

            if (courseEventsSource == null)
            {
                courseEventsSource = FindMonoBehaviourImplementing<ICourseEvents>();
            }

            _hovercraftData = hovercraftDataSource as IHovercraftData;
            _courseEvents = courseEventsSource as ICourseEvents;

            if (hovercraftInput == null && hovercraftDataSource != null)
            {
                hovercraftInput = hovercraftDataSource.GetComponent<HovercraftInput>();
            }

            if (hovercraftInput == null)
            {
                hovercraftInput = FindFirstObjectByType<HovercraftInput>(FindObjectsInactive.Include);
            }

            if (hovercraftHealth == null && hovercraftDataSource != null)
            {
                hovercraftHealth = hovercraftDataSource.GetComponent<HovercraftHealth>();
            }

            if (hovercraftHealth == null)
            {
                hovercraftHealth = FindFirstObjectByType<HovercraftHealth>(FindObjectsInactive.Include);
            }
        }

        private TutorialSequence ResolveSequenceForCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (string.Equals(sceneName, GameConstants.SCENE_WORKSHOP, StringComparison.OrdinalIgnoreCase))
            {
                return TutorialSequence.CreateWorkshopIntroSequence();
            }

            if (forceShowTutorial)
            {
                return TutorialSequence.CreateFirstPlaySequence();
            }

            for (int i = 0; i < firstPlayCourseScenes.Length; i++)
            {
                string candidate = firstPlayCourseScenes[i];
                if (!string.IsNullOrWhiteSpace(candidate) && string.Equals(sceneName, candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return TutorialSequence.CreateFirstPlaySequence();
                }
            }

            return null;
        }

        private static MonoBehaviour FindMonoBehaviourImplementing<TInterface>() where TInterface : class
        {
            MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allBehaviours.Length; i++)
            {
                if (allBehaviours[i] is TInterface)
                {
                    return allBehaviours[i];
                }
            }

            return null;
        }

        private static bool WasTapPressedThisFrame()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        }
    }
}
