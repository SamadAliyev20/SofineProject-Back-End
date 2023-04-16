using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.ViewModels.HomeViewModels;

namespace SofineProject.Controllers
{  
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM 
            { 
             Sliders=await _context.Sliders.Where(s=>s.IsDeleted==false).ToListAsync(),
             Products=await _context.Products.Where(p=>p.IsDeleted==false).ToListAsync(),
            };
            return View(homeVM);
        }
    }
}
