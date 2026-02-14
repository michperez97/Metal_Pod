using System;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    public class HovercraftHealth : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private HovercraftStatsSO stats;
        [SerializeField] private float respawnInvincibilitySeconds = 1.25f;

        public event Action<float, float> OnDamage;
        public event Action<float, float, DamageType> OnDamageTyped;
        public event Action OnShieldBreak;
        public event Action<float, float> OnHealthChanged;
        public event Action OnDestroyed;
        public event Action OnRestored;

        public float CurrentHealth { get; private set; }
        public float CurrentShield { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool IsInvincible { get; private set; }

        public float MaxHealth => stats == null ? 0f : stats.maxHealth * _durabilityMultiplier;
        public float MaxShield => stats == null ? 0f : stats.maxShield * _durabilityMultiplier;

        public float HealthNormalized => MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth;
        public float ShieldNormalized => MaxShield <= 0f ? 0f : CurrentShield / MaxShield;

        public float SpeedMultiplier
        {
            get
            {
                float hpRatio = HealthNormalized;
                if (hpRatio <= 0.25f) return 0.6f;
                if (hpRatio <= 0.5f) return 0.8f;
                return 1f;
            }
        }

        public float HandlingMultiplier
        {
            get
            {
                float hpRatio = HealthNormalized;
                if (hpRatio <= 0.25f) return 0.7f;
                if (hpRatio <= 0.5f) return 0.85f;
                return 1f;
            }
        }

        private float _timeSinceLastHit = float.MaxValue;
        private float _invincibilityTimer;
        private float _durabilityMultiplier = 1f;
        private void Start()
        {
            RestoreToFull();
        }

        private void Update()
        {
            if (IsInvincible)
            {
                _invincibilityTimer -= Time.deltaTime;
                if (_invincibilityTimer <= 0f)
                {
                    IsInvincible = false;
                    _invincibilityTimer = 0f;
                }
            }

            if (stats == null || IsDestroyed)
            {
                return;
            }

            _timeSinceLastHit += Time.deltaTime;

            if (_timeSinceLastHit >= stats.shieldRegenDelay && CurrentShield < MaxShield)
            {
                CurrentShield = Mathf.Min(MaxShield, CurrentShield + (stats.shieldRegenRate * Time.deltaTime));
                OnHealthChanged?.Invoke(CurrentHealth, CurrentShield);
            }
        }

        public void ApplyDamage(float amount)
        {
            TakeDamage(amount, DamageType.Physical);
        }

        public void TakeDamage(float amount)
        {
            TakeDamage(amount, DamageType.Physical);
        }

        public void TakeDamage(float amount, DamageType type)
        {
            if (IsDestroyed || IsInvincible || stats == null || amount <= 0f)
            {
                return;
            }

            float finalDamage = amount * GetDamageTypeModifier(type);
            _timeSinceLastHit = 0f;
            float remainingDamage = finalDamage;

            if (CurrentShield > 0f)
            {
                float absorbed = Mathf.Min(CurrentShield, remainingDamage);
                CurrentShield -= absorbed;
                remainingDamage -= absorbed;

                if (CurrentShield <= 0f)
                {
                    CurrentShield = 0f;
                    OnShieldBreak?.Invoke();
                }
            }

            if (remainingDamage > 0f)
            {
                CurrentHealth = Mathf.Max(0f, CurrentHealth - remainingDamage);
            }

            OnDamage?.Invoke(finalDamage, remainingDamage);
            OnDamageTyped?.Invoke(finalDamage, remainingDamage, type);
            OnHealthChanged?.Invoke(CurrentHealth, CurrentShield);
            MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnDamageTaken();

            if (CurrentHealth <= 0f)
            {
                HandleDestroyed();
            }
        }

        public void RestoreHealth(float amount)
        {
            if (amount <= 0f || IsDestroyed)
            {
                return;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, CurrentShield);
        }

        public void RestoreShield(float amount)
        {
            if (amount <= 0f || IsDestroyed)
            {
                return;
            }

            CurrentShield = Mathf.Clamp(CurrentShield + amount, 0f, MaxShield);
            OnHealthChanged?.Invoke(CurrentHealth, CurrentShield);
        }

        public void SetDurabilityMultiplier(float multiplier)
        {
            float clamped = Mathf.Max(0.1f, multiplier);
            if (Mathf.Approximately(_durabilityMultiplier, clamped) || stats == null)
            {
                _durabilityMultiplier = clamped;
                return;
            }

            float oldMaxHealth = MaxHealth;
            float oldMaxShield = MaxShield;
            _durabilityMultiplier = clamped;
            float newMaxHealth = MaxHealth;
            float newMaxShield = MaxShield;

            float healthRatio = oldMaxHealth <= 0f ? 1f : Mathf.Clamp01(CurrentHealth / oldMaxHealth);
            float shieldRatio = oldMaxShield <= 0f ? 1f : Mathf.Clamp01(CurrentShield / oldMaxShield);

            CurrentHealth = newMaxHealth * healthRatio;
            CurrentShield = newMaxShield * shieldRatio;
            OnHealthChanged?.Invoke(CurrentHealth, CurrentShield);
        }

        public void RestoreToFull()
        {
            if (stats == null)
            {
                return;
            }

            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;
            IsDestroyed = false;
            _timeSinceLastHit = float.MaxValue;
            ActivateInvincibility(respawnInvincibilitySeconds);
            OnHealthChanged?.Invoke(CurrentHealth, CurrentShield);
            OnRestored?.Invoke();
        }

        private void HandleDestroyed()
        {
            IsDestroyed = true;
            IsInvincible = false;
            _invincibilityTimer = 0f;
            MetalPod.Accessibility.HapticFeedbackManager.Instance?.OnDestroyed();
            OnDestroyed?.Invoke();
        }

        private void ActivateInvincibility(float duration)
        {
            if (duration <= 0f)
            {
                IsInvincible = false;
                _invincibilityTimer = 0f;
                return;
            }

            IsInvincible = true;
            _invincibilityTimer = duration;
        }

        private static float GetDamageTypeModifier(DamageType type)
        {
            return type switch
            {
                DamageType.Electric => 1.2f,
                DamageType.Explosive => 1.5f,
                DamageType.Fire => 1.1f,
                _ => 1f
            };
        }
    }
}
