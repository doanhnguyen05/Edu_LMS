using EduLMS.Web.Data;
using EduLMS.Web.Models;

namespace EduLMS.Web.Services
{
    public interface IActivityLogService
    {
        Task LogAsync(
            string action,
            string description,
            string? userId = null,
            string? ipAddress = null,
            CancellationToken cancellationToken = default);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _db;

        public ActivityLogService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(
            string action,
            string description,
            string? userId = null,
            string? ipAddress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action is required.", nameof(action));
            }

            var normalizedAction = action.Trim();
            var normalizedDescription = string.IsNullOrWhiteSpace(description)
                ? normalizedAction
                : description.Trim();

            _db.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                Action = normalizedAction,
                Description = normalizedDescription,
                IpAddress = Truncate(ipAddress, 50),
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Length <= maxLength
                ? value
                : value[..maxLength];
        }
    }
}
