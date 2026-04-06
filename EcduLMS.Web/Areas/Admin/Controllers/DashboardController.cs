using ClosedXML.Excel;
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
            var totalUsers = await _userManager.Users.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalEnrollments = await _context.Enrollments.CountAsync();
            var completedEnrollments = await _context.Enrollments
                .CountAsync(e => e.Status == Models.Enums.EnrollmentStatus.Completed);
            var totalStudyHours = await _context.Courses.SumAsync(c => c.DurationHours);

            var recentLogs = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new AdminDashboardViewModel.ActivityLogItem
                {
                    Icon = a.Action.Contains("Login") ? "bi-box-arrow-in-right" :
                           a.Action.Contains("Create") ? "bi-plus-circle" :
                           a.Action.Contains("Enroll") ? "bi-person-plus" :
                           a.Action.Contains("Complete") ? "bi-check-circle" :
                           "bi-activity",
                    IconColor = a.Action.Contains("Login") ? "#2563EB" :
                                a.Action.Contains("Create") ? "#16A34A" :
                                a.Action.Contains("Enroll") ? "#7C3AED" :
                                a.Action.Contains("Complete") ? "#059669" :
                                "#64748B",
                    Description = a.Description ?? a.Action,
                    Time = FormatTimeAgo(a.CreatedAt)
                })
                .ToListAsync();

            // Build last-7-days access chart data from activity logs
            var today = DateTime.UtcNow.Date;
            var last7 = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
            var cutoff = last7.First();

            var allLogDates = await _context.ActivityLogs
                .Where(a => a.CreatedAt >= cutoff)
                .Select(a => a.CreatedAt.Date)
                .ToListAsync();

            var accessByDay = last7.Select(d => allLogDates.Count(l => l == d)).ToArray();
            var accessLabels = last7.Select(d => VnDayName(d.DayOfWeek)).ToArray();

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalCourses = totalCourses,
                TotalGroups = await _context.Enrollments.Select(e => e.UserId).Distinct().CountAsync(),
                TotalStudyHours = totalStudyHours,
                CompletionRate = totalEnrollments > 0
                    ? Math.Round((decimal)completedEnrollments / totalEnrollments * 100, 1)
                    : 0,
                RecentActivities = recentLogs,
                AccessChartLabels = accessLabels,
                AccessChartData = accessByDay
            };

            return View(vm);
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
