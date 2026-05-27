using EduLMS.Web.Data;
using EduLMS.Web.Models;
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
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GradesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var enrollments = await _db.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                        .ThenInclude(ch => ch.Assignments)
                            .ThenInclude(a => a.Submissions.Where(s => s.UserId == userId))
                                .ThenInclude(s => s.Grade)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var courseGrades = enrollments.Select(e =>
            {
                var allAssignments = e.Course.Chapters.SelectMany(c => c.Assignments).ToList();
                var gradedSubmissions = allAssignments
                    .SelectMany(a => a.Submissions)
                    .Where(s => s.Grade != null)
                    .ToList();

                decimal? avg = gradedSubmissions.Any()
                    ? gradedSubmissions.Average(s => s.Grade!.Score)
                    : null;

                return new CourseGradeItem
                {
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    AverageScore = avg,
                    GradedProgress = $"{gradedSubmissions.Count}/{allAssignments.Count}",
                    Status = e.Status == Models.Enums.EnrollmentStatus.Completed ? "Hoàn thành"
                        : e.ProgressPercent > 0 ? "Đang học" : "Chưa bắt đầu"
                };
            }).ToList();

            var model = new GradesOverviewViewModel
            {
                CourseGrades = courseGrades
            };

            return View(model);
        }

        public async Task<IActionResult> CourseDetail(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var enrollment = await _db.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                        .ThenInclude(ch => ch.Assignments)
                            .ThenInclude(a => a.Submissions.Where(s => s.UserId == userId))
                                .ThenInclude(s => s.Grade)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                        .ThenInclude(ch => ch.Lessons)
                .FirstOrDefaultAsync(e => e.CourseId == id && e.UserId == userId);

            if (enrollment == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng ký khóa học này hoặc khóa học không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var orderedLessons = BuildOrderedLessons(enrollment.Course);
            var orderedLessonIds = orderedLessons.Select(x => x.LessonId).ToList();
            var completedLessonIds = orderedLessonIds.Count == 0
                ? new HashSet<int>()
                : (await _db.LessonProgresses
                    .Where(lp => lp.UserId == userId
                        && orderedLessonIds.Contains(lp.LessonId)
                        && lp.Status == LessonStatus.Completed)
                    .Select(lp => lp.LessonId)
                    .ToListAsync())
                    .ToHashSet();

            var allAssignments = enrollment.Course.Chapters
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .SelectMany(c => c.Assignments
                    .OrderBy(a => a.DisplayOrder)
                    .ThenBy(a => a.Id)
                    .Select(a => new
                    {
                        Assignment = a,
                        ChapterOrder = c.DisplayOrder
                    }))
                .ToList();

            var assignments = allAssignments.Select(item =>
            {
                var assignment = item.Assignment;
                var submission = assignment.Submissions.OrderByDescending(s => s.Id).FirstOrDefault();
                var hasSubmitted = submission != null && submission.Status != SubmissionStatus.Draft;
                var (canStart, blockedReason) = CanStartAssignment(
                    assignment,
                    orderedLessons,
                    completedLessonIds,
                    item.ChapterOrder);

                return new GradeAssignmentItem
                {
                    AssignmentId = assignment.Id,
                    SubmissionId = hasSubmitted ? submission!.Id : 0,
                    Title = assignment.Title,
                    Type = "Assignment",
                    Score = submission?.Grade?.Score,
                    MaxScore = assignment.MaxScore,
                    Status = submission?.Grade != null
                        ? (submission.Grade.PassStatus == Models.Enums.PassStatus.Pass ? "Pass" : "Fail")
                        : (hasSubmitted ? "Chưa chấm" : "Chưa nộp"),
                    CanStartAssignment = !hasSubmitted && canStart,
                    StartBlockedReason = !hasSubmitted ? blockedReason : null
                };
            }).ToList();

            decimal? avg = assignments.Where(a => a.Score.HasValue).Any()
                ? assignments.Where(a => a.Score.HasValue).Average(a => a.Score!.Value)
                : null;

            var model = new GradeCourseDetailViewModel
            {
                CourseId = id,
                CourseName = enrollment.Course.Name,
                AverageScore = avg,
                Assignments = assignments
            };

            return View(model);
        }

        private static List<OrderedLessonItem> BuildOrderedLessons(Course course)
        {
            return course.Chapters
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .SelectMany(c => c.Lessons
                    .OrderBy(l => l.DisplayOrder)
                    .ThenBy(l => l.Id)
                    .Select(l => new OrderedLessonItem
                    {
                        LessonId = l.Id,
                        ChapterOrder = c.DisplayOrder
                    }))
                .ToList();
        }

        private static (bool canStart, string? blockedReason) CanStartAssignment(
            Assignment assignment,
            List<OrderedLessonItem> orderedLessons,
            HashSet<int> completedLessonIds,
            int assignmentChapterOrder)
        {
            if (!orderedLessons.Any())
            {
                return (true, null);
            }

            IEnumerable<int> requiredLessonIds;
            if (assignment.LessonId.HasValue)
            {
                var linkedLessonIndex = orderedLessons.FindIndex(x => x.LessonId == assignment.LessonId.Value);
                requiredLessonIds = linkedLessonIndex >= 0
                    ? orderedLessons.Take(linkedLessonIndex + 1).Select(x => x.LessonId)
                    : orderedLessons
                        .Where(x => x.ChapterOrder < assignmentChapterOrder)
                        .Select(x => x.LessonId);
            }
            else
            {
                requiredLessonIds = orderedLessons.Select(x => x.LessonId);
            }

            var canStart = requiredLessonIds.All(completedLessonIds.Contains);
            return canStart
                ? (true, null)
                : (false, "Bạn hãy hoàn thành khóa học đó trước.");
        }

        private sealed class OrderedLessonItem
        {
            public int LessonId { get; set; }
            public int ChapterOrder { get; set; }
        }

        public async Task<IActionResult> GradeDetail(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Chapter)
                        .ThenInclude(c => c.Course)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (submission?.Grade == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kết quả chấm điểm cho bài làm này.";
                return RedirectToAction(nameof(Index));
            }

            var model = new GradeDetailViewModel
            {
                AssignmentTitle = submission.Assignment.Title,
                Type = "Assignment",
                Score = submission.Grade.Score,
                MaxScore = submission.Assignment.MaxScore,
                PassStatus = submission.Grade.PassStatus.ToString(),
                Comment = submission.Grade.Comment,
                CourseId = submission.Assignment.Chapter.CourseId
            };

            return View(model);
        }
    }
}
