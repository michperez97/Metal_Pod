using System.Collections;
using System.Collections.Generic;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Primary Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource secondaryMusicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Mix")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float ambientVolume = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.9f;

        [Header("Advanced")]
        [Range(2, 16)] [SerializeField] private int sfxPoolSize = 8;
        [Range(0.05f, 3f)] [SerializeField] private float musicCrossfadeSeconds = 1f;

        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        private AudioSource activeMusicSource;
        private AudioSource inactiveMusicSource;
        private Coroutine musicCrossfadeRoutine;
        private int nextSfxIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSources();
            ApplySavedVolumes();
            ApplyVolumes();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnValidate()
        {
            sfxPoolSize = Mathf.Clamp(sfxPoolSize, 2, 16);
            musicCrossfadeSeconds = Mathf.Clamp(musicCrossfadeSeconds, 0.05f, 3f);

            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            ambientVolume = Mathf.Clamp01(ambientVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);

            if (Application.isPlaying)
            {
                ApplyVolumes();
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null)
            {
                return;
            }

            if (activeMusicSource == null || inactiveMusicSource == null)
            {
                InitializeSources();
            }

            if (activeMusicSource == null)
            {
                return;
            }

            if (activeMusicSource.isPlaying && activeMusicSource.clip == clip)
            {
                activeMusicSource.loop = loop;
                return;
            }

            if (!activeMusicSource.isPlaying)
            {
                activeMusicSource.clip = clip;
                activeMusicSource.loop = loop;
                activeMusicSource.volume = GetMusicTargetVolume();
                activeMusicSource.Play();
                return;
            }

            if (musicCrossfadeRoutine != null)
            {
                StopCoroutine(musicCrossfadeRoutine);
            }

            inactiveMusicSource.clip = clip;
            inactiveMusicSource.loop = loop;
            inactiveMusicSource.volume = 0f;
            inactiveMusicSource.Play();

            musicCrossfadeRoutine = StartCoroutine(CrossfadeMusicRoutine());
        }

        public void StopMusic()
        {
            if (musicCrossfadeRoutine != null)
            {
                StopCoroutine(musicCrossfadeRoutine);
                musicCrossfadeRoutine = null;
            }

            if (musicSource != null)
            {
                musicSource.Stop();
            }

            if (secondaryMusicSource != null)
            {
                secondaryMusicSource.Stop();
            }
        }

        public void PlayAmbient(AudioClip clip, bool loop = true)
        {
            if (ambientSource == null || clip == null)
            {
                return;
            }

            ambientSource.clip = clip;
            ambientSource.loop = loop;
            ambientSource.volume = GetAmbientTargetVolume();
            ambientSource.Play();
        }

        public void StopAmbient()
        {
            if (ambientSource != null)
            {
                ambientSource.Stop();
            }
        }

        public void PlaySfx(AudioClip clip)
        {
            PlaySFX(clip);
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource source = GetNextSfxSource();
            if (source == null)
            {
                return;
            }

            source.spatialBlend = 0f;
            source.transform.position = transform.position;
            source.PlayOneShot(clip, 1f);
        }

        public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
        {
            if (clip == null)
            {
                return;
            }

            GameObject pointAudio = new GameObject($"SFX_3D_{clip.name}");
            pointAudio.transform.position = position;

            AudioSource source = pointAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 30f;
            source.volume = GetSfxTargetVolume();
            source.Play();

            Destroy(pointAudio, clip.length + 0.1f);
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(GameConstants.PREF_MASTER_VOLUME, masterVolume);
            ApplyVolumes();
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(GameConstants.PREF_MUSIC_VOLUME, musicVolume);
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(GameConstants.PREF_SFX_VOLUME, sfxVolume);
            ApplyVolumes();
        }

        public void SetAmbientVolume(float value)
        {
            ambientVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        private void InitializeSources()
        {
            if (musicSource == null)
            {
                musicSource = CreateChildAudioSource("MusicSource_A", false, 0f);
            }

            if (secondaryMusicSource == null)
            {
                secondaryMusicSource = CreateChildAudioSource("MusicSource_B", false, 0f);
            }

            if (ambientSource == null)
            {
                ambientSource = CreateChildAudioSource("AmbientSource", true, 0f);
            }

            if (sfxSource == null)
            {
                sfxSource = CreateChildAudioSource("SFXSource_0", false, 0f);
            }

            musicSource.playOnAwake = false;
            secondaryMusicSource.playOnAwake = false;
            ambientSource.playOnAwake = false;
            sfxSource.playOnAwake = false;

            activeMusicSource = musicSource;
            inactiveMusicSource = secondaryMusicSource;

            BuildSfxPool();
        }

        private void BuildSfxPool()
        {
            sfxPool.Clear();
            sfxPool.Add(sfxSource);

            for (int i = 1; i < sfxPoolSize; i++)
            {
                AudioSource pooledSource = CreateChildAudioSource($"SFXSource_{i}", false, 0f);
                pooledSource.spatialBlend = 0f;
                sfxPool.Add(pooledSource);
            }

            nextSfxIndex = 0;
        }

        private AudioSource CreateChildAudioSource(string childName, bool loop, float spatialBlend)
        {
            Transform child = transform.Find(childName);
            GameObject sourceObject = child != null ? child.gameObject : new GameObject(childName);
            sourceObject.transform.SetParent(transform, false);

            AudioSource source = sourceObject.GetComponent<AudioSource>();
            if (source == null)
            {
                source = sourceObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            return source;
        }

        private AudioSource GetNextSfxSource()
        {
            if (sfxPool.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < sfxPool.Count; i++)
            {
                int index = (nextSfxIndex + i) % sfxPool.Count;
                AudioSource candidate = sfxPool[index];
                if (!candidate.isPlaying)
                {
                    nextSfxIndex = (index + 1) % sfxPool.Count;
                    return candidate;
                }
            }

            AudioSource fallback = sfxPool[nextSfxIndex];
            nextSfxIndex = (nextSfxIndex + 1) % sfxPool.Count;
            return fallback;
        }

        private IEnumerator CrossfadeMusicRoutine()
        {
            float timer = 0f;
            float fromVolume = activeMusicSource.volume;
            float toVolume = GetMusicTargetVolume();

            while (timer < musicCrossfadeSeconds)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / musicCrossfadeSeconds);

                activeMusicSource.volume = Mathf.Lerp(fromVolume, 0f, t);
                inactiveMusicSource.volume = Mathf.Lerp(0f, toVolume, t);
                yield return null;
            }

            activeMusicSource.Stop();
            activeMusicSource.volume = 0f;
            inactiveMusicSource.volume = toVolume;

            AudioSource previousActive = activeMusicSource;
            activeMusicSource = inactiveMusicSource;
            inactiveMusicSource = previousActive;

            musicCrossfadeRoutine = null;
        }

        private void ApplySavedVolumes()
        {
            if (PlayerPrefs.HasKey(GameConstants.PREF_MASTER_VOLUME))
            {
                masterVolume = PlayerPrefs.GetFloat(GameConstants.PREF_MASTER_VOLUME);
            }

            if (PlayerPrefs.HasKey(GameConstants.PREF_MUSIC_VOLUME))
            {
                musicVolume = PlayerPrefs.GetFloat(GameConstants.PREF_MUSIC_VOLUME);
            }

            if (PlayerPrefs.HasKey(GameConstants.PREF_SFX_VOLUME))
            {
                sfxVolume = PlayerPrefs.GetFloat(GameConstants.PREF_SFX_VOLUME);
            }
        }

        private void ApplyVolumes()
        {
            float targetMusic = GetMusicTargetVolume();
            float targetAmbient = GetAmbientTargetVolume();
            float targetSfx = GetSfxTargetVolume();

            if (activeMusicSource != null && activeMusicSource.isPlaying && musicCrossfadeRoutine == null)
            {
                activeMusicSource.volume = targetMusic;
            }

            if (inactiveMusicSource != null && !inactiveMusicSource.isPlaying)
            {
                inactiveMusicSource.volume = 0f;
            }

            if (ambientSource != null)
            {
                ambientSource.volume = targetAmbient;
            }

            foreach (AudioSource source in sfxPool)
            {
                if (source != null)
                {
                    source.volume = targetSfx;
                }
            }
        }

        private float GetMusicTargetVolume()
        {
            return masterVolume * musicVolume;
        }

        private float GetAmbientTargetVolume()
        {
            return masterVolume * ambientVolume;
        }

        private float GetSfxTargetVolume()
        {
            return masterVolume * sfxVolume;
        }
    }
}
