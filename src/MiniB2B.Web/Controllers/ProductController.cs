using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;
using MiniB2B.Web.Models;

namespace MiniB2B.Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IGridConfigService _gridConfigService;

    public ProductController(IProductService productService, IGridConfigService gridConfigService)
    {
        _productService = productService;
        _gridConfigService = gridConfigService;
    }

    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter)
    {
        filter.IsActive = true;
        var products = await _productService.GetProductsAsync(filter);
        var columns = await _gridConfigService.GetColumnsAsync("ProductB2BGrid");
        var categories = await _productService.GetActiveCategoriesAsync();
        var brands = await _productService.GetDistinctBrandsAsync();

        var model = new ProductPageViewModel
        {
            Products = products,
            Columns = columns.Where(c => c.IsVisible).OrderBy(c => c.DisplayOrder).ToList(),
            Filter = filter,
            Categories = categories,
            Brands = brands
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DetailModal(int id)
    {
        var product = await _productService.GetProductDetailAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return PartialView("_ProductDetailModal", product);
    }
}
