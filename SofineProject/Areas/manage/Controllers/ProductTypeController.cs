using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels;

namespace SofineProject.Areas.manage.Controllers
{
    [Area("manage")]
    public class ProductTypeController : Controller
    {
        private readonly AppDbContext _context;

        public ProductTypeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageindex = 1)
        {
            IQueryable<ProductType> productTypes = _context.ProductTypes.Include(c => c.Products).Where(c => c.IsDeleted == false).OrderByDescending(p => p.Id);

            return View(PageNatedList<ProductType>.Create(productTypes, pageindex, 3));

        }

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			return View();
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductType productType)
        {
            if (!ModelState.IsValid)
            {
                return View(productType);
            }
            if (await _context.ProductTypes.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower().Contains(productType.Name.Trim().ToLower())))
            {
                TempData["ToasterMessage3"] = $" {productType.Name} kateqoriyası artıq mövcuddur.";

                return View(productType);
            }
            productType.Name = productType.Name.Trim();
            productType.CreatedBy = "System";
            productType.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.ProductTypes.AddAsync(productType);
            await _context.SaveChangesAsync();
            TempData["ToasterMessage4"] = $" {productType.Name} kateqoriyası uğurla Yaradıldı!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null) return BadRequest();

            ProductType productType = await _context.ProductTypes.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (productType == null) return NotFound();


            return View(productType);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            ProductType productType = await _context.ProductTypes.FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (productType == null) return NotFound();

            return View(productType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, ProductType productType)
        {
            if (!ModelState.IsValid)
            {
                return View(productType);
            }
            if (id == null) return BadRequest();

            if (id != productType.Id) return BadRequest();

            ProductType DBproductType = await _context.ProductTypes.FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (productType == null) return NotFound();

            if (await _context.ProductTypes.AnyAsync(b => b.IsDeleted == false && b.Name.ToLower().Contains(productType.Name.Trim().ToLower()) && productType.Id != b.Id))
            {
                TempData["ToasterMessage3"] = $" {productType.Name} kateqoriyası artıq mövcuddur.";

                return View(productType);
            }
            DBproductType.Name = productType.Name.Trim();
            DBproductType.UpdatedBy = "System";
            DBproductType.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();
            TempData["ToasterMessage4"] = "Kateqoriya uğurla Dəyişdirildi!";
            return RedirectToAction(nameof(Index));


        }


        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            ProductType productType = await _context.ProductTypes.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (productType == null) return NotFound();


            return View(productType);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProductType(int? id)
        {
            if (id == null) return BadRequest();

            ProductType productType = await _context.ProductTypes.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (productType == null) return NotFound();

            productType.IsDeleted = true;
            productType.DeletedBy = "System";
            productType.DeletedAt = DateTime.UtcNow.AddHours(4);

            foreach (Product product in productType.Products)
            {
                product.IsDeleted = true;
                product.DeletedBy = "System";
                product.DeletedAt = DateTime.UtcNow.AddHours(4);
            }


            await _context.SaveChangesAsync();

            TempData["ToasterMessage4"] = "Kateqoriya uğurla Silindi!";
            return RedirectToAction(nameof(Index));
        }

    }
}
