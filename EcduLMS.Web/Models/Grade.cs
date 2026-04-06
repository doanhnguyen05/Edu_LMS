using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduLMS.Web.Models
{
    public class Grade
    {
        public int Id { get; set; }

        public int SubmissionId { get; set; }
        public AssignmentSubmission Submission { get; set; } = null!;

        public string GradedById { get; set; } = string.Empty;
        public ApplicationUser GradedBy { get; set; } = null!;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Score { get; set; }

        public PassStatus PassStatus { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        public DateTime GradedAt { get; set; } = DateTime.UtcNow;
    }
}
