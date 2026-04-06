using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.ViewModels.Home;
using Microsoft.AspNetCore.Identity;
using EduLMS.Web.Models.Identity;

namespace EduLMS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var featuredCourses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Where(c => c.Status == CourseStatus.Published)
            .OrderByDescending(c => c.AverageRating)
            .ThenByDescending(c => c.RatingCount)
            .Take(3)
            .ToListAsync();

        var instructorRole = await _context.Roles
            .Where(r => r.Name == "Instructor")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var instructors = new List<ApplicationUser>();
        if (instructorRole != null)
        {
            instructors = await _context.Users
                .Join(_context.UserRoles,
                    u => u.Id,
                    ur => ur.UserId,
                    (u, ur) => new { User = u, ur.RoleId })
                .Where(x => x.RoleId == instructorRole)
                .Select(x => (ApplicationUser)x.User)
                .Where(u => u.IsActive)
                .Take(4)
                .ToListAsync();
        }

        var viewModel = new HomeViewModel
        {
            FeaturedCourses = featuredCourses,
            Instructors = instructors
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
