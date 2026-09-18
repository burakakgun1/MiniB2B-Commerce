using MiniB2B.Core.Entities;
using MiniB2B.Core.Models;

namespace MiniB2B.Web.Models;

public class HomeViewModel
{
    public List<SliderDto> Sliders { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<ProductListItemDto> FeaturedProducts { get; set; } = new();
}
