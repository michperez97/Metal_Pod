namespace MetalPod.Shared
{
    /// <summary>
    /// Global event bus for cross-system communication.
    /// Prefer direct component events for tightly coupled systems.
    /// </summary>
    public static class EventBus
    {
        public static bool IsInitialized { get; private set; }

        public static event System.Action<int> OnCurrencyChanged;
        public static event System.Action<int> OnCurrencyEarned;

        public static event System.Action<string> OnCourseSelected;
        public static event System.Action<string, float, int> OnCourseCompleted;
        public static event System.Action<string> OnCourseUnlocked;

        public static event System.Action<string, int> OnUpgradePurchased;

        public static event System.Action<string> OnCosmeticEquipped;

        public static void Initialize()
        {
            // Supports Enter Play Mode without domain reload by resetting stale listeners.
            ClearListeners();
            IsInitialized = true;
        }

        public static void Shutdown()
        {
            ClearListeners();
            IsInitialized = false;
        }

        public static void RaiseCurrencyChanged(int total) => OnCurrencyChanged?.Invoke(total);
        public static void RaiseCurrencyEarned(int amount) => OnCurrencyEarned?.Invoke(amount);
        public static void RaiseCourseSelected(string id) => OnCourseSelected?.Invoke(id);

        public static void RaiseCourseCompleted(string id, float time, int medal)
        {
            OnCourseCompleted?.Invoke(id, time, medal);
        }

        public static void RaiseCourseUnlocked(string id) => OnCourseUnlocked?.Invoke(id);

        public static void RaiseUpgradePurchased(string id, int level)
        {
            OnUpgradePurchased?.Invoke(id, level);
        }

        public static void RaiseCosmeticEquipped(string id) => OnCosmeticEquipped?.Invoke(id);

        private static void ClearListeners()
        {
            OnCurrencyChanged = null;
            OnCurrencyEarned = null;
            OnCourseSelected = null;
            OnCourseCompleted = null;
            OnCourseUnlocked = null;
            OnUpgradePurchased = null;
            OnCosmeticEquipped = null;
        }
    }
}
