using SofineProject.Models;

namespace SofineProject.ViewModels.HomeViewModels
{
    public class HomeVM
    {
        public IEnumerable<Slider> Sliders { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}
