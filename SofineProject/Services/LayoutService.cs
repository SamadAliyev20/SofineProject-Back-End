using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SofineProject.DataAccessLayer;
using SofineProject.Interfaces;
using SofineProject.Models;
using SofineProject.ViewModels.BasketViewModels;

namespace SofineProject.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LayoutService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IEnumerable<BasketVM>> GetBaskets()
        {
            string basket = _httpContextAccessor.HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (!string.IsNullOrWhiteSpace(basket))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

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
            }
            else
            {
                basketVMs = new List<BasketVM>();
            }
            return basketVMs;

        }
    }
}
