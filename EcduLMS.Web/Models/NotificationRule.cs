using EduLMS.Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduLMS.Web.Models
{
    public class NotificationRule
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsEnabled { get; set; } = true;

        public NotificationChannel Channel { get; set; } = NotificationChannel.Both;

        [MaxLength(5000)]
        public string TemplateBody { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
