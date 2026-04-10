using EduLMS.Web.Areas.Instructor.Controllers;
using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Instructor;
using EcduLMS.Web.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcduLMS.Web.Tests.Instructor;

public class GradingControllerScoreValidationTests
{
    // Test case: Điểm số vượt quá max score của bài tập
    [Fact]
    public async Task SubmitGrade_WhenScoreOutOfRange_ShouldReturnBadRequestAndNotPersistGrade()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedSubmissionAsync(db);

        var userManagerMock = TestInfrastructure.CreateUserManagerMock(seed.InstructorId);
        var controller = new GradingController(db, userManagerMock.Object);
        TestInfrastructure.AttachAuthenticatedUser(controller, seed.InstructorId);

        var result = await controller.SubmitGrade(new SubmitGradeRequest
        {
            SubmissionId = seed.SubmissionId,
            Score = 11,
            PassStatus = "Pass",
            Comment = "Out of range"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errorText = badRequest.Value?.GetType().GetProperty("error")?.GetValue(badRequest.Value)?.ToString() ?? string.Empty;
        Assert.Contains("Điểm phải từ 0 đến", errorText);

        var submission = await db.AssignmentSubmissions.Include(s => s.Grade)
            .SingleAsync(s => s.Id == seed.SubmissionId);
        Assert.Equal(SubmissionStatus.Submitted, submission.Status);
        Assert.Null(submission.Grade);
    }
    // Test case: Điểm số âm
    [Fact]
    public async Task SubmitGrade_WhenScoreValid_ShouldPersistGradeAndMarkSubmissionAsGraded()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedSubmissionAsync(db);

        var userManagerMock = TestInfrastructure.CreateUserManagerMock(seed.InstructorId);
        var controller = new GradingController(db, userManagerMock.Object);
        TestInfrastructure.AttachAuthenticatedUser(controller, seed.InstructorId);

        var result = await controller.SubmitGrade(new SubmitGradeRequest
        {
            SubmissionId = seed.SubmissionId,
            Score = 8.5m,
            PassStatus = "Fail",
            Comment = "Need improvement"
        });

        Assert.IsType<OkObjectResult>(result);

        var submission = await db.AssignmentSubmissions.Include(s => s.Grade)
            .SingleAsync(s => s.Id == seed.SubmissionId);

        Assert.Equal(SubmissionStatus.Graded, submission.Status);
        Assert.NotNull(submission.Grade);
        Assert.Equal(8.5m, submission.Grade!.Score);
        Assert.Equal(PassStatus.Fail, submission.Grade.PassStatus);
        Assert.Equal(seed.InstructorId, submission.Grade.GradedById);
    }
// Các test case khác như điểm số âm, điểm số đúng, v.v. có thể được thêm tương tự
    private static async Task<GradingSeedData> SeedSubmissionAsync(ApplicationDbContext db)
    {
        var instructor = new ApplicationUser
        {
            Id = "instructor-grading",
            UserName = "instructor-grading",
            NormalizedUserName = "INSTRUCTOR-GRADING",
            Email = "instructor.grading@example.com",
            NormalizedEmail = "INSTRUCTOR.GRADING@EXAMPLE.COM",
            FullName = "Instructor Grading"
        };

        var learner = new ApplicationUser
        {
            Id = "learner-grading",
            UserName = "learner-grading",
            NormalizedUserName = "LEARNER-GRADING",
            Email = "learner.grading@example.com",
            NormalizedEmail = "LEARNER.GRADING@EXAMPLE.COM",
            FullName = "Learner Grading"
        };

        var category = new CourseCategory
        {
            Name = "Testing",
            Slug = $"testing-{Guid.NewGuid():N}"
        };

        var course = new Course
        {
            Code = $"GRD{Guid.NewGuid():N}"[..12],
            Name = "Grading Course",
            Category = category,
            InstructorId = instructor.Id,
            Instructor = instructor,
            Level = CourseLevel.Beginner,
            DurationHours = 2,
            CourseType = CourseType.Paid,
            OriginalPrice = 100000,
            Status = CourseStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var chapter = new Chapter
        {
            Course = course,
            Title = "Assignment Chapter",
            DisplayOrder = 1
        };

        var assignment = new Assignment
        {
            Chapter = chapter,
            Title = "Assignment 1",
            MaxScore = 10,
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow
        };

        var submission = new AssignmentSubmission
        {
            Assignment = assignment,
            UserId = learner.Id,
            User = learner,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(instructor, learner);
        db.CourseCategories.Add(category);
        db.Courses.Add(course);
        db.Chapters.Add(chapter);
        db.Assignments.Add(assignment);
        db.AssignmentSubmissions.Add(submission);

        await db.SaveChangesAsync();

        return new GradingSeedData
        {
            InstructorId = instructor.Id,
            SubmissionId = submission.Id
        };
    }

    private sealed class GradingSeedData
    {
        public string InstructorId { get; init; } = string.Empty;
        public int SubmissionId { get; init; }
    }
}
