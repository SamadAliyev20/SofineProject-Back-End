﻿using SofineProject.Models;

namespace SofineProject.ViewModels.ShopViewModels
{
	public class ShopVM
	{
		public IEnumerable<Category> Categories { get; set; }
		public IEnumerable<ProductType> ProductTypes { get; set; }
		public PageNatedList<Product> Products { get;set; }
        public string SortBy { get; set; }
        public string Filter { get; set; }
    }
}
