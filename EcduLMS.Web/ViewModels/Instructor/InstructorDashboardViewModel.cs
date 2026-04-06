namespace EduLMS.Web.ViewModels.Instructor
{
    public class InstructorDashboardViewModel
    {
        public int ActiveStudents { get; set; }
        public int AssignedCourses { get; set; }
        public int NewCoursesThisWeek { get; set; }
        public decimal StudentGrowthPercent { get; set; }
        // progressTrendChart
        public string[] TrendLabels { get; set; } = Array.Empty<string>();
        public int[] WeeklySubmissions { get; set; } = Array.Empty<int>();
        public int[] WeeklyEnrollments { get; set; } = Array.Empty<int>();
        // scoreCompareChart
        public string[] CourseLabels { get; set; } = Array.Empty<string>();
        public decimal[] CourseAvgScores { get; set; } = Array.Empty<decimal>();
        public decimal[] CourseMaxScores { get; set; } = Array.Empty<decimal>();
    }

    public class InstructorCourseListViewModel
    {
        public List<InstructorCourseItem> Courses { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class InstructorCourseItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string Price { get; set; } = string.Empty;
        public int ChapterCount { get; set; }
        public string UpdatedAt { get; set; } = string.Empty;
        // For edit modal
        public int CategoryId { get; set; }
        public decimal OriginalPrice { get; set; }
        public string CourseTypeValue { get; set; } = "Free";
    }

    public class CreateCourseViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? GroupTopic { get; set; }
        public string? Description { get; set; }
        public IFormFile? CoverImageFile { get; set; }
        public int CategoryId { get; set; } = 1;
        public string CourseType { get; set; } = "Free";
        public decimal OriginalPrice { get; set; }
        public decimal PromoPrice { get; set; }
    }

    public class GradingListViewModel
    {
        public List<SubmissionItem> UngradedSubmissions { get; set; } = new();
        public List<SubmissionItem> GradedSubmissions { get; set; } = new();
    }

    public class SubmissionItem
    {
        public int Id { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubmittedAgo { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class GradingDetailViewModel
    {
        public int SubmissionId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string? AssignmentDescription { get; set; }
        public decimal MaxScore { get; set; } = 10;
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? Content { get; set; }
        public string? SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        // Existing grade (for re-grading)
        public decimal? ExistingScore { get; set; }
        public string? ExistingComment { get; set; }
        public string? ExistingPassStatus { get; set; }
    }

    public class StudentProgressViewModel
    {
        public decimal CompletionRate { get; set; }
        public decimal OnTimeRate { get; set; }
        public List<TrainingMatrixRow> Matrix { get; set; } = new();
        // avgScoreChart
        public string[] ScoreLabels { get; set; } = Array.Empty<string>();
        public decimal[] AvgScores { get; set; } = Array.Empty<decimal>();
        public decimal[] MaxScores { get; set; } = Array.Empty<decimal>();
    }

    public class TrainingMatrixRow
    {
        public string StudentName { get; set; } = string.Empty;
        public List<string> LessonStatuses { get; set; } = new(); // "completed", "in_progress", "not_started"
    }

    public class SubmitGradeRequest
    {
        public int SubmissionId { get; set; }
        public decimal Score { get; set; }
        public string PassStatus { get; set; } = "Pass";
        public string? Comment { get; set; }
    }

    public class ReturnSubmissionRequest
    {
        public int SubmissionId { get; set; }
    }
}
