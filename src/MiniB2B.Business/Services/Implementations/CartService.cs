using Microsoft.EntityFrameworkCore;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Entities;
using MiniB2B.Core.Enums;
using MiniB2B.Core.Models;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.Business.Services.Implementations;

public class CartService : ICartService
{
    private readonly MiniB2BDbContext _context;

    public CartService(MiniB2BDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return new CartDto { UserId = userId };
        }

        var dto = new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items
                .Where(i => i.Product != null && i.Product.IsActive)
                .Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductCode = i.Product!.ProductCode,
                    ProductName = i.Product.Name,
                    Brand = i.Product.Brand,
                    ImageUrl = i.Product.ImageUrl,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity,
                    AvailableStock = i.Product.StockQuantity,
                    StockStatus = i.Product.StockQuantity <= 0 ? StockStatus.OutOfStock :
                                  (i.Product.StockQuantity <= i.Product.CriticalStockLevel ? StockStatus.Critical : StockStatus.InStock)
                })
                .ToList()
        };

        return dto;
    }

    public async Task<ServiceResult> AddToCartAsync(int userId, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            return ServiceResult.Failure("Eklenecek adet en az 1 olmalıdır.");
        }

        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsActive)
        {
            return ServiceResult.Failure("Ürün bulunamadı veya satışta değil.");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        var targetQuantity = (existingItem?.Quantity ?? 0) + quantity;

        if (targetQuantity > product.StockQuantity)
        {
            return ServiceResult.Failure($"'{product.Name}' için yeterli stok bulunmamaktadır. Mevcut stok: {product.StockQuantity}.");
        }

        if (existingItem != null)
        {
            existingItem.Quantity = targetQuantity;
            existingItem.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                CreatedDate = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Ürün sepete eklendi.");
    }

    public async Task<ServiceResult> UpdateQuantityAsync(int userId, int cartItemId, int quantity)
    {
        if (quantity <= 0)
        {
            return await RemoveFromCartAsync(userId, cartItemId);
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return ServiceResult.Failure("Sepet bulunamadı.");
        }

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
        {
            return ServiceResult.Failure("Sepet kalemi bulunamadı.");
        }

        if (item.Product == null || !item.Product.IsActive)
        {
            return ServiceResult.Failure("Ürün aktif değil.");
        }

        if (quantity > item.Product.StockQuantity)
        {
            return ServiceResult.Failure($"'{item.Product.Name}' için yeterli stok bulunmamaktadır. Mevcut stok: {item.Product.StockQuantity}.");
        }

        item.Quantity = quantity;
        item.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Sepet güncellendi.");
    }

    public async Task<ServiceResult> RemoveFromCartAsync(int userId, int cartItemId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return ServiceResult.Failure("Sepet bulunamadı.");
        }

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
        {
            return ServiceResult.Failure("Sepet kalemi bulunamadı.");
        }

        cart.Items.Remove(item);
        _context.CartItems.Remove(item);

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Ürün sepetten çıkarıldı.");
    }

    public async Task<ServiceResult> ClearCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart != null && cart.Items.Any())
        {
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }

        return ServiceResult.Success("Sepet temizlendi.");
    }

    public async Task<int> GetCartItemCountAsync(int userId)
    {
        return await _context.Carts
            .Where(c => c.UserId == userId)
            .SelectMany(c => c.Items)
            .SumAsync(i => (int?)i.Quantity) ?? 0;
    }
}
