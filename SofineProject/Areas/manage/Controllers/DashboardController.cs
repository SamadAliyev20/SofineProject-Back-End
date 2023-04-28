using Microsoft.AspNetCore.Authorization;
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

        public DashboardController(AppDbContext context)
        {
            _context= context;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Order> orders =await _context.Orders.Include(o=>o.OrderItems).Where(p=>p.IsDeleted == false).ToListAsync();

            DashboardVM dashboardVM = new DashboardVM
            {
              Orders = orders,
            
            
            };
            return View(dashboardVM);
        }
    }
}
