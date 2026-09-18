using MiniB2B.Core.Enums;

namespace MiniB2B.Core.Entities;

public class Product : BaseEntity
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string? ManufacturerCode { get; set; }
    public string? SpecialCode1 { get; set; }
    public string? SpecialCode2 { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; } = 5;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public StockStatus StockStatus
    {
        get
        {
            if (StockQuantity <= 0)
                return StockStatus.OutOfStock;
            if (StockQuantity <= CriticalStockLevel)
                return StockStatus.Critical;
            return StockStatus.InStock;
        }
    }
}
