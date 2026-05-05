using System.Text.Json.Serialization;
using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Controllers
{
    /// <summary>
    /// Public webhook endpoint for SePay payment notifications.
    /// No authentication required - SePay calls this when a bank transfer arrives.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(ApplicationDbContext db, IConfiguration config, ILogger<WebhookController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// SePay webhook: POST /api/webhook/sepay
        /// SePay sends transaction data when a bank transfer is received.
        /// Must respond with {"success": true} within 5 seconds.
        /// </summary>
        [HttpGet("sepay")]
        public IActionResult SePayHealth()
        {
            return Ok(new
            {
                success = true,
                endpoint = "/api/webhook/sepay",
                method = "POST",
                status = "ready"
            });
        }

        [HttpPost("sepay")]
        public async Task<IActionResult> SePay([FromBody] SePayWebhookPayload payload)
        {
            _logger.LogInformation(
                "SePay webhook received: Content={Content}, Description={Description}, ReferenceCode={ReferenceCode}, Amount={Amount}, Account={AccountNumber}, Type={TransferType}",
                payload?.Content,
                payload?.Description,
                payload?.ReferenceCode,
                payload?.TransferAmount,
                payload?.AccountNumber,
                payload?.TransferType);

            if (payload == null)
            {
                _logger.LogWarning("SePay webhook payload is null.");
                return Ok(new { success = true });
            }

            // Extract order code from transfer content (e.g., "EDLMS250318143521123")
            // SePay sends the transfer description which should contain our order code
            var searchableContent = string.Join(" ", new[]
                {
                    payload.Content,
                    payload.Description,
                    payload.ReferenceCode
                }
                .Where(value => !string.IsNullOrWhiteSpace(value)))
                .ToUpperInvariant()
                .Trim();

            if (string.IsNullOrWhiteSpace(searchableContent))
            {
                _logger.LogWarning("SePay webhook does not contain content/description/referenceCode.");
                return Ok(new { success = true });
            }

            // Find payment by transaction code in the transfer content
            var payment = await _db.Payments
                .Include(p => p.Course)
                .Where(p => p.Status == PaymentStatus.Pending
                    && p.TransactionCode != null
                    && searchableContent.Contains(p.TransactionCode.ToUpper()))
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                _logger.LogWarning("No matching pending payment found for webhook searchable content: {Content}", searchableContent);
                return Ok(new { success = true });
            }

            // Verify amount matches
            if ((payload.TransferAmount ?? 0) < payment.Amount)
            {
                _logger.LogWarning("Amount mismatch: expected {Expected}, got {Actual} for payment {PaymentId}",
                    payment.Amount, payload.TransferAmount, payment.Id);
                return Ok(new { success = true });
            }

            // Mark payment as completed
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;

            // Create enrollment
            var alreadyEnrolled = await _db.Enrollments
                .AnyAsync(e => e.UserId == payment.UserId && e.CourseId == payment.CourseId);

            if (!alreadyEnrolled)
            {
                _db.Enrollments.Add(new Enrollment
                {
                    UserId = payment.UserId,
                    CourseId = payment.CourseId,
                    Status = EnrollmentStatus.Active,
                    EnrolledAt = DateTime.UtcNow
                });
            }

            // Create instructor earning record
            var instructor = await _db.Users.FindAsync(payment.Course.InstructorId);
            if (instructor != null && payment.Amount > 0)
            {
                var rate = instructor.CommissionRate; // platform fee rate
                var platformFee = Math.Round(payment.Amount * rate, 0);
                _db.InstructorEarnings.Add(new InstructorEarning
                {
                    InstructorId = instructor.Id,
                    CourseId = payment.CourseId,
                    PaymentId = payment.Id,
                    GrossAmount = payment.Amount,
                    PlatformFeeRate = rate,
                    PlatformFee = platformFee,
                    NetAmount = payment.Amount - platformFee,
                    Status = EarningStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} completed. User {UserId} enrolled in course {CourseId}",
                payment.Id, payment.UserId, payment.CourseId);

            return Ok(new { success = true });
        }
    }

    /// <summary>
    /// SePay webhook payload structure
    /// </summary>
    public class SePayWebhookPayload
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("gateway")]
        public string? Gateway { get; set; }

        [JsonPropertyName("transactionDate")]
        public string? TransactionDate { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("subAccount")]
        public string? SubAccount { get; set; }

        [JsonPropertyName("transferAmount")]
        public decimal? TransferAmount { get; set; }

        [JsonPropertyName("transferType")]
        public string? TransferType { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonPropertyName("referenceCode")]
        public string? ReferenceCode { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("accumulated")]
        public decimal? Accumulated { get; set; }
    }
}
