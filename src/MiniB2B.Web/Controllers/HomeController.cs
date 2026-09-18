using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;
using MiniB2B.Web.Models;

namespace MiniB2B.Web.Controllers;

public class HomeController : Controller
{
    private readonly ISliderService _sliderService;
    private readonly IProductService _productService;

    public HomeController(ISliderService sliderService, IProductService productService)
    {
        _sliderService = sliderService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var sliders = await _sliderService.GetActiveSlidersAsync();
        var categories = await _productService.GetActiveCategoriesAsync();
        var latestProducts = await _productService.GetProductsAsync(new ProductFilterDto { PageSize = 8, IsActive = true });

        var viewModel = new HomeViewModel
        {
            Sliders = sliders,
            Categories = categories,
            FeaturedProducts = latestProducts.Items
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
