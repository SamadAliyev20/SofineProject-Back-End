using Microsoft.AspNetCore.Mvc;

namespace SofineProject.Controllers
{
	public class ContactController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
