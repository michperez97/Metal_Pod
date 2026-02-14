using UnityEngine;

namespace MetalPod.Transitions
{
    // ScriptableObject mapping scenes to transition configurations.
    // Optional alternative to the hardcoded mapping in SceneTransitionManager.
    [CreateAssetMenu(fileName = "TransitionConfig", menuName = "MetalPod/Transition Config")]
    public class TransitionConfig : ScriptableObject
    {
        [System.Serializable]
        public class SceneTransitionMapping
        {
            public string sceneName;
            public TransitionType transitionType;
            public EnvironmentTheme environmentTheme;
            public WipeDirection wipeDirection;
            public Color fadeColor = Color.black;
            public float transitionInDuration = 0.5f;
            public float transitionOutDuration = 0.5f;
        }

        public enum TransitionType
        {
            Fade,
            Wipe,
            EnvironmentDissolve
        }

        public SceneTransitionMapping[] mappings;
        public SceneTransitionMapping defaultMapping;
    }
}
