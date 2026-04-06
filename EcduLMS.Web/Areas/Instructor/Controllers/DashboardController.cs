using EduLMS.Web.Data;
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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);

            var courseIds = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Select(c => c.Id)
                .ToListAsync();

            var activeStudents = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Active)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();

            var assignedCourses = courseIds.Count;

            var newCoursesThisWeek = await _context.Courses
                .Where(c => c.InstructorId == userId && c.CreatedAt >= weekAgo)
                .CountAsync();

            var thisWeekEnrollments = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.EnrolledAt >= weekAgo)
                .CountAsync();

            var prevWeekEnrollments = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.EnrolledAt >= weekAgo.AddDays(-7) && e.EnrolledAt < weekAgo)
                .CountAsync();

            var growth = prevWeekEnrollments > 0
                ? Math.Round((decimal)(thisWeekEnrollments - prevWeekEnrollments) / prevWeekEnrollments * 100, 1)
                : (thisWeekEnrollments > 0 ? 100m : 0m);

            // --- Chart data ---
            var today = now.Date;
            var last7 = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
            var cutoff = last7.First();

            // Weekly submissions per day (submitted/graded)
            var submittedDates = await _context.AssignmentSubmissions
                .Include(s => s.Assignment).ThenInclude(a => a.Chapter)
                .Where(s => courseIds.Contains(s.Assignment.Chapter.CourseId)
                         && s.Status != SubmissionStatus.Draft
                         && s.SubmittedAt != null && s.SubmittedAt >= cutoff)
                .Select(s => s.SubmittedAt!.Value.Date)
                .ToListAsync();

            // Weekly new enrollments per day
            var enrollDates = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.EnrolledAt >= cutoff)
                .Select(e => e.EnrolledAt.Date)
                .ToListAsync();

            var weeklySubmissions = last7.Select(d => submittedDates.Count(x => x == d)).ToArray();
            var weeklyEnrollments = last7.Select(d => enrollDates.Count(x => x == d)).ToArray();
            var trendLabels = last7.Select(d => VnDayName(d.DayOfWeek)).ToArray();

            // Score comparison per course (avg & max grades, normalized to 10)
            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            var allGrades = await _context.Grades
                .Include(g => g.Submission).ThenInclude(s => s.Assignment).ThenInclude(a => a.Chapter)
                .Where(g => courseIds.Contains(g.Submission.Assignment.Chapter.CourseId))
                .Select(g => new {
                    CourseId = g.Submission.Assignment.Chapter.CourseId,
                    MaxScore = g.Submission.Assignment.MaxScore,
                    Score = g.Score
                })
                .ToListAsync();

            var courseLabels = courses.Select(c => c.Name.Length > 15 ? c.Name[..15] + "…" : c.Name).ToArray();
            var courseAvg = courses.Select(c => {
                var gs = allGrades.Where(g => g.CourseId == c.Id).ToList();
                if (!gs.Any()) return 0m;
                return Math.Round(gs.Average(g => g.MaxScore > 0 ? g.Score / g.MaxScore * 10 : g.Score), 1);
            }).ToArray();
            var courseMax = courses.Select(c => {
                var gs = allGrades.Where(g => g.CourseId == c.Id).ToList();
                if (!gs.Any()) return 0m;
                return Math.Round(gs.Max(g => g.MaxScore > 0 ? g.Score / g.MaxScore * 10 : g.Score), 1);
            }).ToArray();

            var model = new InstructorDashboardViewModel
            {
                ActiveStudents = activeStudents,
                AssignedCourses = assignedCourses,
                NewCoursesThisWeek = newCoursesThisWeek,
                StudentGrowthPercent = growth,
                TrendLabels = trendLabels,
                WeeklySubmissions = weeklySubmissions,
                WeeklyEnrollments = weeklyEnrollments,
                CourseLabels = courseLabels,
                CourseAvgScores = courseAvg,
                CourseMaxScores = courseMax
            };

            return View(model);
        }

        private static string VnDayName(DayOfWeek d) => d switch
        {
            DayOfWeek.Monday => "T2",
            DayOfWeek.Tuesday => "T3",
            DayOfWeek.Wednesday => "T4",
            DayOfWeek.Thursday => "T5",
            DayOfWeek.Friday => "T6",
            DayOfWeek.Saturday => "T7",
            _ => "CN"
        };
    }
}
