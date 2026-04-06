using System.ComponentModel.DataAnnotations;

namespace EduLMS.Web.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        public int ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        public int DurationMinutes { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<LessonResource> Resources { get; set; } = new List<LessonResource>();
        public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
