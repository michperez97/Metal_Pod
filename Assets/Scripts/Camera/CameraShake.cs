using UnityEngine;

namespace MetalPod.GameCamera
{
    /// <summary>
    /// Reusable Perlin noise-based camera shake.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float defaultFrequency = 25f;

        private float currentIntensity;
        private float decayRate;
        private float seed;
        private float frequency;
        private Vector3 shakeOffset;

        public bool IsShaking => currentIntensity > 0.001f;
        public Vector3 ShakeOffset => shakeOffset;

        private void Awake()
        {
            frequency = Mathf.Max(0.01f, defaultFrequency);
            seed = Random.Range(0f, 1000f);
        }

        public void Shake(float intensity, float duration)
        {
            float clampedIntensity = Mathf.Max(0f, intensity);
            float clampedDuration = Mathf.Max(0.01f, duration);

            // Stack shake requests by preserving higher active intensity.
            currentIntensity = Mathf.Max(currentIntensity, clampedIntensity);
            decayRate = currentIntensity / clampedDuration;
            seed = Random.Range(0f, 1000f);
        }

        public void SetFrequency(float value)
        {
            frequency = Mathf.Max(0.01f, value);
        }

        public void StopShake()
        {
            currentIntensity = 0f;
            shakeOffset = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (currentIntensity <= 0.001f)
            {
                currentIntensity = 0f;
                shakeOffset = Vector3.zero;
                return;
            }

            float t = Time.unscaledTime * frequency;
            shakeOffset = new Vector3(
                (Mathf.PerlinNoise(seed, t) - 0.5f) * 2f * currentIntensity,
                (Mathf.PerlinNoise(seed + 100f, t) - 0.5f) * 2f * currentIntensity,
                0f);

            currentIntensity -= decayRate * Time.unscaledDeltaTime;
            if (currentIntensity < 0f)
            {
                currentIntensity = 0f;
            }
        }
    }
}
