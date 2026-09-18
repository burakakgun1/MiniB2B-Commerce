using Microsoft.EntityFrameworkCore;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Entities;
using MiniB2B.Core.Enums;
using MiniB2B.Core.Models;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.Business.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly MiniB2BDbContext _context;

    public OrderService(MiniB2BDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<string>> CreateOrderFromCartAsync(int userId, string? notes)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return ServiceResult<string>.Failure("Sepetinizde ürün bulunmamaktadır.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in cart.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<string>.Failure("Sepetinizdeki bazı ürünler artık mevcut değil.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<string>.Failure($"'{product.Name}' için yeterli stok bulunmamaktadır. Mevcut stok: {product.StockQuantity}.");
                }

                product.StockQuantity -= item.Quantity;
                product.UpdatedDate = DateTime.UtcNow;

                var lineTotal = product.Price * item.Quantity;
                totalAmount += lineTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = lineTotal,
                    CreatedDate = DateTime.UtcNow
                });
            }

            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                TotalAmount = totalAmount,
                Notes = notes?.Trim(),
                Items = orderItems,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order);

            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<string>.Success(orderNumber, "Siparişiniz başarıyla oluşturuldu.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<string>.Failure($"Sipariş oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<PagedResult<OrderListDto>> GetOrdersAsync(int? userId = null, OrderStatus? status = null, string? search = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.OrderStatus == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(o =>
                EF.Functions.Like(o.OrderNumber, pattern) ||
                (o.User != null && (
                    EF.Functions.Like(o.User.FirstName, pattern) ||
                    EF.Functions.Like(o.User.LastName, pattern) ||
                    EF.Functions.Like(o.User.Email, pattern) ||
                    EF.Functions.Like(o.User.Username, pattern)
                )));
        }

        var totalCount = await query.CountAsync();
        var pageNumber = page < 1 ? 1 : page;
        var take = pageSize < 1 ? 20 : pageSize;

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * take)
            .Take(take)
            .Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                CustomerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : string.Empty,
                CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                ItemCount = o.Items.Count
            })
            .ToListAsync();

        return new PagedResult<OrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = take
        };
    }

    public async Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, int? userId = null)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
            .Where(o => o.Id == orderId);

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        return await query
            .Select(o => new OrderDetailDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                CustomerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : string.Empty,
                CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                CustomerPhone = o.User != null ? o.User.PhoneNumber : null,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return ServiceResult.Failure("Sipariş bulunamadı.");
        }

        order.OrderStatus = newStatus;
        order.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Sipariş durumu güncellendi.");
    }
}
