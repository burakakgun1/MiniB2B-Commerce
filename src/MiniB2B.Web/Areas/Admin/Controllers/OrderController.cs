using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Enums;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class OrderController : AdminBaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<IActionResult> Index(OrderStatus? status, string? search, int page = 1)
    {
        var orders = await _orderService.GetOrdersAsync(status: status, search: search, page: page, pageSize: 20);
        ViewBag.CurrentStatus = status;
        ViewBag.SearchText = search;
        return View(orders);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var order = await _orderService.GetOrderDetailAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Geçersiz durum güncelleme isteği.";
            return RedirectToAction(nameof(Detail), new { id = request.OrderId });
        }

        var result = await _orderService.UpdateOrderStatusAsync(request.OrderId, request.OrderStatus);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Detail), new { id = request.OrderId });
    }
}
