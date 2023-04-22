using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.ContentModel;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels.BasketViewModels;

namespace SofineProject.Controllers
{
	public class BasketController : Controller
	{
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            string basket = HttpContext.Request.Cookies["basket"];
            List<BasketVM> basketVMs = null;

            if (basket != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

                foreach (BasketVM basketVM in basketVMs)
                {
                    Product product = _context.Products.FirstOrDefault(p => p.Id == basketVM.Id && p.IsDeleted == false);

                    if (product != null)
                    {
                        basketVM.Title = product.Title;
                        basketVM.Image = product.MainImage;

                        if (product.DiscountedPrice > 0)
                        {
                            basketVM.Price = product.DiscountedPrice;
                        }
                        else
                        {
                            basketVM.Price = product.Price;
                        }
                    }
                }
            }
            else
            {
                basketVMs = new List<BasketVM>();
            }

            return View(basketVMs);
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) { return NotFound(); }

            string basket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (string.IsNullOrWhiteSpace(basket))
            {
                basketVMs = new List<BasketVM>
                {
                   new BasketVM {Id= (int)id,Count=1}
                };
            }
            else
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

                if (basketVMs.Exists(b => b.Id == id))
                {
                    basketVMs.Find(b => b.Id == id).Count += 1;
                }
                else
                {
                    basketVMs.Add(new BasketVM { Id = (int)id, Count = 1 });
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                AppUser appUser = await _userManager.Users
                    .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
                    .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.ToUpperInvariant());

                if (appUser.Baskets.Any(b => b.ProductId == id))
                {
                    appUser.Baskets.FirstOrDefault(b => b.ProductId == id).Count
                    = basketVMs.FirstOrDefault(b => b.Id == id).Count;
                }
                else
                {
                    Basket dbbasket = new Basket
                    {
                        ProductId = id,
                        Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                    };
                    appUser.Baskets.Add(dbbasket);
                }
                await _context.SaveChangesAsync();
            }
            basket = JsonConvert.SerializeObject(basketVMs);

            HttpContext.Response.Cookies.Append("basket", basket);

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id && p.IsDeleted == false);

                if (product != null)
                {
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Title = product.Title;
                    basketVM.Image = product.MainImage;
                }
            }
            return PartialView("_BasketMiniCartPartial", basketVMs);
        }
        public async Task<IActionResult> DeleteBasket(int? id)
        {
            if (id == null) return BadRequest();

            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound();

            string basket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (string.IsNullOrWhiteSpace(basket)) { return BadRequest(); }

            else
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
                if (basketVMs.Exists(b => b.Id == id))
                {
                    BasketVM basketVM = basketVMs.Find(b => b.Id == id);
                    basketVMs.Remove(basketVM);
                    basket = JsonConvert.SerializeObject(basketVMs);
                    HttpContext.Response.Cookies.Append("basket", basket);
                }
                else
                {
                    return NotFound();
                }
            }
            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id && p.IsDeleted == false);

                if (product != null)
                {
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Title = product.Title;
                    basketVM.Image = product.MainImage;
                }
            }

            return PartialView("_BasketMiniCartPartial", basketVMs);
        }
        public async Task<IActionResult> GetBasketForBasket()
        {

            string basket = HttpContext.Request.Cookies["basket"];
            List<BasketVM> basketVMs = null;
            if (string.IsNullOrEmpty(basket))
            {
                return BadRequest();
            }
            else
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
            }
            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == basketVM.Id && p.IsDeleted == false);

                if (product != null)
                {
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Title = product.Title;
                    basketVM.Image = product.MainImage;
                }
            }
            return PartialView("_BasketCartPartial", basketVMs);
        }




    }
}
