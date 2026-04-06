using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace EduLMS.Web.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [MaxLength(10000)]
        public string? Content { get; set; }

        [MaxLength(500)]
        public string? FileUrl { get; set; }

        [MaxLength(300)]
        public string? FileName { get; set; }

        public long? FileSizeBytes { get; set; }

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

        public DateTime? SubmittedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Grade? Grade { get; set; }
    }
}
