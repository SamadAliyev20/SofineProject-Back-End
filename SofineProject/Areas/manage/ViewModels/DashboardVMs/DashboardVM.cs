using SofineProject.Models;

namespace SofineProject.Areas.manage.ViewModels.DashboardVMs
{
    public class DashboardVM
    {
        public IEnumerable<Order> Orders { get; set; }
    }
}
