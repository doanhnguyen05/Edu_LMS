namespace EduLMS.Web.ViewModels.Learner
{
    public class LearnerDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public string? ResumeCourseName { get; set; }
        public string? ResumeLessonTitle { get; set; }
        public int ResumeProgress { get; set; }
        public int? ResumeCourseId { get; set; }
        public int? ResumeLessonId { get; set; }
        public List<NotificationItem> Notifications { get; set; } = new();
        public List<ActivityItem> Activities { get; set; } = new();
        public string[] StudyChartLabels { get; set; } = Array.Empty<string>();
        public int[] StudyChartData { get; set; } = Array.Empty<int>();
        public string[] GradeLabels { get; set; } = Array.Empty<string>();
        public decimal[] GradeScores { get; set; } = Array.Empty<decimal>();
    }
}
