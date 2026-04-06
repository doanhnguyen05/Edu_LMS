using EduLMS.Web.Data;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduLMS.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    [Authorize(Roles = "Learner")]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var now = DateTime.UtcNow;

            var enrolledCourseIds = await _db.Enrollments
                .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.CourseId)
                .ToListAsync();

            var upcoming = new List<CalendarEventItem>();

            if (enrolledCourseIds.Any())
            {
                var classEvents = await _db.Schedules
                    .Include(s => s.Course)
                    .Where(s => enrolledCourseIds.Contains(s.CourseId) && s.StartTime > now)
                    .OrderBy(s => s.StartTime)
                    .Take(10)
                    .ToListAsync();

                upcoming.AddRange(classEvents.Select(s => new CalendarEventItem
                {
                    Id = s.Id,
                    Title = s.Title,
                    CourseName = s.Course.Name,
                    ZoomLink = s.ZoomLink,
                    StartTime = s.StartTime.ToLocalTime(),
                    EndTime = s.EndTime.ToLocalTime(),
                    EventType = "Class"
                }));
            }

            var calEvents = await _db.CalendarEvents
                .Where(e => e.UserId == userId && e.StartTime > now)
                .OrderBy(e => e.StartTime)
                .Take(10)
                .ToListAsync();

            upcoming.AddRange(calEvents.Select(e => new CalendarEventItem
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime.ToLocalTime(),
                EndTime = e.EndTime.ToLocalTime(),
                EventType = e.EventType.ToString()
            }));

            var vm = new CalendarViewModel
            {
                UpcomingEvents = upcoming.OrderBy(e => e.StartTime).Take(10).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(string start, string end)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var startUtc = DateTimeOffset.Parse(start).UtcDateTime;
            var endUtc = DateTimeOffset.Parse(end).UtcDateTime;

            var enrolledCourseIds = await _db.Enrollments
                .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.CourseId)
                .ToListAsync();

            var result = new List<object>();

            if (enrolledCourseIds.Any())
            {
                var schedules = await _db.Schedules
                    .Include(s => s.Course)
                    .Where(s => enrolledCourseIds.Contains(s.CourseId)
                        && s.StartTime >= startUtc && s.StartTime <= endUtc)
                    .ToListAsync();

                result.AddRange(schedules.Select(s => (object)new
                {
                    id = $"s_{s.Id}",
                    title = s.Title,
                    start = s.StartTime.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = s.EndTime.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#2563EB",
                    extendedProps = new
                    {
                        eventType = "Class",
                        courseName = s.Course.Name,
                        zoomLink = s.ZoomLink,
                        description = s.Description
                    }
                }));
            }

            var calEvents = await _db.CalendarEvents
                .Where(e => e.UserId == userId && e.StartTime >= startUtc && e.EndTime <= endUtc)
                .ToListAsync();

            result.AddRange(calEvents.Select(e => (object)new
            {
                id = $"e_{e.Id}",
                title = e.Title,
                start = e.StartTime.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                end = e.EndTime.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                color = e.EventType == EventType.Assignment ? "#DC2626" : "#16A34A",
                extendedProps = new
                {
                    eventType = e.EventType.ToString(),
                    courseName = (string?)null,
                    zoomLink = (string?)null,
                    description = e.Description
                }
            }));

            return Json(result);
        }
    }
}
