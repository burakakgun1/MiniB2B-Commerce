using MiniB2B.Core.Entities;
using MiniB2B.Core.Models;

namespace MiniB2B.Business.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductFilterDto filter);
    Task<ProductDetailDto?> GetProductDetailAsync(int id);
    Task<ProductCreateEditDto?> GetProductForEditAsync(int id);
    Task<ServiceResult<int>> CreateProductAsync(ProductCreateEditDto dto);
    Task<ServiceResult> UpdateProductAsync(ProductCreateEditDto dto);
    Task<ServiceResult> DeleteProductAsync(int id);
    Task<ServiceResult> ToggleProductStatusAsync(int id);
    Task<List<string>> GetDistinctBrandsAsync();
    Task<List<Category>> GetActiveCategoriesAsync();
}
