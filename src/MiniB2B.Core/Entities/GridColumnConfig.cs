using MiniB2B.Core.Enums;

namespace MiniB2B.Core.Entities;

public class GridColumnConfig : BaseEntity
{
    public string GridName { get; set; } = "ProductB2BGrid";
    public string PropertyName { get; set; } = string.Empty;
    public string HeaderTitle { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public GridRenderType RenderType { get; set; } = GridRenderType.Text;
    public string? Width { get; set; }
    public string Alignment { get; set; } = "left";
    public bool VisibleOnMobile { get; set; } = true;
    public bool VisibleOnTablet { get; set; } = true;
    public bool VisibleOnDesktop { get; set; } = true;
}
