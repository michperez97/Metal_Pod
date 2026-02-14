using UnityEngine;

namespace MetalPod.Accessibility
{
    /// <summary>
    /// iOS haptic feedback manager with throttling and intensity controls.
    /// </summary>
    public class HapticFeedbackManager : MonoBehaviour
    {
        public enum HapticType
        {
            Light,
            Medium,
            Heavy,
            Success,
            Warning,
            Error
        }

        [Header("Settings")]
        [SerializeField] private bool hapticsEnabled = true;
        [SerializeField] [Range(0f, 1f)] private float hapticIntensity = 1f;
        [SerializeField] private float minTimeBetweenHaptics = 0.05f;

        public static HapticFeedbackManager Instance { get; private set; }

        private float _lastHapticTime;

        public bool HapticsEnabled
        {
            get => hapticsEnabled;
            set => hapticsEnabled = value;
        }

        public float HapticIntensity
        {
            get => hapticIntensity;
            set => hapticIntensity = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_IOS && !UNITY_EDITOR
            MetalPod.iOSNativePlugin.PrepareHapticEngine();
#endif
        }

        public void TriggerHaptic(HapticType type)
        {
            if (!hapticsEnabled)
            {
                return;
            }

            if (Time.unscaledTime - _lastHapticTime < minTimeBetweenHaptics)
            {
                return;
            }

            _lastHapticTime = Time.unscaledTime;

#if UNITY_IOS && !UNITY_EDITOR
            switch (type)
            {
                case HapticType.Light:
                    MetalPod.iOSNativePlugin.TriggerImpactHaptic(0, hapticIntensity);
                    break;
                case HapticType.Medium:
                    MetalPod.iOSNativePlugin.TriggerImpactHaptic(1, hapticIntensity);
                    break;
                case HapticType.Heavy:
                    MetalPod.iOSNativePlugin.TriggerImpactHaptic(2, hapticIntensity);
                    break;
                case HapticType.Success:
                    MetalPod.iOSNativePlugin.TriggerNotificationHaptic(0);
                    break;
                case HapticType.Warning:
                    MetalPod.iOSNativePlugin.TriggerNotificationHaptic(1);
                    break;
                case HapticType.Error:
                    MetalPod.iOSNativePlugin.TriggerNotificationHaptic(2);
                    break;
            }
#endif
        }

        public void OnBoostActivated() => TriggerHaptic(HapticType.Medium);
        public void OnDamageTaken() => TriggerHaptic(HapticType.Heavy);
        public void OnCollision() => TriggerHaptic(HapticType.Heavy);
        public void OnCheckpointReached() => TriggerHaptic(HapticType.Medium);
        public void OnCollectiblePickup() => TriggerHaptic(HapticType.Light);
        public void OnMedalEarned() => TriggerHaptic(HapticType.Success);
        public void OnLowHealth() => TriggerHaptic(HapticType.Warning);
        public void OnDestroyed() => TriggerHaptic(HapticType.Error);
        public void OnUIButtonTap() => TriggerHaptic(HapticType.Light);
        public void OnUpgradePurchased() => TriggerHaptic(HapticType.Success);
    }
}
