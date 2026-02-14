using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnvironmentData", menuName = "MetalPod/EnvironmentData")]
    public class EnvironmentDataSO : ScriptableObject
    {
        public string environmentId;
        public string environmentName;
        public EnvironmentType environmentType;

        [TextArea]
        public string description;

        public Sprite environmentIcon;
        public Color primaryColor;
        public Color secondaryColor;
        public int requiredMedalsToUnlock;
        public CourseDataSO[] courses;

        [Header("Audio")]
        public AudioClip ambientLoop;
        public AudioClip musicTrack;

        [Header("Visual")]
        public Material skyboxMaterial;
    }
}
