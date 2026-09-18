using System.ComponentModel.DataAnnotations;
using MiniB2B.Core.Enums;

namespace MiniB2B.Core.Models;

public class CartDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public int AvailableStock { get; set; }
    public StockStatus StockStatus { get; set; }
}

public class AddToCartRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 100000, ErrorMessage = "En az 1 adet eklemelisiniz.")]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Required]
    public int CartItemId { get; set; }

    [Range(1, 100000, ErrorMessage = "Adet en az 1 olmalıdır.")]
    public int Quantity { get; set; }
}
