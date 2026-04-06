using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var adminId = _userManager.GetUserId(User)!;

            var rules = await _context.NotificationRules
                .OrderBy(r => r.Id)
                .Select(r => new NotificationRuleItem
                {
                    Id = r.Id,
                    DisplayName = r.DisplayName,
                    Description = r.Description ?? "",
                    IsEnabled = r.IsEnabled,
                    Channel = r.Channel.ToString(),
                    TemplateBody = r.TemplateBody
                })
                .ToListAsync();

            // Sent broadcasts history
            var sentRaw = await _context.Notifications
                .Include(n => n.Course)
                .Where(n => n.SenderAdminId == adminId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var broadcasts = sentRaw
                .GroupBy(n => new { n.Title, Minute = new DateTime(n.CreatedAt.Year, n.CreatedAt.Month, n.CreatedAt.Day, n.CreatedAt.Hour, n.CreatedAt.Minute, 0) })
                .Select(g => new AdminBroadcastGroup
                {
                    Title = g.Key.Title,
                    Message = g.First().Message,
                    TargetLabel = GetTargetLabel(g.First().Type, g.First().Course?.Name),
                    SentAt = g.Key.Minute,
                    RecipientCount = g.Count()
                })
                .ToList();

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .OrderBy(c => c.Name)
                .Select(c => new AdminCourseOption
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    InstructorName = c.Instructor.FullName
                })
                .ToListAsync();

            var vm = new NotificationRulesViewModel
            {
                Rules = rules,
                SentBroadcasts = broadcasts,
                Courses = courses
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string title, string message, string target, int? courseId)
        {
            var adminId = _userManager.GetUserId(User)!;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Tiêu đề và nội dung không được để trống.";
                return RedirectToAction(nameof(Index), new { tab = "broadcast" });
            }

            List<string> recipientIds;
            string type;

            switch (target)
            {
                case "learners":
                    var learners = await _userManager.GetUsersInRoleAsync("Learner");
                    recipientIds = learners.Select(u => u.Id).ToList();
                    type = "admin_broadcast_learner";
                    break;

                case "instructors":
                    var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
                    recipientIds = instructors.Select(u => u.Id).ToList();
                    type = "admin_broadcast_instructor";
                    break;

                case "course":
                    if (!courseId.HasValue)
                    {
                        TempData["Error"] = "Vui lòng chọn khoá học.";
                        return RedirectToAction(nameof(Index), new { tab = "broadcast" });
                    }
                    recipientIds = await _context.Enrollments
                        .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
                        .Select(e => e.UserId)
                        .Distinct()
                        .ToListAsync();
                    type = "admin_broadcast_course";
                    break;

                default: // "all"
                    recipientIds = await _context.Users.Select(u => u.Id).ToListAsync();
                    type = "admin_broadcast_all";
                    break;
            }

            if (!recipientIds.Any())
            {
                TempData["Error"] = "Không có người dùng nào phù hợp để gửi thông báo.";
                return RedirectToAction(nameof(Index), new { tab = "broadcast" });
            }

            var now = DateTime.UtcNow;
            var notifications = recipientIds.Select(uid => new Notification
            {
                UserId = uid,
                Title = title,
                Message = message,
                Type = type,
                Channel = NotificationChannel.InApp,
                IsRead = false,
                CreatedAt = now,
                SenderAdminId = adminId,
                CourseId = target == "course" ? courseId : null
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã gửi thông báo đến {notifications.Count} người dùng.";
            return RedirectToAction(nameof(Index), new { tab = "broadcast" });
        }

        private static string GetTargetLabel(string type, string? courseName) => type switch
        {
            "admin_broadcast_learner"    => "Tất cả học viên",
            "admin_broadcast_instructor" => "Tất cả giảng viên",
            "admin_broadcast_course"     => $"Khoá: {courseName ?? "?"}",
            "admin_broadcast_all"        => "Tất cả người dùng",
            _                            => type
        };
    }
}
