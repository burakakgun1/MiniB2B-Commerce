using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class UserController : AdminBaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index([FromQuery] UserFilterDto filter)
    {
        filter.PageSize = 20;
        var pagedUsers = await _userService.GetUsersAsync(filter);
        ViewBag.Filter = filter;
        return View(pagedUsers);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var userDetail = await _userService.GetUserDetailAsync(id);
        if (userDetail == null)
        {
            return NotFound();
        }

        return View(userDetail);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _userService.ToggleUserStatusAsync(id);
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetUserForEditAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _userService.UpdateUserAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Güncelleme başarısız.");
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
