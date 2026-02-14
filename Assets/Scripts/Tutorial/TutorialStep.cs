using UnityEngine;

namespace MetalPod.Tutorial
{
    public enum TutorialCondition
    {
        None,
        TiltDevice,
        TapBoost,
        TapBrake,
        ReachSpeed,
        TakeDamage,
        UseShield,
        ReachCheckpoint,
        CollectItem,
        FinishCourse,
        WaitSeconds
    }

    public enum TutorialPromptPosition
    {
        TopCenter,
        BottomCenter,
        Center,
        NearControl
    }

    [System.Serializable]
    public class TutorialStep
    {
        [Header("Content")]
        public string stepId;
        [TextArea(2, 5)] public string promptText;
        [TextArea(1, 3)] public string subtitleText;
        public Sprite iconSprite;

        [Header("Behavior")]
        public TutorialCondition completionCondition;
        public float conditionValue;
        public float autoAdvanceDelay;
        public bool pauseGame;
        public bool slowMotion;
        public bool requireTapToContinue;

        [Header("Highlight")]
        public string highlightUIElement;
        public TutorialPromptPosition promptPosition;

        [Header("Visual")]
        public bool showArrowPointing;
        public Vector2 arrowTarget = new Vector2(0.5f, 0.5f);
        public bool dimBackground;
    }
}
