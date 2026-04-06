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
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainingController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var enrollments = await _db.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var activeCourses = enrollments
                .Where(e => e.Status == Models.Enums.EnrollmentStatus.Active)
                .Select(e => new TrainingCourseItem
                {
                    Id = e.CourseId,
                    Code = e.Course.Code,
                    Name = e.Course.Name,
                    CoverImageUrl = e.Course.CoverImageUrl,
                    Progress = e.ProgressPercent,
                    Status = e.ProgressPercent > 0 ? "Đang học" : "Chưa bắt đầu"
                }).ToList();

            var completedCourses = enrollments
                .Where(e => e.Status == Models.Enums.EnrollmentStatus.Completed)
                .Select(e => new TrainingCourseItem
                {
                    Id = e.CourseId,
                    Code = e.Course.Code,
                    Name = e.Course.Name,
                    CoverImageUrl = e.Course.CoverImageUrl,
                    Progress = 100,
                    Status = "Hoàn thành"
                }).ToList();

            var model = new TrainingViewModel
            {
                ActiveCourses = activeCourses,
                CompletedCourses = completedCourses
            };

            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var enrollment = await _db.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                        .ThenInclude(ch => ch.Lessons)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                        .ThenInclude(ch => ch.Assignments)
                .Where(e => e.CourseId == id && e.UserId == userId)
                .FirstOrDefaultAsync();

            if (enrollment == null)
            {
                return NotFound();
            }

            var lessonProgresses = await _db.LessonProgresses
                .Where(lp => lp.UserId == userId)
                .ToListAsync();

            var submissions = await _db.AssignmentSubmissions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var orderedLessons = BuildOrderedLessons(enrollment.Course);
            var orderedLessonIds = orderedLessons.Select(x => x.LessonId).ToList();
            var completedLessonIds = lessonProgresses
                .Where(p => p.Status == LessonStatus.Completed && orderedLessonIds.Contains(p.LessonId))
                .Select(p => p.LessonId)
                .ToHashSet();

            var lessons = orderedLessons.Select((l, index) =>
            {
                var progress = lessonProgresses.FirstOrDefault(p => p.LessonId == l.LessonId);
                var status = progress?.Status == LessonStatus.Completed ? "completed"
                    : progress?.Status == LessonStatus.InProgress ? "in_progress"
                    : "not_started";

                var isLocked = index > 0
                    && status != "completed"
                    && !completedLessonIds.Contains(orderedLessons[index - 1].LessonId);

                return new LessonItem
                {
                    Id = l.LessonId,
                    Title = l.Title,
                    Status = status,
                    IsLocked = isLocked,
                    LockMessage = isLocked ? "Hoàn thành bài trước đó để mở bài này." : null
                };
            }).ToList();

            var assignments = enrollment.Course.Chapters
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
                .Select(item =>
                {
                    var assignment = item.Assignment;
                    var (canStart, blockedReason) = CanStartAssignment(
                        assignment,
                        orderedLessons,
                        completedLessonIds,
                        item.ChapterOrder);
                    var isSubmitted = submissions.Any(s => s.AssignmentId == assignment.Id && s.Status != Models.Enums.SubmissionStatus.Draft);

                    return new AssignmentListItem
                    {
                        Id = assignment.Id,
                        Title = assignment.Title,
                        IsSubmitted = isSubmitted,
                        CanStart = isSubmitted || canStart,
                        BlockedReason = isSubmitted ? null : blockedReason
                    };
                })
                .ToList();

            var detailModel = new CourseDetailViewModel
            {
                CourseId = enrollment.CourseId,
                CourseName = enrollment.Course.Name,
                InstructorName = enrollment.Course.Instructor.FullName,
                Status = enrollment.Status == Models.Enums.EnrollmentStatus.Completed ? "Hoàn thành" : (enrollment.ProgressPercent > 0 ? "Đang học" : "Chưa bắt đầu"),
                Progress = enrollment.ProgressPercent,
                CoverImageUrl = enrollment.Course.CoverImageUrl,
                Lessons = lessons,
                Assignments = assignments
            };

            return View(detailModel);
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
                        Title = l.Title,
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
            public string Title { get; set; } = string.Empty;
            public int ChapterOrder { get; set; }
        }
    }
}
