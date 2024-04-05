﻿#nullable disable

namespace DressUp.Web.ViewModels.Product;

public class AllProductsViewModel
{
	public int Id { get; set; }
	
	public string Name { get; set; }

	public string Brand  { get; set; }

	public string Category { get; set; }

	public decimal Price { get; set; }

	public int Quantity { get; set; }
}