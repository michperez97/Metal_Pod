namespace MetalPod.Localization
{
    /// <summary>
    /// Static shorthand for localization access.
    /// </summary>
    public static class Loc
    {
        public static string Get(string key)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetString(key);
            }

            return $"[{key}]";
        }

        public static string Get(string key, params object[] args)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetString(key, args);
            }

            return $"[{key}]";
        }

        public static string CurrentLanguage =>
            LocalizationManager.Instance != null ? LocalizationManager.Instance.CurrentLanguageCode : "en";
    }
}
