namespace EduLMS.Web.ViewModels.Instructor
{
    public class InstructorNotificationsViewModel
    {
        public List<SentNotificationGroup> SentNotifications { get; set; } = new();
        public List<CourseOption> Courses { get; set; } = new();
    }

    public class SentNotificationGroup
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? CourseName { get; set; }   // null = tất cả khoá học
        public DateTime SentAt { get; set; }
        public int RecipientCount { get; set; }
    }

    public class CourseOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
