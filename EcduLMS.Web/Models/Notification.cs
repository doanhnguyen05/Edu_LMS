using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace EduLMS.Web.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Instructor announcements
        public string? SenderInstructorId { get; set; }
        public ApplicationUser? SenderInstructor { get; set; }

        // Admin broadcasts
        public string? SenderAdminId { get; set; }
        public ApplicationUser? SenderAdmin { get; set; }

        public int? CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
