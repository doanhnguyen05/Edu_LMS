using EduLMS.Web.Areas.Learner.Controllers;
using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EcduLMS.Web.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcduLMS.Web.Tests.Learner;

public class LessonControllerProgressTests
{
    [Fact]
    public async Task MarkComplete_FirstLesson_ShouldCreateProgressAndIncreaseEnrollmentPercent()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedCourseWithTwoLessonsAsync(db, firstLessonCompleted: false);

        var controller = new LessonController(db, TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(controller, seed.LearnerId);

        var result = await controller.MarkComplete(seed.Lesson1Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LessonController.View), redirect.ActionName);

        var enrollment = await db.Enrollments.SingleAsync(e => e.UserId == seed.LearnerId && e.CourseId == seed.CourseId);
        Assert.Equal(50m, enrollment.ProgressPercent);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);

        var progress = await db.LessonProgresses.SingleAsync(lp => lp.UserId == seed.LearnerId && lp.LessonId == seed.Lesson1Id);
        Assert.Equal(LessonStatus.Completed, progress.Status);
        Assert.NotNull(progress.CompletedAt);
    }

    [Fact]
    public async Task MarkComplete_SecondLessonWithoutFirstCompleted_ShouldRedirectToPreviousLesson()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedCourseWithTwoLessonsAsync(db, firstLessonCompleted: false);

        var controller = new LessonController(db, TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(controller, seed.LearnerId);

        var result = await controller.MarkComplete(seed.Lesson2Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LessonController.View), redirect.ActionName);
        Assert.Equal(seed.Lesson1Id, redirect.RouteValues?["id"]);

        var lesson2Completed = await db.LessonProgresses.AnyAsync(lp => lp.UserId == seed.LearnerId
            && lp.LessonId == seed.Lesson2Id
            && lp.Status == LessonStatus.Completed);
        Assert.False(lesson2Completed);
    }

    [Fact]
    public async Task MarkComplete_LastLesson_ShouldCompleteEnrollmentAt100Percent()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedCourseWithTwoLessonsAsync(db, firstLessonCompleted: true);

        var controller = new LessonController(db, TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(controller, seed.LearnerId);

        var result = await controller.MarkComplete(seed.Lesson2Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LessonController.View), redirect.ActionName);

        var enrollment = await db.Enrollments.SingleAsync(e => e.UserId == seed.LearnerId && e.CourseId == seed.CourseId);
        Assert.Equal(100m, enrollment.ProgressPercent);
        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);
        Assert.NotNull(enrollment.CompletedAt);
    }

    private static async Task<LessonSeedData> SeedCourseWithTwoLessonsAsync(ApplicationDbContext db, bool firstLessonCompleted)
    {
        var instructor = new ApplicationUser
        {
            Id = "instructor-progress",
            UserName = "instructor-progress",
            NormalizedUserName = "INSTRUCTOR-PROGRESS",
            Email = "instructor.progress@example.com",
            NormalizedEmail = "INSTRUCTOR.PROGRESS@EXAMPLE.COM",
            FullName = "Instructor Progress"
        };

        var learner = new ApplicationUser
        {
            Id = "learner-progress",
            UserName = "learner-progress",
            NormalizedUserName = "LEARNER-PROGRESS",
            Email = "learner.progress@example.com",
            NormalizedEmail = "LEARNER.PROGRESS@EXAMPLE.COM",
            FullName = "Learner Progress"
        };

        var category = new CourseCategory
        {
            Name = "Programming",
            Slug = $"programming-{Guid.NewGuid():N}"
        };

        var course = new Course
        {
            Code = $"PRG{Guid.NewGuid():N}"[..12],
            Name = "Progress Testing Course",
            Category = category,
            InstructorId = instructor.Id,
            Instructor = instructor,
            Level = CourseLevel.Beginner,
            DurationHours = 4,
            CourseType = CourseType.Paid,
            OriginalPrice = 100000,
            Status = CourseStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var chapter1 = new Chapter
        {
            Course = course,
            Title = "Chapter 1",
            DisplayOrder = 1
        };

        var chapter2 = new Chapter
        {
            Course = course,
            Title = "Chapter 2",
            DisplayOrder = 2
        };

        var lesson1 = new Lesson
        {
            Chapter = chapter1,
            Title = "Lesson 1",
            DisplayOrder = 1,
            DurationMinutes = 10
        };

        var lesson2 = new Lesson
        {
            Chapter = chapter2,
            Title = "Lesson 2",
            DisplayOrder = 1,
            DurationMinutes = 12
        };

        db.Users.AddRange(instructor, learner);
        db.CourseCategories.Add(category);
        db.Courses.Add(course);
        db.Chapters.AddRange(chapter1, chapter2);
        db.Lessons.AddRange(lesson1, lesson2);
        await db.SaveChangesAsync();

        db.Enrollments.Add(new Enrollment
        {
            UserId = learner.Id,
            CourseId = course.Id,
            Status = EnrollmentStatus.Active,
            ProgressPercent = firstLessonCompleted ? 50m : 0m,
            EnrolledAt = DateTime.UtcNow
        });

        if (firstLessonCompleted)
        {
            db.LessonProgresses.Add(new LessonProgress
            {
                UserId = learner.Id,
                LessonId = lesson1.Id,
                Status = LessonStatus.Completed,
                CompletedAt = DateTime.UtcNow.AddMinutes(-30)
            });
        }

        await db.SaveChangesAsync();

        return new LessonSeedData
        {
            LearnerId = learner.Id,
            CourseId = course.Id,
            Lesson1Id = lesson1.Id,
            Lesson2Id = lesson2.Id
        };
    }

    private sealed class LessonSeedData
    {
        public string LearnerId { get; init; } = string.Empty;
        public int CourseId { get; init; }
        public int Lesson1Id { get; init; }
        public int Lesson2Id { get; init; }
    }
}
