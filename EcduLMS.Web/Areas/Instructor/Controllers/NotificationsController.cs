using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var courses = await _db.Courses
                .Where(c => c.InstructorId == userId)
                .OrderBy(c => c.Name)
                .Select(c => new CourseOption { Id = c.Id, Name = c.Name, Code = c.Code })
                .ToListAsync();

            // Group sent notifications by (Title, CreatedAt minute) to avoid duplicates
            var sentRaw = await _db.Notifications
                .Include(n => n.Course)
                .Where(n => n.SenderInstructorId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var groups = sentRaw
                .GroupBy(n => new { n.Title, Minute = new DateTime(n.CreatedAt.Year, n.CreatedAt.Month, n.CreatedAt.Day, n.CreatedAt.Hour, n.CreatedAt.Minute, 0) })
                .Select(g => new SentNotificationGroup
                {
                    Title = g.Key.Title,
                    Message = g.First().Message,
                    CourseName = g.First().Course?.Name,
                    SentAt = g.Key.Minute,
                    RecipientCount = g.Count()
                })
                .ToList();

            var vm = new InstructorNotificationsViewModel
            {
                Courses = courses,
                SentNotifications = groups
            };

            return View(vm);
        }

        public async Task<IActionResult> Inbox()
        {
            var userId = _userManager.GetUserId(User)!;
            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Mark all as read
            var unread = notifications.Where(n => !n.IsRead).ToList();
            if (unread.Any())
            {
                unread.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
                await _db.SaveChangesAsync();
            }

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string title, string message, int? courseId)
        {
            var userId = _userManager.GetUserId(User)!;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Tiêu đề và nội dung không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            // Get recipient user IDs
            List<string> recipientIds;
            if (courseId.HasValue)
            {
                // Verify instructor owns this course
                var owns = await _db.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == userId);
                if (!owns)
                {
                    TempData["Error"] = "Không tìm thấy khoá học.";
                    return RedirectToAction(nameof(Index));
                }

                recipientIds = await _db.Enrollments
                    .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
                    .Select(e => e.UserId)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                // All students enrolled in any of this instructor's courses
                var instructorCourseIds = await _db.Courses
                    .Where(c => c.InstructorId == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                recipientIds = await _db.Enrollments
                    .Where(e => instructorCourseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Active)
                    .Select(e => e.UserId)
                    .Distinct()
                    .ToListAsync();
            }

            if (!recipientIds.Any())
            {
                TempData["Error"] = "Không có học viên nào để gửi thông báo.";
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.UtcNow;
            var notifications = recipientIds.Select(uid => new Notification
            {
                UserId = uid,
                Title = title,
                Message = message,
                Type = "instructor_announcement",
                Channel = NotificationChannel.InApp,
                IsRead = false,
                CreatedAt = now,
                SenderInstructorId = userId,
                CourseId = courseId
            }).ToList();

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã gửi thông báo đến {notifications.Count} học viên.";
            return RedirectToAction(nameof(Index));
        }
    }
}
