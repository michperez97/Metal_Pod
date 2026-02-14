using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CameraProfile", menuName = "MetalPod/Camera Profile")]
    public class CameraProfileSO : ScriptableObject
    {
        [Header("Follow Mode")]
        [Tooltip("Offset behind and above the hovercraft (local space).")]
        public Vector3 followOffset = new Vector3(0f, 4f, -8f);

        [Tooltip("How quickly camera catches up to target position (lower = smoother).")]
        [Range(0.01f, 1f)] public float positionSmoothTime = 0.15f;

        [Tooltip("How quickly camera rotation catches up (lower = smoother).")]
        [Range(0.01f, 1f)] public float rotationSmoothTime = 0.1f;

        [Tooltip("How far ahead of the hovercraft the camera looks, based on velocity.")]
        [Range(0f, 20f)] public float lookAheadDistance = 6f;

        [Tooltip("Vertical offset for the look-at target above hovercraft center.")]
        public float lookAtHeightOffset = 1.5f;

        [Header("Speed Effects")]
        [Tooltip("Camera FOV at zero speed.")]
        [Range(40f, 90f)] public float baseFOV = 60f;

        [Tooltip("Camera FOV at maximum speed.")]
        [Range(40f, 120f)] public float maxSpeedFOV = 78f;

        [Tooltip("Speed (m/s) at which FOV reaches maxSpeedFOV.")]
        public float fovMaxSpeedReference = 40f;

        [Tooltip("How quickly FOV transitions.")]
        [Range(0.01f, 1f)] public float fovSmoothTime = 0.25f;

        [Header("Boost Effects")]
        [Tooltip("Extra FOV added during boost.")]
        public float boostFOVBonus = 8f;

        [Tooltip("Extra follow distance pulled back during boost.")]
        public float boostExtraDistance = 2f;

        [Header("Cinematic Intro")]
        [Tooltip("Duration of intro flyover before countdown.")]
        public float introDuration = 3f;

        [Tooltip("Orbit radius around start position during intro.")]
        public float introOrbitRadius = 15f;

        [Tooltip("Orbit height during intro.")]
        public float introOrbitHeight = 8f;

        [Tooltip("Orbit speed in degrees per second.")]
        public float introOrbitSpeed = 60f;

        [Header("Finish Camera")]
        [Tooltip("Time scale during finish cam.")]
        [Range(0.1f, 1f)] public float finishTimeScale = 0.3f;

        [Tooltip("Orbit radius during finish celebration.")]
        public float finishOrbitRadius = 10f;

        [Tooltip("Duration of finish cam before results screen.")]
        public float finishDuration = 2.5f;

        [Tooltip("Orbit speed during finish.")]
        public float finishOrbitSpeed = 45f;

        [Header("Respawn")]
        [Tooltip("How fast camera snaps to new position on respawn.")]
        [Range(0.01f, 0.5f)] public float respawnSnapTime = 0.3f;

        [Header("Shake")]
        [Tooltip("Default shake intensity for light hits.")]
        public float lightShakeIntensity = 0.15f;

        [Tooltip("Default shake intensity for heavy hits.")]
        public float heavyShakeIntensity = 0.5f;

        [Tooltip("Shake intensity for explosions.")]
        public float explosionShakeIntensity = 0.8f;

        [Tooltip("Default shake duration.")]
        public float shakeDecayTime = 0.3f;

        [Tooltip("Perlin noise frequency for shake.")]
        public float shakeFrequency = 25f;

        private void OnValidate()
        {
            positionSmoothTime = Mathf.Clamp(positionSmoothTime, 0.01f, 1f);
            rotationSmoothTime = Mathf.Clamp(rotationSmoothTime, 0.01f, 1f);
            lookAheadDistance = Mathf.Clamp(lookAheadDistance, 0f, 20f);

            baseFOV = Mathf.Clamp(baseFOV, 40f, 90f);
            maxSpeedFOV = Mathf.Clamp(maxSpeedFOV, 40f, 120f);
            maxSpeedFOV = Mathf.Max(maxSpeedFOV, baseFOV);
            fovMaxSpeedReference = Mathf.Max(0.01f, fovMaxSpeedReference);
            fovSmoothTime = Mathf.Clamp(fovSmoothTime, 0.01f, 1f);

            introDuration = Mathf.Max(0f, introDuration);
            introOrbitRadius = Mathf.Max(0f, introOrbitRadius);
            introOrbitHeight = Mathf.Max(0f, introOrbitHeight);

            finishTimeScale = Mathf.Clamp(finishTimeScale, 0.1f, 1f);
            finishOrbitRadius = Mathf.Max(0f, finishOrbitRadius);
            finishDuration = Mathf.Max(0f, finishDuration);

            respawnSnapTime = Mathf.Clamp(respawnSnapTime, 0.01f, 0.5f);

            lightShakeIntensity = Mathf.Max(0f, lightShakeIntensity);
            heavyShakeIntensity = Mathf.Max(lightShakeIntensity, heavyShakeIntensity);
            explosionShakeIntensity = Mathf.Max(0f, explosionShakeIntensity);
            shakeDecayTime = Mathf.Max(0.01f, shakeDecayTime);
            shakeFrequency = Mathf.Max(0.01f, shakeFrequency);
        }
    }
}
