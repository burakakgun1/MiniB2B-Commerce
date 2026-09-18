using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public OrderController(IOrderService orderService, ICartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var userId = GetCurrentUserId();
        var cart = await _cartService.GetCartAsync(userId);

        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Sepetinizde ürün bulunmamaktadır.";
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.Cart = cart;
        return View(new CreateOrderRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.CreateOrderFromCartAsync(userId, request.Notes);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            var cart = await _cartService.GetCartAsync(userId);
            ViewBag.Cart = cart;
            return View(request);
        }

        return RedirectToAction("OrderConfirmation", new { orderNumber = result.Data });
    }

    [HttpGet]
    public IActionResult OrderConfirmation(string orderNumber)
    {
        ViewBag.OrderNumber = orderNumber;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders(int page = 1)
    {
        var userId = GetCurrentUserId();
        var pagedOrders = await _orderService.GetOrdersAsync(userId: userId, page: page, pageSize: 15);
        return View(pagedOrders);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var userId = GetCurrentUserId();
        var order = await _orderService.GetOrderDetailAsync(id, userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
