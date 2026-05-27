using Microsoft.AspNetCore.Identity;

namespace EduLMS.Web.Services
{
    public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "Mật khẩu phải có ít nhất một ký tự đặc biệt."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = "Mật khẩu phải có ít nhất một chữ cái thường ('a'-'z')."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "Mật khẩu phải có ít nhất một chữ cái hoa ('A'-'Z')."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "Mật khẩu phải có ít nhất một chữ số ('0'-'9')."
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = $"Mật khẩu phải có ít nhất {length} ký tự."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = $"Email '{email}' đã được sử dụng."
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"Tên đăng nhập '{userName}' đã được sử dụng."
            };
        }

        public override IdentityError InvalidEmail(string? email)
        {
            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = $"Email '{email}' không hợp lệ."
            };
        }

        public override IdentityError InvalidUserName(string? userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = $"Tên đăng nhập '{userName}' không hợp lệ."
            };
        }
    }
}
