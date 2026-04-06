using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;

namespace EduLMS.Web.Models
{
    public class InstructorEarning
    {
        public int Id { get; set; }
        public string InstructorId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int PaymentId { get; set; }
        public int? PayoutBatchId { get; set; }

        public decimal GrossAmount { get; set; }      // Tiền học viên trả
        public decimal PlatformFeeRate { get; set; }  // % platform thu (VD: 0.30)
        public decimal PlatformFee { get; set; }      // GrossAmount * PlatformFeeRate
        public decimal NetAmount { get; set; }        // GrossAmount - PlatformFee

        public EarningStatus Status { get; set; } = EarningStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Navigation
        public ApplicationUser Instructor { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public Payment Payment { get; set; } = null!;
        public PayoutBatch? PayoutBatch { get; set; }
    }
}
