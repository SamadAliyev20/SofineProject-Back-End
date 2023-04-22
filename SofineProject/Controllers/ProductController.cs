using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Models;

namespace SofineProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> ProductModal(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Product product = await _context.Products.Include(p => p.ProductImages).Include(p=>p.Reviews).FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

            if (product == null) return NotFound();

            return PartialView("_ModalPartial", product);
        }

        public async Task<IActionResult> Search(string search)
        {
			IEnumerable<Product> products = await _context.Products.Where(p => p.IsDeleted == false && p.Title.ToLower().Contains(search.ToLower().Trim())).ToListAsync();
            return PartialView("_SearchPartial", products);
		}







    }
}
