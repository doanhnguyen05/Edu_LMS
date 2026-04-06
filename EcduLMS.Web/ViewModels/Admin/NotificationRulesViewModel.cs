namespace EduLMS.Web.ViewModels.Admin
{
    public class NotificationRulesViewModel
    {
        public List<NotificationRuleItem> Rules { get; set; } = new();
        public List<AdminBroadcastGroup> SentBroadcasts { get; set; } = new();
        public List<AdminCourseOption> Courses { get; set; } = new();
    }

    public class NotificationRuleItem
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string TemplateBody { get; set; } = string.Empty;
    }

    public class AdminBroadcastGroup
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetLabel { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int RecipientCount { get; set; }
    }

    public class AdminCourseOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
    }
}
