using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PayoutsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PayoutsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Admin/Payouts — list instructors with pending earnings
        public async Task<IActionResult> Index(string? month)
        {
            // Get all instructors who have earnings
            var instructorIds = await _db.InstructorEarnings
                .Select(e => e.InstructorId)
                .Distinct()
                .ToListAsync();

            var instructors = await _db.Users
                .Where(u => instructorIds.Contains(u.Id))
                .ToListAsync();

            var summaries = new List<InstructorPayoutSummary>();

            foreach (var ins in instructors)
            {
                var pending = await _db.InstructorEarnings
                    .Where(e => e.InstructorId == ins.Id && e.Status == EarningStatus.Pending && e.PayoutBatchId == null)
                    .ToListAsync();

                var paid = await _db.InstructorEarnings
                    .Where(e => e.InstructorId == ins.Id && e.Status == EarningStatus.Paid)
                    .ToListAsync();

                var gross = pending.Sum(e => e.GrossAmount);
                var fee = Math.Round(gross * ins.CommissionRate, 0);
                summaries.Add(new InstructorPayoutSummary
                {
                    InstructorId = ins.Id,
                    FullName = ins.FullName,
                    Email = ins.Email ?? "",
                    BankName = ins.BankName,
                    BankAccountNumber = ins.BankAccountNumber,
                    BankAccountName = ins.BankAccountName,
                    CommissionRate = ins.CommissionRate,
                    PendingGross = gross,
                    PendingPlatformFee = fee,
                    PendingAmount = gross - fee,
                    PendingCount = pending.Count,
                    TotalPaidAmount = paid.Sum(e => e.GrossAmount) - Math.Round(paid.Sum(e => e.GrossAmount) * ins.CommissionRate, 0)
                });
            }

            // Recent payout batches
            var batches = await _db.PayoutBatches
                .Include(b => b.Instructor)
                .Include(b => b.Admin)
                .OrderByDescending(b => b.CreatedAt)
                .Take(20)
                .ToListAsync();

            ViewBag.Batches = batches;

            return View(summaries);
        }

        // POST: /Admin/Payouts/CreateBatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBatch(string instructorId, string note)
        {
            var adminId = _userManager.GetUserId(User)!;

            var earnings = await _db.InstructorEarnings
                .Where(e => e.InstructorId == instructorId && e.Status == EarningStatus.Pending && e.PayoutBatchId == null)
                .ToListAsync();

            if (!earnings.Any())
            {
                TempData["ErrorMessage"] = "Không có khoản thu nhập chờ xử lý.";
                return RedirectToAction(nameof(Index));
            }

            var total = earnings.Sum(e => e.NetAmount);

            var batch = new PayoutBatch
            {
                AdminId = adminId,
                InstructorId = instructorId,
                Note = note,
                TotalAmount = total,
                Status = PayoutBatchStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            _db.PayoutBatches.Add(batch);
            await _db.SaveChangesAsync();

            foreach (var e in earnings)
                e.PayoutBatchId = batch.Id;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo đợt thanh toán: {note} — {total:N0}đ";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Payouts/Complete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var batch = await _db.PayoutBatches
                .Include(b => b.Earnings)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null) return NotFound();

            batch.Status = PayoutBatchStatus.Completed;
            batch.CompletedAt = DateTime.UtcNow;

            foreach (var e in batch.Earnings)
            {
                e.Status = EarningStatus.Paid;
                e.PaidAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh dấu thanh toán hoàn tất.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Payouts/UpdateInstructorBank
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInstructorBank(string instructorId, string bankName, string bankAccountNumber, string bankAccountName, decimal commissionRate)
        {
            var user = await _userManager.FindByIdAsync(instructorId);
            if (user == null) return NotFound();

            user.BankName = bankName;
            user.BankAccountNumber = bankAccountNumber;
            user.BankAccountName = bankAccountName;
            user.CommissionRate = commissionRate;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Đã cập nhật thông tin giảng viên.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class InstructorPayoutSummary
    {
        public string InstructorId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal PendingGross { get; set; }
        public decimal PendingPlatformFee { get; set; }
        public decimal PendingAmount { get; set; }   // net
        public int PendingCount { get; set; }
        public decimal TotalPaidAmount { get; set; }
    }
}
