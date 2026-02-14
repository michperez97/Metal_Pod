using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.Transitions
{
    public enum EnvironmentTheme
    {
        Lava,
        Ice,
        Toxic,
        Neutral
    }

    // Environment-themed dissolve transition.
    // Lava: fire/ember dissolve (orange/red)
    // Ice: frost freeze pattern (white/blue)
    // Toxic: acid drip dissolve (green/black)
    // Uses TransitionDissolve.shader with a noise texture.
    public class EnvironmentTransition : TransitionBase
    {
        private static readonly int ProgressProp = Shader.PropertyToID("_Progress");
        private static readonly int EdgeColorProp = Shader.PropertyToID("_EdgeColor");
        private static readonly int EdgeWidthProp = Shader.PropertyToID("_EdgeWidth");
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

        private EnvironmentTheme _theme;

        private static readonly Color LavaEdge = new Color(1f, 0.3f, 0f, 1f);   // Orange
        private static readonly Color LavaBase = new Color(0.15f, 0f, 0f, 1f);   // Dark red
        private static readonly Color IceEdge = new Color(0.6f, 0.85f, 1f, 1f);  // Light blue
        private static readonly Color IceBase = new Color(0.9f, 0.95f, 1f, 1f);  // Near white
        private static readonly Color ToxicEdge = new Color(0.2f, 1f, 0f, 1f);   // Bright green
        private static readonly Color ToxicBase = new Color(0.05f, 0.1f, 0f, 1f);// Dark green

        public EnvironmentTransition(EnvironmentTheme theme)
        {
            _theme = theme;
        }

        private void ApplyThemeColors()
        {
            Color edge, baseColor;
            switch (_theme)
            {
                case EnvironmentTheme.Lava:
                    edge = LavaEdge; baseColor = LavaBase; break;
                case EnvironmentTheme.Ice:
                    edge = IceEdge; baseColor = IceBase; break;
                case EnvironmentTheme.Toxic:
                    edge = ToxicEdge; baseColor = ToxicBase; break;
                default:
                    edge = Color.white; baseColor = Color.black; break;
            }

            TransitionMaterial.SetColor(EdgeColorProp, edge);
            TransitionMaterial.SetColor(BaseColorProp, baseColor);
            TransitionMaterial.SetFloat(EdgeWidthProp, 0.08f);
        }

        public override IEnumerator PlayIn(float duration, Action onComplete)
        {
            IsPlaying = true;
            ApplyThemeColors();
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
            ApplyThemeColors();

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
