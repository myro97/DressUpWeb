﻿using DressUp.Services.Data.Interfaces;
using DressUp.Services.Data.Models.Product;
using DressUp.Web.Infrastructure.Extensions;
using DressUp.Web.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DressUp.Common.NotificationMessagesConstants;

namespace DressUp.Web.Controllers;


public class ProductController : BaseController
{
	private readonly IProductService productService;
	private readonly IFavoriteService favoriteService;
	private readonly IBrandService brandService;
	private readonly ICategoryService categoryService;

	public ProductController(
		IProductService productService,
		IFavoriteService favoriteService,
		IBrandService brandService,
		ICategoryService categoryService)
	{
		this.productService = productService;
		this.favoriteService = favoriteService;
		this.brandService = brandService;
		this.categoryService = categoryService;
	}

	[HttpGet]
	[AllowAnonymous]
	// Show All products with QueryModel for pagination
	public async Task<IActionResult> All([FromQuery] AllProductsQueryModel queryModel)
	{
		try
		{
			AllProductsFilteredAndPagedServiceModel serviceModel = await productService.AllAsync(queryModel);

			queryModel.Products = serviceModel.Products;
			queryModel.TotalProducts = serviceModel.TotalProductsCount;
			queryModel.Categories = await categoryService.GetCategoriesNamesAsync();
			queryModel.Brands = await brandService.GetBrandsNameAsync();
			queryModel.SizeTypes = productService.GetAllSizeTypes();

			return View(queryModel);
		}
		catch
		{
			return GeneralError();
		}
	}


	// Add product to favorite
	public async Task<IActionResult> AddToFavorite(int productId)
	{
		try
		{
			bool isProductExist = await productService.IsProductExistByIdAsync(productId);
			if (!isProductExist)
			{
				TempData[ErrorMessage] = ErrorMessages.InvalidProductToFavorite;
				return NotFound();
			}

			bool isProductFavorite = await productService.IsProductFavorite(User.GetId(), productId);
			if (isProductFavorite)
			{
				TempData[ErrorMessage] = ErrorMessages.AlreadyAddedToFavorite;
				return BadRequest();
			}

			await favoriteService.AddToFavoriteAsync(productId, User.GetId());
			TempData[SuccessMessage] = SuccessMessages.AddedToFavorite;
			return RedirectToAction(nameof(All));
		}
		catch
		{
			return GeneralError();
		}
	}

	public async Task<IActionResult> RemoveFromFavorite(int productId)
	{
		if (!User.Identity?.IsAuthenticated ?? false)
		{
			TempData[ErrorMessage] = ErrorMessages.YouMustLogedInToRemoveFavorite;
			return RedirectToAction(nameof(All));
		}

		try
		{
			bool isProductExist = await productService.IsProductExistByIdAsync(productId);
			if (!isProductExist)
			{
				TempData[ErrorMessage] = ErrorMessages.InvalidProductToFavorite;
				return NotFound();
			}

			bool isProductFavorite = await productService.IsProductFavorite(User.GetId(), productId);
			if (!isProductFavorite)
			{
				TempData[ErrorMessage] = ErrorMessages.InvalidProductToRemoveFromFavorite;
				return BadRequest();
			}

			await favoriteService.RemoveFromFavoriteAsync(productId, User.GetId());
			TempData[SuccessMessage] = SuccessMessages.RemovedFromFavorite;
			return RedirectToAction(nameof(Favorite));
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpGet]
	// Show Favoritre products with QueryModel for pagination
	public async Task<IActionResult> Favorite([FromQuery] AllProductsQueryModel queryModel)
	{
		try
		{
			AllProductsFilteredAndPagedServiceModel serviceModel =
				await favoriteService.AllFavoritesAsync(queryModel, User.GetId());

			queryModel.Products = serviceModel.Products;
			queryModel.TotalProducts = serviceModel.TotalProductsCount;
			queryModel.Categories = await categoryService.GetCategoriesNamesAsync();
			queryModel.Brands = await brandService.GetBrandsNameAsync();
			queryModel.SizeTypes = productService.GetAllSizeTypes();

			return View(queryModel);
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpGet]
	public async Task<IActionResult> Add()
	{
		// Only Admin can Add product
		bool isModer = User.IsModerator();
		if (!User.IsAdmin() && !User.IsModerator())
		{
			TempData[ErrorMessage] = ErrorMessages.AdminOrModeratorToAddProduct;
			return Unauthorized();
		}

		try
		{
			ProductFormModel formModel = new()
			{
				Brands = await brandService.GetAllBrandsAsync(),
				Categories = await categoryService.GetAllCategoriesAsync(),
				SizeTypes = productService.GetAllSizeTypes()
			};

			return View(formModel);
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpPost]
	public async Task<IActionResult> Add(ProductFormModel formModel, IEnumerable<IFormFile> files)
	{
		if (!ModelState.IsValid)
		{
			return View(formModel);
		}

		// Only Admin can Add product
		if (!User.IsAdmin() && !User.IsModerator())
		{
			TempData[ErrorMessage] = ErrorMessages.AdminOrModeratorToAddProduct;
			return Unauthorized();
		}

		try
		{
			// Save uploaded image in wwwroot
			IEnumerable<string> uploadedFiles;
			if (files != null)
			{
				uploadedFiles = productService.UploadImages(files, formModel.SizeType);
				// Add ImageLinks to form model
				foreach (string imgUrl in uploadedFiles)
				{
					formModel.Images.Add(new ProductImagesViewModel
					{
						ImageUrl = imgUrl,
					});
				}
			}

			await productService.AddProductAsync(formModel);

			TempData[SuccessMessage] = SuccessMessages.AddedProduct;
			return RedirectToAction(nameof(All));
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpGet]
	public async Task<IActionResult> Edit(int id)
	{
		if (!User.IsAdmin() && !User.IsModerator())
		{
			TempData[ErrorMessage] = ErrorMessages.AdminOrModeratorToEdit;
			return Unauthorized();
		}

		try
		{
			bool isProductExist = await productService.IsProductExistByIdAsync(id);
			if (!isProductExist)
			{
				TempData[ErrorMessage] = ErrorMessages.ProductDoesNotExist;
				return NotFound();
			}

			ProductFormModel formModel = await productService.GetProductByIdAsync(id);

			formModel.Brands = await brandService.GetAllBrandsAsync();
			formModel.Categories = await categoryService.GetAllCategoriesAsync();
			formModel.SizeTypes = productService.GetAllSizeTypes();

			return View(formModel);
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpPost]
	public async Task<IActionResult> Edit(ProductFormModel formModel, int id)
	{

		if (!User.IsAdmin() && !User.IsModerator())
		{
			TempData[ErrorMessage] = ErrorMessages.AdminOrModeratorToEdit;
			return Unauthorized();
		}

		if (!ModelState.IsValid)
		{
			return View(formModel);
		}

		try
		{
			formModel.Brands = await brandService.GetAllBrandsAsync();
			formModel.Categories = await categoryService.GetAllCategoriesAsync();
			formModel.SizeTypes = productService.GetAllSizeTypes();

			await productService.EditProductAsync(formModel, id);
			TempData[SuccessMessage] = SuccessMessages.EditedProduct;
			return RedirectToAction(nameof(All));
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpGet]
	[AllowAnonymous]
	public async Task<IActionResult> Details(int id, string information)
	{
		try
		{
			bool isProductExist = await productService.IsProductExistByIdAsync(id);
			if (!isProductExist)
			{
				TempData[ErrorMessage] = ErrorMessages.ProductDoesNotExist;
				return NotFound();
			}

			ProductDetailsViewModel model = await productService.GetProductDetailsByIdAsync(id);

			if (model.GetUrlInformation() != information)
			{
				return NotFound();
			}

			return View(model);
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpGet]
	public async Task<IActionResult> Delete(int id)
	{
		if (!User.IsAdmin() && !User.IsModerator())
		{
			TempData[ErrorMessage] = ErrorMessages.AdminOrModeratorToDelete;
			return Unauthorized();
		}

		try
		{
			bool isProductExist = await productService.IsProductExistByIdAsync(id);
			if (!isProductExist)
			{
				TempData[ErrorMessage] = ErrorMessages.ProductDoesNotExist;
				return NotFound();
			}

			ProductPreDeleteDetails model = await productService.GetProductPreDeleteDetailsByIdAsync(id);
			return View(model);
		}
		catch
		{
			return GeneralError();
		}
	}

	[HttpPost]
	public async Task<IActionResult> Delete(ProductPreDeleteDetails model, int id)
	{
		if (!User.IsAdmin() && !User.IsModerator())
		{
			TempData[ErrorMessage] = ErrorMessages.AdminOrModeratorToDelete;
			return Unauthorized();
		}

		try
		{
			bool isProductExist = await productService.IsProductExistByIdAsync(id);
			if (!isProductExist)
			{
				TempData[ErrorMessage] = ErrorMessages.ProductDoesNotExist;
				return NotFound();
			}

			await productService.DeleteProductByIdAsync(id);
			TempData[WarningMessage] = WarningMessages.ProductDeletedSuccessfuly;
		}
		catch
		{
			return GeneralError();
		}

		return RedirectToAction(nameof(All));
	}

	private IActionResult GeneralError()
	{
		TempData[ErrorMessage] = ErrorMessages.UnexpextedError;

		return RedirectToAction("Index", "Home");
	}

}
