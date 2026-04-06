using System.ComponentModel.DataAnnotations;
using EduLMS.Web.ViewModels.Account;

namespace EcduLMS.Web.Tests.Account;

public class LoginViewModelTests
{
    [Fact]
    public void LoginViewModel_WithValidEmailAndPassword_ShouldBeValid()
    {
        var model = new LoginViewModel
        {
            Email = "learner@example.com",
            Password = "Learner@123"
        };

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void LoginViewModel_WithEmptyEmail_ShouldContainRequiredError()
    {
        var model = new LoginViewModel
        {
            Email = string.Empty,
            Password = "Learner@123"
        };

        var results = Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == "Vui lòng nhập email");
    }

    [Fact]
    public void LoginViewModel_WithInvalidEmailFormat_ShouldContainEmailFormatError()
    {
        var model = new LoginViewModel
        {
            Email = "invalid-email",
            Password = "Learner@123"
        };

        var results = Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == "Email không hợp lệ");
    }

    [Fact]
    public void LoginViewModel_WithEmptyPassword_ShouldContainRequiredError()
    {
        var model = new LoginViewModel
        {
            Email = "learner@example.com",
            Password = string.Empty
        };

        var results = Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == "Vui lòng nhập mật khẩu");
    }

    private static List<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}
