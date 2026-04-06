using Microsoft.AspNetCore.Mvc;

namespace EduLMS.Web.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
