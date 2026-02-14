using System;
using System.Reflection;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    public class HovercraftStats : MonoBehaviour, IHovercraftData
    {
        [SerializeField] private HovercraftStatsSO baseStats;

        private HovercraftHealth _health;
        private HovercraftController _controller;
        private HovercraftPhysics _physics;

        private float _speedMultiplier = 1f;
        private float _handlingMultiplier = 1f;
        private float _durabilityMultiplier = 1f;
        private float _boostMultiplier = 1f;

        public event Action<float, float, float, float> OnUpgradeMultipliersApplied;

        public HovercraftStatsSO BaseStats => baseStats;

        public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
        public float MaxHealth => _health != null ? _health.MaxHealth : GetEffectiveMaxHealth();
        public float CurrentShield => _health != null ? _health.CurrentShield : 0f;
        public float MaxShield => _health != null ? _health.MaxShield : GetEffectiveMaxShield();
        public float CurrentSpeed
            => _controller != null ? _controller.CurrentSpeed : (_physics != null ? _physics.CurrentSpeed : 0f);
        public float MaxSpeed => GetEffectiveMaxSpeed();
        public float HealthNormalized => _health != null ? _health.HealthNormalized : 0f;
        public float ShieldNormalized => _health != null ? _health.ShieldNormalized : 0f;
        public float BoostCooldownNormalized => _controller != null ? _controller.BoostCooldownNormalized : 0f;
        public bool IsBoosting => _controller != null && _controller.CurrentState == HovercraftState.Boosting;
        public bool IsDestroyed => _controller != null && _controller.CurrentState == HovercraftState.Destroyed;

        public float SpeedMultiplier => _speedMultiplier;
        public float HandlingMultiplier => _handlingMultiplier;
        public float DurabilityMultiplier => _durabilityMultiplier;
        public float BoostMultiplier => _boostMultiplier;

        private void Awake()
        {
            CacheReferences();
        }

        private void Start()
        {
            ApplyUpgradeMultipliers(_speedMultiplier, _handlingMultiplier, _durabilityMultiplier, _boostMultiplier);
            TryPullUpgradeMultipliersFromScene();
        }

        private void OnEnable()
        {
            EventBus.OnUpgradePurchased += HandleUpgradePurchased;
        }

        private void OnDisable()
        {
            EventBus.OnUpgradePurchased -= HandleUpgradePurchased;
        }

        public void ApplyUpgradeMultipliers(float speed, float handling, float durability, float boost)
        {
            _speedMultiplier = Mathf.Max(0.1f, speed);
            _handlingMultiplier = Mathf.Max(0.1f, handling);
            _durabilityMultiplier = Mathf.Max(0.1f, durability);
            _boostMultiplier = Mathf.Max(0.1f, boost);

            if (_health != null)
            {
                _health.SetDurabilityMultiplier(_durabilityMultiplier);
            }

            OnUpgradeMultipliersApplied?.Invoke(_speedMultiplier, _handlingMultiplier, _durabilityMultiplier, _boostMultiplier);
        }

        public float GetEffectiveBaseSpeed() => baseStats == null ? 0f : baseStats.baseSpeed * _speedMultiplier;
        public float GetEffectiveMaxSpeed() => baseStats == null ? 0f : baseStats.maxSpeed * _speedMultiplier;
        public float GetEffectiveTurnSpeed() => baseStats == null ? 0f : baseStats.turnSpeed * _handlingMultiplier;
        public float GetEffectiveBoostMultiplier() => baseStats == null ? 0f : baseStats.boostMultiplier * _boostMultiplier;
        public float GetEffectiveBoostDuration() => baseStats == null ? 0f : baseStats.boostDuration;
        public float GetEffectiveBoostCooldown() => baseStats == null ? 0f : baseStats.boostCooldown;
        public float GetEffectiveMaxHealth() => baseStats == null ? 0f : baseStats.maxHealth * _durabilityMultiplier;
        public float GetEffectiveMaxShield() => baseStats == null ? 0f : baseStats.maxShield * _durabilityMultiplier;

        private void CacheReferences()
        {
            _health = GetComponent<HovercraftHealth>();
            _controller = GetComponent<HovercraftController>();
            _physics = GetComponent<HovercraftPhysics>();
        }

        private void HandleUpgradePurchased(string _, int __)
        {
            TryPullUpgradeMultipliersFromScene();
        }

        private void TryPullUpgradeMultipliersFromScene()
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour == this)
                {
                    continue;
                }

                MethodInfo method = behaviour.GetType().GetMethod(
                    "GetStatMultipliers",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);

                if (method == null)
                {
                    continue;
                }

                object result = null;
                try
                {
                    result = method.Invoke(behaviour, null);
                }
                catch
                {
                    continue;
                }

                if (result is ValueTuple<float, float, float, float> multipliers)
                {
                    ApplyUpgradeMultipliers(multipliers.Item1, multipliers.Item2, multipliers.Item3, multipliers.Item4);
                    return;
                }
            }
        }
    }
}
