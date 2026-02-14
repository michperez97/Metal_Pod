using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MusicProfile", menuName = "MetalPod/Music Profile")]
    public class MusicProfileSO : ScriptableObject
    {
        [Header("Identity")]
        public string profileId;
        public string displayName;

        [Header("Layers (all clips must be same length and BPM for sync)")]
        [Tooltip("Always playing - bass, rhythm, pads")]
        public AudioClip baseLayer;

        [Tooltip("Fades in at medium speed")]
        public AudioClip drumsLayer;

        [Tooltip("Fades in at high speed - lead guitar, synth")]
        public AudioClip leadLayer;

        [Tooltip("Fades in during boost - power riff")]
        public AudioClip boostLayer;

        [Header("Ambient")]
        [Tooltip("Environmental ambience loop (lava rumble, wind, industrial hum)")]
        public AudioClip ambientLoop;

        [Header("Stingers")]
        [Tooltip("Plays on course completion (short, triumphant)")]
        public AudioClip victoryStinger;

        [Tooltip("Plays on course failure (short, dramatic)")]
        public AudioClip defeatStinger;

        [Tooltip("Plays once when health drops below danger threshold")]
        public AudioClip dangerStinger;

        [Tooltip("Plays during countdown (optional - short hit per beat)")]
        public AudioClip countdownTick;

        [Tooltip("Plays on GO! (optional - impact hit)")]
        public AudioClip goStinger;

        [Header("Timing")]
        [Tooltip("BPM of all layer clips (for beat-synced transitions)")]
        public float bpm = 120f;

        [Tooltip("Speed layers begin at these normalized thresholds (0=stopped, 1=max speed)")]
        [Range(0f, 1f)] public float drumSpeedThreshold = 0.3f;
        [Range(0f, 1f)] public float leadSpeedThreshold = 0.6f;

        [Tooltip("How quickly layers fade in/out (seconds)")]
        [Range(0.1f, 3f)] public float layerFadeTime = 0.8f;

        [Tooltip("Volume ducking when stinger plays (0=full duck, 1=no duck)")]
        [Range(0f, 1f)] public float stingerDuckLevel = 0.3f;
        [Range(0.1f, 2f)] public float stingerDuckFadeTime = 0.3f;

        [Header("Health")]
        [Tooltip("Health threshold (normalized) to trigger danger stinger")]
        [Range(0f, 0.5f)] public float dangerHealthThreshold = 0.25f;

        /// <summary>Duration of one beat in seconds.</summary>
        public float BeatDuration => 60f / Mathf.Max(bpm, 1f);

        /// <summary>Duration of one bar (4 beats) in seconds.</summary>
        public float BarDuration => BeatDuration * 4f;

        private void OnValidate()
        {
            bpm = Mathf.Max(1f, bpm);
            leadSpeedThreshold = Mathf.Max(leadSpeedThreshold, drumSpeedThreshold);
            layerFadeTime = Mathf.Clamp(layerFadeTime, 0.1f, 3f);
            stingerDuckFadeTime = Mathf.Clamp(stingerDuckFadeTime, 0.1f, 2f);
        }
    }
}
