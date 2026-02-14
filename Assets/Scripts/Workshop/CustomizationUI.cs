using System.Collections.Generic;
using System.Reflection;
using TMPro;
using MetalPod.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Workshop
{
    public class CustomizationUI : MonoBehaviour
    {
        private enum CustomizationTab
        {
            Colors,
            Decals,
            Parts
        }

        [System.Serializable]
        private class CustomizationItem
        {
            public string id;
            public string displayName;
            [TextArea] public string description;
            public int cost;
            public Color colorValue = Color.white;
            public Sprite icon;
        }

        [Header("Tabs")]
        [SerializeField] private Button colorsTab;
        [SerializeField] private Button decalsTab;
        [SerializeField] private Button partsTab;

        [Header("Grid")]
        [SerializeField] private Transform itemGrid;
        [SerializeField] private GameObject cosmeticSlotPrefab;

        [Header("Preview")]
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemDescText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text ownedText;

        [Header("Data")]
        [SerializeField] private CustomizationSystem customizationSystem;
        [SerializeField] private MonoBehaviour currencyManagerSource;
        [SerializeField] private MonoBehaviour progressionDataSource;
        [SerializeField] private CustomizationItem[] colorItems;
        [SerializeField] private CustomizationItem[] decalItems;
        [SerializeField] private CustomizationItem[] partItems;

        private readonly List<GameObject> _spawnedSlots = new List<GameObject>();
        private readonly HashSet<string> _ownedItems = new HashSet<string>();

        private CustomizationTab _activeTab = CustomizationTab.Colors;
        private CustomizationItem _selectedItem;
        private string _equippedColorId = "";
        private string _equippedDecalId = "";
        private string _equippedPartId = "";
        private object _cosmeticManagerSource;

        private const string OwnedPrefsKey = "MetalPod.Cosmetics.Owned";
        private const string EquippedColorPrefsKey = "MetalPod.Cosmetics.EquippedColor";
        private const string EquippedDecalPrefsKey = "MetalPod.Cosmetics.EquippedDecal";
        private const string EquippedPartPrefsKey = "MetalPod.Cosmetics.EquippedPart";

        private void Awake()
        {
            if (colorsTab != null) colorsTab.onClick.AddListener(() => SelectTab(CustomizationTab.Colors));
            if (decalsTab != null) decalsTab.onClick.AddListener(() => SelectTab(CustomizationTab.Decals));
            if (partsTab != null) partsTab.onClick.AddListener(() => SelectTab(CustomizationTab.Parts));
            if (equipButton != null) equipButton.onClick.AddListener(EquipSelectedItem);
            if (purchaseButton != null) purchaseButton.onClick.AddListener(PurchaseSelectedItem);
        }

        private void Start()
        {
            if (currencyManagerSource == null)
            {
                currencyManagerSource = FindSourceWithMember("SpendCurrency");
            }

            if (progressionDataSource == null)
            {
                progressionDataSource = FindSourceWithMember("Cosmetics");
            }

            LoadOwnedState();
            EnsureDefaultOwnership();
            SyncEquippedFromProgression();
            SelectTab(_activeTab);
        }

        private void SelectTab(CustomizationTab tab)
        {
            _activeTab = tab;
            RebuildGrid();
            SelectFirstVisibleItem();
            RefreshPreview();
        }

        private void RebuildGrid()
        {
            for (int i = 0; i < _spawnedSlots.Count; i++)
            {
                if (_spawnedSlots[i] != null)
                {
                    Destroy(_spawnedSlots[i]);
                }
            }

            _spawnedSlots.Clear();

            CustomizationItem[] items = GetItemsForTab(_activeTab);
            for (int i = 0; i < items.Length; i++)
            {
                CustomizationItem item = items[i];
                if (item == null)
                {
                    continue;
                }

                GameObject slot = cosmeticSlotPrefab != null
                    ? Instantiate(cosmeticSlotPrefab, itemGrid)
                    : CreateFallbackSlot(item.id);

                BindSlot(slot, item);
                _spawnedSlots.Add(slot);
            }
        }

        private void SelectFirstVisibleItem()
        {
            CustomizationItem[] items = GetItemsForTab(_activeTab);
            _selectedItem = items.Length > 0 ? items[0] : null;
        }

        private void BindSlot(GameObject slot, CustomizationItem item)
        {
            Button button = slot.GetComponentInChildren<Button>() ?? slot.GetComponent<Button>();
            if (button == null)
            {
                button = slot.AddComponent<Button>();
            }

            button.onClick.AddListener(() =>
            {
                _selectedItem = item;
                RefreshPreview();
                RefreshSlotVisuals();
            });

            TMP_Text[] texts = slot.GetComponentsInChildren<TMP_Text>(true);
            TMP_Text nameLabel = FindText(texts, "name");
            TMP_Text ownedLabel = FindText(texts, "owned");

            if (nameLabel != null)
            {
                nameLabel.text = item.displayName;
            }

            if (ownedLabel != null)
            {
                ownedLabel.gameObject.SetActive(IsOwned(item));
                ownedLabel.text = IsEquipped(item) ? "Equipped" : "Owned";
            }

            Image iconImage = FindImage(slot, "icon");
            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                if (item.icon == null && _activeTab == CustomizationTab.Colors)
                {
                    iconImage.color = item.colorValue;
                }
            }
        }

        private void RefreshPreview()
        {
            if (_selectedItem == null)
            {
                if (itemNameText != null) itemNameText.text = "--";
                if (itemDescText != null) itemDescText.text = string.Empty;
                if (costText != null) costText.text = "--";
                if (ownedText != null) ownedText.gameObject.SetActive(false);
                if (equipButton != null) equipButton.interactable = false;
                if (purchaseButton != null) purchaseButton.interactable = false;
                return;
            }

            if (itemNameText != null) itemNameText.text = _selectedItem.displayName;
            if (itemDescText != null) itemDescText.text = _selectedItem.description;

            bool owned = IsOwned(_selectedItem);
            bool equipped = IsEquipped(_selectedItem);

            if (costText != null) costText.text = _selectedItem.cost.ToString();
            if (ownedText != null)
            {
                ownedText.gameObject.SetActive(owned);
                ownedText.text = equipped ? "Equipped" : "Owned";
            }

            if (purchaseButton != null) purchaseButton.interactable = !owned;
            if (equipButton != null) equipButton.interactable = owned && !equipped;

            ApplyPreview(_selectedItem);
            RefreshSlotVisuals();
        }

        private void RefreshSlotVisuals()
        {
            for (int i = 0; i < _spawnedSlots.Count; i++)
            {
                GameObject slot = _spawnedSlots[i];
                if (slot == null)
                {
                    continue;
                }

                Image background = slot.GetComponent<Image>();
                if (background == null)
                {
                    continue;
                }

                bool selected = _selectedItem != null && slot.name.Contains(_selectedItem.id);
                background.color = selected
                    ? new Color(1f, 0.53f, 0f, 0.3f)
                    : new Color(0.22f, 0.22f, 0.22f, 0.75f);
            }
        }

        private void PurchaseSelectedItem()
        {
            if (_selectedItem == null || IsOwned(_selectedItem))
            {
                return;
            }

            object cosmeticManager = ResolveCosmeticManagerSource();
            bool success = cosmeticManager != null
                ? TryPurchaseCosmetic(_selectedItem.id)
                : SpendCurrency(_selectedItem.cost);

            if (!success)
            {
                return;
            }

            _ownedItems.Add(_selectedItem.id);
            SaveOwnedState();
            RefreshPreview();
        }

        private void EquipSelectedItem()
        {
            if (_selectedItem == null || !IsOwned(_selectedItem))
            {
                return;
            }

            object cosmeticManager = ResolveCosmeticManagerSource();
            bool equippedByManager = cosmeticManager != null && TryEquipCosmetic(_selectedItem.id);

            if (cosmeticManager != null && !equippedByManager)
            {
                return;
            }

            if (!equippedByManager)
            {
                switch (_activeTab)
                {
                    case CustomizationTab.Colors:
                        _equippedColorId = _selectedItem.id;
                        break;
                    case CustomizationTab.Decals:
                        _equippedDecalId = _selectedItem.id;
                        break;
                    case CustomizationTab.Parts:
                        _equippedPartId = _selectedItem.id;
                        break;
                }

                SaveOwnedState();
                SharedContractsBridge.Raise("RaiseCosmeticEquipped", _selectedItem.id);
            }

            ApplyPreview(_selectedItem);
            SyncEquippedFromProgression();
            RefreshPreview();
        }

        private void ApplyPreview(CustomizationItem item)
        {
            if (item == null || customizationSystem == null)
            {
                return;
            }

            switch (_activeTab)
            {
                case CustomizationTab.Colors:
                    customizationSystem.ApplyColor(item.colorValue);
                    break;
                case CustomizationTab.Decals:
                    customizationSystem.ApplyDecal(item.id);
                    break;
                case CustomizationTab.Parts:
                    customizationSystem.ApplyPart(item.id);
                    break;
            }
        }

        private bool IsOwned(CustomizationItem item)
        {
            if (item == null)
            {
                return false;
            }

            object cosmeticManager = ResolveCosmeticManagerSource();
            if (cosmeticManager != null)
            {
                object response = ReflectionValueReader.Invoke(cosmeticManager, "OwnsCosmetic", item.id);
                if (response is bool ownsFromManager)
                {
                    return ownsFromManager;
                }
            }

            return _ownedItems.Contains(item.id);
        }

        private bool IsEquipped(CustomizationItem item)
        {
            if (item == null)
            {
                return false;
            }

            switch (_activeTab)
            {
                case CustomizationTab.Colors:
                    return GetEquippedColorId() == item.id;
                case CustomizationTab.Decals:
                    return GetEquippedDecalId() == item.id;
                case CustomizationTab.Parts:
                    return GetEquippedPartId() == item.id;
                default:
                    return false;
            }
        }

        private bool SpendCurrency(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (currencyManagerSource == null)
            {
                return false;
            }

            object result = ReflectionValueReader.Invoke(currencyManagerSource, "SpendCurrency", amount);
            return result is bool success && success;
        }

        private void LoadOwnedState()
        {
            _ownedItems.Clear();
            string ownedCsv = PlayerPrefs.GetString(OwnedPrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(ownedCsv))
            {
                string[] split = ownedCsv.Split(',');
                for (int i = 0; i < split.Length; i++)
                {
                    string id = split[i].Trim();
                    if (!string.IsNullOrEmpty(id))
                    {
                        _ownedItems.Add(id);
                    }
                }
            }

            _equippedColorId = PlayerPrefs.GetString(EquippedColorPrefsKey, string.Empty);
            _equippedDecalId = PlayerPrefs.GetString(EquippedDecalPrefsKey, string.Empty);
            _equippedPartId = PlayerPrefs.GetString(EquippedPartPrefsKey, string.Empty);
        }

        private void SaveOwnedState()
        {
            string ownedCsv = string.Join(",", _ownedItems);
            PlayerPrefs.SetString(OwnedPrefsKey, ownedCsv);
            PlayerPrefs.SetString(EquippedColorPrefsKey, _equippedColorId);
            PlayerPrefs.SetString(EquippedDecalPrefsKey, _equippedDecalId);
            PlayerPrefs.SetString(EquippedPartPrefsKey, _equippedPartId);
            PlayerPrefs.Save();
        }

        private void EnsureDefaultOwnership()
        {
            if (ResolveCosmeticManagerSource() != null)
            {
                return;
            }

            if (colorItems != null && colorItems.Length > 0 && colorItems[0] != null)
            {
                _ownedItems.Add(colorItems[0].id);
                if (string.IsNullOrEmpty(_equippedColorId))
                {
                    _equippedColorId = colorItems[0].id;
                }
            }

            if (decalItems != null && decalItems.Length > 0 && decalItems[0] != null)
            {
                _ownedItems.Add(decalItems[0].id);
                if (string.IsNullOrEmpty(_equippedDecalId))
                {
                    _equippedDecalId = decalItems[0].id;
                }
            }

            if (partItems != null && partItems.Length > 0 && partItems[0] != null)
            {
                _ownedItems.Add(partItems[0].id);
                if (string.IsNullOrEmpty(_equippedPartId))
                {
                    _equippedPartId = partItems[0].id;
                }
            }

            SaveOwnedState();
        }

        private object ResolveCosmeticManagerSource()
        {
            if (_cosmeticManagerSource != null)
            {
                return _cosmeticManagerSource;
            }

            if (progressionDataSource == null)
            {
                return null;
            }

            PropertyInfo cosmeticsProperty = progressionDataSource.GetType().GetProperty(
                "Cosmetics",
                BindingFlags.Public | BindingFlags.Instance);

            if (cosmeticsProperty == null)
            {
                if (ReflectionValueReader.HasMember(progressionDataSource, "OwnsCosmetic"))
                {
                    _cosmeticManagerSource = progressionDataSource;
                }

                return _cosmeticManagerSource;
            }

            _cosmeticManagerSource = cosmeticsProperty.GetValue(progressionDataSource);
            return _cosmeticManagerSource;
        }

        private bool TryPurchaseCosmetic(string cosmeticId)
        {
            object cosmeticManager = ResolveCosmeticManagerSource();
            if (cosmeticManager == null)
            {
                return false;
            }

            object response = ReflectionValueReader.Invoke(cosmeticManager, "TryPurchaseCosmetic", cosmeticId);
            if (response is bool success)
            {
                return success;
            }

            return false;
        }

        private bool TryEquipCosmetic(string cosmeticId)
        {
            object cosmeticManager = ResolveCosmeticManagerSource();
            if (cosmeticManager == null)
            {
                return false;
            }

            if (!ReflectionValueReader.HasMember(cosmeticManager, "EquipCosmetic"))
            {
                return false;
            }

            object ownsResponse = ReflectionValueReader.Invoke(cosmeticManager, "OwnsCosmetic", cosmeticId);
            if (ownsResponse is bool owns && !owns)
            {
                return false;
            }

            ReflectionValueReader.Invoke(cosmeticManager, "EquipCosmetic", cosmeticId);
            return true;
        }

        private void SyncEquippedFromProgression()
        {
            object cosmeticManager = ResolveCosmeticManagerSource();
            if (cosmeticManager == null)
            {
                return;
            }

            string equippedColor = ReflectionValueReader.GetString(cosmeticManager, "EquippedColorScheme", _equippedColorId);
            string equippedDecal = ReflectionValueReader.GetString(cosmeticManager, "EquippedDecal", _equippedDecalId);
            string equippedPart = ReflectionValueReader.GetString(cosmeticManager, "EquippedPart", _equippedPartId);

            if (!string.IsNullOrEmpty(equippedColor))
            {
                _equippedColorId = equippedColor;
            }

            if (!string.IsNullOrEmpty(equippedDecal))
            {
                _equippedDecalId = equippedDecal;
            }

            if (!string.IsNullOrEmpty(equippedPart))
            {
                _equippedPartId = equippedPart;
            }
        }

        private string GetEquippedColorId()
        {
            object cosmeticManager = ResolveCosmeticManagerSource();
            if (cosmeticManager != null)
            {
                string equipped = ReflectionValueReader.GetString(cosmeticManager, "EquippedColorScheme", _equippedColorId);
                if (!string.IsNullOrEmpty(equipped))
                {
                    return equipped;
                }
            }

            return _equippedColorId;
        }

        private string GetEquippedDecalId()
        {
            object cosmeticManager = ResolveCosmeticManagerSource();
            if (cosmeticManager != null)
            {
                string equipped = ReflectionValueReader.GetString(cosmeticManager, "EquippedDecal", _equippedDecalId);
                if (!string.IsNullOrEmpty(equipped))
                {
                    return equipped;
                }
            }

            return _equippedDecalId;
        }

        private string GetEquippedPartId()
        {
            object cosmeticManager = ResolveCosmeticManagerSource();
            if (cosmeticManager != null)
            {
                string equipped = ReflectionValueReader.GetString(cosmeticManager, "EquippedPart", _equippedPartId);
                if (!string.IsNullOrEmpty(equipped))
                {
                    return equipped;
                }
            }

            return _equippedPartId;
        }

        private CustomizationItem[] GetItemsForTab(CustomizationTab tab)
        {
            switch (tab)
            {
                case CustomizationTab.Colors:
                    return colorItems ?? new CustomizationItem[0];
                case CustomizationTab.Decals:
                    return decalItems ?? new CustomizationItem[0];
                case CustomizationTab.Parts:
                    return partItems ?? new CustomizationItem[0];
                default:
                    return new CustomizationItem[0];
            }
        }

        private GameObject CreateFallbackSlot(string itemId)
        {
            GameObject slot = new GameObject($"CosmeticSlot_{itemId}", typeof(RectTransform), typeof(Image), typeof(Button));
            slot.transform.SetParent(itemGrid, false);
            return slot;
        }

        private static TMP_Text FindText(TMP_Text[] texts, string key)
        {
            if (texts == null || texts.Length == 0)
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

        private static Image FindImage(GameObject slot, string key)
        {
            Image[] images = slot.GetComponentsInChildren<Image>(true);
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
    }
}
