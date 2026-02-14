using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace MetalPod
{
    /// <summary>
    /// Static bridge to native iOS functionality.
    /// All methods are safe no-ops on non-iOS platforms.
    /// </summary>
    public static class iOSNativePlugin
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void _PrepareHapticEngine();
        [DllImport("__Internal")] private static extern void _TriggerImpactHaptic(int style, float intensity);
        [DllImport("__Internal")] private static extern void _TriggerNotificationHaptic(int type);
        [DllImport("__Internal")] private static extern void _TriggerSelectionHaptic();
        [DllImport("__Internal")] private static extern bool _IsVoiceOverRunning();
        [DllImport("__Internal")] private static extern void _PostVoiceOverAnnouncement(string message);
        [DllImport("__Internal")] private static extern float _GetPreferredFontScale();
        [DllImport("__Internal")] private static extern void _RequestAppReview();
#endif

        public static void PrepareHapticEngine()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _PrepareHapticEngine();
#endif
        }

        /// <summary>
        /// style: 0 = Light, 1 = Medium, 2 = Heavy.
        /// </summary>
        public static void TriggerImpactHaptic(int style, float intensity)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(style, intensity);
#endif
        }

        /// <summary>
        /// type: 0 = Success, 1 = Warning, 2 = Error.
        /// </summary>
        public static void TriggerNotificationHaptic(int type)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerNotificationHaptic(type);
#endif
        }

        public static void TriggerSelectionHaptic()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerSelectionHaptic();
#endif
        }

        public static bool IsVoiceOverRunning()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _IsVoiceOverRunning();
#else
            return false;
#endif
        }

        public static void PostVoiceOverAnnouncement(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            _PostVoiceOverAnnouncement(message);
#endif
        }

        public static float GetPreferredFontScale()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _GetPreferredFontScale();
#else
            return 1.0f;
#endif
        }

        /// <summary>
        /// Request in-app review prompt. Apple controls display frequency.
        /// </summary>
        public static void RequestAppReview()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _RequestAppReview();
#endif
        }
    }
}
