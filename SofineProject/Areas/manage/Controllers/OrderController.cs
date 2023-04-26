using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels;

namespace SofineProject.Areas.manage.Controllers
{
    [Area("manage")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<Order> queries = _context.Orders.Include(u => u.OrderItems);

            return View(PageNatedList<Order>.Create(queries, pageIndex, 3));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Order order = await _context.Orders
                .Include(u => u.OrderItems.Where(u => u.IsDeleted == false))
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.IsDeleted == false && o.Id == id);

            if (order == null) return NotFound();

            return View(order);

        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeStatus(Order order)
		{
			if (order.Id <= 0)
			{
				return BadRequest();
			}
			Order dbOrder = await _context.Orders
				.Include(u => u.OrderItems.Where(u => u.IsDeleted == false))
				.ThenInclude(oi => oi.Product)
				.FirstOrDefaultAsync(o => o.IsDeleted == false && o.Id == order.Id);

			if (dbOrder == null) return NotFound();

			if (!ModelState.IsValid)
			{
				return View("Detail", dbOrder);
			}
			dbOrder.Status = order.Status;
			dbOrder.Comment = order.Comment;

			await _context.SaveChangesAsync();
			TempData["ToasterMessage4"] = "Orderin Statusu uğurla Dəyişdirildi!";
			return RedirectToAction("Index");

		}
	}
}
