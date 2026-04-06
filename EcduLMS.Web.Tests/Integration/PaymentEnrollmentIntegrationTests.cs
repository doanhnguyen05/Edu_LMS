using EduLMS.Web.Areas.Learner.Controllers;
using EduLMS.Web.Controllers;
using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EduLMS.Web.ViewModels.Learner;
using EcduLMS.Web.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace EcduLMS.Web.Tests.Integration;

public class PaymentEnrollmentIntegrationTests
{
    [Fact]
    public async Task PaymentWorkflow_WhenWebhookCompletesPayment_ShouldShowCourseInLearnerMyTraining()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedPendingPaymentAsync(db);

        var webhookController = new WebhookController(
            db,
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<WebhookController>>());

        var webhookResult = await webhookController.SePay(new SePayWebhookPayload
        {
            Content = $"Thanh toan ma don {seed.TransactionCode}",
            TransferAmount = seed.Amount
        });
        Assert.IsType<OkObjectResult>(webhookResult);

        var payment = await db.Payments.SingleAsync(x => x.Id == seed.PaymentId);
        Assert.Equal(PaymentStatus.Completed, payment.Status);

        var enrollment = await db.Enrollments
            .SingleOrDefaultAsync(x => x.UserId == seed.LearnerId && x.CourseId == seed.CourseId);
        Assert.NotNull(enrollment);
        Assert.Equal(EnrollmentStatus.Active, enrollment!.Status);

        var trainingController = new TrainingController(
            db,
            TestInfrastructure.CreateUserManagerMock(seed.LearnerId).Object);
        TestInfrastructure.AttachAuthenticatedUser(trainingController, seed.LearnerId);

        var trainingResult = await trainingController.Index();
        var trainingView = Assert.IsType<ViewResult>(trainingResult);
        var trainingModel = Assert.IsType<TrainingViewModel>(trainingView.Model);

        var activeCourse = Assert.Single(trainingModel.ActiveCourses, x => x.Id == seed.CourseId);
        Assert.Equal(seed.CourseName, activeCourse.Name);
        Assert.False(string.IsNullOrWhiteSpace(activeCourse.Status));
    }

    private static async Task<PaymentEnrollmentSeed> SeedPendingPaymentAsync(ApplicationDbContext db)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var instructor = new ApplicationUser
        {
            Id = $"inst-pay-int-{suffix}",
            UserName = $"inst-pay-int-{suffix}",
            NormalizedUserName = $"INST-PAY-INT-{suffix}".ToUpperInvariant(),
            Email = $"inst.pay.{suffix}@example.com",
            NormalizedEmail = $"INST.PAY.{suffix}@EXAMPLE.COM",
            FullName = "Instructor Payment Integration",
            CommissionRate = 0.30m
        };

        var learner = new ApplicationUser
        {
            Id = $"learner-pay-int-{suffix}",
            UserName = $"learner-pay-int-{suffix}",
            NormalizedUserName = $"LEARNER-PAY-INT-{suffix}".ToUpperInvariant(),
            Email = $"learner.pay.{suffix}@example.com",
            NormalizedEmail = $"LEARNER.PAY.{suffix}@EXAMPLE.COM",
            FullName = "Learner Payment Integration"
        };

        var category = new CourseCategory
        {
            Name = "Payment Integration",
            Slug = $"payment-integration-{suffix}"
        };

        var course = new Course
        {
            Code = $"PAYINT{suffix}".ToUpperInvariant(),
            Name = $"Payment Integration Course {suffix}",
            Category = category,
            InstructorId = instructor.Id,
            Instructor = instructor,
            Level = CourseLevel.Beginner,
            DurationHours = 2,
            CourseType = CourseType.Paid,
            OriginalPrice = 200000,
            Status = CourseStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var payment = new Payment
        {
            UserId = learner.Id,
            User = learner,
            Course = course,
            Amount = 200000,
            PaymentMethod = PaymentMethod.BankTransfer,
            TransactionCode = $"EDLMS_INT_{suffix}".ToUpperInvariant(),
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(instructor, learner);
        db.CourseCategories.Add(category);
        db.Courses.Add(course);
        db.Payments.Add(payment);

        await db.SaveChangesAsync();

        return new PaymentEnrollmentSeed
        {
            LearnerId = learner.Id,
            CourseId = course.Id,
            CourseName = course.Name,
            PaymentId = payment.Id,
            TransactionCode = payment.TransactionCode!,
            Amount = payment.Amount
        };
    }

    private sealed class PaymentEnrollmentSeed
    {
        public string LearnerId { get; init; } = string.Empty;
        public int CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public int PaymentId { get; init; }
        public string TransactionCode { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
