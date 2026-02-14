using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MetalPod/GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Starting Values")]
        public int startingCurrency = 0;
        public string firstCourseId = "lava_01";

        [Header("Respawn")]
        public float respawnDelay = 2f;
        public float respawnInvincibilityDuration = 3f;

        [Header("Countdown")]
        public int countdownSeconds = 3;

        [Header("Performance")]
        public int targetFrameRate = 60;
        public int maxParticleCount = 500;

        [Header("Controls")]
        public float defaultTiltSensitivity = 1f;
        public float minTiltSensitivity = 0.5f;
        public float maxTiltSensitivity = 2f;
        public float tiltDeadzone = 0.05f;
    }
}
