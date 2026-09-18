using Microsoft.EntityFrameworkCore;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Entities;
using MiniB2B.Core.Enums;
using MiniB2B.Core.Models;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.Business.Services.Implementations;

public class ProductService : IProductService
{
    private readonly MiniB2BDbContext _context;

    public ProductService(MiniB2BDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (filter.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim();
            var pattern = $"%{search}%";

            query = query.Where(p =>
                EF.Functions.Like(p.Name, pattern) ||
                EF.Functions.Like(p.ProductCode, pattern) ||
                EF.Functions.Like(p.Brand, pattern) ||
                (p.ManufacturerCode != null && EF.Functions.Like(p.ManufacturerCode, pattern)) ||
                (p.SpecialCode1 != null && EF.Functions.Like(p.SpecialCode1, pattern)) ||
                (p.SpecialCode2 != null && EF.Functions.Like(p.SpecialCode2, pattern)) ||
                (p.Description != null && EF.Functions.Like(p.Description, pattern)));
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            query = query.Where(p => p.Brand == filter.Brand);
        }

        if (filter.StockStatus.HasValue)
        {
            query = filter.StockStatus.Value switch
            {
                StockStatus.OutOfStock => query.Where(p => p.StockQuantity <= 0),
                StockStatus.Critical => query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStockLevel),
                StockStatus.InStock => query.Where(p => p.StockQuantity > p.CriticalStockLevel),
                _ => query
            };
        }

        query = (filter.SortBy?.ToLower(), filter.SortDescending) switch
        {
            ("price", true) => query.OrderByDescending(p => p.Price),
            ("price", false) => query.OrderBy(p => p.Price),
            ("code", true) => query.OrderByDescending(p => p.ProductCode),
            ("code", false) => query.OrderBy(p => p.ProductCode),
            ("stock", true) => query.OrderByDescending(p => p.StockQuantity),
            ("stock", false) => query.OrderBy(p => p.StockQuantity),
            ("brand", true) => query.OrderByDescending(p => p.Brand),
            ("brand", false) => query.OrderBy(p => p.Brand),
            _ when filter.SortDescending => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 25 : filter.PageSize;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                ManufacturerCode = p.ManufacturerCode,
                SpecialCode1 = p.SpecialCode1,
                SpecialCode2 = p.SpecialCode2,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CriticalStockLevel = p.CriticalStockLevel,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                StockStatus = p.StockQuantity <= 0 ? StockStatus.OutOfStock :
                              (p.StockQuantity <= p.CriticalStockLevel ? StockStatus.Critical : StockStatus.InStock),
                IsActive = p.IsActive
            })
            .ToListAsync();

        return new PagedResult<ProductListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductDetailDto?> GetProductDetailAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                ManufacturerCode = p.ManufacturerCode,
                SpecialCode1 = p.SpecialCode1,
                SpecialCode2 = p.SpecialCode2,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CriticalStockLevel = p.CriticalStockLevel,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                StockStatus = p.StockQuantity <= 0 ? StockStatus.OutOfStock :
                              (p.StockQuantity <= p.CriticalStockLevel ? StockStatus.Critical : StockStatus.InStock),
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductCreateEditDto?> GetProductForEditAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return null;

        return new ProductCreateEditDto
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            ManufacturerCode = product.ManufacturerCode,
            SpecialCode1 = product.SpecialCode1,
            SpecialCode2 = product.SpecialCode2,
            ImageUrl = product.ImageUrl,
            StockQuantity = product.StockQuantity,
            CriticalStockLevel = product.CriticalStockLevel,
            Price = product.Price,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive
        };
    }

    public async Task<ServiceResult<int>> CreateProductAsync(ProductCreateEditDto dto)
    {
        var codeExists = await _context.Products.AnyAsync(p => p.ProductCode == dto.ProductCode.Trim());
        if (codeExists)
        {
            return ServiceResult<int>.Failure($"'{dto.ProductCode}' ürün kodu zaten kullanımda.");
        }

        if (dto.StockQuantity < 0)
        {
            return ServiceResult<int>.Failure("Stok miktarı sıfırdan küçük olamaz.");
        }

        if (dto.Price <= 0)
        {
            return ServiceResult<int>.Failure("Fiyat sıfırdan büyük olmalıdır.");
        }

        var product = new Product
        {
            ProductCode = dto.ProductCode.Trim().ToUpperInvariant(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Brand = dto.Brand.Trim(),
            ManufacturerCode = dto.ManufacturerCode?.Trim(),
            SpecialCode1 = dto.SpecialCode1?.Trim(),
            SpecialCode2 = dto.SpecialCode2?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            StockQuantity = dto.StockQuantity,
            CriticalStockLevel = dto.CriticalStockLevel,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Success(product.Id, "Ürün başarıyla oluşturuldu.");
    }

    public async Task<ServiceResult> UpdateProductAsync(ProductCreateEditDto dto)
    {
        var product = await _context.Products.FindAsync(dto.Id);
        if (product == null)
        {
            return ServiceResult.Failure("Güncellenecek ürün bulunamadı.");
        }

        var codeExists = await _context.Products.AnyAsync(p => p.ProductCode == dto.ProductCode.Trim() && p.Id != dto.Id);
        if (codeExists)
        {
            return ServiceResult.Failure($"'{dto.ProductCode}' ürün kodu başka bir ürün tarafından kullanılıyor.");
        }

        if (dto.StockQuantity < 0)
        {
            return ServiceResult.Failure("Stok miktarı negatif olamaz.");
        }

        if (dto.Price <= 0)
        {
            return ServiceResult.Failure("Fiyat sıfırdan büyük olmalıdır.");
        }

        product.ProductCode = dto.ProductCode.Trim().ToUpperInvariant();
        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Brand = dto.Brand.Trim();
        product.ManufacturerCode = dto.ManufacturerCode?.Trim();
        product.SpecialCode1 = dto.SpecialCode1?.Trim();
        product.SpecialCode2 = dto.SpecialCode2?.Trim();
        product.ImageUrl = dto.ImageUrl?.Trim();
        product.StockQuantity = dto.StockQuantity;
        product.CriticalStockLevel = dto.CriticalStockLevel;
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Ürün başarıyla güncellendi.");
    }

    public async Task<ServiceResult> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return ServiceResult.Failure("Ürün bulunamadı.");
        }

        product.IsActive = false;
        product.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult.Success("Ürün pasife alındı.");
    }

    public async Task<ServiceResult> ToggleProductStatusAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return ServiceResult.Failure("Ürün bulunamadı.");
        }

        product.IsActive = !product.IsActive;
        product.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var statusText = product.IsActive ? "aktife" : "pasife";
        return ServiceResult.Success($"Ürün başarıyla {statusText} alındı.");
    }

    public async Task<List<string>> GetDistinctBrandsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && !string.IsNullOrWhiteSpace(p.Brand))
            .Select(p => p.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
    }

    public async Task<List<Category>> GetActiveCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
}
