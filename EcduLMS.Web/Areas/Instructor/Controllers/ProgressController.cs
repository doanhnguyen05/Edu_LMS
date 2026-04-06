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
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProgressController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            var userId = _userManager.GetUserId(User);

            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            var targetCourseId = courseId ?? courses.FirstOrDefault()?.Id ?? 0;

            var lessons = await _context.Lessons
                .Include(l => l.Chapter)
                .Where(l => l.Chapter.CourseId == targetCourseId)
                .OrderBy(l => l.Chapter.DisplayOrder)
                .ThenBy(l => l.DisplayOrder)
                .ToListAsync();

            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Where(e => e.CourseId == targetCourseId && e.Status == EnrollmentStatus.Active)
                .ToListAsync();

            var totalEnrollments = enrollments.Count;
            var completedCount = enrollments.Count(e => e.CompletedAt != null);

            var completionRate = totalEnrollments > 0
                ? Math.Round((decimal)completedCount / totalEnrollments * 100, 1)
                : 0m;

            var chapterIds = await _context.Chapters
                .Where(ch => ch.CourseId == targetCourseId)
                .Select(ch => ch.Id)
                .ToListAsync();

            var assignmentIds = await _context.Assignments
                .Where(a => chapterIds.Contains(a.ChapterId))
                .Select(a => a.Id)
                .ToListAsync();

            var allSubmissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Where(s => assignmentIds.Contains(s.AssignmentId) && s.Status != SubmissionStatus.Draft)
                .ToListAsync();

            var totalSubs = allSubmissions.Count;
            var onTimeSubs = allSubmissions.Count(s =>
                s.Assignment.DueDate == null || s.SubmittedAt <= s.Assignment.DueDate);

            var onTimeRate = totalSubs > 0
                ? Math.Round((decimal)onTimeSubs / totalSubs * 100, 1)
                : 0m;

            var lessonIds = lessons.Select(l => l.Id).ToList();
            var progresses = await _context.LessonProgresses
                .Where(lp => lessonIds.Contains(lp.LessonId))
                .ToListAsync();

            var matrix = enrollments.Select(e => new TrainingMatrixRow
            {
                StudentName = e.User.FullName,
                LessonStatuses = lessons.Select(l =>
                {
                    var p = progresses.FirstOrDefault(lp => lp.UserId == e.UserId && lp.LessonId == l.Id);
                    if (p == null) return "not_started";
                    return p.Status switch
                    {
                        LessonStatus.Completed => "completed",
                        LessonStatus.InProgress => "in_progress",
                        _ => "not_started"
                    };
                }).ToList()
            }).ToList();

            ViewBag.LessonNames = lessons.Select(l => l.Title).ToList();

            // Avg score chart per assignment
            var assignments = await _context.Assignments
                .Where(a => chapterIds.Contains(a.ChapterId))
                .ToListAsync();

            var allGrades = await _context.Grades
                .Include(g => g.Submission)
                .Where(g => assignmentIds.Contains(g.Submission.AssignmentId))
                .ToListAsync();

            var scoreLabels = assignments.Select(a => a.Title.Length > 15 ? a.Title[..15] + "…" : a.Title).ToArray();
            var avgScores = assignments.Select(a => {
                var gs = allGrades.Where(g => g.Submission.AssignmentId == a.Id).ToList();
                if (!gs.Any()) return 0m;
                return Math.Round(gs.Average(g => a.MaxScore > 0 ? g.Score / a.MaxScore * 10 : g.Score), 1);
            }).ToArray();
            var maxScores = assignments.Select(a => {
                var gs = allGrades.Where(g => g.Submission.AssignmentId == a.Id).ToList();
                if (!gs.Any()) return 0m;
                return Math.Round(gs.Max(g => a.MaxScore > 0 ? g.Score / a.MaxScore * 10 : g.Score), 1);
            }).ToArray();

            var model = new StudentProgressViewModel
            {
                CompletionRate = completionRate,
                OnTimeRate = onTimeRate,
                Matrix = matrix,
                ScoreLabels = scoreLabels,
                AvgScores = avgScores,
                MaxScores = maxScores
            };

            return View(model);
        }
    }
}
