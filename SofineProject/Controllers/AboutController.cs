using Microsoft.AspNetCore.Mvc;

namespace SofineProject.Controllers
{
	public class AboutController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
