using System;

namespace MetalPod.Course
{
    public static class CourseEventBus
    {
        public static event Action<string, float, int> OnCourseCompleted;

        public static void RaiseCourseCompleted(string courseId, float completionTime, int medal)
        {
            OnCourseCompleted?.Invoke(courseId, completionTime, medal);
        }
    }
}
