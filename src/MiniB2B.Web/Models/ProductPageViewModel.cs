using MiniB2B.Core.Entities;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Models;

public class ProductPageViewModel
{
    public PagedResult<ProductListItemDto> Products { get; set; } = new();
    public List<GridColumnDto> Columns { get; set; } = new();
    public ProductFilterDto Filter { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<string> Brands { get; set; } = new();
}
