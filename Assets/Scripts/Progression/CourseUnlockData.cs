using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Progression
{
    [CreateAssetMenu(fileName = "CourseUnlockData", menuName = "MetalPod/CourseUnlockData")]
    public class CourseUnlockData : ScriptableObject
    {
        public CourseDataSO[] courseOrder;

        public CourseDataSO GetCourseById(string courseId)
        {
            if (courseOrder == null || string.IsNullOrWhiteSpace(courseId))
            {
                return null;
            }

            for (int i = 0; i < courseOrder.Length; i++)
            {
                CourseDataSO course = courseOrder[i];
                if (course != null && course.courseId == courseId)
                {
                    return course;
                }
            }

            return null;
        }

        public bool IsCourseUnlocked(CourseDataSO course, SaveData saveData)
        {
            if (course == null || saveData == null)
            {
                return false;
            }

            if (course.environmentType == EnvironmentType.Lava && course.courseIndex == 0)
            {
                return true;
            }

            if (course.prerequisiteCourse != null)
            {
                bool completed = saveData.completedCourses.GetValueOrDefault(course.prerequisiteCourse.courseId, false);
                if (!completed)
                {
                    return false;
                }
            }

            int medals = saveData.totalMedals;
            if (medals < course.requiredMedals)
            {
                return false;
            }

            return true;
        }

        public bool IsCourseUnlocked(string courseId, SaveData saveData)
        {
            return IsCourseUnlocked(GetCourseById(courseId), saveData);
        }
    }
}
