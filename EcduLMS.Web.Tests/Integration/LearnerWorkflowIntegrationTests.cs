using EduLMS.Web.Areas.Instructor.Controllers;
using EduLMS.Web.Areas.Learner.Controllers;
using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Instructor;
using EduLMS.Web.ViewModels.Learner;
using EcduLMS.Web.Tests.TestHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EcduLMS.Web.Tests.Integration;

public class LearnerWorkflowIntegrationTests
{
    [Fact]
    public async Task LearningPath_WhenPrerequisiteIncomplete_ShouldLockLessonAndBlockAssignment()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedLearnerWorkflowAsync(db);

        var trainingController = new TrainingController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(trainingController, seed.LearnerId);

        var trainingResult = await trainingController.Detail(seed.CourseId);
        var trainingView = Assert.IsType<ViewResult>(trainingResult);
        var trainingModel = Assert.IsType<CourseDetailViewModel>(trainingView.Model);

        var secondLesson = Assert.Single(trainingModel.Lessons, x => x.Id == seed.Lesson2Id);
        Assert.True(secondLesson.IsLocked);

        var blockedTrainingAssignment = Assert.Single(trainingModel.Assignments, x => x.Id == seed.AssignmentId);
        Assert.False(blockedTrainingAssignment.CanStart);
        Assert.False(string.IsNullOrWhiteSpace(blockedTrainingAssignment.BlockedReason));

        var gradesController = new GradesController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(gradesController, seed.LearnerId);

        var gradesResult = await gradesController.CourseDetail(seed.CourseId);
        var gradesView = Assert.IsType<ViewResult>(gradesResult);
        var gradesModel = Assert.IsType<GradeCourseDetailViewModel>(gradesView.Model);

        var blockedGradeAssignment = Assert.Single(gradesModel.Assignments, x => x.AssignmentId == seed.AssignmentId);
        Assert.Equal(0, blockedGradeAssignment.SubmissionId);
        Assert.Null(blockedGradeAssignment.Score);
        Assert.False(blockedGradeAssignment.CanStartAssignment);
        Assert.False(string.IsNullOrWhiteSpace(blockedGradeAssignment.StartBlockedReason));
    }

    [Fact]
    public async Task LearningPath_WhenLearnerCompletesLessons_ShouldUnlockAssignmentInTrainingAndGrades()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedLearnerWorkflowAsync(db);

        var lessonController = new LessonController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(lessonController, seed.LearnerId);

        var completeFirst = await lessonController.MarkComplete(seed.Lesson1Id);
        Assert.IsType<RedirectToActionResult>(completeFirst);

        var completeSecond = await lessonController.MarkComplete(seed.Lesson2Id);
        Assert.IsType<RedirectToActionResult>(completeSecond);

        var enrollment = await db.Enrollments
            .SingleAsync(x => x.UserId == seed.LearnerId && x.CourseId == seed.CourseId);
        Assert.Equal(100m, enrollment.ProgressPercent);
        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);

        var trainingController = new TrainingController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(trainingController, seed.LearnerId);

        var trainingResult = await trainingController.Detail(seed.CourseId);
        var trainingView = Assert.IsType<ViewResult>(trainingResult);
        var trainingModel = Assert.IsType<CourseDetailViewModel>(trainingView.Model);

        var unlockedLesson = Assert.Single(trainingModel.Lessons, x => x.Id == seed.Lesson2Id);
        Assert.False(unlockedLesson.IsLocked);

        var startableTrainingAssignment = Assert.Single(trainingModel.Assignments, x => x.Id == seed.AssignmentId);
        Assert.True(startableTrainingAssignment.CanStart);

        var gradesController = new GradesController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(gradesController, seed.LearnerId);

        var gradesResult = await gradesController.CourseDetail(seed.CourseId);
        var gradesView = Assert.IsType<ViewResult>(gradesResult);
        var gradesModel = Assert.IsType<GradeCourseDetailViewModel>(gradesView.Model);

        var startableGradeAssignment = Assert.Single(gradesModel.Assignments, x => x.AssignmentId == seed.AssignmentId);
        Assert.True(startableGradeAssignment.CanStartAssignment);
        Assert.Equal(0, startableGradeAssignment.SubmissionId);
    }

    [Fact]
    public async Task AssignmentFlow_SubmitThenGrade_ShouldAppearAsPassInLearnerGrades()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedLearnerWorkflowAsync(db);

        var lessonController = new LessonController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(lessonController, seed.LearnerId);

        await lessonController.MarkComplete(seed.Lesson1Id);
        await lessonController.MarkComplete(seed.Lesson2Id);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(x => x.WebRootPath).Returns("/tmp");

        var assignmentController = new AssignmentController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object,
            envMock.Object);
        TestInfrastructure.AttachAuthenticatedUser(assignmentController, seed.LearnerId);

        var submitResult = await assignmentController.SubmitFinal(
            assignmentId: seed.AssignmentId,
            content: "Bai lam tich hop",
            file: null);
        Assert.IsType<RedirectToActionResult>(submitResult);

        var submission = await db.AssignmentSubmissions
            .SingleAsync(x => x.AssignmentId == seed.AssignmentId && x.UserId == seed.LearnerId);
        Assert.Equal(SubmissionStatus.Submitted, submission.Status);

        var gradingController = new GradingController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.InstructorId).Object);
        TestInfrastructure.AttachAuthenticatedUser(gradingController, seed.InstructorId);

        var gradeResult = await gradingController.SubmitGrade(new SubmitGradeRequest
        {
            SubmissionId = submission.Id,
            Score = 9m,
            PassStatus = "Pass",
            Comment = "Tot"
        });
        Assert.IsType<OkObjectResult>(gradeResult);

        var gradesController = new GradesController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(gradesController, seed.LearnerId);

        var courseDetailResult = await gradesController.CourseDetail(seed.CourseId);
        var courseDetailView = Assert.IsType<ViewResult>(courseDetailResult);
        var courseDetailModel = Assert.IsType<GradeCourseDetailViewModel>(courseDetailView.Model);

        var gradedAssignment = Assert.Single(courseDetailModel.Assignments, x => x.AssignmentId == seed.AssignmentId);
        Assert.Equal("Pass", gradedAssignment.Status);
        Assert.Equal(9m, gradedAssignment.Score);
        Assert.True(gradedAssignment.SubmissionId > 0);

        var gradeDetailResult = await gradesController.GradeDetail(gradedAssignment.SubmissionId);
        var gradeDetailView = Assert.IsType<ViewResult>(gradeDetailResult);
        var gradeDetailModel = Assert.IsType<GradeDetailViewModel>(gradeDetailView.Model);
        Assert.Equal(9m, gradeDetailModel.Score);
    }

    private static async Task<LearnerWorkflowSeed> SeedLearnerWorkflowAsync(ApplicationDbContext db)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var instructor = new ApplicationUser
        {
            Id = $"inst-int-{suffix}",
            UserName = $"inst-int-{suffix}",
            NormalizedUserName = $"INST-INT-{suffix}".ToUpperInvariant(),
            Email = $"inst.{suffix}@example.com",
            NormalizedEmail = $"INST.{suffix}@EXAMPLE.COM",
            FullName = "Instructor Integration"
        };

        var learner = new ApplicationUser
        {
            Id = $"learner-int-{suffix}",
            UserName = $"learner-int-{suffix}",
            NormalizedUserName = $"LEARNER-INT-{suffix}".ToUpperInvariant(),
            Email = $"learner.{suffix}@example.com",
            NormalizedEmail = $"LEARNER.{suffix}@EXAMPLE.COM",
            FullName = "Learner Integration"
        };

        var category = new CourseCategory
        {
            Name = "Integration",
            Slug = $"integration-{suffix}"
        };

        var course = new Course
        {
            Code = $"INT{suffix}".ToUpperInvariant(),
            Name = $"Integration Course {suffix}",
            Category = category,
            InstructorId = instructor.Id,
            Instructor = instructor,
            Level = CourseLevel.Beginner,
            DurationHours = 3,
            CourseType = CourseType.Paid,
            OriginalPrice = 150000,
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
            DurationMinutes = 20
        };

        var lesson2 = new Lesson
        {
            Chapter = chapter2,
            Title = "Lesson 2",
            DisplayOrder = 1,
            DurationMinutes = 25
        };

        var assignment = new Assignment
        {
            Chapter = chapter2,
            Lesson = lesson2,
            Title = "Final Assignment",
            Description = "Integration assignment",
            MaxScore = 10,
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(instructor, learner);
        db.CourseCategories.Add(category);
        db.Courses.Add(course);
        db.Chapters.AddRange(chapter1, chapter2);
        db.Lessons.AddRange(lesson1, lesson2);
        db.Assignments.Add(assignment);

        await db.SaveChangesAsync();

        db.Enrollments.Add(new Enrollment
        {
            UserId = learner.Id,
            CourseId = course.Id,
            Status = EnrollmentStatus.Active,
            ProgressPercent = 0,
            EnrolledAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return new LearnerWorkflowSeed
        {
            InstructorId = instructor.Id,
            LearnerId = learner.Id,
            CourseId = course.Id,
            Lesson1Id = lesson1.Id,
            Lesson2Id = lesson2.Id,
            AssignmentId = assignment.Id
        };
    }

    private sealed class LearnerWorkflowSeed
    {
        public string InstructorId { get; init; } = string.Empty;
        public string LearnerId { get; init; } = string.Empty;
        public int CourseId { get; init; }
        public int Lesson1Id { get; init; }
        public int Lesson2Id { get; init; }
        public int AssignmentId { get; init; }
    }
}
