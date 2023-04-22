using SofineProject.Models;

namespace SofineProject.ViewModels.HomeViewModels
{
    public class HomeVM
    {
        public IEnumerable<Slider> Sliders { get; set; }
        public IEnumerable<Product> Products { get; set; }
		public IEnumerable<Product> LastProducts { get; set; }
		public IEnumerable<ProductType> ProductTypes { get; set; }
        
    }
}
