using SofineProject.Models;

namespace SofineProject.Areas.manage.ViewModels.DashboardVMs
{
    public class DashboardVM
    {
        public IEnumerable<Order> Orders { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public int TotalUsers { get; set; }
    }
}
