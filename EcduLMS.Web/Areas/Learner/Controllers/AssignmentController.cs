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
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Training");
        }

        public async Task<IActionResult> Submit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var assignment = await _db.Assignments
                .Include(a => a.Chapter)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

            // Verify learner is enrolled
            var enrolled = await _db.Enrollments
                .AnyAsync(e => e.CourseId == assignment.Chapter.CourseId && e.UserId == userId);
            if (!enrolled) return Forbid();

            var orderedLessons = await _db.Lessons
                .Where(l => l.Chapter.CourseId == assignment.Chapter.CourseId)
                .Select(l => new OrderedLessonItem
                {
                    LessonId = l.Id,
                    ChapterOrder = l.Chapter.DisplayOrder,
                    LessonOrder = l.DisplayOrder
                })
                .OrderBy(l => l.ChapterOrder)
                .ThenBy(l => l.LessonOrder)
                .ThenBy(l => l.LessonId)
                .ToListAsync();

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

            var (canStart, blockedReason) = CanStartAssignment(
                assignment,
                orderedLessons,
                completedLessonIds,
                assignment.Chapter.DisplayOrder);

            if (!canStart)
            {
                TempData["AssignmentAccessError"] = blockedReason ?? "Bạn hãy hoàn thành khóa học đó trước.";
                return RedirectToAction("CourseDetail", "Grades", new { area = "Learner", id = assignment.Chapter.CourseId });
            }

            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == id && s.UserId == userId);

            var model = new AssignmentSubmitViewModel
            {
                AssignmentId = assignment.Id,
                CourseName = assignment.Chapter.Course.Name,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate?.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                MaxScore = assignment.MaxScore,
                Status = submission?.Status.ToString() ?? "Draft",
                ExistingContent = submission?.Content,
                ExistingFileName = submission?.FileName,
                CourseId = assignment.Chapter.CourseId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(int assignmentId, string? content, IFormFile? file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var assignment = await _db.Assignments
                .Include(a => a.Chapter)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
            if (assignment == null) return NotFound();

            var enrolled = await _db.Enrollments
                .AnyAsync(e => e.CourseId == assignment.Chapter.CourseId && e.UserId == userId);
            if (!enrolled) return Forbid();

            var orderedLessons = await _db.Lessons
                .Where(l => l.Chapter.CourseId == assignment.Chapter.CourseId)
                .Select(l => new OrderedLessonItem
                {
                    LessonId = l.Id,
                    ChapterOrder = l.Chapter.DisplayOrder,
                    LessonOrder = l.DisplayOrder
                })
                .OrderBy(l => l.ChapterOrder)
                .ThenBy(l => l.LessonOrder)
                .ThenBy(l => l.LessonId)
                .ToListAsync();

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

            var (canStart, blockedReason) = CanStartAssignment(
                assignment,
                orderedLessons,
                completedLessonIds,
                assignment.Chapter.DisplayOrder);
            if (!canStart)
            {
                TempData["AssignmentAccessError"] = blockedReason ?? "Bạn hãy hoàn thành khóa học đó trước.";
                return RedirectToAction("CourseDetail", "Grades", new { area = "Learner", id = assignment.Chapter.CourseId });
            }

            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.UserId == userId);

            // Don't allow editing a submitted submission
            if (submission?.Status == SubmissionStatus.Submitted)
                return RedirectToAction(nameof(Submit), new { id = assignmentId });

            string? fileUrl = null;
            string? fileName = null;
            long? fileSize = null;

            if (file != null && file.Length > 0)
            {
                var result = await SaveFileAsync(file);
                fileUrl = result.url;
                fileName = result.name;
                fileSize = file.Length;
            }

            if (submission == null)
            {
                submission = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    UserId = userId,
                    Content = content,
                    Status = SubmissionStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                if (fileUrl != null) { submission.FileUrl = fileUrl; submission.FileName = fileName; submission.FileSizeBytes = fileSize; }
                _db.AssignmentSubmissions.Add(submission);
            }
            else
            {
                submission.Content = content;
                submission.UpdatedAt = DateTime.UtcNow;
                if (fileUrl != null) { submission.FileUrl = fileUrl; submission.FileName = fileName; submission.FileSizeBytes = fileSize; }
            }

            await _db.SaveChangesAsync();
            TempData["DraftSaved"] = "true";
            return RedirectToAction(nameof(Submit), new { id = assignmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFinal(int assignmentId, string? content, IFormFile? file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var assignment = await _db.Assignments
                .Include(a => a.Chapter)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
            if (assignment == null) return NotFound();

            var enrolled = await _db.Enrollments
                .AnyAsync(e => e.CourseId == assignment.Chapter.CourseId && e.UserId == userId);
            if (!enrolled) return Forbid();

            var orderedLessons = await _db.Lessons
                .Where(l => l.Chapter.CourseId == assignment.Chapter.CourseId)
                .Select(l => new OrderedLessonItem
                {
                    LessonId = l.Id,
                    ChapterOrder = l.Chapter.DisplayOrder,
                    LessonOrder = l.DisplayOrder
                })
                .OrderBy(l => l.ChapterOrder)
                .ThenBy(l => l.LessonOrder)
                .ThenBy(l => l.LessonId)
                .ToListAsync();

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

            var (canStart, blockedReason) = CanStartAssignment(
                assignment,
                orderedLessons,
                completedLessonIds,
                assignment.Chapter.DisplayOrder);
            if (!canStart)
            {
                TempData["AssignmentAccessError"] = blockedReason ?? "Bạn hãy hoàn thành khóa học đó trước.";
                return RedirectToAction("CourseDetail", "Grades", new { area = "Learner", id = assignment.Chapter.CourseId });
            }

            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.UserId == userId);

            // Already submitted (not returned) — no re-submit
            if (submission?.Status == SubmissionStatus.Submitted)
                return RedirectToAction(nameof(Submit), new { id = assignmentId });

            string? fileUrl = submission?.FileUrl;
            string? fileName = submission?.FileName;
            long? fileSize = submission?.FileSizeBytes;

            if (file != null && file.Length > 0)
            {
                var result = await SaveFileAsync(file);
                fileUrl = result.url;
                fileName = result.name;
                fileSize = file.Length;
            }

            if (submission == null)
            {
                submission = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    UserId = userId,
                    Content = content,
                    FileUrl = fileUrl,
                    FileName = fileName,
                    FileSizeBytes = fileSize,
                    Status = SubmissionStatus.Submitted,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.AssignmentSubmissions.Add(submission);
            }
            else
            {
                submission.Content = content;
                submission.FileUrl = fileUrl;
                submission.FileName = fileName;
                submission.FileSizeBytes = fileSize;
                submission.Status = SubmissionStatus.Submitted;
                submission.SubmittedAt = DateTime.UtcNow;
                submission.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            TempData["Submitted"] = "true";
            return RedirectToAction(nameof(Submit), new { id = assignmentId });
        }

        private async Task<(string url, string name)> SaveFileAsync(IFormFile file)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "submissions");
            Directory.CreateDirectory(uploadsDir);
            var ext = Path.GetExtension(file.FileName);
            var safeName = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(uploadsDir, safeName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return ($"/uploads/submissions/{safeName}", file.FileName);
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
            public int LessonOrder { get; set; }
        }
    }
}
