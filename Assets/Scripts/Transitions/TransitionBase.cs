using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Transitions
{
    // Abstract base class for all scene transitions.
    // Subclasses implement the visual effect by controlling a Material.
    public abstract class TransitionBase
    {
        protected Material TransitionMaterial;
        protected RawImage TargetImage;

        public bool IsPlaying { get; protected set; }

        public void SetTarget(RawImage image, Material mat)
        {
            TargetImage = image;
            TransitionMaterial = mat;

            if (TargetImage != null)
            {
                TargetImage.material = TransitionMaterial;
            }
        }

        /// <summary>
        /// Play transition IN (screen becomes covered).
        /// Progress goes from 0 (fully visible) to 1 (fully covered).
        /// </summary>
        public abstract IEnumerator PlayIn(float duration, Action onComplete);

        /// <summary>
        /// Play transition OUT (screen becomes revealed).
        /// Progress goes from 1 (fully covered) to 0 (fully visible).
        /// </summary>
        public abstract IEnumerator PlayOut(float duration, Action onComplete);

        /// <summary>
        /// Set the transition progress directly (for scrubbing in editor preview).
        /// </summary>
        public abstract void SetProgress(float t);
    }
}
