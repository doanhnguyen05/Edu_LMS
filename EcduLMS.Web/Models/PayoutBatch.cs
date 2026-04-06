using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;

namespace EduLMS.Web.Models
{
    public class PayoutBatch
    {
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;  // VD: "Tháng 3/2026"
        public string InstructorId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public PayoutBatchStatus Status { get; set; } = PayoutBatchStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public ApplicationUser Admin { get; set; } = null!;
        public ApplicationUser Instructor { get; set; } = null!;
        public ICollection<InstructorEarning> Earnings { get; set; } = new List<InstructorEarning>();
    }
}
