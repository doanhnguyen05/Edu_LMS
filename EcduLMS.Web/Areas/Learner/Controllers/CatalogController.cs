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
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CatalogController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Chapters.OrderBy(ch => ch.DisplayOrder))
                .Where(c => c.Id == id && c.Status == Models.Enums.CourseStatus.Published)
                .FirstOrDefaultAsync();

            if (course == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isEnrolled = await _db.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);

            var chapters = course.Chapters
                .Take(5)
                .Select(ch => ch.Title)
                .ToList();

            var checkoutUrl = Url.Action("Checkout", "Payment", new { area = "Learner", id = course.Id });
            var trainingUrl = Url.Action("Index", "Training", new { area = "Learner" });

            return Json(new
            {
                id = course.Id,
                name = course.Name,
                description = course.Description ?? "Khóa học cung cấp kiến thức từ cơ bản đến nâng cao, phù hợp cho mọi đối tượng học viên.",
                instructorName = course.Instructor.FullName,
                categoryName = course.Category.Name,
                rating = course.AverageRating.ToString("F1"),
                ratingCount = course.RatingCount,
                durationHours = course.DurationHours,
                price = course.CourseType == Models.Enums.CourseType.Free
                    ? "Miễn phí"
                    : (course.PromoPrice ?? course.OriginalPrice).ToString("N0") + "đ",
                isFree = course.CourseType == Models.Enums.CourseType.Free,
                coverImageUrl = course.CoverImageUrl,
                chapters,
                totalChapters = course.Chapters.Count,
                isEnrolled,
                checkoutUrl,
                trainingUrl
            });
        }

        public async Task<IActionResult> Index(string? search, string? category, string? level)
        {
            var categories = await _db.CourseCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryItem { Id = c.Id, Name = c.Name, Slug = c.Slug })
                .ToListAsync();

            var query = _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => c.Status == Models.Enums.CourseStatus.Published);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Name.Contains(search) || c.Code.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.Category.Slug == category);

            if (!string.IsNullOrEmpty(level))
            {
                if (Enum.TryParse<Models.Enums.CourseLevel>(level, true, out var lvl))
                    query = query.Where(c => c.Level == lvl);
            }

            var userId = _userManager.GetUserId(User);
            var enrolledCourseIds = (await _db.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync()).ToHashSet();

            var courses = await query
                .OrderByDescending(c => c.PublishedAt)
                .Select(c => new CatalogCourseItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    CategoryName = c.Category.Name,
                    InstructorName = c.Instructor.FullName,
                    Rating = c.AverageRating,
                    RatingCount = c.RatingCount,
                    DurationHours = c.DurationHours,
                    Level = c.Level.ToString(),
                    Price = c.CourseType == Models.Enums.CourseType.Free ? "Miễn phí" : (c.PromoPrice ?? c.OriginalPrice).ToString("N0") + "đ",
                    PriceValue = c.PromoPrice ?? c.OriginalPrice,
                    CoverImageUrl = c.CoverImageUrl
                })
                .ToListAsync();

            foreach (var c in courses)
                c.IsEnrolled = enrolledCourseIds.Contains(c.Id);

            // Demo data if empty
            if (!courses.Any())
            {
                courses = new List<CatalogCourseItem>
                {
                    new() { Id = 1, Name = "Lập trình Python từ cơ bản đến nâng cao", CategoryName = "Lập trình", InstructorName = "Nguyễn Văn A", Rating = 4.8m, RatingCount = 234, DurationHours = 40, Level = "Beginner", Price = "599,000đ", PriceValue = 599000 },
                    new() { Id = 2, Name = "React & Redux Masterclass 2024", CategoryName = "Lập trình", InstructorName = "Trần Thị B", Rating = 4.6m, RatingCount = 189, DurationHours = 35, Level = "Intermediate", Price = "799,000đ", PriceValue = 799000 },
                    new() { Id = 3, Name = "UI/UX Design với Figma", CategoryName = "Thiết kế", InstructorName = "Lê Văn C", Rating = 4.9m, RatingCount = 312, DurationHours = 25, Level = "Beginner", Price = "Miễn phí", PriceValue = 0 },
                    new() { Id = 4, Name = "Data Science với Python & Pandas", CategoryName = "Data Science", InstructorName = "Phạm Thị D", Rating = 4.7m, RatingCount = 156, DurationHours = 50, Level = "Advanced", Price = "1,299,000đ", PriceValue = 1299000 },
                    new() { Id = 5, Name = "Digital Marketing A-Z", CategoryName = "Marketing", InstructorName = "Hoàng Văn E", Rating = 4.5m, RatingCount = 98, DurationHours = 20, Level = "Beginner", Price = "399,000đ", PriceValue = 399000 },
                    new() { Id = 6, Name = "Node.js Backend Development", CategoryName = "Lập trình", InstructorName = "Nguyễn Văn F", Rating = 4.4m, RatingCount = 145, DurationHours = 45, Level = "Intermediate", Price = "699,000đ", PriceValue = 699000 },
                    new() { Id = 7, Name = "Quản trị doanh nghiệp hiện đại", CategoryName = "Kinh doanh", InstructorName = "Trần Văn G", Rating = 4.3m, RatingCount = 67, DurationHours = 15, Level = "Beginner", Price = "299,000đ", PriceValue = 299000 },
                    new() { Id = 8, Name = "Machine Learning Fundamentals", CategoryName = "Data Science", InstructorName = "Lê Thị H", Rating = 4.8m, RatingCount = 201, DurationHours = 60, Level = "Advanced", Price = "1,499,000đ", PriceValue = 1499000 }
                };
            }

            if (!categories.Any())
            {
                categories = new List<CategoryItem>
                {
                    new() { Id = 1, Name = "Lập trình", Slug = "lap-trinh" },
                    new() { Id = 2, Name = "Thiết kế", Slug = "thiet-ke" },
                    new() { Id = 3, Name = "Data Science", Slug = "data-science" },
                    new() { Id = 4, Name = "Marketing", Slug = "marketing" },
                    new() { Id = 5, Name = "Kinh doanh", Slug = "kinh-doanh" }
                };
            }

            var model = new CatalogViewModel
            {
                Courses = courses,
                Categories = categories,
                SearchQuery = search,
                SelectedCategory = category,
                LevelFilter = level
            };

            return View(model);
        }
    }
}
