using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduLMS.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── View ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var courses = await _db.Courses
                .Where(c => c.InstructorId == instructorId && c.Status != Models.Enums.CourseStatus.Disabled)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            ViewBag.Courses = courses;
            return View();
        }

        // ─── API: Get events for FullCalendar ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetEvents(string start, string end)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Parse with timezone awareness, then convert to UTC for DB comparison
            var startUtc = DateTimeOffset.Parse(start).UtcDateTime;
            var endUtc = DateTimeOffset.Parse(end).UtcDateTime;

            var schedules = await _db.Schedules
                .Include(s => s.Course)
                .Where(s => s.InstructorId == instructorId
                    && s.StartTime >= startUtc && s.StartTime <= endUtc)
                .ToListAsync();

            string[] palette = ["#2563EB", "#7C3AED", "#059669", "#D97706", "#DC2626", "#0891B2", "#BE185D"];
            var courseColorMap = new Dictionary<int, string>();
            int colorIdx = 0;
            foreach (var s in schedules)
            {
                if (!courseColorMap.ContainsKey(s.CourseId))
                    courseColorMap[s.CourseId] = palette[colorIdx++ % palette.Length];
            }

            var result = schedules.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                // Return local time so FullCalendar renders at correct hour
                start = s.StartTime.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                end = s.EndTime.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                color = courseColorMap.GetValueOrDefault(s.CourseId, "#2563EB"),
                extendedProps = new
                {
                    courseId = s.CourseId,
                    courseName = s.Course.Name,
                    description = s.Description,
                    zoomLink = s.ZoomLink
                }
            });

            return Json(result);
        }

        // ─── API: Upcoming sessions for sidebar ──────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetUpcoming()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var now = DateTime.UtcNow;

            string[] palette = ["#2563EB", "#7C3AED", "#059669", "#D97706", "#DC2626", "#0891B2", "#BE185D"];
            var allCourseIds = await _db.Courses
                .Where(c => c.InstructorId == instructorId)
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .ToListAsync();

            var colorMap = allCourseIds
                .Select((id, idx) => new { id, color = palette[idx % palette.Length] })
                .ToDictionary(x => x.id, x => x.color);

            var upcoming = await _db.Schedules
                .Include(s => s.Course)
                .Where(s => s.InstructorId == instructorId && s.StartTime > now)
                .OrderBy(s => s.StartTime)
                .Take(8)
                .ToListAsync();

            var result = upcoming.Select(s =>
            {
                var localStart = s.StartTime.ToLocalTime();
                var localEnd = s.EndTime.ToLocalTime();
                return new
                {
                    id = s.Id,
                    title = s.Title,
                    courseName = s.Course.Name,
                    start = localStart.ToString("HH:mm"),
                    end = localEnd.ToString("HH:mm"),
                    date = localStart.ToString("dd/MM"),
                    dayLabel = localStart.Date == DateTime.Today ? "Hôm nay"
                        : localStart.Date == DateTime.Today.AddDays(1) ? "Ngày mai"
                        : localStart.ToString("dd/MM"),
                    zoomLink = s.ZoomLink,
                    color = colorMap.GetValueOrDefault(s.CourseId, "#2563EB")
                };
            });

            return Json(result);
        }

        // ─── API: Get single schedule for edit ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var schedule = await _db.Schedules
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);

            if (schedule == null) return NotFound();

            var local = schedule.StartTime.ToLocalTime();
            return Json(new
            {
                id = schedule.Id,
                courseId = schedule.CourseId,
                title = schedule.Title,
                description = schedule.Description,
                date = local.ToString("yyyy-MM-dd"),
                startTime = local.ToString("HH:mm"),
                endTime = schedule.EndTime.ToLocalTime().ToString("HH:mm"),
                zoomLink = schedule.ZoomLink
            });
        }

        // ─── Create ───────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int courseId, string title, string date,
            string startTime, string endTime,
            string? description, string? zoomLink,
            int recurrenceWeeks = 1)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var course = await _db.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            if (course == null)
                return Json(new { success = false, message = "Khóa học không hợp lệ." });

            if (!DateTime.TryParse($"{date}T{startTime}", out var start) ||
                !DateTime.TryParse($"{date}T{endTime}", out var end))
                return Json(new { success = false, message = "Thời gian không hợp lệ." });

            if (end <= start)
                return Json(new { success = false, message = "Giờ kết thúc phải sau giờ bắt đầu." });

            recurrenceWeeks = Math.Max(1, Math.Min(52, recurrenceWeeks));
            var startUtc = start.ToUniversalTime();
            var endUtc = end.ToUniversalTime();

            for (int i = 0; i < recurrenceWeeks; i++)
            {
                _db.Schedules.Add(new Schedule
                {
                    CourseId = courseId,
                    InstructorId = instructorId,
                    Title = title,
                    Description = description,
                    StartTime = startUtc.AddDays(7 * i),
                    EndTime = endUtc.AddDays(7 * i),
                    ZoomLink = zoomLink,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, count = recurrenceWeeks });
        }

        // ─── Edit ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, int courseId, string title, string date,
            string startTime, string endTime,
            string? description, string? zoomLink)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var schedule = await _db.Schedules
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);

            if (schedule == null) return Json(new { success = false, message = "Không tìm thấy lịch." });

            if (!DateTime.TryParse($"{date}T{startTime}", out var start) ||
                !DateTime.TryParse($"{date}T{endTime}", out var end))
                return Json(new { success = false, message = "Thời gian không hợp lệ." });

            if (end <= start)
                return Json(new { success = false, message = "Giờ kết thúc phải sau giờ bắt đầu." });

            schedule.CourseId = courseId;
            schedule.Title = title;
            schedule.Description = description;
            schedule.StartTime = start.ToUniversalTime();
            schedule.EndTime = end.ToUniversalTime();
            schedule.ZoomLink = zoomLink;

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ─── Delete ───────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var schedule = await _db.Schedules
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);

            if (schedule == null) return Json(new { success = false });

            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
