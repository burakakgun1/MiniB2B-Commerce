using System.ComponentModel.DataAnnotations;
using MiniB2B.Core.Enums;

namespace MiniB2B.Core.Models;

public class GridColumnDto
{
    public int Id { get; set; }
    public string GridName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string HeaderTitle { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
    public GridRenderType RenderType { get; set; }
    public string? Width { get; set; }
    public string Alignment { get; set; } = "left";
    public bool VisibleOnMobile { get; set; }
    public bool VisibleOnTablet { get; set; }
    public bool VisibleOnDesktop { get; set; }
}

public class UpdateGridColumnDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Kolon Başlığı")]
    public string HeaderTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Görüntüleme Sırası")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Görünür")]
    public bool IsVisible { get; set; }

    [Display(Name = "Genişlik (örn: 100px, 15%, auto)")]
    public string? Width { get; set; }

    [Display(Name = "Hizalama")]
    public string Alignment { get; set; } = "left";

    [Display(Name = "Mobilde Göster")]
    public bool VisibleOnMobile { get; set; }

    [Display(Name = "Tablette Göster")]
    public bool VisibleOnTablet { get; set; }

    [Display(Name = "Masaüstünde Göster")]
    public bool VisibleOnDesktop { get; set; }
}
