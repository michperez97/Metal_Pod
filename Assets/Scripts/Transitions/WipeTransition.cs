using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.Transitions
{
    public enum WipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    // Directional wipe transition with soft edge.
    // Uses TransitionWipe.shader.
    public class WipeTransition : TransitionBase
    {
        private static readonly int ProgressProp = Shader.PropertyToID("_Progress");
        private static readonly int DirectionProp = Shader.PropertyToID("_Direction");
        private static readonly int SoftnessProp = Shader.PropertyToID("_Softness");

        private WipeDirection _direction;
        private float _softness;

        public WipeTransition(WipeDirection direction, float softness = 0.1f)
        {
            _direction = direction;
            _softness = softness;
        }

        private Vector2 GetDirectionVector()
        {
            switch (_direction)
            {
                case WipeDirection.Left: return new Vector2(-1f, 0f);
                case WipeDirection.Right: return new Vector2(1f, 0f);
                case WipeDirection.Up: return new Vector2(0f, 1f);
                case WipeDirection.Down: return new Vector2(0f, -1f);
                default: return new Vector2(1f, 0f);
            }
        }

        public override IEnumerator PlayIn(float duration, Action onComplete)
        {
            IsPlaying = true;
            TransitionMaterial.SetVector(DirectionProp, GetDirectionVector());
            TransitionMaterial.SetFloat(SoftnessProp, _softness);
            TargetImage.enabled = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease in-out for smooth feel
                t = t * t * (3f - 2f * t);
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
            TransitionMaterial.SetVector(DirectionProp, GetDirectionVector());
            TransitionMaterial.SetFloat(SoftnessProp, _softness);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
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
