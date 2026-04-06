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
    public class LessonController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Training");
        }

        public async Task<IActionResult> View(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var lesson = await _db.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch.Course)
                        .ThenInclude(c => c.Chapters)
                            .ThenInclude(ch => ch.Lessons)
                .Include(l => l.Resources)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                // Demo data
                var demoModel = new LessonViewModel
                {
                    LessonId = id,
                    CourseName = "Lập trình Python từ cơ bản đến nâng cao",
                    InstructorName = "Nguyễn Văn A",
                    LessonTitle = "Bài 12: Vòng lặp for và while",
                    Description = "Trong buổi học này, bạn sẽ tìm hiểu về cách sử dụng vòng lặp for và while trong Python. Chúng ta sẽ đi qua các ví dụ thực tế, bao gồm duyệt danh sách, xử lý chuỗi, và các bài toán lặp phổ biến.",
                    DurationMinutes = 45,
                    VideoUrl = null,
                    Status = "in_progress",
                    CourseId = 1,
                    CanMarkComplete = true,
                    Resources = new List<ResourceItem>
                    {
                        new() { Id = 1, FileName = "Slide_Bai12_VongLap.pdf", FileType = "PDF", FileSize = "2.3 MB", FileUrl = "#" },
                        new() { Id = 2, FileName = "BaiTap_VongLap.zip", FileType = "ZIP", FileSize = "1.1 MB", FileUrl = "#" },
                        new() { Id = 3, FileName = "TaiLieu_BoSung.docx", FileType = "DOCX", FileSize = "540 KB", FileUrl = "#" }
                    }
                };
                return View(demoModel);
            }

            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == lesson.Chapter.CourseId);
            if (enrollment == null)
            {
                return Forbid();
            }

            var orderedLessons = BuildOrderedLessons(lesson.Chapter.Course);
            var currentIndex = orderedLessons.FindIndex(x => x.LessonId == lesson.Id);
            var progress = await _db.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);
            var currentLessonCompleted = progress?.Status == LessonStatus.Completed;

            if (!currentLessonCompleted && currentIndex > 0)
            {
                var previousLessonId = orderedLessons[currentIndex - 1].LessonId;
                var previousLessonCompleted = await _db.LessonProgresses
                    .AnyAsync(p => p.UserId == userId
                        && p.LessonId == previousLessonId
                        && p.Status == LessonStatus.Completed);

                if (!previousLessonCompleted)
                {
                    TempData["LessonAccessError"] = "Bạn cần hoàn thành bài học trước đó để mở bài này.";
                    return RedirectToAction(nameof(View), new { id = previousLessonId });
                }
            }

            string FormatFileSize(long bytes)
            {
                if (bytes >= 1048576) return $"{bytes / 1048576.0:F1} MB";
                if (bytes >= 1024) return $"{bytes / 1024.0:F0} KB";
                return $"{bytes} B";
            }

            var model = new LessonViewModel
            {
                LessonId = lesson.Id,
                CourseName = lesson.Chapter.Course.Name,
                InstructorName = lesson.Chapter.Course.Instructor.FullName,
                LessonTitle = lesson.Title,
                Description = lesson.Description,
                DurationMinutes = lesson.DurationMinutes,
                VideoUrl = lesson.VideoUrl,
                Status = progress?.Status.ToString().ToLower() ?? "not_started",
                CourseId = lesson.Chapter.CourseId,
                CanMarkComplete = progress?.Status != LessonStatus.Completed,
                Resources = lesson.Resources.Select(r => new ResourceItem
                {
                    Id = r.Id,
                    FileName = r.FileName,
                    FileType = r.FileType.ToUpper(),
                    FileSize = FormatFileSize(r.FileSizeBytes),
                    FileUrl = r.FileUrl
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var lesson = await _db.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch.Course)
                        .ThenInclude(c => c.Chapters)
                            .ThenInclude(ch => ch.Lessons)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == lesson.Chapter.CourseId);
            if (enrollment == null)
            {
                return Forbid();
            }

            var orderedLessons = BuildOrderedLessons(lesson.Chapter.Course);
            var currentIndex = orderedLessons.FindIndex(x => x.LessonId == lessonId);
            if (currentIndex > 0)
            {
                var previousLessonId = orderedLessons[currentIndex - 1].LessonId;
                var previousLessonCompleted = await _db.LessonProgresses
                    .AnyAsync(p => p.UserId == userId
                        && p.LessonId == previousLessonId
                        && p.Status == LessonStatus.Completed);

                if (!previousLessonCompleted)
                {
                    TempData["LessonAccessError"] = "Bạn cần hoàn thành bài học trước đó để mở bài này.";
                    return RedirectToAction(nameof(View), new { id = previousLessonId });
                }
            }

            var progress = await _db.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                _db.LessonProgresses.Add(new Models.LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    Status = LessonStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                });
            }
            else
            {
                progress.Status = LessonStatus.Completed;
                progress.CompletedAt ??= DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            var courseLessonIds = orderedLessons.Select(x => x.LessonId).ToList();
            var totalLessons = courseLessonIds.Count;
            var completedLessons = totalLessons == 0
                ? 0
                : await _db.LessonProgresses
                    .Where(lp => lp.UserId == userId
                        && courseLessonIds.Contains(lp.LessonId)
                        && lp.Status == LessonStatus.Completed)
                    .Select(lp => lp.LessonId)
                    .Distinct()
                    .CountAsync();

            var progressPercent = totalLessons == 0
                ? 0
                : Math.Round((decimal)completedLessons / totalLessons * 100m, 2, MidpointRounding.AwayFromZero);

            enrollment.ProgressPercent = Math.Max(enrollment.ProgressPercent, progressPercent);
            if (totalLessons > 0 && completedLessons >= totalLessons)
            {
                enrollment.Status = EnrollmentStatus.Completed;
                enrollment.CompletedAt ??= DateTime.UtcNow;
            }
            else
            {
                enrollment.Status = EnrollmentStatus.Active;
                enrollment.CompletedAt = null;
            }

            await _db.SaveChangesAsync();

            TempData["LessonCompleted"] = "true";
            return RedirectToAction(nameof(View), new { id = lessonId });
        }

        private static List<OrderedLessonItem> BuildOrderedLessons(Models.Course course)
        {
            return course.Chapters
                .OrderBy(ch => ch.DisplayOrder)
                .ThenBy(ch => ch.Id)
                .SelectMany(ch => ch.Lessons
                    .OrderBy(l => l.DisplayOrder)
                    .ThenBy(l => l.Id)
                    .Select(l => new OrderedLessonItem
                    {
                        LessonId = l.Id
                    }))
                .ToList();
        }

        private sealed class OrderedLessonItem
        {
            public int LessonId { get; set; }
        }
    }
}
