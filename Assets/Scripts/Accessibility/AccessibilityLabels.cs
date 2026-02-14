namespace MetalPod.Accessibility
{
    /// <summary>
    /// Centralized VoiceOver label strings for localization readiness.
    /// </summary>
    public static class AccessibilityLabels
    {
        // Main Menu
        public const string PlayButton = "Play. Double tap to enter the Workshop.";
        public const string SettingsButton = "Settings. Double tap to open settings.";
        public const string QuitButton = "Quit. Double tap to exit the game.";

        // Workshop
        public const string CoursesTab = "Courses tab. Double tap to browse race courses.";
        public const string UpgradesTab = "Upgrades tab. Double tap to upgrade your hovercraft.";
        public const string CustomizeTab = "Customize tab. Double tap to customize appearance.";
        public const string LaunchButton = "Launch race. Double tap to start the selected course.";
        public const string BackButton = "Back. Double tap to return to the previous screen.";

        // HUD
        public const string SpeedDisplay = "Current speed: {0} kilometers per hour";
        public const string HealthBar = "Health: {0} percent";
        public const string ShieldBar = "Shield: {0} percent";
        public const string BoostBar = "Boost: {0} percent";
        public const string TimerDisplay = "Race time: {0}";
        public const string CheckpointDisplay = "Checkpoint {0} of {1}";
        public const string CurrencyDisplay = "Bolts: {0}";

        // Course Selection
        public const string CourseCard = "{0}. {1} environment. Best time: {2}. Medal: {3}.";
        public const string LockedCourse = "{0}. Locked. Requires: {1}.";

        // Upgrades
        public const string UpgradeCategory = "{0}. Level {1} of 5. Cost to upgrade: {2} bolts.";
        public const string UpgradeMaxed = "{0}. Maximum level reached.";
        public const string PurchaseUpgrade = "Purchase {0} upgrade for {1} bolts. Double tap to confirm.";

        // Cosmetics
        public const string CosmeticItem = "{0}. {1}.";
        public const string CosmeticLocked = "{0}. Locked. Price: {1} bolts.";
        public const string CosmeticEquipped = "{0}. Currently equipped.";

        // Results
        public const string RaceComplete = "Race complete. Time: {0}. Medal: {1}. Bolts earned: {2}.";
        public const string NewRecord = "New record. Previous best: {0}.";

        // Medals
        public const string GoldMedal = "Gold medal";
        public const string SilverMedal = "Silver medal";
        public const string BronzeMedal = "Bronze medal";
        public const string NoMedal = "No medal";

        // Announcements
        public const string CountdownAnnounce = "{0}";
        public const string CheckpointReached = "Checkpoint reached. {0} of {1}.";
        public const string DamageTaken = "Damage taken. Health at {0} percent.";
        public const string MedalEarned = "{0} earned.";
        public const string CourseUnlocked = "New course unlocked: {0}.";
    }
}
