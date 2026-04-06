using Microsoft.AspNetCore.Mvc;

namespace EduLMS.Web.Controllers;

public class HelpController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
