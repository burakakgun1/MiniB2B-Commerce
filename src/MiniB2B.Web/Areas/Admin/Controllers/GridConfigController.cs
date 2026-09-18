using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class GridConfigController : AdminBaseController
{
    private readonly IGridConfigService _gridConfigService;

    public GridConfigController(IGridConfigService gridConfigService)
    {
        _gridConfigService = gridConfigService;
    }

    public async Task<IActionResult> Index()
    {
        var columns = await _gridConfigService.GetColumnsAsync("ProductB2BGrid");
        return View(columns);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var column = await _gridConfigService.GetColumnForEditAsync(id);
        if (column == null)
        {
            return NotFound();
        }

        return View(column);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateGridColumnDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _gridConfigService.UpdateColumnAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Güncelleme başarısız.");
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
