using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Core.Models;

public class SliderDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class SliderCreateEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Slider Başlığı")]
    public string Title { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Alt Başlık")]
    public string? Subtitle { get; set; }

    [Display(Name = "Görsel URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Yönlendirme Linki")]
    public string? TargetUrl { get; set; }

    [Required]
    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; } = 1;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
