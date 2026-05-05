using EduLMS.Web.Models.Identity;
using EduLMS.Web.Services;
using EduLMS.Web.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduLMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IActivityLogService activityLogService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            model.Email = model.Email?.Trim() ?? string.Empty;

            ModelState.Clear();
            if (!TryValidateModel(model))
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                await TryWriteActivityLogAsync(user, roles, "Login");

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                if (roles.Contains("Instructor"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Instructor" });

                return RedirectToAction("Index", "Dashboard", new { area = "Learner" });
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Learner");
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action(
                    nameof(ResetPassword), "Account",
                    new { token, email = user.Email },
                    Request.Scheme)!;

                var body = $@"
                    <p>Xin chào <strong>{user.FullName ?? user.Email}</strong>,</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấp vào liên kết bên dưới để tiếp tục:</p>
                    <p><a href=""{resetLink}"">Đặt lại mật khẩu</a></p>
                    <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
                    <p>Nếu bạn không yêu cầu điều này, hãy bỏ qua email này.</p>";

                await _emailSender.SendEmailAsync(user.Email!, "Đặt lại mật khẩu - EduLMS", body);
            }

            // Always show success to prevent email enumeration
            TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, liên kết khôi phục mật khẩu đã được gửi.";
            return View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string? token, string? email)
        {
            if (token == null || email == null)
                return RedirectToAction(nameof(ForgotPassword));

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET/POST: /Account/Logout
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                await TryWriteActivityLogAsync(user, roles, "Logout");
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task TryWriteActivityLogAsync(
            ApplicationUser user,
            IEnumerable<string> roles,
            string action)
        {
            try
            {
                await _activityLogService.LogAsync(
                    action,
                    BuildActivityDescription(user, roles, action),
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unable to create activity log for user {UserId} and action {Action}.",
                    user.Id,
                    action);
            }
        }

        private static string BuildActivityDescription(
            ApplicationUser user,
            IEnumerable<string> roles,
            string action)
        {
            var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var displayName = string.IsNullOrWhiteSpace(user.FullName)
                ? user.Email ?? user.UserName ?? "Nguoi dung"
                : user.FullName;
            var actionText = string.Equals(action, "Logout", StringComparison.OrdinalIgnoreCase)
                ? "dang xuat he thong"
                : "dang nhap he thong";

            if (roleSet.Contains("Admin"))
            {
                return $"Admin {actionText}";
            }

            if (roleSet.Contains("Instructor"))
            {
                return $"Giang vien {displayName} {actionText}";
            }

            return $"{displayName} {actionText}";
        }
    }
}
