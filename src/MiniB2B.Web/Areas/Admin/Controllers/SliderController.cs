using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class SliderController : AdminBaseController
{
    private readonly ISliderService _sliderService;
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    public SliderController(ISliderService sliderService, IWebHostEnvironment environment)
    {
        _sliderService = sliderService;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var sliders = await _sliderService.GetAllSlidersAsync();
        return View(sliders);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new SliderCreateEditDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SliderCreateEditDto dto, IFormFile? imageFile)
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

        if (string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            ModelState.AddModelError("ImageUrl", "Lütfen bir görsel bağlantısı giriniz veya görsel dosyası yükleyiniz.");
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _sliderService.CreateSliderAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Ekleme başarısız.");
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var slider = await _sliderService.GetSliderForEditAsync(id);
        if (slider == null)
        {
            return NotFound();
        }

        return View(slider);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SliderCreateEditDto dto, IFormFile? imageFile)
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
            return View(dto);
        }

        var result = await _sliderService.UpdateSliderAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Güncelleme başarısız.");
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _sliderService.ToggleSliderStatusAsync(id);
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
        var result = await _sliderService.DeleteSliderAsync(id);
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
        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "sliders");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/sliders/{fileName}";
    }
}
