using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;

namespace EduLMS.Web.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;

        public LessonStatus Status { get; set; } = LessonStatus.NotStarted;

        public DateTime? CompletedAt { get; set; }
    }
}
