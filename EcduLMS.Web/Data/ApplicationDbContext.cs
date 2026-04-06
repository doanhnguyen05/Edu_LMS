using EduLMS.Web.Models;
using EduLMS.Web.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduLMS.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonResource> LessonResources { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRule> NotificationRules { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<InstructorEarning> InstructorEarnings { get; set; }
        public DbSet<PayoutBatch> PayoutBatches { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===== CourseCategory =====
            builder.Entity<CourseCategory>(entity =>
            {
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            // ===== Course =====
            builder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Status);

                entity.Property(e => e.Level).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.CourseType).HasConversion<string>().HasMaxLength(10);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Courses)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Instructor)
                    .WithMany(u => u.InstructorCourses)
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Chapter =====
            builder.Entity<Chapter>(entity =>
            {
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Chapters)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Lesson =====
            builder.Entity<Lesson>(entity =>
            {
                entity.HasOne(e => e.Chapter)
                    .WithMany(c => c.Lessons)
                    .HasForeignKey(e => e.ChapterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== LessonResource =====
            builder.Entity<LessonResource>(entity =>
            {
                entity.HasOne(e => e.Lesson)
                    .WithMany(l => l.Resources)
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Enrollment =====
            builder.Entity<Enrollment>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Enrollments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== LessonProgress =====
            builder.Entity<LessonProgress>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.LessonId }).IsUnique();
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.LessonProgresses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Lesson)
                    .WithMany(l => l.Progresses)
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Assignment =====
            builder.Entity<Assignment>(entity =>
            {
                entity.HasOne(e => e.Chapter)
                    .WithMany(c => c.Assignments)
                    .HasForeignKey(e => e.ChapterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== AssignmentSubmission =====
            builder.Entity<AssignmentSubmission>(entity =>
            {
                entity.HasIndex(e => new { e.AssignmentId, e.UserId });

                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Submissions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Grade =====
            builder.Entity<Grade>(entity =>
            {
                entity.HasIndex(e => e.SubmissionId).IsUnique();

                entity.Property(e => e.PassStatus).HasConversion<string>().HasMaxLength(10);

                entity.HasOne(e => e.Submission)
                    .WithOne(s => s.Grade)
                    .HasForeignKey<Grade>(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.GradedBy)
                    .WithMany()
                    .HasForeignKey(e => e.GradedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Payment =====
            builder.Entity<Payment>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Payments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Payments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Schedule =====
            builder.Entity<Schedule>(entity =>
            {
                entity.HasIndex(e => e.InstructorId);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Schedules)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Instructor)
                    .WithMany(u => u.Schedules)
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CalendarEvent =====
            builder.Entity<CalendarEvent>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.EventType).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Notification =====
            builder.Entity<Notification>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.IsRead });

                entity.Property(e => e.Channel).HasConversion<string>().HasMaxLength(10);
                entity.Property(e => e.Message).HasColumnType("TEXT");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SenderInstructor)
                    .WithMany()
                    .HasForeignKey(e => e.SenderInstructorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.SenderAdmin)
                    .WithMany()
                    .HasForeignKey(e => e.SenderAdminId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== NotificationRule =====
            builder.Entity<NotificationRule>(entity =>
            {
                entity.HasIndex(e => e.EventType).IsUnique();
                entity.Property(e => e.Channel).HasConversion<string>().HasMaxLength(10);
            });

            // ===== InstructorEarning =====
            builder.Entity<InstructorEarning>(entity =>
            {
                entity.HasIndex(e => e.InstructorId);
                entity.HasIndex(e => e.Status);

                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.Instructor)
                    .WithMany()
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Payment)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PayoutBatch)
                    .WithMany(b => b.Earnings)
                    .HasForeignKey(e => e.PayoutBatchId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== PayoutBatch =====
            builder.Entity<PayoutBatch>(entity =>
            {
                entity.HasIndex(e => e.InstructorId);

                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Instructor)
                    .WithMany()
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ActivityLog =====
            builder.Entity<ActivityLog>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ActivityLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
