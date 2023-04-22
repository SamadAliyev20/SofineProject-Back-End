using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels.BasketViewModels;
using SofineProject.ViewModels.OrderViewModels;
using System.Data;

namespace SofineProject.Controllers
{
    [Authorize(Roles = "Member")]
    public class OrderController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _appDbContext;

        public OrderController(UserManager<AppUser> userManager, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
        }
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            string cookie = HttpContext.Request.Cookies["basket"];
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return RedirectToAction("Index", "Shop");

            }
            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _appDbContext.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);

                basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                basketVM.Title = product.Title;
            }
            AppUser appUser = await _userManager.Users.Include(u => u.Addresses.Where(u => u.IsMain && u.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            Order order = new Order
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                Email = appUser.Email,
                Phone = appUser.PhoneNumber,
                AddressLine = appUser.Addresses.FirstOrDefault().AddressLine,
                City = appUser.Addresses.FirstOrDefault().City,
                Country = appUser.Addresses.FirstOrDefault().Country,
                PostalCode = appUser.Addresses.FirstOrDefault().PostalCode,
                State = appUser.Addresses.FirstOrDefault().State,

            };
            OrderVM orderVM = new OrderVM
            {
                Order = order,
                BasketVMs = basketVMs


            };
            return View(orderVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Checkout(Order order)
        {
            string cookie = HttpContext.Request.Cookies["basket"];
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return RedirectToAction("Index", "Shop");

            }
            AppUser appUser = await _userManager.Users
                .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
                .Include(u => u.Orders)
                .Include(u => u.Addresses.Where(u => u.IsMain && u.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _appDbContext.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);

                basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                basketVM.Title = product.Title;
            }
            OrderVM orderVM = new OrderVM
            {
                Order = order,
                BasketVMs = basketVMs


            };
            if (!ModelState.IsValid) { return View(order); }

            List<OrderItem> orderItems = new List<OrderItem>();

            foreach (BasketVM basketVM in basketVMs)
            {
                OrderItem orderItem = new OrderItem
                {
                    Count = basketVM.Count,
                    ProductId = basketVM.Id,
                    Price = basketVM.Price,
                    CreatedAt = DateTime.UtcNow.AddHours(4),
                    CreatedBy = $"{appUser.Name} {appUser.SurName}"
                };
                orderItems.Add(orderItem);
            }

            foreach (Basket basket in appUser.Baskets)
            {
                basket.IsDeleted = true;
            }

            HttpContext.Response.Cookies.Append("basket", "");

            order.UserId = appUser.Id;
            order.CreatedAt = DateTime.UtcNow.AddHours(4);
            order.CreatedBy = $"{appUser.Name} {appUser.SurName}";
            order.OrderItems = orderItems;
            order.No = appUser.Orders != null && appUser.Orders.Count() > 0 ? appUser.Orders.Last().No + 1 : 1;

            await _appDbContext.Orders.AddAsync(order);
            await _appDbContext.SaveChangesAsync();
            TempData["ToasterMessage"] = "Order Placed successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}
