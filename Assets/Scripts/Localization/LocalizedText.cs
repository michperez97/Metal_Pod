using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Localization
{
    /// <summary>
    /// Attach to any UI Text or TextMeshProUGUI to auto-localize.
    /// </summary>
    [AddComponentMenu("MetalPod/Localized Text")]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string localizationKey;

        private Text _uiText;
        private TextMeshProUGUI _tmpText;
        private bool _isSubscribed;

        public string Key
        {
            get => localizationKey;
            set
            {
                localizationKey = value;
                UpdateText();
            }
        }

        private void Awake()
        {
            CacheTextReferences();
        }

        private void Start()
        {
            SubscribeToLanguageChange();
            UpdateText();
        }

        private void OnEnable()
        {
            CacheTextReferences();
            SubscribeToLanguageChange();
            UpdateText();
        }

        private void OnDisable()
        {
            UnsubscribeFromLanguageChange();
        }

        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey))
            {
                return;
            }

            string text = Loc.Get(localizationKey);
            ApplyText(text);
        }

        /// <summary>
        /// Updates text with format arguments.
        /// </summary>
        public void UpdateText(params object[] args)
        {
            if (string.IsNullOrEmpty(localizationKey))
            {
                return;
            }

            string text = Loc.Get(localizationKey, args);
            ApplyText(text);
        }

        private void CacheTextReferences()
        {
            if (_uiText == null)
            {
                _uiText = GetComponent<Text>();
            }

            if (_tmpText == null)
            {
                _tmpText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void ApplyText(string value)
        {
            if (_tmpText != null)
            {
                _tmpText.text = value;
                return;
            }

            if (_uiText != null)
            {
                _uiText.text = value;
            }
        }

        private void SubscribeToLanguageChange()
        {
            if (_isSubscribed || LocalizationManager.Instance == null)
            {
                return;
            }

            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            _isSubscribed = true;
        }

        private void UnsubscribeFromLanguageChange()
        {
            if (!_isSubscribed || LocalizationManager.Instance == null)
            {
                return;
            }

            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
            _isSubscribed = false;
        }
    }
}
