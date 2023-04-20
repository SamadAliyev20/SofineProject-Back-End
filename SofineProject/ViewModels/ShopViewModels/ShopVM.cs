using SofineProject.Models;

namespace SofineProject.ViewModels.ShopViewModels
{
	public class ShopVM
	{
		public IEnumerable<Category> Categories { get; set; }
		public IEnumerable<ProductType> ProductTypes { get; set; }
		public IEnumerable<Product> Products { get;set; }

    }
}
