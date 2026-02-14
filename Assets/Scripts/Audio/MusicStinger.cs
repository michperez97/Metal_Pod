using UnityEngine;

namespace MetalPod.Audio
{
    /// <summary>
    /// Plays one-shot stingers and manages music ducking during stinger playback.
    /// </summary>
    public class MusicStinger
    {
        private readonly AudioSource _source;
        private float _duckLevel = 0.3f;
        private float _duckFadeTime = 0.3f;
        private bool _isPlaying;
        private float _currentDuckMultiplier = 1f;

        public bool IsPlaying => _isPlaying;

        public MusicStinger(AudioSource source)
        {
            _source = source;
            if (_source != null)
            {
                _source.loop = false;
                _source.playOnAwake = false;
            }
        }

        public void Configure(float duckLevel, float duckFadeTime)
        {
            _duckLevel = Mathf.Clamp01(duckLevel);
            _duckFadeTime = Mathf.Clamp(duckFadeTime, 0.05f, 2f);
        }

        /// <summary>
        /// Play a stinger clip.
        /// DynamicMusicManager should apply this class's duck multiplier to layer volumes.
        /// </summary>
        public void Play(AudioClip clip, float volume = 1f)
        {
            if (_source == null || clip == null)
            {
                return;
            }

            _source.PlayOneShot(clip, Mathf.Clamp01(volume));
            _isPlaying = true;
        }

        /// <summary>
        /// Returns the current ducking multiplier (1.0 = no duck, duckLevel = full duck).
        /// Call this every frame.
        /// </summary>
        public float GetDuckMultiplier()
        {
            if (_source == null)
            {
                return 1f;
            }

            if (_isPlaying && !_source.isPlaying)
            {
                _isPlaying = false;
            }

            float target = _isPlaying ? _duckLevel : 1f;
            float step = Time.unscaledDeltaTime / Mathf.Max(_duckFadeTime, 0.01f);
            _currentDuckMultiplier = Mathf.MoveTowards(_currentDuckMultiplier, target, step);
            return _currentDuckMultiplier;
        }
    }
}
