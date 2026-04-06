using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduLMS.Web.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        public int CategoryId { get; set; }
        public CourseCategory Category { get; set; } = null!;

        public string InstructorId { get; set; } = string.Empty;
        public ApplicationUser Instructor { get; set; } = null!;

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DurationHours { get; set; }

        public CourseType CourseType { get; set; } = CourseType.Free;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PromoPrice { get; set; }

        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        [Column(TypeName = "decimal(3,1)")]
        public decimal AverageRating { get; set; }

        public int RatingCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }

        // Navigation
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
