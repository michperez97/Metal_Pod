namespace MetalPod.Shared
{
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
