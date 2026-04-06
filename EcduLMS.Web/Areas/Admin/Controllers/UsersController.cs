using EduLMS.Web.Data;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, string? role, string? status)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FullName.Contains(search) || u.Email!.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "active")
                    usersQuery = usersQuery.Where(u => u.IsActive);
                else if (status == "locked")
                    usersQuery = usersQuery.Where(u => !u.IsActive);
            }

            var users = await usersQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var colors = new[] { "#2563EB", "#7C3AED", "#DC2626", "#059669", "#D97706", "#0891B2" };

            var userItems = new List<UserItem>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Learner";

                if (!string.IsNullOrWhiteSpace(role) && !string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase))
                    continue;

                userItems.Add(new UserItem
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Phone = user.PhoneNumber ?? "N/A",
                    Role = userRole,
                    IsActive = user.IsActive,
                    AvatarInitial = string.IsNullOrEmpty(user.FullName) ? "?" : user.FullName[..1].ToUpper(),
                    AvatarColor = colors[Math.Abs(user.Id.GetHashCode()) % colors.Length]
                });
            }

            var vm = new UserListViewModel
            {
                Users = userItems,
                SearchQuery = search,
                RoleFilter = role,
                StatusFilter = status
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string FullName, string Email, string? Phone, string Role)
        {
            var user = new ApplicationUser
            {
                UserName = Email,
                Email = Email,
                FullName = FullName,
                PhoneNumber = Phone,
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, "Default@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Role);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string Id, string FullName, string Email, string? Phone, string Role, bool IsActive)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null) return NotFound();

            user.FullName = FullName;
            user.Email = Email;
            user.UserName = Email;
            user.PhoneNumber = Phone;
            user.IsActive = IsActive;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, Role);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
