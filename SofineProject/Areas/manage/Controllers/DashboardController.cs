using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.Areas.manage.ViewModels.DashboardVMs;
using SofineProject.DataAccessLayer;
using SofineProject.Models;

namespace SofineProject.Areas.manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context= context;
            _userManager= userManager;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Order> orders =await _context.Orders.Include(o=>o.OrderItems).ThenInclude(oi=>oi.Product).Where(p=>p.IsDeleted == false).OrderByDescending(p=>p.CreatedAt).ToListAsync();
            IEnumerable<Category> categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();
           IEnumerable<Product> product = await _context.Products.Where(p=>p.IsDeleted == false).ToListAsync();
            var totalUsers = await _userManager.Users.CountAsync();

            DashboardVM dashboardVM = new DashboardVM
            {
              Orders = orders,
              Categories = categories,
              Products= product,
              TotalUsers=totalUsers
             
              

            
            
            };
            return View(dashboardVM);
        }
    }
}
