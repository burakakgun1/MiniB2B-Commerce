using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class ProductController : AdminBaseController
{
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _environment;

    public ProductController(IProductService productService, IWebHostEnvironment environment)
    {
        _productService = productService;
        _environment = environment;
    }

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter)
    {
        filter.PageSize = 20;
        var pagedProducts = await _productService.GetProductsAsync(filter);
        var categories = await _productService.GetActiveCategoriesAsync();
        var brands = await _productService.GetDistinctBrandsAsync();

        ViewBag.Categories = categories;
        ViewBag.Brands = brands;
        ViewBag.Filter = filter;

        return View(pagedProducts);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesToViewBag();
        return View(new ProductCreateEditDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateEditDto dto, IFormFile? imageFile)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            var imageValidationError = ValidateUploadedImage(imageFile);
            if (imageValidationError != null)
            {
                ModelState.AddModelError("imageFile", imageValidationError);
            }
            else
            {
                dto.ImageUrl = await SaveImageFileAsync(imageFile);
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesToViewBag();
            return View(dto);
        }

        var result = await _productService.CreateProductAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Ürün eklenemedi.");
            await LoadCategoriesToViewBag();
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductForEditAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        await LoadCategoriesToViewBag();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductCreateEditDto dto, IFormFile? imageFile)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            var imageValidationError = ValidateUploadedImage(imageFile);
            if (imageValidationError != null)
            {
                ModelState.AddModelError("imageFile", imageValidationError);
            }
            else
            {
                dto.ImageUrl = await SaveImageFileAsync(imageFile);
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesToViewBag();
            return View(dto);
        }

        var result = await _productService.UpdateProductAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Güncelleme başarısız.");
            await LoadCategoriesToViewBag();
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _productService.ToggleProductStatusAsync(id);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesToViewBag()
    {
        var categories = await _productService.GetActiveCategoriesAsync();
        ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
    }

    private string? ValidateUploadedImage(IFormFile file)
    {
        if (file.Length > MaxFileSizeInBytes)
        {
            return "Görsel boyutu en fazla 5 MB olabilir.";
        }

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            return "Yalnızca JPG, JPEG, PNG, WEBP veya GIF formatında görseller yüklenebilir.";
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return "Geçersiz dosya formatı. Lütfen bir görsel seçiniz.";
        }

        return null;
    }

    private async Task<string> SaveImageFileAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "products");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }
}
