using EduLMS.Web.Data;
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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId ?? "");

            var enrollments = await _db.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var totalCourses = enrollments.Count;
            var completedCourses = enrollments.Count(e => e.Status == Models.Enums.EnrollmentStatus.Completed);

            // Find the most recent in-progress enrollment for resume
            var resumeEnrollment = enrollments
                .Where(e => e.Status == Models.Enums.EnrollmentStatus.Active)
                .OrderByDescending(e => e.ProgressPercent)
                .ThenByDescending(e => e.EnrolledAt)
                .FirstOrDefault();

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new NotificationItem
                {
                    Icon = n.Type == "course" ? "bi-journal-bookmark" : n.Type == "grade" ? "bi-award" : "bi-bell",
                    IconColor = n.Type == "course" ? "#2563EB" : n.Type == "grade" ? "#16A34A" : "#F59E0B",
                    Message = n.Title,
                    Time = n.CreatedAt.ToString("dd/MM HH:mm")
                })
                .ToListAsync();

            // Derive activities from real data sources
            var enrollActs = await _db.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .Take(10)
                .Select(e => new ActivityItem
                {
                    Title = "Đăng ký khóa học",
                    Detail = e.Course != null ? e.Course.Name : "",
                    SortKey = e.EnrolledAt,
                    Time = e.EnrolledAt.ToString("dd/MM HH:mm")
                })
                .ToListAsync();

            var lessonActs = await _db.LessonProgresses
                .Include(lp => lp.Lesson)
                .Where(lp => lp.UserId == userId && lp.CompletedAt != null)
                .OrderByDescending(lp => lp.CompletedAt)
                .Take(10)
                .Select(lp => new ActivityItem
                {
                    Title = "Hoàn thành bài học",
                    Detail = lp.Lesson != null ? lp.Lesson.Title : "",
                    SortKey = lp.CompletedAt!.Value,
                    Time = lp.CompletedAt!.Value.ToString("dd/MM HH:mm")
                })
                .ToListAsync();

            var submissionActs = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Where(s => s.UserId == userId && s.SubmittedAt != null)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(10)
                .Select(s => new ActivityItem
                {
                    Title = "Nộp bài tập",
                    Detail = s.Assignment != null ? s.Assignment.Title : "",
                    SortKey = s.SubmittedAt!.Value,
                    Time = s.SubmittedAt!.Value.ToString("dd/MM HH:mm")
                })
                .ToListAsync();

            var activities = enrollActs.Concat(lessonActs).Concat(submissionActs)
                .OrderByDescending(a => a.SortKey)
                .Take(5)
                .ToList();

            // --- Chart data ---
            var today = DateTime.UtcNow.Date;
            var last7 = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
            var cutoff = last7.First();

            // Study activity: count lesson completions per day
            var completedAtDates = await _db.LessonProgresses
                .Where(lp => lp.UserId == userId && lp.CompletedAt != null && lp.CompletedAt >= cutoff)
                .Select(lp => lp.CompletedAt!.Value.Date)
                .ToListAsync();

            var studyData = last7.Select(d => completedAtDates.Count(x => x == d)).ToArray();
            var studyLabels = last7.Select(d => VnDayName(d.DayOfWeek)).ToArray();

            // Grade chart: last 6 graded assignments, normalized to 10
            var recentGrades = await _db.Grades
                .Include(g => g.Submission).ThenInclude(s => s.Assignment)
                .Where(g => g.Submission.UserId == userId)
                .OrderBy(g => g.GradedAt)
                .Take(6)
                .ToListAsync();

            var gradeLabels = recentGrades.Select(g => {
                var t = g.Submission?.Assignment?.Title ?? "BT";
                return t.Length > 10 ? t[..10] + "…" : t;
            }).ToArray();
            var gradeScores = recentGrades.Select(g => {
                var maxScore = g.Submission?.Assignment?.MaxScore ?? 0;
                return maxScore > 0 ? Math.Round(g.Score / maxScore * 10, 1) : Math.Round(g.Score, 1);
            }).ToArray();

            var model = new LearnerDashboardViewModel
            {
                UserName = user?.FullName ?? "Learner",
                TotalCourses = totalCourses,
                CompletedCourses = completedCourses,
                ResumeCourseName = resumeEnrollment?.Course?.Name,
                ResumeLessonTitle = null,
                ResumeProgress = resumeEnrollment != null ? (int)resumeEnrollment.ProgressPercent : 0,
                ResumeCourseId = resumeEnrollment?.CourseId ?? 0,
                ResumeLessonId = 0,
                Notifications = notifications,
                Activities = activities,
                StudyChartLabels = studyLabels,
                StudyChartData = studyData,
                GradeLabels = gradeLabels,
                GradeScores = gradeScores
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
