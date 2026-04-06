using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduLMS.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            SetLayout();
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            SetLayout();

            if (!ModelState.IsValid)
                return View("Index", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "PasswordMismatch")
                    ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng.");
                else
                    ModelState.AddModelError("", error.Description);
            }

            return View("Index", model);
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
