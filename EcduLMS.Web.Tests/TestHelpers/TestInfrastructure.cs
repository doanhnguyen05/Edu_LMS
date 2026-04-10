using System.Security.Claims;
using EduLMS.Web.Data;
using EduLMS.Web.Models.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EcduLMS.Web.Tests.TestHelpers;

internal static class TestInfrastructure
{
    
    internal static ApplicationDbContext CreateDbContext(string? databaseName = null)
    {
        //Cấu hình sử dụng InMemoryDatabase (Database chạy trên RAM, tự xóa khi test xong)
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    internal static Mock<UserManager<ApplicationUser>> CreateUserManagerMock(string userId)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mock = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        mock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);

        return mock;
    }

    internal static void AttachAuthenticatedUser(Controller controller, string userId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                authenticationType: "UnitTestAuth"))
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }
}
