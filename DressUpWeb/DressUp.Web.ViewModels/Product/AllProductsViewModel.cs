﻿#nullable disable

using DressUp.Web.ViewModels.Product.Interfaces;

namespace DressUp.Web.ViewModels.Product;

public class AllProductsViewModel : IProductDetailsModel
{
	public AllProductsViewModel() 
	{
		Images = new List<ProductImagesViewModel>();
	}

	public int Id { get; set; }
	
	public string Name { get; set; }

	public string Brand  { get; set; }

	public IEnumerable<ProductImagesViewModel> Images { get; set; }

	public string Category { get; set; }

	public decimal Price { get; set; }

	public int Quantity { get; set; }
}
