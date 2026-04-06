using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.Services;
using EduLMS.Web.ViewModels.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    [Authorize(Roles = "Learner")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ConflictDetectionService _conflictService;

        public PaymentController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IConfiguration config, ConflictDetectionService conflictService)
        {
            _db = db;
            _userManager = userManager;
            _config = config;
            _conflictService = conflictService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Training");
        }

        public async Task<IActionResult> Checkout(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            // Check if already enrolled
            var alreadyEnrolled = await _db.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);
            if (alreadyEnrolled)
            {
                TempData["InfoMessage"] = "Bạn đã đăng ký khóa học này rồi.";
                return RedirectToAction("Index", "Training");
            }

            var course = await _db.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            // Check schedule conflicts before enrollment
            var conflicts = await _conflictService.DetectAsync(userId, id);
            if (conflicts.Any())
            {
                var conflictModel = new CheckoutViewModel
                {
                    CourseId = course.Id,
                    CourseName = course.Name,
                    InstructorName = course.Instructor.FullName,
                    CoverImageUrl = course.CoverImageUrl,
                    OriginalPrice = course.OriginalPrice,
                    PromoPrice = course.PromoPrice,
                    FinalPrice = course.PromoPrice ?? course.OriginalPrice,
                    Conflicts = conflicts
                };
                return View("Conflict", conflictModel);
            }

            // Free course → enroll directly
            if (course.CourseType == CourseType.Free)
            {
                _db.Enrollments.Add(new Enrollment
                {
                    UserId = userId,
                    CourseId = id,
                    Status = EnrollmentStatus.Active,
                    EnrolledAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đăng ký khóa học miễn phí thành công!";
                return RedirectToAction("Index", "Training");
            }

            // Check for existing pending payment
            var existingPayment = await _db.Payments
                .Where(p => p.UserId == userId && p.CourseId == id && p.Status == PaymentStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            // Reuse or create new payment
            var finalPrice = course.PromoPrice ?? course.OriginalPrice;
            Payment payment;

            if (existingPayment != null && existingPayment.CreatedAt > DateTime.UtcNow.AddMinutes(-15))
            {
                payment = existingPayment;
            }
            else
            {
                // Generate unique order code: EDLMS + timestamp
                var orderCode = "EDLMS" + DateTime.UtcNow.ToString("yyMMddHHmmss") + new Random().Next(100, 999);

                payment = new Payment
                {
                    UserId = userId,
                    CourseId = id,
                    Amount = finalPrice,
                    PaymentMethod = PaymentMethod.QRCode,
                    TransactionCode = orderCode,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();
            }

            // Build VietQR URL
            var bankBIN = _config["VietQR:BankBIN"] ?? "970422";
            var accountNo = _config["VietQR:AccountNo"] ?? "0123456789";
            var accountName = _config["VietQR:AccountName"] ?? "CONG TY EDULMS";
            var template = _config["VietQR:Template"] ?? "compact2";
            var amount = (int)payment.Amount;
            var addInfo = payment.TransactionCode;

            var qrUrl = $"https://img.vietqr.io/image/{bankBIN}-{accountNo}-{template}.png?amount={amount}&addInfo={Uri.EscapeDataString(addInfo!)}&accountName={Uri.EscapeDataString(accountName)}";

            var model = new CheckoutViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name,
                InstructorName = course.Instructor.FullName,
                CoverImageUrl = course.CoverImageUrl,
                OriginalPrice = course.OriginalPrice,
                PromoPrice = course.PromoPrice,
                FinalPrice = finalPrice,
                PaymentId = payment.Id,
                OrderCode = payment.TransactionCode!,
                QrImageUrl = qrUrl,
                BankName = GetBankName(_config["VietQR:BankBIN"] ?? ""),
                AccountNo = accountNo,
                AccountName = accountName
            };

            return View(model);
        }

        /// <summary>
        /// API endpoint for frontend to poll payment status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckStatus(int paymentId)
        {
            var userId = _userManager.GetUserId(User)!;
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

            if (payment == null)
                return Json(new { status = "not_found" });

            return Json(new { status = payment.Status.ToString().ToLower() });
        }

        /// <summary>
        /// Success page after payment confirmed
        /// </summary>
        public async Task<IActionResult> Success(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var payment = await _db.Payments
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.Status == PaymentStatus.Completed);

            if (payment == null)
                return RedirectToAction("Index", "Training");

            ViewBag.CourseName = payment.Course.Name;
            ViewBag.CourseId = payment.CourseId;
            return View();
        }

        private static string GetBankName(string bankBIN) => bankBIN switch
        {
            "970418" => "BIDV",
            "970422" => "MB Bank",
            "970436" => "Vietcombank",
            "970415" => "VietinBank",
            "970407" => "Techcombank",
            "970416" => "ACB",
            "970432" => "VPBank",
            "970423" => "TPBank",
            _ => "Ngân hàng"
        };
    }
}
