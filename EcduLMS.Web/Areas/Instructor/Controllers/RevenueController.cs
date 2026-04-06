using EduLMS.Web.Data;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class RevenueController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RevenueController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.FindByIdAsync(userId);

            var earnings = await _db.InstructorEarnings
                .Include(e => e.Course)
                .Include(e => e.PayoutBatch)
                .Where(e => e.InstructorId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var batches = await _db.PayoutBatches
                .Where(b => b.InstructorId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.TotalEarned = earnings.Where(e => e.Status == EarningStatus.Paid).Sum(e => e.NetAmount);
            ViewBag.PendingAmount = earnings.Where(e => e.Status == EarningStatus.Pending).Sum(e => e.NetAmount);
            ViewBag.PendingCount = earnings.Count(e => e.Status == EarningStatus.Pending);
            ViewBag.TotalSales = earnings.Count;
            ViewBag.CommissionRate = user?.CommissionRate ?? 0.30m;
            ViewBag.BankName = user?.BankName;
            ViewBag.BankAccountNumber = user?.BankAccountNumber;
            ViewBag.BankAccountName = user?.BankAccountName;
            ViewBag.Batches = batches;

            return View(earnings);
        }
    }
}
