using EduLMS.Web.Data;
using EduLMS.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Services
{
    public class ConflictResult
    {
        public string NewScheduleTitle { get; set; } = string.Empty;
        public DateTime NewScheduleStart { get; set; }
        public DateTime NewScheduleEnd { get; set; }
        public string ConflictingCourseName { get; set; } = string.Empty;
        public string ConflictingScheduleTitle { get; set; } = string.Empty;
        public DateTime ConflictingScheduleStart { get; set; }
        public DateTime ConflictingScheduleEnd { get; set; }
    }

    public class ConflictDetectionService
    {
        private readonly ApplicationDbContext _db;

        public ConflictDetectionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ConflictResult>> DetectAsync(string userId, int targetCourseId)
        {
            var now = DateTime.UtcNow;

            // Schedules of the course the learner wants to enroll in
            var targetSchedules = await _db.Schedules
                .Include(s => s.Course)
                .Where(s => s.CourseId == targetCourseId && s.StartTime > now)
                .ToListAsync();

            if (!targetSchedules.Any())
                return new List<ConflictResult>();

            // Courses learner is already enrolled in (active)
            var enrolledCourseIds = await _db.Enrollments
                .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.CourseId)
                .ToListAsync();

            if (!enrolledCourseIds.Any())
                return new List<ConflictResult>();

            // Existing schedules from enrolled courses
            var existingSchedules = await _db.Schedules
                .Include(s => s.Course)
                .Where(s => enrolledCourseIds.Contains(s.CourseId) && s.StartTime > now)
                .ToListAsync();

            var conflicts = new List<ConflictResult>();

            foreach (var target in targetSchedules)
            {
                foreach (var existing in existingSchedules)
                {
                    // Overlap: target.Start < existing.End AND target.End > existing.Start
                    if (target.StartTime < existing.EndTime && target.EndTime > existing.StartTime)
                    {
                        conflicts.Add(new ConflictResult
                        {
                            NewScheduleTitle = target.Title,
                            NewScheduleStart = target.StartTime.ToLocalTime(),
                            NewScheduleEnd = target.EndTime.ToLocalTime(),
                            ConflictingCourseName = existing.Course.Name,
                            ConflictingScheduleTitle = existing.Title,
                            ConflictingScheduleStart = existing.StartTime.ToLocalTime(),
                            ConflictingScheduleEnd = existing.EndTime.ToLocalTime()
                        });
                    }
                }
            }

            return conflicts;
        }
    }
}
