using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels.ShopViewModels;
using System;
using System.Globalization;

namespace SofineProject.Controllers
{
	public class ShopController : Controller
	{
		private readonly AppDbContext _context;
		public ShopController(AppDbContext context)
		{
			_context = context;
		}


        public async Task<IActionResult> Index(int? categoryId, int? productTypeId, string sortby = "1", string filter = "")
        {
        
            IEnumerable<Product> AllProducts = await _context.Products.Where(p => p.IsDeleted == false).Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false)).ToListAsync();
            IEnumerable<Category> categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();
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
            if (!string.IsNullOrEmpty(filter))
            {
                AllProducts = AllProducts.Where(p => p.Title.Contains(filter));
            }

            switch (sortby)
            {
                case "0":
                    break;
                case "1":
                    AllProducts = AllProducts.OrderBy(p => p.Title);
                    break;
                case "2":
                    AllProducts = AllProducts.OrderByDescending(p => p.Title);
                    break;
                case "3":
                    AllProducts = AllProducts.OrderBy(p => p.Price);
                    break;
                case "4":
                    AllProducts = AllProducts.OrderByDescending(p => p.Price);
                    break;
                default:
                    AllProducts = AllProducts.OrderBy(p => p.Id);
                    break;
            }


            ShopVM shopVM = new ShopVM
            {
                Products = AllProducts,
                Categories = categories,
                ProductTypes = ProductTypes,
            };

            return View(shopVM);
        }


        public async Task<IActionResult> RangeFilter(string? range = "")
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


            IEnumerable<Product> product = await _context.Products.Where
                (p => p.IsDeleted == false && ((p.DiscountedPrice > 0
                ? p.DiscountedPrice : p.Price) >= minValue
                && (p.DiscountedPrice > 0 ? p.DiscountedPrice
                : p.Price) <= (maxValue == 0 ? 400 : maxValue))).ToListAsync();
            return PartialView("_ProductListPartial", product);

        }
    }
}
