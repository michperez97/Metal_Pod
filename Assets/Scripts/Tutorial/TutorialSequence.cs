namespace MetalPod.Tutorial
{
    [System.Serializable]
    public class TutorialSequence
    {
        public string sequenceId;
        public string sequenceName;
        public TutorialStep[] steps;

        public static TutorialSequence CreateFirstPlaySequence()
        {
            return new TutorialSequence
            {
                sequenceId = "first_play",
                sequenceName = "First Time Playing",
                steps = new[]
                {
                    new TutorialStep
                    {
                        stepId = "welcome",
                        promptText = "WELCOME TO METAL POD",
                        subtitleText = "Let's learn the basics. Tap to continue.",
                        completionCondition = TutorialCondition.None,
                        requireTapToContinue = true,
                        pauseGame = true,
                        promptPosition = TutorialPromptPosition.Center,
                        dimBackground = true
                    },
                    new TutorialStep
                    {
                        stepId = "steering",
                        promptText = "TILT TO STEER",
                        subtitleText = "Tilt your device left and right to turn.",
                        completionCondition = TutorialCondition.TiltDevice,
                        slowMotion = true,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        autoAdvanceDelay = 8f
                    },
                    new TutorialStep
                    {
                        stepId = "speed",
                        promptText = "YOU'RE MOVING!",
                        subtitleText = "Your pod accelerates automatically. Watch your speed on the HUD.",
                        completionCondition = TutorialCondition.ReachSpeed,
                        conditionValue = 10f,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        highlightUIElement = "SpeedDisplay"
                    },
                    new TutorialStep
                    {
                        stepId = "boost",
                        promptText = "TAP TO BOOST",
                        subtitleText = "Tap the right side of the screen for a speed burst!",
                        completionCondition = TutorialCondition.TapBoost,
                        promptPosition = TutorialPromptPosition.NearControl,
                        highlightUIElement = "BoostIndicator",
                        showArrowPointing = true,
                        arrowTarget = new UnityEngine.Vector2(0.8f, 0.2f)
                    },
                    new TutorialStep
                    {
                        stepId = "brake",
                        promptText = "HOLD TO BRAKE",
                        subtitleText = "Hold the left side of the screen to slow down.",
                        completionCondition = TutorialCondition.TapBrake,
                        promptPosition = TutorialPromptPosition.NearControl,
                        showArrowPointing = true,
                        arrowTarget = new UnityEngine.Vector2(0.2f, 0.2f),
                        autoAdvanceDelay = 10f
                    },
                    new TutorialStep
                    {
                        stepId = "health",
                        promptText = "WATCH YOUR HEALTH",
                        subtitleText = "Your shield (blue) absorbs damage first. Health (red) is below it. Avoid hazards!",
                        completionCondition = TutorialCondition.WaitSeconds,
                        conditionValue = 4f,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        highlightUIElement = "HealthBar"
                    },
                    new TutorialStep
                    {
                        stepId = "damage_warning",
                        promptText = "HAZARD AHEAD!",
                        subtitleText = "Lava flows deal damage. Steer clear or boost past them!",
                        completionCondition = TutorialCondition.None,
                        requireTapToContinue = true,
                        slowMotion = true,
                        promptPosition = TutorialPromptPosition.Center
                    },
                    new TutorialStep
                    {
                        stepId = "checkpoint",
                        promptText = "CHECKPOINT!",
                        subtitleText = "If you're destroyed, you'll respawn here.",
                        completionCondition = TutorialCondition.ReachCheckpoint,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        highlightUIElement = "TimerDisplay"
                    },
                    new TutorialStep
                    {
                        stepId = "collectible",
                        promptText = "GRAB COLLECTIBLES",
                        subtitleText = "Floating pickups give bonus currency and health.",
                        completionCondition = TutorialCondition.CollectItem,
                        promptPosition = TutorialPromptPosition.TopCenter,
                        autoAdvanceDelay = 15f
                    },
                    new TutorialStep
                    {
                        stepId = "finish_line",
                        promptText = "REACH THE FINISH!",
                        subtitleText = "Race to the end. Faster times earn better medals!",
                        completionCondition = TutorialCondition.FinishCourse,
                        promptPosition = TutorialPromptPosition.TopCenter
                    }
                }
            };
        }

        public static TutorialSequence CreateWorkshopIntroSequence()
        {
            return new TutorialSequence
            {
                sequenceId = "workshop_intro",
                sequenceName = "Workshop Introduction",
                steps = new[]
                {
                    new TutorialStep
                    {
                        stepId = "workshop_welcome",
                        promptText = "THE WORKSHOP",
                        subtitleText = "This is your base. Upgrade your pod, customize it, and pick your next course.",
                        completionCondition = TutorialCondition.None,
                        requireTapToContinue = true,
                        pauseGame = true,
                        promptPosition = TutorialPromptPosition.Center
                    },
                    new TutorialStep
                    {
                        stepId = "workshop_upgrades",
                        promptText = "UPGRADES",
                        subtitleText = "Spend currency to improve Speed, Handling, Shields, and Boost.",
                        completionCondition = TutorialCondition.None,
                        requireTapToContinue = true,
                        promptPosition = TutorialPromptPosition.BottomCenter,
                        highlightUIElement = "UpgradesButton"
                    },
                    new TutorialStep
                    {
                        stepId = "workshop_courses",
                        promptText = "CHOOSE A COURSE",
                        subtitleText = "Tap Courses to pick your next challenge. Earn medals to unlock more!",
                        completionCondition = TutorialCondition.None,
                        requireTapToContinue = true,
                        promptPosition = TutorialPromptPosition.BottomCenter,
                        highlightUIElement = "CoursesButton"
                    }
                }
            };
        }
    }
}
