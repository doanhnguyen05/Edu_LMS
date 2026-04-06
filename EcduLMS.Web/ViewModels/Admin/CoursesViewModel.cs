using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;

namespace EduLMS.Web.ViewModels.Admin
{
    public class CoursesViewModel
    {
        public List<CourseItem> Courses { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();

        // Stats
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int DisabledCourses { get; set; }
        public int DraftCourses { get; set; }
        public int PendingReviewCourses { get; set; }

        // Filters
        public string? SearchQuery { get; set; }
        public CourseStatus? StatusFilter { get; set; }
        public int? CategoryFilter { get; set; }
        public string? InstructorFilter { get; set; }
    }

    public class CourseItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public CourseStatus Status { get; set; }
        public CourseType CourseType { get; set; }
        public decimal OriginalPrice { get; set; }
        public int EnrollmentCount { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
    }

    public class CourseReviewViewModel
    {
        public Course Course { get; set; } = null!;
    }
}
