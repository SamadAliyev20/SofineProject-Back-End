using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels.BasketViewModels;
using SofineProject.ViewModels.WishlistViewModels;

namespace SofineProject.Controllers
{
    public class WishListController : Controller
    {
        private readonly AppDbContext _context;

        public WishListController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            string wishlist = HttpContext.Request.Cookies["wishlist"];
            List<WishlistVM> wishlistVMs = null;

            if (wishlist != null)
            {
                wishlistVMs = JsonConvert.DeserializeObject<List<WishlistVM>>(wishlist);

                foreach (WishlistVM wishlistVM in wishlistVMs)
                {
                    Product product = _context.Products.FirstOrDefault(p => p.Id == wishlistVM.Id && p.IsDeleted == false);

                    if (product != null)
                    {
                        wishlistVM.Title = product.Title;
                        wishlistVM.Image = product.MainImage;

                        if (product.DiscountedPrice > 0)
                        {
                            wishlistVM.Price = product.DiscountedPrice;
                        }
                        else
                        {
                            wishlistVM.Price = product.Price;
                        }
                    }
                }
            }
            else
            {
                wishlistVMs = new List<WishlistVM>();
            }

            return View(wishlistVMs);
        }
        public async Task<IActionResult> AddWishlist(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) { return NotFound(); }

            string wishList = HttpContext.Request.Cookies["wishlist"];

            List<WishlistVM> wishListVMs = null;

            if (string.IsNullOrWhiteSpace(wishList))
            {
                wishListVMs = new List<WishlistVM>
                {
                   new WishlistVM {Id= (int)id,Count=1}
                };
            }
            else
            {
                wishListVMs = JsonConvert.DeserializeObject<List<WishlistVM>>(wishList);


                if (wishListVMs.Exists(b => b.Id == id))
                {
                    wishListVMs.Find(b => b.Id == id).Count += 1;
                }
                else
                {
                    wishListVMs.Add(new WishlistVM { Id = (int)id, Count = 1 });
                }

            }
            wishList = JsonConvert.SerializeObject(wishListVMs);
            HttpContext.Response.Cookies.Append("wishlist", wishList);

            foreach (WishlistVM wishListVM in wishListVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == wishListVM.Id && p.IsDeleted == false);

                if (product != null)
                {
                    wishListVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    wishListVM.Title = product.Title;
                    wishListVM.Image = product.MainImage;
                }
            }


            return PartialView("_WishlistMiniPartial", wishListVMs);
        }
        public async Task<IActionResult> DeleteWishList(int? id)
        {
            if (id == null) return BadRequest();

            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound();

            string wishList = HttpContext.Request.Cookies["wishlist"];

            List<WishlistVM> wishListVMs = null;

            if (string.IsNullOrWhiteSpace(wishList)) { return BadRequest(); }

            else
            {
                wishListVMs = JsonConvert.DeserializeObject<List<WishlistVM>>(wishList);
                if (wishListVMs.Exists(b => b.Id == id))
                {
                    WishlistVM wishListVM = wishListVMs.Find(b => b.Id == id);
                    wishListVMs.Remove(wishListVM);
                    wishList = JsonConvert.SerializeObject(wishListVMs);
                    HttpContext.Response.Cookies.Append("wishlist", wishList);
                }
                else
                {
                    return NotFound();
                }
            }
            foreach (WishlistVM wishListVM in wishListVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == wishListVM.Id && p.IsDeleted == false);

                if (product != null)
                {
                    wishListVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    wishListVM.Title = product.Title;
                    wishListVM.Image = product.MainImage;
                }
            }

            return PartialView("_WishlistMiniPartial", wishListVMs);
        }
    }
}
