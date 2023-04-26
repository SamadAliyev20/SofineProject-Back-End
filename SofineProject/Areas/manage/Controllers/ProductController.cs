using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Extentions;
using SofineProject.Helpers;
using SofineProject.Models;
using SofineProject.ViewModels;

namespace SofineProject.Areas.manage.Controllers
{
    [Area("manage")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context,IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            IQueryable<Product> products = _context.Products.Where(p => p.IsDeleted == false).OrderByDescending(p => p.Id);
            return View(PageNatedList<Product>.Create(products, pageIndex, 5));
        }

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			ViewBag.Categories = await _context.Categories
				.Where(c => c.IsDeleted == false)
				.ToListAsync();
			ViewBag.ProductTypes = await _context.ProductTypes
				.Where(c => c.IsDeleted == false)
				.ToListAsync();
			return View();
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .ToListAsync();
            ViewBag.ProductTypes = await _context.ProductTypes
                .Where(c => c.IsDeleted == false)
                .ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(product);
            }
            if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Id == product.CategoryId))
            {
                TempData["ToasterMessage3"] = $"Daxil Olunan Category Id {product.CategoryId} yanlışdır!";
                ModelState.AddModelError("CategoryId", $"Daxil Olunan Category Id {product.CategoryId} yanlisdir!");

                return View(product);
            }
            if (!await _context.ProductTypes.AnyAsync(c => c.IsDeleted == false && c.Id == product.ProductTypeId))
            {
                TempData["ToasterMessage3"] = $"Daxil Olunan ProductType Id {product.ProductTypeId} yanlışdır!";
                ModelState.AddModelError("ProductTypeId", $"Daxil Olunan ProductType Id {product.ProductTypeId} yanlisdir");

                return View(product);
            }
            if (product.MainFile != null)
            {
                if (!product.MainFile.CheckFileContentType("image/jpeg"))
                {
                    TempData["ToasterMessage3"] ="Main File ,Jpeg tipinde olmalidir!";
                    ModelState.AddModelError("MainFile", "Main File ,Jpeg tipinde olmalidir!");
                    return View(product);
                }
                if (!product.MainFile.CheckFileLength(1500))
                {
                    TempData["ToasterMessage3"] = "Main File ,Main File 1.5 MB dan cox ola bilmez!";
                    ModelState.AddModelError("MainFile", "Main File ,Main File 1.5 MB dan cox ola bilmez!");
                    return View(product);
                }
                product.MainImage = await product.MainFile.CreateFileAsync(_env, "assets", "img", "Product");
            }
            else
            {
                TempData["ToasterMessage3"] = "Main File ,Mutleq Secilmelidir!";
                ModelState.AddModelError("MainFile", "Main File ,Mutleq Secilmelidir!");
                return View(product);
            }
            if (product.HoverFile != null)
            {
                if (!product.HoverFile.CheckFileContentType("image/jpeg"))
                {
                    TempData["ToasterMessage3"] = "Hover File ,Jpeg tipinde olmalidir!";
                    ModelState.AddModelError("HoverFile", "Hover File ,Jpeg tipinde olmalidir!");
                    return View(product);
                }
                if (!product.HoverFile.CheckFileLength(1500))
                {   TempData["ToasterMessage3"] = "Hover File ,Hover File 1.5 MB dan cox ola bilmez!";
                    ModelState.AddModelError("HoverFile", "Hover File ,Hover File 1.5 MB dan cox ola bilmez!");
                    return View(product);
                }
                product.HoverImage = await product.HoverFile.CreateFileAsync(_env, "assets", "img", "Product");
            }
            else
            {   TempData["ToasterMessage3"] = "Hover File ,Mutleq Secilmelidir!";
                ModelState.AddModelError("HoverFile", "Hover File ,Mutleq Secilmelidir!");
                return View(product);
            }
            if (product.Files != null && product.Files.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile file in product.Files)
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        TempData["ToasterMessage3"] = $"{file.FileName} ,Jpeg tipinde olmalidir!";
                        ModelState.AddModelError("Files", $"{file.FileName} ,Jpeg tipinde olmalidir!");
                        return View(product);
                    }
                    if (!file.CheckFileLength(1500))
                    {
                        TempData["ToasterMessage3"] = $"{file.FileName} ,Jpeg tipinde olmalidir!";
                        ModelState.AddModelError("Files", $"{file.FileName}  ,Main File 1.5 MB dən cox ola bilmez!");
                        return View(product);
                    }
                    ProductImage productImage = new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env, "assets", "img", "Product"),
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"
                    };

                    productImages.Add(productImage);

                }

                product.ProductImages = productImages;
                product.CreatedAt = DateTime.UtcNow.AddHours(4);
                product.CreatedBy = "System";
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            TempData["ToasterMessage4"] = $"{product.Title} uğurla Yaradıldı!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            ViewBag.Categories = await _context.Categories
               .Where(c => c.IsDeleted == false)
               .ToListAsync();
            ViewBag.ProductTypes = await _context.ProductTypes
            .Where(c => c.IsDeleted == false)
            .ToListAsync();

            if (id == null) return BadRequest();

            Product product = await _context.Products.Include
                (b => b.ProductImages.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (product == null) return NotFound();


            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Categories = await _context.Categories
              .Where(c => c.IsDeleted == false)
              .ToListAsync();
            ViewBag.ProductTypes = await _context.ProductTypes
              .Where(c => c.IsDeleted == false)
              .ToListAsync();
            if (id == null) return BadRequest();
            Product product = await _context.Products
                .Include(p => p.ProductImages.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);
            if (product == null) return NotFound();


            return View(product);
        }

		[HttpGet]
		public async Task<IActionResult> DeleteImage(int? id, int? imageId)
		{
			if (id == null) return BadRequest();
			if (imageId == null) return BadRequest();
			Product product = await _context.Products
				.Include(p => p.ProductImages.Where(p => p.IsDeleted == false))
				.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

			if (product == null) return NotFound();
			if (product.ProductImages?.Count() <= 1) return BadRequest();

			if (!product.ProductImages.Any(p => p.Id == imageId)) return BadRequest();
			product.ProductImages.FirstOrDefault(p => p.Id == imageId).IsDeleted = true;
			FileHelper.DeleteFile(product.ProductImages.FirstOrDefault(p => p.Id == imageId).Image, _env, "assets", "img", "Product");
			List<ProductImage> productImages = product.ProductImages.Where(p => p.IsDeleted == false).ToList();
			await _context.SaveChangesAsync();
            TempData["ToasterMessage4"] = "Product Şəkili uğurla Silindi!";
            return PartialView("_ProductImagePartial", productImages);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(int? id, Product product)
		{
			ViewBag.Categories = await _context.Categories
			 .Where(c => c.IsDeleted == false)
			 .ToListAsync();
			ViewBag.ProductTypes = await _context.ProductTypes
			  .Where(c => c.IsDeleted == false)
			  .ToListAsync();
			if (id == null) return BadRequest();
			Product dbproduct = await _context.Products
			.Include(pt => pt.ProductImages.Where(p => p.IsDeleted == false))
			.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);
			if (dbproduct == null) return NotFound();
			int canupload = 6 - dbproduct.ProductImages.Count();
			if (product.Files != null && canupload < product.Files.Count())
			{
				TempData["ToasterMessage3"] = $"Maks {canupload} qeder şekil yükleye bilersiniz";
				ModelState.AddModelError("Files", $"Maks {canupload} qeder şekil yükleye bilersiniz");
				return View(product);
			}
			if (product.Files != null && product.Files.Count() > 0)
			{
				List<ProductImage> productImages = new List<ProductImage>();

				foreach (IFormFile file in product.Files)
				{
					if (!file.CheckFileContentType("image/jpeg"))
					{
						TempData["ToasterMessage3"] = $"{file.FileName} ,Jpeg tipinde olmalidir!";
						ModelState.AddModelError("Files", $"{file.FileName} ,Jpeg tipinde olmalidir!");
						return View(product);
					}
					if (!file.CheckFileLength(300))
					{   TempData["ToasterMessage3"] = $"{file.FileName}  ,Files 300 KB dan cox ola bilmez!";
						ModelState.AddModelError("Files", $"{file.FileName}  ,Files 300 KB dan cox ola bilmez!");
						return View(product);
					}
					ProductImage productImage = new ProductImage
					{
						Image = await file.CreateFileAsync(_env, "assets", "img", "Product"),
						CreatedAt = DateTime.UtcNow.AddHours(4),
						CreatedBy = "System"
					};
					productImages.Add(productImage);

				}

				dbproduct.ProductImages.AddRange(productImages);
			}
			if (product.MainFile != null)
			{
				if (!product.MainFile.CheckFileContentType("image/jpeg"))
				{
					TempData["ToasterMessage3"] = "Main File ,Jpeg tipinde olmalidir!";
					ModelState.AddModelError("MainFile", "Main File ,Jpeg tipinde olmalidir!");
					return View(product);
				}
				if (!product.MainFile.CheckFileLength(1500))
				{   TempData["ToasterMessage3"] = "Main File ,Main File 1.5 MB dan cox ola bilmez!";
					ModelState.AddModelError("MainFile", "Main File ,Main File 1.5 MB dan cox ola bilmez!");
					return View(product);
				}
				dbproduct.MainImage = null;
				FileHelper.DeleteFile(dbproduct.MainImage, _env, "assets", "img", "Product");

				dbproduct.MainImage = await product.MainFile.CreateFileAsync(_env, "assets", "img", "Product");
			}
			if (product.HoverFile != null)
			{
				if (!product.HoverFile.CheckFileContentType("image/jpeg"))
				{   TempData["ToasterMessage3"] = "Hover File ,Jpeg tipinde olmalidir!";
					ModelState.AddModelError("HoverFile", "Hover File ,Jpeg tipinde olmalidir!");
					return View(product);
				}
				if (!product.HoverFile.CheckFileLength(1500))
				{   TempData["ToasterMessage3"] = "Hover File ,Main File 1.5 MB dan cox ola bilmez!";
					ModelState.AddModelError("Hover File", "Hover File ,Main File 1.5 MB dan cox ola bilmez!");
					return View(product);
				}
				dbproduct.HoverImage = null;
				FileHelper.DeleteFile(dbproduct.HoverImage, _env, "assets", "img", "Product");

				dbproduct.HoverImage = await product.HoverFile.CreateFileAsync(_env, "assets", "img", "Product");
			}
			dbproduct.Title = product.Title;
			dbproduct.Price = product.Price;
			dbproduct.DiscountedPrice = product.DiscountedPrice;
			dbproduct.Count = product.Count;
			dbproduct.ShortDescription = product.ShortDescription;
			dbproduct.LongDescription = product.LongDescription;
			dbproduct.CategoryId = product.CategoryId;
			dbproduct.ProductTypeId = product.ProductTypeId;
			await _context.SaveChangesAsync();
			TempData["ToasterMessage4"] = "Product uğurla Dəyişdirildi!";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return BadRequest();
			}
			Product product = await _context.Products
				.Include(c => c.ProductImages.Where(c => c.IsDeleted == false))
				.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

			if (product == null) return NotFound();

			return View(product);

		}

        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Product product = await _context.Products
                .Include(c => c.ProductImages.Where(c => c.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (product == null) return NotFound();

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow.AddHours(4);
            product.DeletedBy = "System";

            await _context.SaveChangesAsync();

            TempData["ToasterMessage4"] = "Product uğurla Silindi!";
            return RedirectToAction("Index");

        }



    }
}
