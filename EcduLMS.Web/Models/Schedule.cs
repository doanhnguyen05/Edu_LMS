using EduLMS.Web.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace EduLMS.Web.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public string InstructorId { get; set; } = string.Empty;
        public ApplicationUser Instructor { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [MaxLength(500)]
        public string? ZoomLink { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
