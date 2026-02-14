using System;

namespace MetalPod.Course
{
    public interface ICourseEvents
    {
        event Action OnCountdownStarted;
        event Action<int> OnCountdownTick;
        event Action OnRaceStarted;
        event Action<float> OnRaceFinished;
        event Action OnRaceFailed;
        event Action<int> OnCheckpointReached;
        event Action OnPlayerRespawning;
    }
}
