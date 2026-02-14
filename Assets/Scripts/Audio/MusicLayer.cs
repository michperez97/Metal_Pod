using UnityEngine;

namespace MetalPod.Audio
{
    /// <summary>
    /// Represents a single music layer with fade control.
    /// Managed by DynamicMusicManager.
    /// </summary>
    public class MusicLayer
    {
        public string Name { get; }
        public AudioSource Source { get; }
        public float TargetVolume { get; private set; }
        public float CurrentVolume => Source != null ? Source.volume : 0f;
        public bool IsActive => TargetVolume > 0.01f;

        private float _fadeSpeed;
        private float _maxVolume;

        public MusicLayer(string name, AudioSource source, float maxVolume = 1f)
        {
            Name = name;
            Source = source;
            _maxVolume = Mathf.Clamp01(maxVolume);
            _fadeSpeed = _maxVolume;
            TargetVolume = 0f;

            if (Source != null)
            {
                Source.volume = 0f;
                Source.loop = true;
                Source.playOnAwake = false;
            }
        }

        public void SetClip(AudioClip clip)
        {
            if (Source == null)
            {
                return;
            }

            bool wasPlaying = Source.isPlaying;
            Source.clip = clip;

            if (!wasPlaying)
            {
                return;
            }

            if (clip != null)
            {
                Source.Play();
            }
            else
            {
                Source.Stop();
            }
        }

        public void Play(AudioClip clip, double scheduledTime = 0d)
        {
            if (Source == null)
            {
                return;
            }

            Source.Stop();
            Source.clip = clip;
            if (clip == null)
            {
                TargetVolume = 0f;
                Source.volume = 0f;
                return;
            }

            if (scheduledTime > AudioSettings.dspTime)
            {
                Source.PlayScheduled(scheduledTime);
            }
            else
            {
                Source.Play();
            }
        }

        public void Stop()
        {
            if (Source == null)
            {
                return;
            }

            Source.Stop();
            Source.volume = 0f;
            TargetVolume = 0f;
        }

        public void FadeIn(float fadeTime)
        {
            TargetVolume = _maxVolume;
            _fadeSpeed = _maxVolume / Mathf.Max(fadeTime, 0.01f);
        }

        public void FadeOut(float fadeTime)
        {
            TargetVolume = 0f;
            _fadeSpeed = _maxVolume / Mathf.Max(fadeTime, 0.01f);
        }

        public void SetVolumeImmediate(float volume)
        {
            if (Source == null)
            {
                return;
            }

            float clamped = Mathf.Clamp01(volume);
            TargetVolume = clamped;
            Source.volume = clamped;
        }

        public void SetMaxVolume(float maxVolume)
        {
            _maxVolume = Mathf.Clamp01(maxVolume);
            TargetVolume = Mathf.Clamp01(TargetVolume);
        }

        public void Update(float masterMusicVolume)
        {
            if (Source == null)
            {
                return;
            }

            float target = Mathf.Clamp01(TargetVolume) * Mathf.Clamp01(masterMusicVolume);
            if (Mathf.Abs(Source.volume - target) <= 0.0001f)
            {
                Source.volume = target;
                return;
            }

            Source.volume = Mathf.MoveTowards(
                Source.volume,
                target,
                Mathf.Max(0.001f, _fadeSpeed) * Time.unscaledDeltaTime);
        }

        /// <summary>Sync playback position to match another layer.</summary>
        public void SyncTo(MusicLayer other)
        {
            if (Source == null || other == null || other.Source == null)
            {
                return;
            }

            if (!other.Source.isPlaying || Source.clip == null)
            {
                return;
            }

            int sample = Mathf.Clamp(other.Source.timeSamples, 0, Mathf.Max(0, Source.clip.samples - 1));
            Source.timeSamples = sample;
            if (!Source.isPlaying)
            {
                Source.Play();
            }
        }
    }
}
