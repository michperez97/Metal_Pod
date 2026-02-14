using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.Transitions
{
    // Simple fade to/from a solid color (default: black).
    // Uses TransitionFade.shader.
    public class FadeTransition : TransitionBase
    {
        private static readonly int ProgressProp = Shader.PropertyToID("_Progress");
        private static readonly int ColorProp = Shader.PropertyToID("_FadeColor");

        private Color _fadeColor;

        public FadeTransition(Color fadeColor)
        {
            _fadeColor = fadeColor;
        }

        public override IEnumerator PlayIn(float duration, Action onComplete)
        {
            IsPlaying = true;
            TransitionMaterial.SetColor(ColorProp, _fadeColor);
            TargetImage.enabled = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetProgress(t);
                yield return null;
            }
            SetProgress(1f);
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override IEnumerator PlayOut(float duration, Action onComplete)
        {
            IsPlaying = true;
            TransitionMaterial.SetColor(ColorProp, _fadeColor);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / duration);
                SetProgress(t);
                yield return null;
            }
            SetProgress(0f);
            TargetImage.enabled = false;
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override void SetProgress(float t)
        {
            TransitionMaterial.SetFloat(ProgressProp, t);
        }
    }
}
