using System.ComponentModel.DataAnnotations;

namespace EduLMS.Web.Models
{
    public class LessonResource
    {
        public int Id { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(10)]
        public string FileType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
