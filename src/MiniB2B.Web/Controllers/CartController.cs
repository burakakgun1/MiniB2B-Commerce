using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Json(new { success = false, requireLogin = true, message = "Sepete ürün eklemek için lütfen giriş yapınız." });
        }

        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz istek parametreleri." });
        }

        var userId = GetCurrentUserId();
        var result = await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
        var count = await _cartService.GetCartItemCountAsync(userId);

        return Json(new
        {
            success = result.IsSuccess,
            message = result.Message,
            cartCount = count
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz adet bilgisi." });
        }

        var userId = GetCurrentUserId();
        var result = await _cartService.UpdateQuantityAsync(userId, request.CartItemId, request.Quantity);
        var cart = await _cartService.GetCartAsync(userId);

        return Json(new
        {
            success = result.IsSuccess,
            message = result.Message,
            cartCount = cart.TotalQuantity,
            cartTotal = cart.TotalAmount.ToString("N2") + " ₺"
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Remove([FromBody] RemoveItemRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _cartService.RemoveFromCartAsync(userId, request.CartItemId);
        var cart = await _cartService.GetCartAsync(userId);

        return Json(new
        {
            success = result.IsSuccess,
            message = result.Message,
            cartCount = cart.TotalQuantity,
            cartTotal = cart.TotalAmount.ToString("N2") + " ₺"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Json(new { count = 0 });
        }

        var userId = GetCurrentUserId();
        var count = await _cartService.GetCartItemCountAsync(userId);
        return Json(new { count });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : 0;
    }
}

public class RemoveItemRequest
{
    public int CartItemId { get; set; }
}
