using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Audio
{
    /// <summary>
    /// Attach in a scene to explicitly override automatic music profile detection.
    /// </summary>
    public class EnvironmentMusicTrigger : MonoBehaviour
    {
        [SerializeField] private MusicProfileSO musicProfile;
        [SerializeField] private bool crossfade = true;

        private void Start()
        {
            if (musicProfile != null && DynamicMusicManager.Instance != null)
            {
                DynamicMusicManager.Instance.SetProfile(musicProfile, crossfade);
            }
        }
    }
}
