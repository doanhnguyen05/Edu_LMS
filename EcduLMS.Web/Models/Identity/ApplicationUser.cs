using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EduLMS.Web.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MaxLength(2000)]
        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Instructor payout settings
        public decimal CommissionRate { get; set; } = 0.30m; // Platform takes 30% by default
        [MaxLength(100)]
        public string? BankName { get; set; }
        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }
        [MaxLength(200)]
        public string? BankAccountName { get; set; }

        // Navigation properties
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Course> InstructorCourses { get; set; } = new List<Course>();
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}
