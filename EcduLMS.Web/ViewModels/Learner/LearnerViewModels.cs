namespace EduLMS.Web.ViewModels.Learner
{
    // Dashboard
    public class NotificationItem
    {
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    public class ActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public DateTime SortKey { get; set; }
    }

    // Training
    public class TrainingViewModel
    {
        public List<TrainingCourseItem> ActiveCourses { get; set; } = new();
        public List<TrainingCourseItem> CompletedCourses { get; set; } = new();
        public string ActiveTab { get; set; } = "active";
    }

    public class TrainingCourseItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public decimal Progress { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Course Detail
    public class CourseDetailViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Progress { get; set; }
        public string? CoverImageUrl { get; set; }
        public List<LessonItem> Lessons { get; set; } = new();
        public List<AssignmentListItem> Assignments { get; set; } = new();
    }

    public class LessonItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = "not_started"; // completed, in_progress, not_started
        public bool IsLocked { get; set; }
        public string? LockMessage { get; set; }
    }

    public class AssignmentListItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsSubmitted { get; set; }
        public bool CanStart { get; set; } = true;
        public string? BlockedReason { get; set; }
    }

    // Lesson View
    public class LessonViewModel
    {
        public int LessonId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public string? VideoUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public bool CanMarkComplete { get; set; }
        public string? LockMessage { get; set; }
        public List<ResourceItem> Resources { get; set; } = new();
    }

    public class ResourceItem
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }

    // Assignment Submit
    public class AssignmentSubmitViewModel
    {
        public int AssignmentId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DueDate { get; set; }
        public decimal MaxScore { get; set; } = 10;
        public string Status { get; set; } = "Draft";
        public string? ExistingContent { get; set; }
        public string? ExistingFileName { get; set; }
        public int CourseId { get; set; }
    }

    // Catalog
    public class CatalogViewModel
    {
        public List<CatalogCourseItem> Courses { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? SelectedCategory { get; set; }
        public string? LevelFilter { get; set; }
    }

    public class CatalogCourseItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public decimal DurationHours { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public decimal PriceValue { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsEnrolled { get; set; }
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    // Payment / Checkout
    public class CheckoutViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal? PromoPrice { get; set; }
        public decimal FinalPrice { get; set; }

        // Payment info
        public int PaymentId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string QrImageUrl { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;

        // Schedule conflicts
        public List<Services.ConflictResult> Conflicts { get; set; } = new();
        public bool HasConflict => Conflicts.Any();
    }

    // Calendar
    public class CalendarEventItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public string? ZoomLink { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    public class CalendarViewModel
    {
        public List<CalendarEventItem> UpcomingEvents { get; set; } = new();
    }

    // Grades
    public class GradesOverviewViewModel
    {
        public List<CourseGradeItem> CourseGrades { get; set; } = new();
    }

    public class CourseGradeItem
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal? AverageScore { get; set; }
        public string GradedProgress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // Grade Course Detail
    public class GradeCourseDetailViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal? AverageScore { get; set; }
        public List<GradeAssignmentItem> Assignments { get; set; } = new();
    }

    public class GradeAssignmentItem
    {
        public int AssignmentId { get; set; }
        public int SubmissionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public decimal MaxScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool CanStartAssignment { get; set; } = true;
        public string? StartBlockedReason { get; set; }
    }

    // Grade Detail
    public class GradeDetailViewModel
    {
        public string AssignmentTitle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string PassStatus { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public int CourseId { get; set; }
    }
}
