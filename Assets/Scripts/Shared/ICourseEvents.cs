namespace MetalPod.Shared
{
    public interface ICourseEvents
    {
        event System.Action OnCountdownStarted;
        event System.Action<int> OnCountdownTick;
        event System.Action OnRaceStarted;
        event System.Action<float> OnRaceFinished;
        event System.Action OnRaceFailed;
        event System.Action<int> OnCheckpointReached;
        event System.Action OnPlayerRespawning;
    }
}
