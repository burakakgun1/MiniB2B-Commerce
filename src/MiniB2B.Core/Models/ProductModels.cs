using System.ComponentModel.DataAnnotations;
using MiniB2B.Core.Enums;

namespace MiniB2B.Core.Models;

public class ProductFilterDto
{
    public string? SearchText { get; set; }
    public int? CategoryId { get; set; }
    public string? Brand { get; set; }
    public StockStatus? StockStatus { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class ProductListItemDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string? ManufacturerCode { get; set; }
    public string? SpecialCode1 { get; set; }
    public string? SpecialCode2 { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public StockStatus StockStatus { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDetailDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string? ManufacturerCode { get; set; }
    public string? SpecialCode1 { get; set; }
    public string? SpecialCode2 { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public StockStatus StockStatus { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ProductCreateEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün kodu zorunludur.")]
    [StringLength(50, ErrorMessage = "Ürün kodu en fazla 50 karakter olabilir.")]
    [Display(Name = "Ürün Kodu")]
    public string ProductCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir.")]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Marka bilgisi zorunludur.")]
    [StringLength(100, ErrorMessage = "Marka en fazla 100 karakter olabilir.")]
    [Display(Name = "Marka")]
    public string Brand { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Üretici kodu en fazla 50 karakter olabilir.")]
    [Display(Name = "Üretici Kodu")]
    public string? ManufacturerCode { get; set; }

    [StringLength(50, ErrorMessage = "Özel Kod 1 en fazla 50 karakter olabilir.")]
    [Display(Name = "Özel Kod 1")]
    public string? SpecialCode1 { get; set; }

    [StringLength(50, ErrorMessage = "Özel Kod 2 en fazla 50 karakter olabilir.")]
    [Display(Name = "Özel Kod 2")]
    public string? SpecialCode2 { get; set; }

    [Display(Name = "Görsel URL")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Stok miktarı zorunludur.")]
    [Range(0, 1000000, ErrorMessage = "Stok miktarı negatif olamaz.")]
    [Display(Name = "Stok Miktarı")]
    public int StockQuantity { get; set; } = 0;

    [Required(ErrorMessage = "Kritik stok seviyesi zorunludur.")]
    [Range(0, 10000, ErrorMessage = "Kritik stok seviyesi negatif olamaz.")]
    [Display(Name = "Kritik Stok Seviyesi")]
    public int CriticalStockLevel { get; set; } = 5;

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.01, 10000000, ErrorMessage = "Geçerli bir pozitif fiyat giriniz.")]
    [Display(Name = "Birim Fiyat (TL)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir kategori seçiniz.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
