using MetalPod.ScriptableObjects;

namespace MetalPod.Course
{
    public enum Medal
    {
        None,
        Bronze,
        Silver,
        Gold
    }

    public static class MedalSystem
    {
        public static Medal EvaluatePerformance(float completionTime, CourseDataSO courseData)
        {
            if (courseData == null)
            {
                return Medal.None;
            }

            if (completionTime <= courseData.goldTime) return Medal.Gold;
            if (completionTime <= courseData.silverTime) return Medal.Silver;
            if (completionTime <= courseData.bronzeTime) return Medal.Bronze;
            return Medal.None;
        }
    }
}
