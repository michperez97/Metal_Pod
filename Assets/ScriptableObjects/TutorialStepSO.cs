using MetalPod.Tutorial;
using UnityEngine;

namespace MetalPod.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "MetalPod/Tutorial/Tutorial Step")]
    public class TutorialStepSO : ScriptableObject
    {
        public TutorialStep step = new TutorialStep
        {
            stepId = "new_step",
            promptText = "Prompt",
            subtitleText = "Details",
            completionCondition = TutorialCondition.None,
            promptPosition = TutorialPromptPosition.Center
        };
    }
}
