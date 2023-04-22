using SofineProject.Models;
using SofineProject.ViewModels.BasketViewModels;

namespace SofineProject.ViewModels.OrderViewModels
{
    public class OrderVM
    {
        public List<BasketVM> BasketVMs { get; set; }
        public Order Order { get; set; }
    }
}
