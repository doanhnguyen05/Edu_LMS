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
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IWebHostEnvironment _env;

        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(string? search, string? status)
        {
            ViewBag.Categories = await _context.CourseCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var userId = _userManager.GetUserId(User);

            var query = _context.Courses
                .Include(c => c.Chapters)
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search) || c.Code.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CourseStatus>(status, true, out var courseStatus))
            {
                query = query.Where(c => c.Status == courseStatus);
            }

            var courses = await query.OrderByDescending(c => c.UpdatedAt).ToListAsync();

            var model = new InstructorCourseListViewModel
            {
                SearchQuery = search,
                StatusFilter = status,
                Courses = courses.Select(c => new InstructorCourseItem
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    CoverImageUrl = c.CoverImageUrl,
                    Status = c.Status.ToString(),
                    StatusDisplay = c.Status switch
                    {
                        CourseStatus.Published => "Đã công bố",
                        CourseStatus.Draft => "Bản nháp",
                        CourseStatus.Disabled => "Vô hiệu hóa",
                        CourseStatus.PendingReview => "Chờ duyệt",
                        _ => c.Status.ToString()
                    },
                    StudentCount = c.Enrollments.Count,
                    Price = c.CourseType == CourseType.Free ? "Miễn phí" : $"{c.OriginalPrice:N0}đ",
                    ChapterCount = c.Chapters.Count,
                    UpdatedAt = c.UpdatedAt.ToString("dd/MM/yyyy"),
                    CategoryId = c.CategoryId,
                    OriginalPrice = c.OriginalPrice,
                    CourseTypeValue = c.CourseType.ToString()
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var cats = await _context.CourseCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return Json(cats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            // Handle cover image upload
            string? coverImageUrl = null;
            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "courses");
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(model.CoverImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.CoverImageFile.CopyToAsync(stream);
                coverImageUrl = $"/uploads/courses/{fileName}";
            }

            // Use first available category if not specified
            var categoryId = model.CategoryId > 0 ? model.CategoryId
                : await _context.CourseCategories.Select(c => c.Id).FirstOrDefaultAsync();

            var course = new Course
            {
                Name = model.Name,
                Code = model.Code,
                Description = model.Description,
                CoverImageUrl = coverImageUrl,
                InstructorId = userId!,
                CourseType = model.CourseType == "Paid" ? CourseType.Paid : CourseType.Free,
                OriginalPrice = model.OriginalPrice,
                PromoPrice = model.PromoPrice > 0 ? model.PromoPrice : null,
                Status = CourseStatus.Draft,
                CategoryId = categoryId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Manage(int id)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Chapters.OrderBy(ch => ch.DisplayOrder))
                    .ThenInclude(ch => ch.Lessons.OrderBy(l => l.DisplayOrder))
                        .ThenInclude(l => l.Assignments.OrderBy(a => a.DisplayOrder))
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Assignments.Where(a => a.LessonId == null))
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);

            if (course == null) return NotFound();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChapter(int courseId, string title)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == userId);
            if (course == null) return NotFound();

            var order = await _context.Chapters.CountAsync(ch => ch.CourseId == courseId) + 1;
            _context.Chapters.Add(new Chapter { CourseId = courseId, Title = title, DisplayOrder = order });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(int chapterId, string title, int durationMinutes, string? videoUrl, IFormFile? videoFile)
        {
            var userId = _userManager.GetUserId(User);
            var chapter = await _context.Chapters
                .Include(ch => ch.Course)
                .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.Course.InstructorId == userId);
            if (chapter == null) return NotFound();

            // Handle video file upload (takes priority over URL)
            string? finalVideoUrl = string.IsNullOrWhiteSpace(videoUrl) ? null : videoUrl.Trim();
            if (videoFile != null && videoFile.Length > 0)
            {
                var videosDir = Path.Combine(_env.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(videosDir);
                var ext = Path.GetExtension(videoFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(videosDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await videoFile.CopyToAsync(stream);
                finalVideoUrl = $"/uploads/videos/{fileName}";
            }

            var order = await _context.Lessons.CountAsync(l => l.ChapterId == chapterId) + 1;
            _context.Lessons.Add(new Lesson
            {
                ChapterId = chapterId,
                Title = title,
                DurationMinutes = durationMinutes,
                VideoUrl = finalVideoUrl,
                DisplayOrder = order
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = chapter.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clone(int id)
        {
            var userId = _userManager.GetUserId(User);
            var source = await _context.Courses
                .Include(c => c.Chapters).ThenInclude(ch => ch.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);
            if (source == null) return NotFound();

            var newCode = source.Code + "_COPY";
            var exists = await _context.Courses.AnyAsync(c => c.Code == newCode);
            if (exists) newCode = source.Code + "_COPY_" + DateTime.UtcNow.Ticks.ToString()[^4..];

            var clone = new Course
            {
                Code = newCode,
                Name = source.Name + " (Bản sao)",
                Description = source.Description,
                CoverImageUrl = source.CoverImageUrl,
                CategoryId = source.CategoryId,
                InstructorId = userId!,
                CourseType = source.CourseType,
                OriginalPrice = source.OriginalPrice,
                PromoPrice = source.PromoPrice,
                Level = source.Level,
                DurationHours = source.DurationHours,
                Status = CourseStatus.Draft
            };
            _context.Courses.Add(clone);
            await _context.SaveChangesAsync();

            foreach (var ch in source.Chapters.OrderBy(c => c.DisplayOrder))
            {
                var newChapter = new Chapter { CourseId = clone.Id, Title = ch.Title, DisplayOrder = ch.DisplayOrder };
                _context.Chapters.Add(newChapter);
                await _context.SaveChangesAsync();
                foreach (var l in ch.Lessons.OrderBy(x => x.DisplayOrder))
                    _context.Lessons.Add(new Lesson { ChapterId = newChapter.Id, Title = l.Title, VideoUrl = l.VideoUrl, DurationMinutes = l.DurationMinutes, DisplayOrder = l.DisplayOrder });
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã sao chép khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, string name, string code, string? description, int categoryId, string courseType, decimal originalPrice, IFormFile? coverImageFile)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);
            if (course == null) return NotFound();

            if (coverImageFile != null && coverImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "courses");
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(coverImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                using var stream = new FileStream(Path.Combine(uploadsDir, fileName), FileMode.Create);
                await coverImageFile.CopyToAsync(stream);
                course.CoverImageUrl = $"/uploads/courses/{fileName}";
            }

            course.Name = name;
            course.Code = code;
            course.Description = description;
            course.CategoryId = categoryId;
            course.CourseType = courseType == "Paid" ? Models.Enums.CourseType.Paid : Models.Enums.CourseType.Free;
            course.OriginalPrice = originalPrice;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật khóa học.";
            return RedirectToAction(nameof(Manage), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(int id, string title)
        {
            var userId = _userManager.GetUserId(User);
            var chapter = await _context.Chapters.Include(ch => ch.Course)
                .FirstOrDefaultAsync(ch => ch.Id == id && ch.Course.InstructorId == userId);
            if (chapter == null) return NotFound();

            chapter.Title = title;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = chapter.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(int id, string title, int durationMinutes, string? videoUrl, IFormFile? videoFile)
        {
            var userId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons.Include(l => l.Chapter).ThenInclude(ch => ch.Course)
                .FirstOrDefaultAsync(l => l.Id == id && l.Chapter.Course.InstructorId == userId);
            if (lesson == null) return NotFound();

            if (videoFile != null && videoFile.Length > 0)
            {
                var videosDir = Path.Combine(_env.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(videosDir);
                var ext = Path.GetExtension(videoFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                using var stream = new FileStream(Path.Combine(videosDir, fileName), FileMode.Create);
                await videoFile.CopyToAsync(stream);
                lesson.VideoUrl = $"/uploads/videos/{fileName}";
            }
            else if (!string.IsNullOrWhiteSpace(videoUrl))
            {
                lesson.VideoUrl = videoUrl.Trim();
            }

            lesson.Title = title;
            lesson.DurationMinutes = durationMinutes;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật bài học.";
            return RedirectToAction(nameof(Manage), new { id = lesson.Chapter.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            var userId = _userManager.GetUserId(User);
            var chapter = await _context.Chapters
                .Include(ch => ch.Course)
                .FirstOrDefaultAsync(ch => ch.Id == id && ch.Course.InstructorId == userId);
            if (chapter == null) return NotFound();

            var courseId = chapter.CourseId;
            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var userId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons
                .Include(l => l.Chapter).ThenInclude(ch => ch.Course)
                .FirstOrDefaultAsync(l => l.Id == id && l.Chapter.Course.InstructorId == userId);
            if (lesson == null) return NotFound();

            var courseId = lesson.Chapter.CourseId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignment(int? lessonId, int? chapterId, string title, string? description, decimal maxScore, DateTime? dueDate, IFormFile? attachment)
        {
            var userId = _userManager.GetUserId(User);
            int courseId;

            if (lessonId.HasValue)
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Chapter).ThenInclude(ch => ch.Course)
                    .FirstOrDefaultAsync(l => l.Id == lessonId && l.Chapter.Course.InstructorId == userId);
                if (lesson == null) return NotFound();

                chapterId = lesson.ChapterId;
                courseId = lesson.Chapter.CourseId;
                string? attachmentUrl = await SaveAttachmentAsync(attachment);
                var order = await _context.Assignments.CountAsync(a => a.LessonId == lessonId) + 1;
                _context.Assignments.Add(new Assignment
                {
                    ChapterId = chapterId.Value,
                    LessonId = lessonId,
                    Title = title,
                    Description = description,
                    AttachmentUrl = attachmentUrl,
                    MaxScore = maxScore > 0 ? maxScore : 10,
                    DueDate = dueDate?.ToUniversalTime(),
                    DisplayOrder = order
                });
            }
            else
            {
                var chapter = await _context.Chapters
                    .Include(ch => ch.Course)
                    .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.Course.InstructorId == userId);
                if (chapter == null) return NotFound();

                courseId = chapter.CourseId;
                string? attachmentUrl = await SaveAttachmentAsync(attachment);
                var order = await _context.Assignments.CountAsync(a => a.ChapterId == chapterId && a.LessonId == null) + 1;
                _context.Assignments.Add(new Assignment
                {
                    ChapterId = chapterId!.Value,
                    Title = title,
                    Description = description,
                    AttachmentUrl = attachmentUrl,
                    MaxScore = maxScore > 0 ? maxScore : 10,
                    DueDate = dueDate?.ToUniversalTime(),
                    DisplayOrder = order
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm bài tập.";
            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment(int id, string title, string? description, decimal maxScore, DateTime? dueDate, IFormFile? attachment, bool removeAttachment = false)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Chapter).ThenInclude(ch => ch.Course)
                .FirstOrDefaultAsync(a => a.Id == id && a.Chapter.Course.InstructorId == userId);
            if (assignment == null) return NotFound();

            assignment.Title = title;
            assignment.Description = description;
            assignment.MaxScore = maxScore > 0 ? maxScore : 10;
            assignment.DueDate = dueDate?.ToUniversalTime();

            if (removeAttachment)
            {
                DeleteAttachmentFile(assignment.AttachmentUrl);
                assignment.AttachmentUrl = null;
            }
            else if (attachment != null)
            {
                DeleteAttachmentFile(assignment.AttachmentUrl);
                assignment.AttachmentUrl = await SaveAttachmentAsync(attachment);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật bài tập.";
            return RedirectToAction(nameof(Manage), new { id = assignment.Chapter.CourseId });
        }

        private async Task<string?> SaveAttachmentAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            if (file.Length > 10 * 1024 * 1024) return null; // 10MB limit

            var allowed = new[] { ".pdf", ".doc", ".docx", ".zip", ".rar", ".png", ".jpg", ".jpeg", ".txt", ".xlsx" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return null;

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "assignments");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/assignments/{fileName}";
        }

        private void DeleteAttachmentFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var filePath = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Chapter).ThenInclude(ch => ch.Course)
                .FirstOrDefaultAsync(a => a.Id == id && a.Chapter.Course.InstructorId == userId);
            if (assignment == null) return NotFound();

            var courseId = assignment.Chapter.CourseId;
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài tập.";
            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);

            if (course == null) return NotFound();

            if (course.Status != CourseStatus.Draft)
            {
                TempData["SuccessMessage"] = "Chỉ có thể gửi duyệt khóa học đang ở trạng thái Bản nháp.";
                return RedirectToAction(nameof(Index));
            }

            course.Status = CourseStatus.PendingReview;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã gửi khóa học đến Admin để duyệt!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);

            if (course == null) return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa khóa học.";
            return RedirectToAction(nameof(Index));
        }
    }
}
