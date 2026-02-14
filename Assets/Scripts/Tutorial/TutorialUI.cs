using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Tutorial
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("Prompt Box")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI tapToContinueText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Positioning")]
        [SerializeField] private RectTransform promptRect;

        [Header("Highlight")]
        [SerializeField] private Image dimOverlay;
        [SerializeField] private Image highlightRing;
        [SerializeField] private Image arrowPointer;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;

        private Coroutine _pulseRoutine;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (promptRect == null && promptPanel != null)
            {
                promptRect = promptPanel.GetComponent<RectTransform>();
            }
        }

        public void ShowStep(TutorialStep step)
        {
            if (step == null)
            {
                return;
            }

            StopAllCoroutines();

            if (promptPanel != null)
            {
                promptPanel.SetActive(true);
            }

            if (promptText != null)
            {
                promptText.text = step.promptText;
            }

            if (subtitleText != null)
            {
                subtitleText.text = string.IsNullOrWhiteSpace(step.subtitleText) ? string.Empty : step.subtitleText;
            }

            if (tapToContinueText != null)
            {
                tapToContinueText.gameObject.SetActive(false);
            }

            if (iconImage != null)
            {
                bool hasIcon = step.iconSprite != null;
                iconImage.gameObject.SetActive(hasIcon);
                if (hasIcon)
                {
                    iconImage.sprite = step.iconSprite;
                }
            }

            PositionPrompt(step.promptPosition);

            if (dimOverlay != null)
            {
                dimOverlay.gameObject.SetActive(step.dimBackground);
            }

            if (arrowPointer != null)
            {
                arrowPointer.gameObject.SetActive(step.showArrowPointing);
                if (step.showArrowPointing)
                {
                    PositionArrow(step.arrowTarget);
                }
            }

            if (!string.IsNullOrWhiteSpace(step.highlightUIElement))
            {
                HighlightElement(step.highlightUIElement);
            }
            else if (highlightRing != null)
            {
                highlightRing.gameObject.SetActive(false);
            }

            StartCoroutine(FadeIn());
        }

        public void ShowTapToContinue()
        {
            if (tapToContinueText != null)
            {
                tapToContinueText.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutAndDisable());
        }

        private void PositionPrompt(TutorialPromptPosition position)
        {
            if (promptRect == null)
            {
                return;
            }

            switch (position)
            {
                case TutorialPromptPosition.TopCenter:
                    promptRect.anchorMin = new Vector2(0.1f, 0.75f);
                    promptRect.anchorMax = new Vector2(0.9f, 0.95f);
                    break;
                case TutorialPromptPosition.BottomCenter:
                    promptRect.anchorMin = new Vector2(0.1f, 0.05f);
                    promptRect.anchorMax = new Vector2(0.9f, 0.25f);
                    break;
                case TutorialPromptPosition.Center:
                    promptRect.anchorMin = new Vector2(0.15f, 0.35f);
                    promptRect.anchorMax = new Vector2(0.85f, 0.65f);
                    break;
                case TutorialPromptPosition.NearControl:
                    promptRect.anchorMin = new Vector2(0.1f, 0.25f);
                    promptRect.anchorMax = new Vector2(0.9f, 0.45f);
                    break;
            }

            promptRect.anchoredPosition = Vector2.zero;
        }

        private void PositionArrow(Vector2 normalizedScreenPos)
        {
            if (arrowPointer == null)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
            RectTransform arrowRect = arrowPointer.rectTransform;

            if (canvasRect == null || arrowRect == null)
            {
                return;
            }

            normalizedScreenPos = new Vector2(
                Mathf.Clamp01(normalizedScreenPos.x),
                Mathf.Clamp01(normalizedScreenPos.y));

            arrowRect.anchorMin = normalizedScreenPos;
            arrowRect.anchorMax = normalizedScreenPos;
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = Vector2.zero;
        }

        private void HighlightElement(string elementName)
        {
            if (highlightRing == null)
            {
                return;
            }

            RectTransform element = FindUiElement(elementName);
            if (element == null)
            {
                highlightRing.gameObject.SetActive(false);
                return;
            }

            highlightRing.gameObject.SetActive(true);
            highlightRing.rectTransform.position = element.position;
            highlightRing.rectTransform.sizeDelta = element.rect.size + new Vector2(24f, 24f);

            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
            }

            _pulseRoutine = StartCoroutine(PulseHighlight());
        }

        private RectTransform FindUiElement(string elementName)
        {
            TutorialHighlight[] highlights = FindObjectsByType<TutorialHighlight>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < highlights.Length; i++)
            {
                TutorialHighlight highlight = highlights[i];
                if (highlight != null && highlight.HighlightId == elementName)
                {
                    return highlight.GetComponent<RectTransform>();
                }
            }

            RectTransform[] allRects = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allRects.Length; i++)
            {
                if (allRects[i] != null && allRects[i].name == elementName)
                {
                    return allRects[i];
                }
            }

            return null;
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            float duration = Mathf.Max(0.01f, fadeInDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutAndDisable()
        {
            if (canvasGroup == null)
            {
                if (promptPanel != null)
                {
                    promptPanel.SetActive(false);
                }

                yield break;
            }

            float duration = Mathf.Max(0.01f, fadeOutDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;

            if (promptPanel != null)
            {
                promptPanel.SetActive(false);
            }

            if (highlightRing != null)
            {
                highlightRing.gameObject.SetActive(false);
            }

            if (arrowPointer != null)
            {
                arrowPointer.gameObject.SetActive(false);
            }

            if (tapToContinueText != null)
            {
                tapToContinueText.gameObject.SetActive(false);
            }
        }

        private IEnumerator PulseHighlight()
        {
            while (highlightRing != null && highlightRing.gameObject.activeSelf)
            {
                float scale = 1f + (0.15f * Mathf.Sin(Time.unscaledTime * 3f));
                highlightRing.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
    }
}
