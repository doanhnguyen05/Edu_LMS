using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduLMS.Web.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;

        public int? LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime? DueDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxScore { get; set; } = 10;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
