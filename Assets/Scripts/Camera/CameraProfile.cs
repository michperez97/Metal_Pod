using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.GameCamera
{
    /// <summary>
    /// Utility methods for evaluating camera profile runtime values.
    /// </summary>
    public static class CameraProfile
    {
        public static float GetSpeedNormalized(CameraProfileSO profile, float speed)
        {
            if (profile == null)
            {
                return 0f;
            }

            float reference = Mathf.Max(0.01f, profile.fovMaxSpeedReference);
            return Mathf.Clamp01(speed / reference);
        }

        public static float GetTargetFov(CameraProfileSO profile, float speed, bool isBoosting)
        {
            if (profile == null)
            {
                return 60f;
            }

            float speed01 = GetSpeedNormalized(profile, speed);
            float target = Mathf.Lerp(profile.baseFOV, profile.maxSpeedFOV, speed01);
            if (isBoosting)
            {
                target += profile.boostFOVBonus;
            }

            return target;
        }

        public static Vector3 GetFollowOffset(CameraProfileSO profile, bool isBoosting)
        {
            if (profile == null)
            {
                return new Vector3(0f, 4f, -8f);
            }

            Vector3 offset = profile.followOffset;
            if (isBoosting)
            {
                offset.z -= profile.boostExtraDistance;
            }

            return offset;
        }
    }
}
