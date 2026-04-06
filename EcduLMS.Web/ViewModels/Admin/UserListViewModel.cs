namespace EduLMS.Web.ViewModels.Admin
{
    public class UserListViewModel
    {
        public List<UserItem> Users { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class UserItem
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string AvatarInitial { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = string.Empty;
    }

    public class CreateUserViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
