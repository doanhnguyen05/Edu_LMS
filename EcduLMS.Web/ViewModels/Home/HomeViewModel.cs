using EduLMS.Web.Models;
using EduLMS.Web.Models.Identity;

namespace EduLMS.Web.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<Course> FeaturedCourses { get; set; } = new();
        public List<ApplicationUser> Instructors { get; set; } = new();
    }
}
