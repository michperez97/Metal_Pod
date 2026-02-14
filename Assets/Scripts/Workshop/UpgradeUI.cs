using System.Collections.Generic;
using TMPro;
using MetalPod.ScriptableObjects;
using MetalPod.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Workshop
{
    public class UpgradeUI : MonoBehaviour
    {
        [SerializeField] private Transform upgradeSlotContainer;
        [SerializeField] private GameObject upgradeSlotPrefab;

        [Header("Preview")]
        [SerializeField] private TMP_Text currentStatText;
        [SerializeField] private TMP_Text newStatText;
        [SerializeField] private Image statChangeArrow;

        [Header("Purchase")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text maxedOutText;

        [Header("Data")]
        [SerializeField] private UpgradeDataSO[] upgrades;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private MonoBehaviour progressionDataSource;
        [SerializeField] private ProtagonistController protagonistController;

        private readonly Dictionary<string, int> _localLevels = new Dictionary<string, int>();
        private readonly List<UpgradeSlotBinding> _slots = new List<UpgradeSlotBinding>();
        private UpgradeDataSO _selectedUpgrade;

        private void Awake()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(PurchaseSelectedUpgrade);
            }
        }

        private void Start()
        {
            if (progressionDataSource == null)
            {
                progressionDataSource = FindSourceWithMember("GetUpgradeLevel");
            }

            BuildSlots();
            SelectDefaultUpgrade();
            RefreshAll();
        }

        private void BuildSlots()
        {
            _slots.Clear();
            if (upgradeSlotContainer == null || upgrades == null)
            {
                return;
            }

            for (int i = 0; i < upgrades.Length; i++)
            {
                UpgradeDataSO upgrade = upgrades[i];
                if (upgrade == null)
                {
                    continue;
                }

                GameObject slotObj = upgradeSlotPrefab != null
                    ? Instantiate(upgradeSlotPrefab, upgradeSlotContainer)
                    : CreateFallbackSlot(upgrade.upgradeId);

                UpgradeSlotBinding slot = new UpgradeSlotBinding(slotObj);
                slot.Setup(upgrade, () => SelectUpgrade(upgrade));
                _slots.Add(slot);
            }
        }

        private void SelectDefaultUpgrade()
        {
            if (upgrades == null || upgrades.Length == 0)
            {
                _selectedUpgrade = null;
                return;
            }

            for (int i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] != null)
                {
                    _selectedUpgrade = upgrades[i];
                    return;
                }
            }

            _selectedUpgrade = null;
        }

        private void SelectUpgrade(UpgradeDataSO upgrade)
        {
            _selectedUpgrade = upgrade;
            RefreshAll();
        }

        private void RefreshAll()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                UpgradeSlotBinding slot = _slots[i];
                UpgradeDataSO upgrade = slot.Upgrade;
                int level = GetCurrentLevel(upgrade);
                int nextCost = GetNextCost(upgrade, level);
                bool maxed = IsMaxed(upgrade, level);

                slot.UpdateState(level, nextCost, maxed, upgrade == _selectedUpgrade);
            }

            RefreshPreview();
            RefreshPurchaseSection();
        }

        private void RefreshPreview()
        {
            if (_selectedUpgrade == null)
            {
                if (currentStatText != null) currentStatText.text = "--";
                if (newStatText != null) newStatText.text = "--";
                if (statChangeArrow != null) statChangeArrow.gameObject.SetActive(false);
                return;
            }

            int currentLevel = GetCurrentLevel(_selectedUpgrade);
            bool maxed = IsMaxed(_selectedUpgrade, currentLevel);

            float currentValue = GetStatMultiplier(_selectedUpgrade, Mathf.Max(0, currentLevel - 1));
            float newValue = maxed ? currentValue : GetStatMultiplier(_selectedUpgrade, currentLevel);

            if (currentStatText != null)
            {
                currentStatText.text = $"Current: x{currentValue:0.00}";
            }

            if (newStatText != null)
            {
                newStatText.text = maxed ? "Max Level" : $"Next: x{newValue:0.00}";
            }

            if (statChangeArrow != null)
            {
                statChangeArrow.gameObject.SetActive(!maxed);
            }
        }

        private void RefreshPurchaseSection()
        {
            if (_selectedUpgrade == null)
            {
                if (purchaseButton != null) purchaseButton.interactable = false;
                if (costText != null) costText.text = "--";
                if (maxedOutText != null) maxedOutText.gameObject.SetActive(false);
                return;
            }

            int currentLevel = GetCurrentLevel(_selectedUpgrade);
            bool maxed = IsMaxed(_selectedUpgrade, currentLevel);

            if (purchaseButton != null)
            {
                purchaseButton.interactable = !maxed;
            }

            if (costText != null)
            {
                costText.text = maxed ? "MAX" : GetNextCost(_selectedUpgrade, currentLevel).ToString();
            }

            if (maxedOutText != null)
            {
                maxedOutText.gameObject.SetActive(maxed);
            }
        }

        private void PurchaseSelectedUpgrade()
        {
            if (_selectedUpgrade == null)
            {
                return;
            }

            int currentLevel = GetCurrentLevel(_selectedUpgrade);
            if (IsMaxed(_selectedUpgrade, currentLevel))
            {
                return;
            }

            bool purchased = upgradeSystem != null && upgradeSystem.TryPurchaseUpgrade(_selectedUpgrade, currentLevel);
            if (!purchased)
            {
                return;
            }

            int newLevel = currentLevel + 1;
            _localLevels[_selectedUpgrade.upgradeId] = newLevel;
            SharedContractsBridge.Raise("RaiseUpgradePurchased", _selectedUpgrade.upgradeId, newLevel);

            if (protagonistController != null)
            {
                protagonistController.SetCelebrating();
            }

            RefreshAll();
        }

        private int GetCurrentLevel(UpgradeDataSO upgrade)
        {
            if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeId))
            {
                return 0;
            }

            if (progressionDataSource != null)
            {
                object levelObj = ReflectionValueReader.Invoke(progressionDataSource, "GetUpgradeLevel", upgrade.upgradeId);
                if (levelObj is int levelFromSource)
                {
                    return Mathf.Max(0, levelFromSource);
                }
            }

            if (_localLevels.TryGetValue(upgrade.upgradeId, out int localLevel))
            {
                return localLevel;
            }

            return 0;
        }

        private static int GetNextCost(UpgradeDataSO upgrade, int currentLevel)
        {
            if (upgrade == null || upgrade.levels == null || currentLevel < 0 || currentLevel >= upgrade.levels.Length)
            {
                return 0;
            }

            return Mathf.Max(0, upgrade.levels[currentLevel].cost);
        }

        private static bool IsMaxed(UpgradeDataSO upgrade, int currentLevel)
        {
            if (upgrade == null || upgrade.levels == null)
            {
                return true;
            }

            return currentLevel >= upgrade.levels.Length;
        }

        private static float GetStatMultiplier(UpgradeDataSO upgrade, int levelIndex)
        {
            if (upgrade == null || upgrade.levels == null || upgrade.levels.Length == 0)
            {
                return 1f;
            }

            levelIndex = Mathf.Clamp(levelIndex, 0, upgrade.levels.Length - 1);
            return Mathf.Max(0.01f, upgrade.levels[levelIndex].statMultiplier);
        }

        private GameObject CreateFallbackSlot(string upgradeId)
        {
            GameObject slot = new GameObject($"UpgradeSlot_{upgradeId}", typeof(RectTransform), typeof(Image), typeof(Button));
            slot.transform.SetParent(upgradeSlotContainer, false);
            return slot;
        }

        private static MonoBehaviour FindSourceWithMember(string memberName)
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (ReflectionValueReader.HasMember(behaviour, memberName))
                {
                    return behaviour;
                }
            }

            return null;
        }

        private sealed class UpgradeSlotBinding
        {
            public UpgradeDataSO Upgrade { get; private set; }

            private readonly GameObject _root;
            private readonly Button _button;
            private readonly Image _background;
            private readonly Image _icon;
            private readonly TMP_Text _nameText;
            private readonly TMP_Text _levelText;
            private readonly TMP_Text _costText;

            public UpgradeSlotBinding(GameObject root)
            {
                _root = root;
                _button = root.GetComponentInChildren<Button>() ?? root.GetComponent<Button>();
                _background = root.GetComponent<Image>();
                _icon = FindImage(root, "icon");
                _nameText = FindText(root, "name");
                _levelText = FindText(root, "level");
                _costText = FindText(root, "cost");
            }

            public void Setup(UpgradeDataSO upgrade, UnityEngine.Events.UnityAction onSelected)
            {
                Upgrade = upgrade;
                if (_button != null)
                {
                    _button.onClick.RemoveAllListeners();
                    _button.onClick.AddListener(onSelected);
                }

                if (_nameText != null)
                {
                    _nameText.text = upgrade.upgradeName;
                }

                if (_icon != null)
                {
                    _icon.sprite = upgrade.icon;
                }
            }

            public void UpdateState(int currentLevel, int nextCost, bool maxed, bool selected)
            {
                if (_levelText != null)
                {
                    _levelText.text = $"Lv {currentLevel}";
                }

                if (_costText != null)
                {
                    _costText.text = maxed ? "MAX" : nextCost.ToString();
                }

                if (_background != null)
                {
                    _background.color = selected
                        ? new Color(1f, 0.53f, 0f, 0.35f)
                        : new Color(0.21f, 0.21f, 0.21f, 0.8f);
                }
            }

            private static TMP_Text FindText(GameObject root, string key)
            {
                TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length == 0)
                {
                    return null;
                }

                for (int i = 0; i < texts.Length; i++)
                {
                    TMP_Text text = texts[i];
                    if (text != null && text.name.ToLowerInvariant().Contains(key))
                    {
                        return text;
                    }
                }

                return texts[0];
            }

            private static Image FindImage(GameObject root, string key)
            {
                Image[] images = root.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    Image image = images[i];
                    if (image != null && image.name.ToLowerInvariant().Contains(key))
                    {
                        return image;
                    }
                }

                return null;
            }
        }
    }
}
