#if !METALPOD_SHARED_CONTRACTS
namespace MetalPod.Shared
{
    // Fallback contract for standalone Agent 4 execution.
    // Remove or disable with METALPOD_SHARED_CONTRACTS once Agent 1 shared contracts are present.
    public interface IProgressionData
    {
        int Currency { get; }
        int TotalMedals { get; }
        int GetUpgradeLevel(string upgradeId);
        float GetBestTime(string courseId);
        int GetBestMedal(string courseId);
        bool IsCourseUnlocked(string courseId);
    }
}
#endif
