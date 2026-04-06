using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, CourseStatus? status, int? categoryId, string? instructor)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search) || c.Code.Contains(search));

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(instructor))
                query = query.Where(c => c.Instructor.FullName.Contains(instructor) || c.Instructor.Email!.Contains(instructor));

            var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            var categories = await _context.CourseCategories
                .Include(cat => cat.Courses)
                .OrderBy(cat => cat.DisplayOrder)
                .ToListAsync();

            var allCourses = await _context.Courses.ToListAsync();

            var vm = new CoursesViewModel
            {
                Courses = courses.Select(c => new CourseItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    CoverImageUrl = c.CoverImageUrl,
                    InstructorName = c.Instructor.FullName,
                    CategoryName = c.Category.Name,
                    Status = c.Status,
                    CourseType = c.CourseType,
                    OriginalPrice = c.OriginalPrice,
                    EnrollmentCount = c.Enrollments.Count,
                    AverageRating = c.AverageRating,
                    CreatedAt = c.CreatedAt,
                    PublishedAt = c.PublishedAt
                }).ToList(),
                Categories = categories.Select(cat => new CategoryItem
                {
                    Id = cat.Id,
                    Name = cat.Name,
                    Slug = cat.Slug,
                    DisplayOrder = cat.DisplayOrder,
                    IsActive = cat.IsActive,
                    CourseCount = cat.Courses.Count
                }).ToList(),
                TotalCourses = allCourses.Count,
                PublishedCourses = allCourses.Count(c => c.Status == CourseStatus.Published),
                DisabledCourses = allCourses.Count(c => c.Status == CourseStatus.Disabled),
                DraftCourses = allCourses.Count(c => c.Status == CourseStatus.Draft),
                PendingReviewCourses = allCourses.Count(c => c.Status == CourseStatus.PendingReview),
                SearchQuery = search,
                StatusFilter = status,
                CategoryFilter = categoryId,
                InstructorFilter = instructor
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            if (course.Status == CourseStatus.Published)
            {
                course.Status = CourseStatus.Disabled;
            }
            else if (course.Status == CourseStatus.Disabled)
            {
                course.Status = CourseStatus.Published;
            }

            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật trạng thái khoá học \"{course.Name}\".";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Chapters.OrderBy(ch => ch.DisplayOrder))
                    .ThenInclude(ch => ch.Lessons.OrderBy(l => l.DisplayOrder))
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Assignments.Where(a => a.LessonId == null))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var vm = new EduLMS.Web.ViewModels.Admin.CourseReviewViewModel { Course = course };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Status = CourseStatus.Published;
            course.PublishedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã duyệt và công bố khoá học \"{course.Name}\".";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Status = CourseStatus.Draft;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã từ chối khoá học \"{course.Name}\", trả về bản nháp cho giảng viên.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name, string slug, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            {
                TempData["Error"] = "Tên và slug không được để trống.";
                return RedirectToAction(nameof(Index), new { tab = "categories" });
            }

            var exists = await _context.CourseCategories.AnyAsync(c => c.Slug == slug);
            if (exists)
            {
                TempData["Error"] = "Slug đã tồn tại.";
                return RedirectToAction(nameof(Index), new { tab = "categories" });
            }

            _context.CourseCategories.Add(new CourseCategory
            {
                Name = name,
                Slug = slug,
                DisplayOrder = displayOrder,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo danh mục \"{name}\".";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, string name, string slug, int displayOrder, bool isActive)
        {
            var category = await _context.CourseCategories.FindAsync(id);
            if (category == null) return NotFound();

            var slugConflict = await _context.CourseCategories.AnyAsync(c => c.Slug == slug && c.Id != id);
            if (slugConflict)
            {
                TempData["Error"] = "Slug đã tồn tại.";
                return RedirectToAction(nameof(Index), new { tab = "categories" });
            }

            category.Name = name;
            category.Slug = slug;
            category.DisplayOrder = displayOrder;
            category.IsActive = isActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật danh mục \"{name}\".";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.CourseCategories
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.Courses.Any())
            {
                TempData["Error"] = $"Không thể xoá danh mục \"{category.Name}\" vì còn {category.Courses.Count} khoá học đang dùng.";
                return RedirectToAction(nameof(Index), new { tab = "categories" });
            }

            _context.CourseCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xoá danh mục \"{category.Name}\".";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }
    }
}
