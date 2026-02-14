using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TMP_Text sensitivityValueText;
        [SerializeField] private Toggle invertTiltToggle;
        [SerializeField] private Toggle hapticsToggle;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown qualityDropdown;

        [SerializeField] private Button backButton;

        private bool _listenersRegistered;

        private void OnEnable()
        {
            LoadPrefsToControls();
            RegisterListeners();
        }

        private void OnDisable()
        {
            UnregisterListeners();
            PlayerPrefs.Save();
        }

        private void RegisterListeners()
        {
            if (_listenersRegistered)
            {
                return;
            }

            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }

            if (invertTiltToggle != null)
            {
                invertTiltToggle.onValueChanged.AddListener(OnInvertChanged);
            }

            if (hapticsToggle != null)
            {
                hapticsToggle.onValueChanged.AddListener(OnHapticsChanged);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBack);
            }

            _listenersRegistered = true;
        }

        private void UnregisterListeners()
        {
            if (!_listenersRegistered)
            {
                return;
            }

            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
            }

            if (invertTiltToggle != null)
            {
                invertTiltToggle.onValueChanged.RemoveListener(OnInvertChanged);
            }

            if (hapticsToggle != null)
            {
                hapticsToggle.onValueChanged.RemoveListener(OnHapticsChanged);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBack);
            }

            _listenersRegistered = false;
        }

        private void LoadPrefsToControls()
        {
            float sensitivity = PlayerPrefs.GetFloat(SharedContractsBridge.PrefTiltSensitivity, 1f);
            bool invert = PlayerPrefs.GetInt(SharedContractsBridge.PrefInvertTilt, 0) == 1;
            bool haptics = PlayerPrefs.GetInt(SharedContractsBridge.PrefHapticsEnabled, 1) == 1;
            float master = PlayerPrefs.GetFloat(SharedContractsBridge.PrefMasterVolume, 1f);
            float music = PlayerPrefs.GetFloat(SharedContractsBridge.PrefMusicVolume, 0.7f);
            float sfx = PlayerPrefs.GetFloat(SharedContractsBridge.PrefSfxVolume, 1f);
            int quality = PlayerPrefs.GetInt(SharedContractsBridge.PrefQualityLevel, Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, Mathf.Max(0, QualitySettings.names.Length - 1)));

            if (sensitivitySlider != null)
            {
                sensitivitySlider.SetValueWithoutNotify(sensitivity);
                UpdateSensitivityLabel(sensitivity);
            }

            if (invertTiltToggle != null)
            {
                invertTiltToggle.SetIsOnWithoutNotify(invert);
            }

            if (hapticsToggle != null)
            {
                hapticsToggle.SetIsOnWithoutNotify(haptics);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(master);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(music);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(sfx);
            }

            if (qualityDropdown != null)
            {
                quality = Mathf.Clamp(quality, 0, Mathf.Max(0, qualityDropdown.options.Count - 1));
                qualityDropdown.SetValueWithoutNotify(quality);
            }

            ApplyVolumeToAudioManager(master, music, sfx);
            ApplyQuality(quality);
        }

        private void OnSensitivityChanged(float value)
        {
            PlayerPrefs.SetFloat(SharedContractsBridge.PrefTiltSensitivity, value);
            UpdateSensitivityLabel(value);
        }

        private void OnInvertChanged(bool isOn)
        {
            PlayerPrefs.SetInt(SharedContractsBridge.PrefInvertTilt, isOn ? 1 : 0);
        }

        private void OnHapticsChanged(bool isOn)
        {
            PlayerPrefs.SetInt(SharedContractsBridge.PrefHapticsEnabled, isOn ? 1 : 0);
        }

        private void OnMasterVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SharedContractsBridge.PrefMasterVolume, Mathf.Clamp01(value));
            ApplyVolumeToAudioManager(
                Mathf.Clamp01(value),
                musicVolumeSlider != null ? Mathf.Clamp01(musicVolumeSlider.value) : 0.7f,
                sfxVolumeSlider != null ? Mathf.Clamp01(sfxVolumeSlider.value) : 1f);
        }

        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SharedContractsBridge.PrefMusicVolume, Mathf.Clamp01(value));
            ApplyVolumeToAudioManager(
                masterVolumeSlider != null ? Mathf.Clamp01(masterVolumeSlider.value) : 1f,
                Mathf.Clamp01(value),
                sfxVolumeSlider != null ? Mathf.Clamp01(sfxVolumeSlider.value) : 1f);
        }

        private void OnSfxVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SharedContractsBridge.PrefSfxVolume, Mathf.Clamp01(value));
            ApplyVolumeToAudioManager(
                masterVolumeSlider != null ? Mathf.Clamp01(masterVolumeSlider.value) : 1f,
                musicVolumeSlider != null ? Mathf.Clamp01(musicVolumeSlider.value) : 0.7f,
                Mathf.Clamp01(value));
        }

        private void OnQualityChanged(int qualityLevel)
        {
            PlayerPrefs.SetInt(SharedContractsBridge.PrefQualityLevel, qualityLevel);
            ApplyQuality(qualityLevel);
        }

        private void OnBack()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.GoBack();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private static void ApplyQuality(int qualityLevel)
        {
            int maxQuality = Mathf.Max(0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(Mathf.Clamp(qualityLevel, 0, maxQuality), true);
        }

        private void UpdateSensitivityLabel(float value)
        {
            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = $"{value:F1}x";
            }
        }

        private static void ApplyVolumeToAudioManager(float master, float music, float sfx)
        {
            MonoBehaviour audioManager = FindAudioManager();
            if (audioManager != null)
            {
                ReflectionValueReader.Invoke(audioManager, "SetMasterVolume", master);
                ReflectionValueReader.Invoke(audioManager, "SetMusicVolume", music);
                ReflectionValueReader.Invoke(audioManager, "SetSfxVolume", sfx);
            }
        }

        private static MonoBehaviour FindAudioManager()
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (behaviour.GetType().Name != "AudioManager")
                {
                    continue;
                }

                MethodInfo method = behaviour.GetType().GetMethod("SetMasterVolume", BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    return behaviour;
                }
            }

            return null;
        }
    }
}
