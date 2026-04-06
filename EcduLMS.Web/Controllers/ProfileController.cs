using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduLMS.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            SetLayout();

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                Phone = user.PhoneNumber,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                Role = roles.FirstOrDefault() ?? "Learner",
                CreatedAt = user.CreatedAt.ToString("dd/MM/yyyy"),
                AvatarInitial = string.IsNullOrEmpty(user.FullName) ? "?" : user.FullName[..1].ToUpper(),
                AvatarColor = "#2563EB"
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            SetLayout();

            var vm = new EditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                Phone = user.PhoneNumber,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                Role = roles.FirstOrDefault() ?? "Learner"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model, IFormFile? avatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            model.Email = user.Email ?? "";
            model.Role = roles.FirstOrDefault() ?? "Learner";
            SetLayout();

            if (!ModelState.IsValid)
                return View(model);

            user.FullName = model.FullName;
            user.PhoneNumber = model.Phone;
            user.Bio = model.Bio;

            // Upload avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(avatarFile.ContentType))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh JPG, PNG hoặc WebP.");
                    return View(model);
                }
                if (avatarFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File ảnh không được vượt quá 2MB.");
                    return View(model);
                }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsDir);

                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = $"{user.Id}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await avatarFile.CopyToAsync(stream);

                user.AvatarUrl = $"/uploads/avatars/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction(nameof(Index));
        }

        private void SetLayout()
        {
            if (User.IsInRole("Admin"))
                ViewData["Layout"] = "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";
            else if (User.IsInRole("Instructor"))
                ViewData["Layout"] = "~/Areas/Instructor/Views/Shared/_InstructorLayout.cshtml";
            else
                ViewData["Layout"] = "~/Areas/Learner/Views/Shared/_LearnerLayout.cshtml";
        }
    }
}
