using MiniB2B.Core.Enums;
using MiniB2B.Core.Models;

namespace MiniB2B.Business.Services.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<string>> CreateOrderFromCartAsync(int userId, string? notes);
    Task<PagedResult<OrderListDto>> GetOrdersAsync(int? userId = null, OrderStatus? status = null, string? search = null, int page = 1, int pageSize = 20);
    Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, int? userId = null);
    Task<ServiceResult> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
}
