using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels.ShopViewModels;

namespace SofineProject.Controllers
{
	public class ShopController : Controller
	{
		private readonly AppDbContext _context;
		public ShopController(AppDbContext context)
		{
			_context = context;
		}


        public async Task<IActionResult> Index()
        {
            ShopVM shopVM = new ShopVM 
            { 
             Products= await _context.Products.Where(p=>p.IsDeleted == false).ToListAsync(),
             Categories = await _context.Categories.Where(p => p.IsDeleted == false).ToListAsync(),
             ProductTypes = await _context.ProductTypes.Where(p => p.IsDeleted == false).ToListAsync(),

            };   
            return View(shopVM);
        }




        public async  Task<IActionResult> ProductFilter(int? categoryId,int? productTypeId,int? sortId,string? range="")
		{
            double minValue = 0;
            double maxValue = 0;

            range = range?.Replace("$", "");
            if (!string.IsNullOrWhiteSpace(range))
            {
                string[] arr = range.Split(" - ");

                minValue = double.Parse(arr[0]);
                maxValue = double.Parse(arr[1]);
            }
            IEnumerable<Product> AllProducts = await _context.Products.Where(p => p.IsDeleted == false).Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false)).ToListAsync();
            IEnumerable<Category> categories=await _context.Categories.Where(c=>c.IsDeleted==false).ToListAsync();
            IEnumerable<ProductType> ProductTypes = await _context.ProductTypes.Where(pt => pt.IsDeleted == false).ToListAsync();
            ViewBag.categoryId = categoryId;
            ViewBag.productTypeId = productTypeId;


            if (categoryId != null)
            {
                AllProducts = AllProducts.Where(p => p.CategoryId == categoryId);
            }

            if (productTypeId != null)
            {
                AllProducts = AllProducts.Where(p => p.ProductTypeId == productTypeId);
            }
            if (minValue > 0 || maxValue > 0)
            {
                AllProducts = AllProducts.Where(p => p.IsDeleted == false && ((p.DiscountedPrice > 0 ? p.DiscountedPrice : p.Price) >= minValue && (p.DiscountedPrice > 0 ? p.DiscountedPrice : p.Price) <= (maxValue == 0 ? 400 : maxValue)));
            }

            if (sortId == 1)
            {
                AllProducts=AllProducts.OrderBy(p=>p.Title).ToList();
            }

            if (sortId == 2)
            {
                AllProducts = AllProducts.OrderByDescending(p => p.Title).ToList();
            }

            if (sortId == 3)
            {
                AllProducts = AllProducts.OrderBy(p => p.Price).ToList();
            }

            if (sortId == 4)
            {
                AllProducts = AllProducts.OrderByDescending(p => p.Price).ToList();
            }
            

            ShopVM shopVM= new ShopVM { 
            Products= AllProducts,
            Categories=categories,
            ProductTypes=ProductTypes,
            };


            return PartialView("_ProductListPartial",shopVM);
		}
	}
}
