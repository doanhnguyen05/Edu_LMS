using EduLMS.Web.Data;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Learner;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Controllers
{
    // Public controller — no [Authorize], accessible to anyone
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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

            // Check enrollment only if user is logged in
            var userId = _userManager.GetUserId(User);
            HashSet<int> enrolledCourseIds = new();
            if (userId != null)
            {
                enrolledCourseIds = (await _db.Enrollments
                    .Where(e => e.UserId == userId)
                    .Select(e => e.CourseId)
                    .ToListAsync()).ToHashSet();
            }

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
            var isEnrolled = false;
            if (userId != null)
                isEnrolled = await _db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == id);

            var chapters = course.Chapters.Take(5).Select(ch => ch.Title).ToList();

            // Anonymous → login page; authenticated → checkout
            var actionUrl = userId == null
                ? Url.Action("Login", "Account", new { returnUrl = "/Learner/Catalog" })
                : Url.Action("Checkout", "Payment", new { area = "Learner", id = course.Id });

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
                isAnonymous = userId == null,
                actionUrl,
                trainingUrl
            });
        }
    }
}
