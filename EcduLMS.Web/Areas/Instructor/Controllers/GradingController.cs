using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class GradingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GradingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            var userId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;

            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourse = courseId;

            var courseIds = courseId.HasValue
                ? new List<int> { courseId.Value }
                : courses.Select(c => c.Id).ToList();

            var chapterIds = await _context.Chapters
                .Where(ch => courseIds.Contains(ch.CourseId))
                .Select(ch => ch.Id)
                .ToListAsync();

            var assignmentIds = await _context.Assignments
                .Where(a => chapterIds.Contains(a.ChapterId))
                .Select(a => a.Id)
                .ToListAsync();

            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment).ThenInclude(a => a.Chapter).ThenInclude(ch => ch.Course)
                .Include(s => s.User)
                .Include(s => s.Grade)
                .Where(s => assignmentIds.Contains(s.AssignmentId) && s.Status != SubmissionStatus.Draft)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            var model = new GradingListViewModel
            {
                UngradedSubmissions = submissions
                    .Where(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Returned)
                    .Select(s => new SubmissionItem
                    {
                        Id = s.Id,
                        AssignmentTitle = s.Assignment.Title,
                        CourseName = s.Assignment.Chapter.Course.Name,
                        StudentName = s.User.FullName,
                        SubmittedAgo = FormatTimeAgo(s.SubmittedAt, now),
                        Status = s.Status.ToString()
                    }).ToList(),
                GradedSubmissions = submissions
                    .Where(s => s.Status == SubmissionStatus.Graded)
                    .Select(s => new SubmissionItem
                    {
                        Id = s.Id,
                        AssignmentTitle = s.Assignment.Title,
                        CourseName = s.Assignment.Chapter.Course.Name,
                        StudentName = s.User.FullName,
                        SubmittedAgo = FormatTimeAgo(s.SubmittedAt, now),
                        Score = s.Grade?.Score,
                        Status = s.Status.ToString()
                    }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = _userManager.GetUserId(User);

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment).ThenInclude(a => a.Chapter).ThenInclude(ch => ch.Course)
                .Include(s => s.User)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            // Security: verify instructor owns this course
            if (submission.Assignment.Chapter.Course.InstructorId != userId)
                return Forbid();

            var model = new GradingDetailViewModel
            {
                SubmissionId = submission.Id,
                AssignmentTitle = submission.Assignment.Title,
                AssignmentDescription = submission.Assignment.Description,
                MaxScore = submission.Assignment.MaxScore,
                CourseName = submission.Assignment.Chapter.Course.Name,
                StudentName = submission.User.FullName,
                FileUrl = submission.FileUrl,
                FileName = submission.FileName,
                Content = submission.Content,
                SubmittedAt = submission.SubmittedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                Status = submission.Status.ToString(),
                ExistingScore = submission.Grade?.Score,
                ExistingComment = submission.Grade?.Comment,
                ExistingPassStatus = submission.Grade?.PassStatus.ToString()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitGrade([FromBody] SubmitGradeRequest req)
        {
            var userId = _userManager.GetUserId(User);

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment).ThenInclude(a => a.Chapter).ThenInclude(ch => ch.Course)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == req.SubmissionId);

            if (submission == null) return NotFound();
            if (submission.Assignment.Chapter.Course.InstructorId != userId) return Forbid();

            var maxScore = submission.Assignment.MaxScore;
            if (req.Score < 0 || req.Score > maxScore)
                return BadRequest(new { error = $"Điểm phải từ 0 đến {maxScore}" });

            var passStatus = Enum.TryParse<PassStatus>(req.PassStatus, out var ps) ? ps : PassStatus.Pass;

            if (submission.Grade != null)
            {
                // Update existing grade
                submission.Grade.Score = req.Score;
                submission.Grade.PassStatus = passStatus;
                submission.Grade.Comment = req.Comment;
                submission.Grade.GradedAt = DateTime.UtcNow;
                submission.Grade.GradedById = userId!;
            }
            else
            {
                _context.Grades.Add(new Grade
                {
                    SubmissionId = req.SubmissionId,
                    Score = req.Score,
                    PassStatus = passStatus,
                    Comment = req.Comment,
                    GradedById = userId!,
                    GradedAt = DateTime.UtcNow
                });
            }

            submission.Status = SubmissionStatus.Graded;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnSubmission([FromBody] ReturnSubmissionRequest req)
        {
            var userId = _userManager.GetUserId(User);

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment).ThenInclude(a => a.Chapter).ThenInclude(ch => ch.Course)
                .FirstOrDefaultAsync(s => s.Id == req.SubmissionId);

            if (submission == null) return NotFound();
            if (submission.Assignment.Chapter.Course.InstructorId != userId) return Forbid();

            submission.Status = SubmissionStatus.Returned;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        private static string FormatTimeAgo(DateTime? dateTime, DateTime now)
        {
            if (dateTime == null) return "N/A";
            var diff = now - dateTime.Value;
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            return dateTime.Value.ToString("dd/MM/yyyy");
        }
    }
}
