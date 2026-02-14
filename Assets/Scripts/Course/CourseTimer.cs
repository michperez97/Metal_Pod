using UnityEngine;

namespace MetalPod.Course
{
    public class CourseTimer : MonoBehaviour
    {
        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        private void Update()
        {
            if (IsRunning && !IsPaused)
            {
                ElapsedTime += Time.deltaTime;
            }
        }

        public void ResetTimer()
        {
            ElapsedTime = 0f;
            IsRunning = false;
            IsPaused = false;
        }

        public void StartTimer()
        {
            IsRunning = true;
            IsPaused = false;
        }

        public void StopTimer()
        {
            IsRunning = false;
            IsPaused = false;
        }

        public void PauseTimer()
        {
            if (IsRunning)
            {
                IsPaused = true;
            }
        }

        public void ResumeTimer()
        {
            if (IsRunning)
            {
                IsPaused = false;
            }
        }

        public string GetFormattedTime(float? timeOverride = null)
        {
            float time = timeOverride ?? ElapsedTime;
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int centiseconds = Mathf.FloorToInt((time - Mathf.Floor(time)) * 100f);
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
        }
    }
}
