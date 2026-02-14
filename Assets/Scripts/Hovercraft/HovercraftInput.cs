using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    public class HovercraftInput : MonoBehaviour
    {
        private const string LegacyTiltSensitivityKey = "MetalPod.TiltSensitivity";
        private const string LegacyInvertTiltKey = "MetalPod.InvertTilt";
        private const string LegacyHapticsEnabledKey = "MetalPod.HapticsEnabled";

        [Header("Tilt")]
        [SerializeField] private float tiltDeadzone = 0.05f;
        [SerializeField] private float altitudeDeadzone = 0.05f;
        [SerializeField] private float defaultTiltSensitivity = 1f;
        [SerializeField] private AnimationCurve sensitivityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Touch Zones")]
        [SerializeField] private float bottomZoneHeightRatio = 0.35f;
        [SerializeField] private float leftZoneWidthRatio = 0.5f;

        public float SteeringInput { get; private set; }
        public float AltitudeInput { get; private set; }
        public bool BoostPressedThisFrame { get; private set; }
        public bool BrakeHeld { get; private set; }
        public bool SpecialPressedThisFrame { get; private set; }

        public float TiltSensitivity { get; private set; }
        public bool InvertTilt { get; private set; }
        public bool HapticsEnabled { get; private set; }

        private void Awake()
        {
            TiltSensitivity = ReadFloatPref(
                GameConstants.PREF_TILT_SENSITIVITY,
                LegacyTiltSensitivityKey,
                defaultTiltSensitivity);
            InvertTilt = ReadIntPref(GameConstants.PREF_INVERT_TILT, LegacyInvertTiltKey, 0) == 1;
            HapticsEnabled = ReadIntPref(GameConstants.PREF_HAPTICS_ENABLED, LegacyHapticsEnabledKey, 1) == 1;
        }

        private void Update()
        {
            ResetFrameInputs();
            ReadTiltInput();
            ReadTouchZones();
        }

        public void SetTiltSensitivity(float value)
        {
            TiltSensitivity = Mathf.Clamp(value, 0.5f, 2f);
            PlayerPrefs.SetFloat(GameConstants.PREF_TILT_SENSITIVITY, TiltSensitivity);
            PlayerPrefs.Save();
        }

        public void SetInvertTilt(bool invert)
        {
            InvertTilt = invert;
            PlayerPrefs.SetInt(GameConstants.PREF_INVERT_TILT, invert ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetHapticsEnabled(bool enabled)
        {
            HapticsEnabled = enabled;
            PlayerPrefs.SetInt(GameConstants.PREF_HAPTICS_ENABLED, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void TriggerHapticFeedback()
        {
            if (!HapticsEnabled)
            {
                return;
            }

            Handheld.Vibrate();
        }

        private void ResetFrameInputs()
        {
            BoostPressedThisFrame = false;
            SpecialPressedThisFrame = false;
            BrakeHeld = false;
        }

        private void ReadTiltInput()
        {
            Vector3 acceleration = Input.acceleration;

#if UNITY_EDITOR
            // Keyboard fallback in editor for quick iteration.
            acceleration.x = Input.GetAxisRaw("Horizontal") * 0.6f;
            acceleration.y = Input.GetAxisRaw("Vertical") * 0.6f;
#endif

            float steeringRaw = ApplyDeadzone(acceleration.x, tiltDeadzone);
            float altitudeRaw = ApplyDeadzone(acceleration.y, altitudeDeadzone);

            float steeringSign = InvertTilt ? -1f : 1f;
            SteeringInput = steeringSign * ApplySensitivityCurve(steeringRaw * TiltSensitivity);
            AltitudeInput = ApplySensitivityCurve(altitudeRaw * TiltSensitivity);
        }

        private void ReadTouchZones()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float bottomThreshold = screenHeight * bottomZoneHeightRatio;
            float leftThreshold = screenWidth * leftZoneWidthRatio;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 pos = touch.position;

                bool isBottomZone = pos.y <= bottomThreshold;
                bool isLeftSide = pos.x <= leftThreshold;
                bool isRightSide = pos.x > leftThreshold;

                if (isBottomZone && isLeftSide)
                {
                    if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        BrakeHeld = true;
                    }
                }

                if (isBottomZone && isRightSide)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        BoostPressedThisFrame = true;
                    }
                }

                if (!isBottomZone && isRightSide && touch.phase == TouchPhase.Began)
                {
                    SpecialPressedThisFrame = true;
                }
            }

#if UNITY_EDITOR
            // Editor testing support.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                BoostPressedThisFrame = true;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                BrakeHeld = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                SpecialPressedThisFrame = true;
            }
#endif
        }

        private float ApplyDeadzone(float value, float deadzone)
        {
            if (Mathf.Abs(value) <= deadzone)
            {
                return 0f;
            }

            float sign = Mathf.Sign(value);
            float adjusted = (Mathf.Abs(value) - deadzone) / (1f - deadzone);
            return sign * Mathf.Clamp01(adjusted);
        }

        private float ApplySensitivityCurve(float input)
        {
            float sign = Mathf.Sign(input);
            float magnitude = Mathf.Clamp01(Mathf.Abs(input));
            float curved = sensitivityCurve.Evaluate(magnitude);
            return sign * curved;
        }

        private static float ReadFloatPref(string key, string legacyKey, float defaultValue)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetFloat(key, defaultValue);
            }

            if (!PlayerPrefs.HasKey(legacyKey))
            {
                return defaultValue;
            }

            float migrated = PlayerPrefs.GetFloat(legacyKey, defaultValue);
            PlayerPrefs.SetFloat(key, migrated);
            PlayerPrefs.Save();
            return migrated;
        }

        private static int ReadIntPref(string key, string legacyKey, int defaultValue)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetInt(key, defaultValue);
            }

            if (!PlayerPrefs.HasKey(legacyKey))
            {
                return defaultValue;
            }

            int migrated = PlayerPrefs.GetInt(legacyKey, defaultValue);
            PlayerPrefs.SetInt(key, migrated);
            PlayerPrefs.Save();
            return migrated;
        }
    }
}
