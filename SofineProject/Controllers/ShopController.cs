using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels;
using SofineProject.ViewModels.ShopViewModels;
using System;
using System.Data;
using System.Globalization;

namespace SofineProject.Controllers
{
	public class ShopController : Controller
	{
		private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ShopController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index(int? categoryId, int? productTypeId, string sortby = "1", int pageIndex = 1, string filter = "")
        {
        
            IQueryable<Product> AllProducts =  _context.Products.Where(p => p.IsDeleted == false).Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false)).Include(P=>P.Reviews);
            IEnumerable<Category> categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();
            IEnumerable<ProductType> ProductTypes = await _context.ProductTypes.Where(pt => pt.IsDeleted == false).ToListAsync();
            ViewBag.categoryId = categoryId;
            ViewBag.productTypeId = productTypeId;
            ViewBag.sortby = sortby;
           


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
                Products = PageNatedList<Product>.Create(AllProducts, pageIndex, 10),
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

        public async Task<IActionResult> Detail(int? productId)
        {
			if (productId == null)
			{
				return BadRequest();
			}
			Product product = await _context.Products
				.Include(p => p.ProductImages.Where(p => p.IsDeleted == false))
				.Include(p => p.Reviews.Where(p => p.IsDeleted == false))
				.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == productId);

			if (product == null)
			{
				return NotFound();
			}
			ProductReviewVM productReviewVM = new ProductReviewVM
			{
				Product = product,
				Review = new Review { ProductId = productId },
			};

			return View(productReviewVM);
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]

		public async Task<IActionResult> AddReview(Review review)
		{
			Product product = await _context.Products
				.Include(p => p.ProductImages.Where(p => p.IsDeleted == false))
				.Include(p => p.Reviews.Where(p => p.IsDeleted == false))
				.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == review.ProductId);

			ProductReviewVM productReviewVM = new ProductReviewVM { Product = product, Review = review };

			if (!ModelState.IsValid) return View("Detail", productReviewVM);

			if (!User.Identity.IsAuthenticated)
			{
				ModelState.AddModelError("Name", "You need to be logged in to add a review.");
				return RedirectToAction("Detail", "Shop", productReviewVM);
			}

			AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

			if (product.Reviews != null && product.Reviews.Count() > 0 && product.Reviews.Any(r => r.AppUserId == appUser.Id))
			{
				ModelState.AddModelError("Name", "You have already submitted a review for this product!");
				return View("Detail", productReviewVM);
			}

			review.CreatedBy = $"{appUser.Name} {appUser.SurName}";
			review.CreatedAt = DateTime.UtcNow.AddHours(4);
			review.AppUserId = appUser.Id;

			await _context.Reviews.AddAsync(review);
			await _context.SaveChangesAsync();
            TempData["Tab"] = "review";
            return RedirectToAction("Detail", "Shop", new { productId = product.Id });
		}
	}
}
