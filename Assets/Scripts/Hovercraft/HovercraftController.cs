using System;
using MetalPod.Core;
using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    public enum HovercraftState
    {
        Normal,
        Boosting,
        Braking,
        Damaged,
        Destroyed
    }

    [RequireComponent(typeof(HovercraftPhysics))]
    [RequireComponent(typeof(HovercraftHealth))]
    [RequireComponent(typeof(HovercraftInput))]
    public class HovercraftController : MonoBehaviour
    {
        [SerializeField] private HovercraftStatsSO stats;
        [SerializeField] private float forwardInput = 1f;

        private HovercraftPhysics _physics;
        private HovercraftHealth _health;
        private HovercraftInput _input;
        private HovercraftStats _runtimeStats;

        private float _boostTimer;
        private float _boostCooldownTimer;
        private float _damagedStateTimer;

        public event Action<HovercraftState, HovercraftState> OnStateChanged;

        public HovercraftState CurrentState { get; private set; } = HovercraftState.Normal;
        public float CurrentSpeed => _physics != null ? _physics.CurrentSpeed : 0f;

        public float BoostCooldownNormalized
        {
            get
            {
                float boostCooldown = GetBoostCooldown();
                if (boostCooldown <= 0f)
                {
                    return 0f;
                }

                return Mathf.Clamp01(_boostCooldownTimer / boostCooldown);
            }
        }

        private void Awake()
        {
            _physics = GetComponent<HovercraftPhysics>();
            _health = GetComponent<HovercraftHealth>();
            _input = GetComponent<HovercraftInput>();
            _runtimeStats = GetComponent<HovercraftStats>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDestroyed += HandleDestroyed;
                _health.OnDamage += HandleDamageTaken;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDestroyed -= HandleDestroyed;
                _health.OnDamage -= HandleDamageTaken;
            }
        }

        private void Start()
        {
            GameManager.Instance?.RegisterHovercraft(this);
        }

        private void Update()
        {
            if (CurrentState == HovercraftState.Destroyed || GetBaseStats() == null)
            {
                return;
            }

            UpdateBoostTimers();
            HandleBoostInput();
            UpdateStateFromBrake();

            if (_damagedStateTimer > 0f)
            {
                _damagedStateTimer -= Time.deltaTime;
                if (_damagedStateTimer <= 0f && CurrentState == HovercraftState.Damaged)
                {
                    TransitionToState(HovercraftState.Normal);
                }
            }
        }

        private void FixedUpdate()
        {
            if (CurrentState == HovercraftState.Destroyed || GetBaseStats() == null)
            {
                _physics.SetDriveInput(0f, 0f);
                return;
            }

            float handlingMultiplier = _health != null ? _health.HandlingMultiplier : 1f;
            float speedMultiplier = _health != null ? _health.SpeedMultiplier : 1f;
            float upgradeHandling = _runtimeStats != null ? _runtimeStats.HandlingMultiplier : 1f;
            float upgradeSpeed = _runtimeStats != null ? _runtimeStats.SpeedMultiplier : 1f;

            handlingMultiplier *= upgradeHandling;
            speedMultiplier *= upgradeSpeed;

            if (CurrentState == HovercraftState.Boosting)
            {
                speedMultiplier *= GetBoostMultiplier();
            }

            _physics.SetSpeedMultiplier(speedMultiplier);
            _physics.SetTurnMultiplier(handlingMultiplier);
            _physics.SetAltitudeInput(_input.AltitudeInput);
            _physics.SetBraking(_input.BrakeHeld);
            _physics.SetDriveInput(forwardInput, _input.SteeringInput);
        }

        public void NotifyRespawned()
        {
            _boostTimer = 0f;
            _boostCooldownTimer = 0f;
            TransitionToState(HovercraftState.Normal);
        }

        private void UpdateBoostTimers()
        {
            if (_boostCooldownTimer > 0f)
            {
                _boostCooldownTimer -= Time.deltaTime;
            }

            if (CurrentState == HovercraftState.Boosting)
            {
                _boostTimer -= Time.deltaTime;
                if (_boostTimer <= 0f)
                {
                    _boostCooldownTimer = GetBoostCooldown();
                    TransitionToState(HovercraftState.Normal);
                }
            }
        }

        private void HandleBoostInput()
        {
            if (!_input.BoostPressedThisFrame)
            {
                return;
            }

            if (_boostCooldownTimer > 0f || CurrentState == HovercraftState.Boosting)
            {
                return;
            }

            _boostTimer = GetBoostDuration();
            TransitionToState(HovercraftState.Boosting);
            _input.TriggerHapticFeedback();
            MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnBoostActivated();
        }

        private void UpdateStateFromBrake()
        {
            if (CurrentState == HovercraftState.Boosting || CurrentState == HovercraftState.Damaged)
            {
                return;
            }

            if (_input.BrakeHeld)
            {
                TransitionToState(HovercraftState.Braking);
            }
            else if (CurrentState == HovercraftState.Braking)
            {
                TransitionToState(HovercraftState.Normal);
            }
        }

        private void HandleDamageTaken(float _, float __)
        {
            if (CurrentState == HovercraftState.Destroyed)
            {
                return;
            }

            _damagedStateTimer = 0.2f;
            TransitionToState(HovercraftState.Damaged);
            _input.TriggerHapticFeedback();
        }

        private void HandleDestroyed()
        {
            TransitionToState(HovercraftState.Destroyed);
        }

        private void TransitionToState(HovercraftState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            HovercraftState previous = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(previous, newState);
        }

        private HovercraftStatsSO GetBaseStats()
        {
            if (_runtimeStats != null && _runtimeStats.BaseStats != null)
            {
                return _runtimeStats.BaseStats;
            }

            return stats;
        }

        private float GetBoostDuration()
        {
            if (_runtimeStats != null)
            {
                return _runtimeStats.GetEffectiveBoostDuration();
            }

            HovercraftStatsSO baseStats = GetBaseStats();
            return baseStats != null ? baseStats.boostDuration : 0f;
        }

        private float GetBoostCooldown()
        {
            if (_runtimeStats != null)
            {
                return _runtimeStats.GetEffectiveBoostCooldown();
            }

            HovercraftStatsSO baseStats = GetBaseStats();
            return baseStats != null ? baseStats.boostCooldown : 0f;
        }

        private float GetBoostMultiplier()
        {
            if (_runtimeStats != null)
            {
                return _runtimeStats.GetEffectiveBoostMultiplier();
            }

            HovercraftStatsSO baseStats = GetBaseStats();
            return baseStats != null ? baseStats.boostMultiplier : 1f;
        }
    }
}
