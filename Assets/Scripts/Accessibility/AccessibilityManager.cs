using UnityEngine;

namespace MetalPod.Accessibility
{
    /// <summary>
    /// Centralized accessibility manager for iOS VoiceOver and Dynamic Type support.
    /// Attach this to a persistent object (for example: GameManager root).
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool enableVoiceOverSupport = true;
        [SerializeField] private bool enableDynamicType = true;
        [SerializeField] private float minimumFontScale = 0.8f;
        [SerializeField] private float maximumFontScale = 1.5f;

        public static AccessibilityManager Instance { get; private set; }

        public bool IsVoiceOverRunning { get; private set; }
        public float FontScale { get; private set; } = 1.0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            UpdateAccessibilityState();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                UpdateAccessibilityState();
            }
        }

        /// <summary>
        /// Query iOS for current accessibility settings.
        /// </summary>
        public void UpdateAccessibilityState()
        {
#if UNITY_IOS && !UNITY_EDITOR
            IsVoiceOverRunning = MetalPod.iOSNativePlugin.IsVoiceOverRunning();
            if (enableDynamicType)
            {
                float nativeScale = MetalPod.iOSNativePlugin.GetPreferredFontScale();
                FontScale = Mathf.Clamp(nativeScale, minimumFontScale, maximumFontScale);
            }
            else
            {
                FontScale = 1.0f;
            }
#else
            IsVoiceOverRunning = false;
            FontScale = 1.0f;
#endif
        }

        /// <summary>
        /// Announce text through VoiceOver when enabled.
        /// </summary>
        public void Announce(string message)
        {
            if (!enableVoiceOverSupport || !IsVoiceOverRunning || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            MetalPod.iOSNativePlugin.PostVoiceOverAnnouncement(message);
#endif
        }

        /// <summary>
        /// Set/attach accessibility metadata on a UI element.
        /// Native mapping to UIKit accessibility elements can consume this metadata.
        /// </summary>
        public void SetLabel(GameObject uiElement, string label, string hint = "", bool isButton = false)
        {
            if (!enableVoiceOverSupport || uiElement == null)
            {
                return;
            }

            AccessibilityLabel labelComponent = uiElement.GetComponent<AccessibilityLabel>();
            if (labelComponent == null)
            {
                labelComponent = uiElement.AddComponent<AccessibilityLabel>();
            }

            labelComponent.Label = label;
            labelComponent.Hint = hint;
            labelComponent.IsButton = isButton;
        }
    }

    /// <summary>
    /// Attach to a UI GameObject to store VoiceOver metadata.
    /// </summary>
    public class AccessibilityLabel : MonoBehaviour
    {
        [TextArea] public string Label;
        [TextArea] public string Hint;
        public bool IsButton;
    }
}
