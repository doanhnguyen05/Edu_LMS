using ClosedXML.Excel;
using EduLMS.Web.Data;
using EduLMS.Web.Models.Enums;
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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await BuildDashboardViewModelAsync();
            return View(vm);
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> LiveData()
        {
            var vm = await BuildDashboardViewModelAsync();
            return Json(vm);
        }

        private static string VnDayName(DayOfWeek d) => d switch
        {
            DayOfWeek.Monday => "T2",
            DayOfWeek.Tuesday => "T3",
            DayOfWeek.Wednesday => "T4",
            DayOfWeek.Thursday => "T5",
            DayOfWeek.Friday => "T6",
            DayOfWeek.Saturday => "T7",
            _ => "CN"
        };

        private async Task<AdminDashboardViewModel> BuildDashboardViewModelAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            var totalCourses = await _context.Courses.CountAsync();
            var publishedCourses = await _context.Courses.CountAsync(c => c.Status == CourseStatus.Published);
            var totalStudyHours = await _context.Courses
                .Where(c => c.Status == CourseStatus.Published)
                .SumAsync(c => (decimal?)c.DurationHours) ?? 0m;
            var totalSchedules = await _context.Schedules.CountAsync();

            var instructorUsers = from user in _context.Users
                                  join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                  join role in _context.Roles on userRole.RoleId equals role.Id
                                  where role.Name == "Instructor"
                                  select user;

            var totalInstructors = await instructorUsers
                .Select(u => u.Id)
                .Distinct()
                .CountAsync();

            var activeInstructors = await instructorUsers
                .Where(u => u.IsActive)
                .Select(u => u.Id)
                .Distinct()
                .CountAsync();

            var trackedEnrollmentsQuery = _context.Enrollments
                .Where(e => e.Status != EnrollmentStatus.Dropped);

            var trackedEnrollments = await trackedEnrollmentsQuery.CountAsync();
            var averageProgressRate = trackedEnrollments > 0
                ? Math.Round(await trackedEnrollmentsQuery.AverageAsync(e => e.ProgressPercent), 1)
                : 0m;

            var recentLogs = await _context.ActivityLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new
                {
                    a.Action,
                    a.Description,
                    a.CreatedAt
                })
                .ToListAsync();

            var recentActivities = recentLogs
                .Select(a => new AdminDashboardViewModel.ActivityLogItem
                {
                    Icon = ResolveActivityIcon(a.Action),
                    IconColor = ResolveActivityColor(a.Action),
                    Description = a.Description ?? a.Action,
                    Time = FormatTimeAgo(a.CreatedAt)
                })
                .ToList();

            var today = DateTime.UtcNow.Date;
            var last7 = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
            var cutoff = last7.First();

            var accessLogDates = await _context.ActivityLogs
                .AsNoTracking()
                .Where(a => a.CreatedAt >= cutoff && a.Action == "Login")
                .Select(a => a.CreatedAt.Date)
                .ToListAsync();

            var accessByDay = last7.Select(d => accessLogDates.Count(l => l == d)).ToArray();
            var accessLabels = last7.Select(d => VnDayName(d.DayOfWeek)).ToArray();

            return new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalCourses = totalCourses,
                PublishedCourses = publishedCourses,
                TotalInstructors = totalInstructors,
                ActiveInstructors = activeInstructors,
                TotalStudyHours = totalStudyHours,
                TotalSchedules = totalSchedules,
                TrackedEnrollments = trackedEnrollments,
                AverageProgressRate = averageProgressRate,
                RecentActivities = recentActivities,
                AccessChartLabels = accessLabels,
                AccessChartData = accessByDay
            };
        }

        private static string ResolveActivityIcon(string action)
        {
            if (action.Contains("Logout", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-box-arrow-right";
            }

            if (action.Contains("Login", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-box-arrow-in-right";
            }

            if (action.Contains("Notification", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-bell";
            }

            if (action.Contains("Create", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-plus-circle";
            }

            if (action.Contains("Enroll", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-person-plus";
            }

            if (action.Contains("Submit", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-file-earmark-arrow-up";
            }

            if (action.Contains("Complete", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-check-circle";
            }

            return "bi-activity";
        }

        private static string ResolveActivityColor(string action)
        {
            if (action.Contains("Logout", StringComparison.OrdinalIgnoreCase))
            {
                return "#F59E0B";
            }

            if (action.Contains("Login", StringComparison.OrdinalIgnoreCase))
            {
                return "#2563EB";
            }

            if (action.Contains("Notification", StringComparison.OrdinalIgnoreCase))
            {
                return "#0EA5E9";
            }

            if (action.Contains("Create", StringComparison.OrdinalIgnoreCase))
            {
                return "#16A34A";
            }

            if (action.Contains("Enroll", StringComparison.OrdinalIgnoreCase))
            {
                return "#7C3AED";
            }

            if (action.Contains("Submit", StringComparison.OrdinalIgnoreCase))
            {
                return "#F97316";
            }

            if (action.Contains("Complete", StringComparison.OrdinalIgnoreCase))
            {
                return "#059669";
            }

            return "#64748B";
        }

        // === BÁO CÁO NGƯỜI DÙNG ===
        public async Task<IActionResult> ExportUsersReport()
        {
            var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Người dùng");

            // Header
            var headers = new[] { "Họ tên", "Email", "Vai trò", "Số điện thoại", "Ngày đăng ký", "Trạng thái" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                ws.Cell(row, 1).Value = user.FullName;
                ws.Cell(row, 2).Value = user.Email;
                ws.Cell(row, 3).Value = roles.FirstOrDefault() ?? "N/A";
                ws.Cell(row, 4).Value = user.PhoneNumber ?? "N/A";
                ws.Cell(row, 5).Value = user.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(row, 6).Value = user.IsActive ? "Hoạt động" : "Đã khóa";
                row++;
            }

            ws.Columns().AdjustToContents();
            return ExcelFile(workbook, "BaoCao_NguoiDung");
        }

        // === BÁO CÁO KHÓA HỌC ===
        public async Task<IActionResult> ExportCoursesReport()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Khóa học");

            var headers = new[] { "Mã", "Tên khóa học", "Giảng viên", "Danh mục", "Số học viên", "Tỷ lệ hoàn thành (%)", "Giờ học", "Giá gốc", "Trạng thái" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var c in courses)
            {
                var enrollCount = c.Enrollments.Count;
                var completedCount = c.Enrollments.Count(e => e.Status == Models.Enums.EnrollmentStatus.Completed);
                ws.Cell(row, 1).Value = c.Code;
                ws.Cell(row, 2).Value = c.Name;
                ws.Cell(row, 3).Value = c.Instructor?.FullName ?? "N/A";
                ws.Cell(row, 4).Value = c.Category?.Name ?? "N/A";
                ws.Cell(row, 5).Value = enrollCount;
                ws.Cell(row, 6).Value = enrollCount > 0 ? Math.Round((double)completedCount / enrollCount * 100, 1) : 0;
                ws.Cell(row, 7).Value = c.DurationHours;
                ws.Cell(row, 8).Value = c.OriginalPrice;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0 ₫";
                ws.Cell(row, 9).Value = c.Status.ToString();
                row++;
            }

            ws.Columns().AdjustToContents();
            return ExcelFile(workbook, "BaoCao_KhoaHoc");
        }

        // === BÁO CÁO ĐIỂM SỐ ===
        public async Task<IActionResult> ExportGradesReport()
        {
            var grades = await _context.Grades
                .Include(g => g.Submission).ThenInclude(s => s.Assignment).ThenInclude(a => a.Chapter).ThenInclude(ch => ch.Course)
                .Include(g => g.Submission).ThenInclude(s => s.User)
                .Include(g => g.GradedBy)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Điểm số");

            var headers = new[] { "Học viên", "Email", "Khóa học", "Bài tập", "Điểm", "Kết quả", "Người chấm", "Ngày chấm" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var g in grades)
            {
                ws.Cell(row, 1).Value = g.Submission?.User?.FullName ?? "N/A";
                ws.Cell(row, 2).Value = g.Submission?.User?.Email ?? "N/A";
                ws.Cell(row, 3).Value = g.Submission?.Assignment?.Chapter?.Course?.Name ?? "N/A";
                ws.Cell(row, 4).Value = g.Submission?.Assignment?.Title ?? "N/A";
                ws.Cell(row, 5).Value = g.Score;
                ws.Cell(row, 6).Value = g.PassStatus.ToString();
                ws.Cell(row, 7).Value = g.GradedBy?.FullName ?? "N/A";
                ws.Cell(row, 8).Value = g.GradedAt.ToString("dd/MM/yyyy HH:mm");
                row++;
            }

            ws.Columns().AdjustToContents();
            return ExcelFile(workbook, "BaoCao_DiemSo");
        }

        // === BÁO CÁO DOANH THU ===
        public async Task<IActionResult> ExportPaymentsReport()
        {
            var payments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Doanh thu");

            var headers = new[] { "Học viên", "Email", "Khóa học", "Số tiền", "Phương thức", "Trạng thái", "Ngày thanh toán" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var p in payments)
            {
                ws.Cell(row, 1).Value = p.User?.FullName ?? "N/A";
                ws.Cell(row, 2).Value = p.User?.Email ?? "N/A";
                ws.Cell(row, 3).Value = p.Course?.Name ?? "N/A";
                ws.Cell(row, 4).Value = p.Amount;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0 ₫";
                ws.Cell(row, 5).Value = p.PaymentMethod.ToString();
                ws.Cell(row, 6).Value = p.Status.ToString();
                ws.Cell(row, 7).Value = p.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                row++;
            }

            ws.Columns().AdjustToContents();
            return ExcelFile(workbook, "BaoCao_DoanhThu");
        }

        private FileContentResult ExcelFile(XLWorkbook workbook, string baseName)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"{baseName}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private static string FormatTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.UtcNow - createdAt;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            return createdAt.ToString("dd/MM/yyyy");
        }
    }
}
