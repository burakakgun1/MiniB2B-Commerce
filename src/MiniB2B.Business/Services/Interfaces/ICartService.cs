using MiniB2B.Core.Models;

namespace MiniB2B.Business.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<ServiceResult> AddToCartAsync(int userId, int productId, int quantity);
    Task<ServiceResult> UpdateQuantityAsync(int userId, int cartItemId, int quantity);
    Task<ServiceResult> RemoveFromCartAsync(int userId, int cartItemId);
    Task<ServiceResult> ClearCartAsync(int userId);
    Task<int> GetCartItemCountAsync(int userId);
}
