using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SofineProject.Areas.manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
