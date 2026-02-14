namespace MetalPod.CloudSave
{
    public enum ConflictResolution
    {
        UseLocal,
        UseCloud,
        UseNewest,
        UseMostProgress
    }

    /// <summary>
    /// Resolves conflicts between local and cloud saves.
    /// Default strategy: most recent wins. Fallback: most progress wins.
    /// </summary>
    public static class CloudSaveConflictResolver
    {
        /// <summary>
        /// Determine which save to use based on the conflict data.
        /// </summary>
        public static ConflictResolution Resolve(
            CloudSaveConflict conflict,
            ConflictResolution strategy = ConflictResolution.UseNewest)
        {
            switch (strategy)
            {
                case ConflictResolution.UseNewest:
                    return conflict.cloudTimestamp > conflict.localTimestamp
                        ? ConflictResolution.UseCloud
                        : ConflictResolution.UseLocal;

                case ConflictResolution.UseMostProgress:
                    int localScore = CalculateProgressScore(conflict.localInfo);
                    int cloudScore = CalculateProgressScore(conflict.cloudInfo);
                    return cloudScore > localScore
                        ? ConflictResolution.UseCloud
                        : ConflictResolution.UseLocal;

                default:
                    return strategy;
            }
        }

        /// <summary>
        /// Calculate a progress score used for conflict comparison.
        /// </summary>
        private static int CalculateProgressScore(CloudSaveInfo info)
        {
            // Weight: medals (x100) + courses (x50) + currency (x1)
            return (info.totalMedals * 100) +
                   (info.totalCoursesCompleted * 50) +
                   info.currency;
        }
    }
}
