using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.UI
{
    public class LoadingScreenUI : MonoBehaviour
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private TMP_Text tipText;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private string[] tips =
        {
            "Use boost to break through ice walls!",
            "Shield regenerates after a few seconds of safety.",
            "Collect currency to upgrade your pod in the workshop.",
            "Gold medals require mastering every shortcut.",
            "Watch for ground shadows. Something is falling!",
            "The avalanche waits for no one. Keep moving!",
            "Upgrade handling to tame ice patches.",
            "Electric fences stun your controls briefly."
        };

        private Coroutine _fadeRoutine;

        public void Show()
        {
            gameObject.SetActive(true);
            if (tipText != null && tips != null && tips.Length > 0)
            {
                tipText.text = tips[Random.Range(0, tips.Length)];
            }

            if (progressFill != null)
            {
                progressFill.fillAmount = 0f;
            }

            UpdateProgress(0f);
            StartFade(1f);
        }

        public void UpdateProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progressFill != null)
            {
                progressFill.fillAmount = progress;
            }

            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100f)}%";
            }
        }

        public IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(false);
                yield break;
            }

            float elapsed = 0f;
            const float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = 1f - t;
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void StartFade(float targetAlpha)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            float startAlpha = canvasGroup.alpha;
            const float duration = 0.35f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            _fadeRoutine = null;
        }
    }
}
