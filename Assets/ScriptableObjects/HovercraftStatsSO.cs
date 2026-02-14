using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "HovercraftStats", menuName = "MetalPod/HovercraftStats")]
    public class HovercraftStatsSO : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Cruising speed before boost multipliers and damage penalties.")]
        [Min(0f)]
        public float baseSpeed = 20f;
        [Tooltip("Hard upper speed cap under ideal conditions.")]
        [Min(0f)]
        public float maxSpeed = 40f;
        [Tooltip("Multiplier applied to speed while boost is active.")]
        [Range(1f, 3f)]
        public float boostMultiplier = 1.5f;
        [Tooltip("Seconds boost remains active per activation.")]
        [Range(0f, 10f)]
        public float boostDuration = 3f;
        [Tooltip("Cooldown in seconds between boost activations.")]
        [Range(0f, 15f)]
        public float boostCooldown = 5f;
        [Tooltip("Deceleration force when braking.")]
        [Min(0f)]
        public float brakeForce = 15f;
        [Tooltip("Yaw responsiveness during steering.")]
        [Min(0f)]
        public float turnSpeed = 3f;

        [Header("Hover Physics")]
        [Tooltip("Target hover height from ground surface.")]
        [Range(0.1f, 10f)]
        public float hoverHeight = 2f;
        [Tooltip("Upward force used to maintain hover height.")]
        [Min(0f)]
        public float hoverForce = 65f;
        [Tooltip("Damping force to stabilize vertical oscillation.")]
        [Min(0f)]
        public float hoverDamping = 5f;
        [Tooltip("Number of hover raycasts sampled each frame.")]
        [Range(1, 12)]
        public int raycastCount = 4;

        [Header("Durability")]
        [Tooltip("Maximum health points.")]
        [Min(1f)]
        public float maxHealth = 100f;
        [Tooltip("Maximum shield points.")]
        [Min(0f)]
        public float maxShield = 50f;
        [Tooltip("Shield points regenerated per second after delay.")]
        [Min(0f)]
        public float shieldRegenRate = 5f;
        [Tooltip("Delay in seconds after taking damage before shield regeneration starts.")]
        [Min(0f)]
        public float shieldRegenDelay = 3f;

        [Header("Handling")]
        [Tooltip("Momentum retention while turning. Higher values produce smoother drift.")]
        [Range(0f, 1f)]
        public float driftFactor = 0.95f;
        [Tooltip("Multiplier applied to tilt-based steering input.")]
        [Range(0.1f, 3f)]
        public float tiltSensitivity = 1f;
        [Tooltip("Auto-leveling force to resist roll and pitch instability.")]
        [Min(0f)]
        public float stabilizationForce = 10f;

        private void OnValidate()
        {
            baseSpeed = Mathf.Max(0f, baseSpeed);
            maxSpeed = Mathf.Max(baseSpeed, maxSpeed);
            boostMultiplier = Mathf.Clamp(boostMultiplier, 1f, 3f);
            boostDuration = Mathf.Clamp(boostDuration, 0f, 10f);
            boostCooldown = Mathf.Clamp(boostCooldown, 0f, 15f);
            brakeForce = Mathf.Max(0f, brakeForce);
            turnSpeed = Mathf.Max(0f, turnSpeed);

            hoverHeight = Mathf.Clamp(hoverHeight, 0.1f, 10f);
            hoverForce = Mathf.Max(0f, hoverForce);
            hoverDamping = Mathf.Max(0f, hoverDamping);
            raycastCount = Mathf.Clamp(raycastCount, 1, 12);

            maxHealth = Mathf.Max(1f, maxHealth);
            maxShield = Mathf.Max(0f, maxShield);
            shieldRegenRate = Mathf.Max(0f, shieldRegenRate);
            shieldRegenDelay = Mathf.Max(0f, shieldRegenDelay);

            driftFactor = Mathf.Clamp01(driftFactor);
            tiltSensitivity = Mathf.Clamp(tiltSensitivity, 0.1f, 3f);
            stabilizationForce = Mathf.Max(0f, stabilizationForce);
        }

        public bool IsValid(out string validationError)
        {
            if (maxSpeed < baseSpeed)
            {
                validationError = "Max speed must be greater than or equal to base speed.";
                return false;
            }

            if (maxHealth <= 0f)
            {
                validationError = "Max health must be greater than zero.";
                return false;
            }

            validationError = string.Empty;
            return true;
        }
    }
}
