using System;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;

namespace MetalPod.Progression
{
    public class CosmeticManager
    {
        private SaveSystem _saveSystem;
        private CurrencyManager _currencyManager;
        private CosmeticDataSO[] _allCosmetics;

        public string EquippedColorScheme => _saveSystem?.CurrentData?.equippedColorScheme ?? "default";
        public string EquippedDecal => _saveSystem?.CurrentData?.equippedDecal ?? string.Empty;
        public string EquippedPart => _saveSystem?.CurrentData?.equippedPart ?? string.Empty;

        public void Initialize(SaveSystem saveSystem, CurrencyManager currencyManager, CosmeticDataSO[] cosmetics)
        {
            _saveSystem = saveSystem;
            _currencyManager = currencyManager;
            _allCosmetics = cosmetics ?? Array.Empty<CosmeticDataSO>();
        }

        public bool OwnsCosmetic(string cosmeticId)
        {
            if (_saveSystem == null || _saveSystem.CurrentData == null || string.IsNullOrWhiteSpace(cosmeticId))
            {
                return false;
            }

            return _saveSystem.CurrentData.ownedCosmetics.Contains(cosmeticId);
        }

        public bool CanPurchaseCosmetic(string cosmeticId)
        {
            if (OwnsCosmetic(cosmeticId))
            {
                return false;
            }

            CosmeticDataSO cosmetic = GetCosmeticData(cosmeticId);
            if (cosmetic == null)
            {
                return false;
            }

            if (cosmetic.requiredMedals > 0 && _saveSystem.CurrentData.totalMedals < cosmetic.requiredMedals)
            {
                return false;
            }

            return _currencyManager != null && _currencyManager.CanAfford(cosmetic.cost);
        }

        public bool TryPurchaseCosmetic(string cosmeticId)
        {
            if (_saveSystem == null || _saveSystem.CurrentData == null)
            {
                return false;
            }

            if (!CanPurchaseCosmetic(cosmeticId))
            {
                return false;
            }

            CosmeticDataSO cosmetic = GetCosmeticData(cosmeticId);
            if (cosmetic == null || !_currencyManager.SpendCurrency(cosmetic.cost))
            {
                return false;
            }

            if (!_saveSystem.CurrentData.ownedCosmetics.Contains(cosmeticId))
            {
                _saveSystem.CurrentData.ownedCosmetics.Add(cosmeticId);
            }

            _saveSystem.MarkDirty();
            _saveSystem.Save();
            return true;
        }

        public void EquipCosmetic(string cosmeticId)
        {
            if (_saveSystem == null || _saveSystem.CurrentData == null || !OwnsCosmetic(cosmeticId))
            {
                return;
            }

            CosmeticDataSO cosmetic = GetCosmeticData(cosmeticId);
            if (cosmetic == null)
            {
                return;
            }

            switch (cosmetic.cosmeticType)
            {
                case CosmeticType.ColorScheme:
                    _saveSystem.CurrentData.equippedColorScheme = cosmeticId;
                    break;
                case CosmeticType.Decal:
                    _saveSystem.CurrentData.equippedDecal = cosmeticId;
                    break;
                case CosmeticType.Part:
                    _saveSystem.CurrentData.equippedPart = cosmeticId;
                    break;
            }

            _saveSystem.MarkDirty();
            _saveSystem.Save();
            EventBus.RaiseCosmeticEquipped(cosmeticId);
        }

        public CosmeticDataSO GetCosmeticData(string id)
        {
            if (_allCosmetics == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            for (int i = 0; i < _allCosmetics.Length; i++)
            {
                CosmeticDataSO candidate = _allCosmetics[i];
                if (candidate != null && candidate.cosmeticId == id)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
