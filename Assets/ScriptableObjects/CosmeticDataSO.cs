using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CosmeticData", menuName = "MetalPod/CosmeticData")]
    public class CosmeticDataSO : ScriptableObject
    {
        public string cosmeticId;
        public string cosmeticName;
        public CosmeticType cosmeticType;
        public Sprite icon;
        public int cost;
        public int requiredMedals;

        [TextArea]
        public string description;

        [Header("Color Data")]
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;

        [Header("Decal Data")]
        public Texture2D decalTexture;

        [Header("Part Data")]
        public GameObject partPrefab;
        public string attachPoint;
    }

    public enum CosmeticType
    {
        ColorScheme,
        Decal,
        Part
    }
}
