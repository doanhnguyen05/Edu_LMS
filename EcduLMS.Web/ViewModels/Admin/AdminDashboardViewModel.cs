namespace EduLMS.Web.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalGroups { get; set; }
        public decimal TotalStudyHours { get; set; }
        public decimal CompletionRate { get; set; }
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
