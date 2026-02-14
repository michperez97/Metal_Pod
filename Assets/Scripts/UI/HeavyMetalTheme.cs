using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.UI
{
    public class HeavyMetalTheme : MonoBehaviour
    {
        [Header("Palette")]
        [SerializeField] private Color backgroundColor = new Color32(0x1A, 0x1A, 0x1A, 0xFF);
        [SerializeField] private Color panelColor = new Color32(0x2D, 0x2D, 0x2D, 0xFF);
        [SerializeField] private Color accentColor = new Color32(0xFF, 0x88, 0x00, 0xFF);
        [SerializeField] private Color textColor = new Color32(0xE0, 0xE0, 0xE0, 0xFF);
        [SerializeField] private Color highlightColor = new Color32(0xFF, 0xB0, 0x30, 0xFF);
        [SerializeField] private Color dangerColor = new Color32(0xFF, 0x22, 0x22, 0xFF);
        [SerializeField] private Color successColor = new Color32(0x44, 0xCC, 0x44, 0xFF);
        [SerializeField] private Color currencyColor = new Color32(0xFF, 0xD7, 0x00, 0xFF);

        [Header("Targets")]
        [SerializeField] private Graphic[] panelGraphics;
        [SerializeField] private Graphic[] accentGraphics;
        [SerializeField] private Graphic[] highlightGraphics;
        [SerializeField] private Graphic[] dangerGraphics;
        [SerializeField] private Graphic[] successGraphics;
        [SerializeField] private Graphic[] currencyGraphics;
        [SerializeField] private TMP_Text[] titleTexts;
        [SerializeField] private TMP_Text[] bodyTexts;
        [SerializeField] private Image[] backgrounds;

        private void Awake()
        {
            ApplyTheme();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyTheme();
            }
        }

        [ContextMenu("Apply Theme")]
        public void ApplyTheme()
        {
            ApplyGraphicColor(panelGraphics, panelColor);
            ApplyGraphicColor(accentGraphics, accentColor);
            ApplyGraphicColor(highlightGraphics, highlightColor);
            ApplyGraphicColor(dangerGraphics, dangerColor);
            ApplyGraphicColor(successGraphics, successColor);
            ApplyGraphicColor(currencyGraphics, currencyColor);
            ApplyTextColor(titleTexts, highlightColor);
            ApplyTextColor(bodyTexts, textColor);
            ApplyGraphicColor(backgrounds, backgroundColor);
        }

        private static void ApplyGraphicColor(Graphic[] graphics, Color color)
        {
            if (graphics == null)
            {
                return;
            }

            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null)
                {
                    graphics[i].color = color;
                }
            }
        }

        private static void ApplyTextColor(TMP_Text[] texts, Color color)
        {
            if (texts == null)
            {
                return;
            }

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    texts[i].color = color;
                }
            }
        }
    }
}
