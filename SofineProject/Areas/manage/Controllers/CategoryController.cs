using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels;
using System.Data;

namespace SofineProject.Areas.manage.Controllers
{
	[Area("manage")]
    [Authorize(Roles = "SuperAdmin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int pageindex = 1)
        {
            IQueryable<Category> categories = _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false).OrderByDescending(p => p.Id);

            return View(PageNatedList<Category>.Create(categories, pageindex, 3));

        }

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			return View();
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower().Contains(category.Name.Trim().ToLower())))
            {
                TempData["ToasterMessage3"] = $" {category.Name} kateqoriyası artıq mövcuddur.";

                return View(category);
            }
            category.Name = category.Name.Trim();
            category.CreatedBy = "System";
            category.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            TempData["ToasterMessage4"] = $" {category.Name} kateqoriyası uğurla Yaradıldı!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null) return BadRequest();

            Category category = await _context.Categories.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (category == null) return NotFound();


            return View(category);
        }


        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories.FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }


        [HttpPost]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            if (id == null) return BadRequest();

            if (id != category.Id) return BadRequest();

            Category DBcategory = await _context.Categories.FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (category == null) return NotFound();

            if (await _context.Categories.AnyAsync(b => b.IsDeleted == false && b.Name.ToLower().Contains(category.Name.Trim().ToLower()) && category.Id != b.Id))
            {
                TempData["ToasterMessage3"] = $" {category.Name} kateqoriyası artıq mövcuddur.";
         
                return View(category);
            }
            DBcategory.Name = category.Name.Trim();
            DBcategory.UpdatedBy = "System";
            DBcategory.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();
            TempData["ToasterMessage4"] = "Kateqoriya uğurla Dəyişdirildi!";
            return RedirectToAction(nameof(Index));


        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (category == null) return NotFound();


            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (category == null) return NotFound();

            category.IsDeleted = true;
            category.DeletedBy = "System";
            category.DeletedAt = DateTime.UtcNow.AddHours(4);

            foreach (Product product in category.Products)
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
