using EduLMS.Web.Controllers;
using EduLMS.Web.Data;
using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using EcduLMS.Web.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace EcduLMS.Web.Tests.Controllers;

public class WebhookControllerPaymentTests
{
    // Test case: Webhook nhận được payload khớp với một payment đang chờ xử lý
    [Fact]
    public async Task SePay_WhenPayloadMatchesPendingPayment_ShouldCompletePaymentAndCreateEnrollment()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedPendingPaymentAsync(db);

        var controller = new WebhookController(
            db,
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<WebhookController>>());

        var result = await controller.SePay(new SePayWebhookPayload
        {
            Content = $"Thanh toan don hang {seed.TransactionCode}",
            TransferAmount = seed.Amount
        });

        Assert.IsType<OkObjectResult>(result);

        var payment = await db.Payments.SingleAsync(p => p.Id == seed.PaymentId);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.NotNull(payment.CompletedAt);

        var enrollment = await db.Enrollments.SingleOrDefaultAsync(e => e.UserId == seed.LearnerId && e.CourseId == seed.CourseId);
        Assert.NotNull(enrollment);
        Assert.Equal(EnrollmentStatus.Active, enrollment!.Status);

        var earning = await db.InstructorEarnings.SingleOrDefaultAsync(e => e.PaymentId == seed.PaymentId);
        Assert.NotNull(earning);
        Assert.Equal(60000m, earning!.PlatformFee);
        Assert.Equal(140000m, earning.NetAmount);
    }
    // Test case: Webhook nhận được payload có transfer amount thấp hơn amount của payment
    [Fact]
    public async Task SePay_WhenTransferAmountIsLowerThanPaymentAmount_ShouldNotCompletePayment()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var seed = await SeedPendingPaymentAsync(db);

        var controller = new WebhookController(
            db,
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<WebhookController>>());

        var result = await controller.SePay(new SePayWebhookPayload
        {
            Content = $"thanh toan {seed.TransactionCode}",
            TransferAmount = seed.Amount - 1
        });

        Assert.IsType<OkObjectResult>(result);

        var payment = await db.Payments.SingleAsync(p => p.Id == seed.PaymentId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Null(payment.CompletedAt);

        var hasEnrollment = await db.Enrollments.AnyAsync(e => e.UserId == seed.LearnerId && e.CourseId == seed.CourseId);
        Assert.False(hasEnrollment);

        var hasEarning = await db.InstructorEarnings.AnyAsync(e => e.PaymentId == seed.PaymentId);
        Assert.False(hasEarning);
    }

    private static async Task<WebhookSeedData> SeedPendingPaymentAsync(ApplicationDbContext db)
    {
        var instructor = new ApplicationUser
        {
            Id = "instructor-payment",
            UserName = "instructor-payment",
            NormalizedUserName = "INSTRUCTOR-PAYMENT",
            Email = "instructor.payment@example.com",
            NormalizedEmail = "INSTRUCTOR.PAYMENT@EXAMPLE.COM",
            FullName = "Instructor Payment",
            CommissionRate = 0.30m
        };

        var learner = new ApplicationUser
        {
            Id = "learner-payment",
            UserName = "learner-payment",
            NormalizedUserName = "LEARNER-PAYMENT",
            Email = "learner.payment@example.com",
            NormalizedEmail = "LEARNER.PAYMENT@EXAMPLE.COM",
            FullName = "Learner Payment"
        };

        var category = new CourseCategory
        {
            Name = "Payment",
            Slug = $"payment-{Guid.NewGuid():N}"
        };

        var course = new Course
        {
            Code = $"PAY{Guid.NewGuid():N}"[..12],
            Name = "Payment Course",
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
            TransactionCode = "EDLMS_UNIT_001",
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(instructor, learner);
        db.CourseCategories.Add(category);
        db.Courses.Add(course);
        db.Payments.Add(payment);

        await db.SaveChangesAsync();

        return new WebhookSeedData
        {
            InstructorId = instructor.Id,
            LearnerId = learner.Id,
            CourseId = course.Id,
            PaymentId = payment.Id,
            TransactionCode = payment.TransactionCode!,
            Amount = payment.Amount
        };
    }

    private sealed class WebhookSeedData
    {
        public string InstructorId { get; init; } = string.Empty;
        public string LearnerId { get; init; } = string.Empty;
        public int CourseId { get; init; }
        public int PaymentId { get; init; }
        public string TransactionCode { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
