namespace MetalPod.Shared
{
    public static class GameConstants
    {
        public const string TAG_PLAYER = "Player";
        public const string TAG_CHECKPOINT = "Checkpoint";
        public const string TAG_HAZARD = "Hazard";
        public const string TAG_COLLECTIBLE = "Collectible";
        public const string TAG_FINISH = "FinishLine";

        public const string LAYER_HOVERCRAFT = "Hovercraft";
        public const string LAYER_GROUND = "Ground";
        public const string LAYER_HAZARD = "Hazard";
        public const string LAYER_COLLECTIBLE = "Collectible";

        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_WORKSHOP = "Workshop";
        public const string SCENE_PERSISTENT = "_Persistent";

        public const string PREF_TILT_SENSITIVITY = "TiltSensitivity";
        public const string PREF_INVERT_TILT = "InvertTilt";
        public const string PREF_MASTER_VOLUME = "MasterVolume";
        public const string PREF_MUSIC_VOLUME = "MusicVolume";
        public const string PREF_SFX_VOLUME = "SFXVolume";
        public const string PREF_HAPTICS_ENABLED = "HapticsEnabled";
        public const string PREF_QUALITY_LEVEL = "QualityLevel";

        public const float MEDAL_BONUS_BRONZE = 0.25f;
        public const float MEDAL_BONUS_SILVER = 0.50f;
        public const float MEDAL_BONUS_GOLD = 1.00f;
        public const float REPLAY_REWARD_MULTIPLIER = 0.5f;
        public const float HEALTH_SPEED_THRESHOLD_50 = 0.5f;
        public const float HEALTH_SPEED_MULTIPLIER_50 = 0.8f;
        public const float HEALTH_SPEED_THRESHOLD_25 = 0.25f;
        public const float HEALTH_SPEED_MULTIPLIER_25 = 0.6f;
        public const float HEALTH_HANDLING_MULTIPLIER_25 = 0.7f;
    }
}
