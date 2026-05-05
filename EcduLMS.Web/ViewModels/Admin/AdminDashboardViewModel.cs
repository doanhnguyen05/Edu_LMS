namespace EduLMS.Web.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int TotalInstructors { get; set; }
        public int ActiveInstructors { get; set; }
        public decimal TotalStudyHours { get; set; }
        public int TotalSchedules { get; set; }
        public int TrackedEnrollments { get; set; }
        public decimal AverageProgressRate { get; set; }
        public List<ActivityLogItem> RecentActivities { get; set; } = new();
        public string[] AccessChartLabels { get; set; } = Array.Empty<string>();
        public int[] AccessChartData { get; set; } = Array.Empty<int>();

        public class ActivityLogItem
        {
            public string Icon { get; set; } = string.Empty;
            public string IconColor { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Time { get; set; } = string.Empty;
        }
    }
}
