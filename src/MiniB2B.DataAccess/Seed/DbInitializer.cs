using Microsoft.EntityFrameworkCore;
using MiniB2B.Core.Entities;
using MiniB2B.Core.Enums;
using MiniB2B.Core.Security;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.DataAccess.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(MiniB2BDbContext context, IPasswordHasher passwordHasher, string? adminInitialPassword = null, string? customerInitialPassword = null, string? customer2InitialPassword = null)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Users.AnyAsync())
        {
            if (string.IsNullOrWhiteSpace(adminInitialPassword) || string.IsNullOrWhiteSpace(customerInitialPassword) || string.IsNullOrWhiteSpace(customer2InitialPassword))
            {
                return;
            }

            var adminPassword = passwordHasher.HashPassword(adminInitialPassword);
            var adminUser = new User
            {
                FirstName = "Sistem",
                LastName = "Yöneticisi",
                Email = "admin@b2b.com",
                Username = "admin",
                PasswordHash = adminPassword.Hash,
                PasswordSalt = adminPassword.Salt,
                PhoneNumber = "0555 111 22 33",
                Role = UserRole.Admin,
                IsActive = true
            };

            var customerPassword = passwordHasher.HashPassword(customerInitialPassword);
            var customerUser = new User
            {
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "musteri@b2b.com",
                Username = "b2bmusteri",
                PasswordHash = customerPassword.Hash,
                PasswordSalt = customerPassword.Salt,
                PhoneNumber = "0532 444 55 66",
                Role = UserRole.Customer,
                IsActive = true
            };

            var customer2Password = passwordHasher.HashPassword(customer2InitialPassword);
            var customer2User = new User
            {
                FirstName = "Mehmet",
                LastName = "Demir",
                Email = "alfa@sanayi.com",
                Username = "alfasanayi",
                PasswordHash = customer2Password.Hash,
                PasswordSalt = customer2Password.Salt,
                PhoneNumber = "0544 777 88 99",
                Role = UserRole.Customer,
                IsActive = true
            };

            await context.Users.AddRangeAsync(adminUser, customerUser, customer2User);
            await context.SaveChangesAsync();
        }

        if (!await context.GridColumnConfigs.AnyAsync())
        {
            var columns = new List<GridColumnConfig>
            {
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "ImageUrl",
                    HeaderTitle = "Görsel",
                    DisplayOrder = 1,
                    IsVisible = true,
                    RenderType = GridRenderType.Image,
                    Width = "70px",
                    Alignment = "center",
                    VisibleOnMobile = false,
                    VisibleOnTablet = true,
                    VisibleOnDesktop = true
                },
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "ProductCode",
                    HeaderTitle = "Ürün Kodu",
                    DisplayOrder = 2,
                    IsVisible = true,
                    RenderType = GridRenderType.DetailLink,
                    Width = "140px",
                    Alignment = "left",
                    VisibleOnMobile = true,
                    VisibleOnTablet = true,
                    VisibleOnDesktop = true
                },
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "Name",
                    HeaderTitle = "Ürün Adı",
                    DisplayOrder = 3,
                    IsVisible = true,
                    RenderType = GridRenderType.DetailLink,
                    Width = "auto",
                    Alignment = "left",
                    VisibleOnMobile = true,
                    VisibleOnTablet = true,
                    VisibleOnDesktop = true
                },
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "Brand",
                    HeaderTitle = "Marka",
                    DisplayOrder = 4,
                    IsVisible = true,
                    RenderType = GridRenderType.Text,
                    Width = "130px",
                    Alignment = "left",
                    VisibleOnMobile = false,
                    VisibleOnTablet = true,
                    VisibleOnDesktop = true
                },
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "ManufacturerCode",
                    HeaderTitle = "Üretici Kodu",
                    DisplayOrder = 5,
                    IsVisible = true,
                    RenderType = GridRenderType.Text,
                    Width = "130px",
                    Alignment = "left",
                    VisibleOnMobile = false,
                    VisibleOnTablet = false,
                    VisibleOnDesktop = true
                },
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "StockStatus",
                    HeaderTitle = "Stok Durumu",
                    DisplayOrder = 6,
                    IsVisible = true,
                    RenderType = GridRenderType.StockBadge,
                    Width = "120px",
                    Alignment = "center",
                    VisibleOnMobile = true,
                    VisibleOnTablet = true,
                    VisibleOnDesktop = true
                },
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "Price",
                    HeaderTitle = "Birim Fiyat",
                    DisplayOrder = 7,
                    IsVisible = true,
                    RenderType = GridRenderType.Currency,
                    Width = "130px",
                    Alignment = "right",
                    VisibleOnMobile = true,
                    VisibleOnTablet = true,
                    VisibleOnDesktop = true
                },
                new()
                {
                    GridName = "ProductB2BGrid",
                    PropertyName = "Action",
                    HeaderTitle = "Hızlı Sipariş",
                    DisplayOrder = 8,
                    IsVisible = true,
                    RenderType = GridRenderType.AddToCartAction,
                    Width = "170px",
                    Alignment = "center",
                    VisibleOnMobile = true,
                    VisibleOnTablet = true,
                    VisibleOnDesktop = true
                }
            };

            await context.GridColumnConfigs.AddRangeAsync(columns);
            await context.SaveChangesAsync();
        }

        if (!await context.SliderItems.AnyAsync())
        {
            var sliders = new List<SliderItem>
            {
                new()
                {
                    Title = "Endüstriyel Otomasyon Çözümlerinde %20'ye Varan Özel B2B İndirimi",
                    Subtitle = "Siemens ve Schneider otomasyon şalt malzemelerinde toptan sipariş avantajları.",
                    ImageUrl = "https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=1200&q=80",
                    TargetUrl = "/Product?brand=Siemens",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new()
                {
                    Title = "SKF ve FAG Ağır Hizmet Rulmanlarında Stoktan Hemen Teslim",
                    Subtitle = "Binlerce rulman çeşidi ve güç aktarım ekipmanı avantajlı B2B fiyatlarıyla tek tıkla kapınızda.",
                    ImageUrl = "https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?auto=format&fit=crop&w=1200&q=80",
                    TargetUrl = "/Product?brand=SKF",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new()
                {
                    Title = "Festo & SMC Pnömatik Valf ve Silindir Grubu Yeni Sezon Kataloğu",
                    Subtitle = "Üretim hatlarınız için yüksek hassasiyetli pnömatik bileşenleri inceleyin.",
                    ImageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=1200&q=80",
                    TargetUrl = "/Product?brand=Festo",
                    DisplayOrder = 3,
                    IsActive = true
                }
            };

            await context.SliderItems.AddRangeAsync(sliders);
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            var catElektrik = new Category { Name = "Elektrik & Otomasyon", Description = "Kontaktörler, röleler, invertör ve şalt ürünleri", DisplayOrder = 1 };
            var catRulman = new Category { Name = "Rulman & Güç Aktarım", Description = "Sabit bilyalı, makaralı ve konik rulman grupları", DisplayOrder = 2 };
            var catHirdavat = new Category { Name = "Hırdavat & Bağlantı Elemanları", Description = "Civata, somun, pul ve endüstriyel bağlantı elemanları", DisplayOrder = 3 };
            var catPnomatik = new Category { Name = "Pnömatik & Hidrolik Sistemler", Description = "Valf adaları, silindirler, regülatörler ve hortumlar", DisplayOrder = 4 };
            var catYag = new Category { Name = "Endüstriyel Yağlar & Kimyasallar", Description = "Gresler, hidrolik sistem yağları ve temizleyiciler", DisplayOrder = 5 };

            await context.Categories.AddRangeAsync(catElektrik, catRulman, catHirdavat, catPnomatik, catYag);
            await context.SaveChangesAsync();

            var products = new List<Product>
            {
                new()
                {
                    ProductCode = "ELK-SIE-001",
                    Name = "Siemens Sirius 3RT2026-1BB40 Güç Kontaktörü 24V DC 25A",
                    Description = "3 kutuplu, AC-3 11 kW / 400 V, 1 NO + 1 NC yardımcı kontaklı modern kontaktör.",
                    Brand = "Siemens",
                    ManufacturerCode = "3RT2026-1BB40",
                    SpecialCode1 = "ELK-A",
                    SpecialCode2 = "ALMANYA",
                    ImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 45,
                    CriticalStockLevel = 10,
                    Price = 1450.00m,
                    CategoryId = catElektrik.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "ELK-SCH-002",
                    Name = "Schneider Electric TeSys D LC1D32M7 220V AC 32A Kontaktör",
                    Description = "Yüksek dayanımlı motor kontrol ve kompanzasyon uygulamaları için kompakt tasarım.",
                    Brand = "Schneider",
                    ManufacturerCode = "LC1D32M7",
                    SpecialCode1 = "ELK-B",
                    SpecialCode2 = "FRANSA",
                    ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 4,
                    CriticalStockLevel = 10,
                    Price = 1890.00m,
                    CategoryId = catElektrik.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "ELK-ABB-003",
                    Name = "ABB AF16-30-10-13 100-250V AC/DC Bobin Kontaktör 16A",
                    Description = "Geniş kontrol gerilim aralığına sahip, dahili darbe bastırıcılı endüstriyel kontaktör.",
                    Brand = "ABB",
                    ManufacturerCode = "1SBL177001R1310",
                    SpecialCode1 = "ELK-A",
                    SpecialCode2 = "ISVEC",
                    ImageUrl = "https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 0,
                    CriticalStockLevel = 5,
                    Price = 1320.00m,
                    CategoryId = catElektrik.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "RLM-SKF-001",
                    Name = "SKF 6205-2RSH Derin Kanal Sabit Bilyalı Rulman 25x52x15",
                    Description = "Çift tarafı kauçuk kapaklı, yüksek devir ve toza karşı tam korumalı endüstriyel rulman.",
                    Brand = "SKF",
                    ManufacturerCode = "6205-2RSH",
                    SpecialCode1 = "RLM-STD",
                    SpecialCode2 = "ITHAL",
                    ImageUrl = "https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 120,
                    CriticalStockLevel = 25,
                    Price = 285.50m,
                    CategoryId = catRulman.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "RLM-FAG-002",
                    Name = "FAG 22212-E1-K Oynak Makaralı Rulman 60x110x28",
                    Description = "Ağır radyal ve eksenel yüklere uygun, konik delikli yüksek mukavemetli rulman.",
                    Brand = "FAG",
                    ManufacturerCode = "22212-E1-K",
                    SpecialCode1 = "RLM-AGR",
                    SpecialCode2 = "ALMANYA",
                    ImageUrl = "https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 3,
                    CriticalStockLevel = 8,
                    Price = 2450.00m,
                    CategoryId = catRulman.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "RLM-NSK-003",
                    Name = "NSK 6308-DDU CM Yüksek Devirli Bilyalı Rulman 40x90x23",
                    Description = "Elektrik motorları için düşük gürültü seviyeli ve optimize edilmiş iç boşluklu rulman.",
                    Brand = "NSK",
                    ManufacturerCode = "6308-DDU",
                    SpecialCode1 = "RLM-STD",
                    SpecialCode2 = "JAPONYA",
                    ImageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 0,
                    CriticalStockLevel = 10,
                    Price = 520.00m,
                    CategoryId = catRulman.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "HRD-WRT-001",
                    Name = "Würth DIN 933 M10x40 8.8 Kalite Tam Diş Çelik Civata (100 Adet)",
                    Description = "Galvaniz kaplamalı, yüksek çekme dayanımına sahip standart makine montaj civatası.",
                    Brand = "Würth",
                    ManufacturerCode = "0057-10-40",
                    SpecialCode1 = "CVT-88",
                    SpecialCode2 = "YERLI",
                    ImageUrl = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 85,
                    CriticalStockLevel = 15,
                    Price = 420.00m,
                    CategoryId = catHirdavat.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "HRD-NORM-002",
                    Name = "Norm Civata DIN 912 M8x30 İmbus Civata 12.9 Yüksek Mukavemet",
                    Description = "Siyah oksit kaplamalı, kalıp ve ağır sanayi makineleri için 12.9 kalite imbus civata.",
                    Brand = "Norm",
                    ManufacturerCode = "NC-912-830",
                    SpecialCode1 = "CVT-129",
                    SpecialCode2 = "YERLI",
                    ImageUrl = "https://images.unsplash.com/photo-1581092334651-ddf26d9a09d0?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 6,
                    CriticalStockLevel = 20,
                    Price = 310.00m,
                    CategoryId = catHirdavat.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "HRD-BOSCH-003",
                    Name = "Bosch Professional GWS 750-115 Avuç Taşlama 750W",
                    Description = "İnce gövde tasarımlı, uzun ömürlü kömür fırçalarına sahip ergonomik avuç taşlama.",
                    Brand = "Bosch",
                    ManufacturerCode = "0601394000",
                    SpecialCode1 = "EL-ALETI",
                    SpecialCode2 = "ALMANYA",
                    ImageUrl = "https://images.unsplash.com/photo-1581092334651-ddf26d9a09d0?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 18,
                    CriticalStockLevel = 5,
                    Price = 2750.00m,
                    CategoryId = catHirdavat.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "PNM-FST-001",
                    Name = "Festo DSNU-25-100-PPV-A Yuvarlak Pnömatik Silindir",
                    Description = "ISO 6432 standartlarında, her iki uçta ayarlanabilir pnömatik yastıklamalı silindir.",
                    Brand = "Festo",
                    ManufacturerCode = "19238",
                    SpecialCode1 = "PNM-SIL",
                    SpecialCode2 = "ALMANYA",
                    ImageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 22,
                    CriticalStockLevel = 5,
                    Price = 3650.00m,
                    CategoryId = catPnomatik.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "PNM-SMC-002",
                    Name = "SMC SY5120-5LZD-01 5/2 Tek Bobin Pnömatik Valf 24V DC",
                    Description = "Düşük güç tüketimli, hızlı tepki süreli, LED göstergeli yüksek performanslı selenoid valf.",
                    Brand = "SMC",
                    ManufacturerCode = "SY5120-5LZD-01",
                    SpecialCode1 = "PNM-VLF",
                    SpecialCode2 = "JAPONYA",
                    ImageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 2,
                    CriticalStockLevel = 6,
                    Price = 1680.00m,
                    CategoryId = catPnomatik.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "PNM-REX-003",
                    Name = "Bosch Rexroth 4WE6D6X/EG24N9K4 Yön Kontrol Valfi 24V DC",
                    Description = "Endüstriyel hidrolik güç üniteleri için doğrudan kumandalı sürgülü yön kontrol valfi.",
                    Brand = "Bosch Rexroth",
                    ManufacturerCode = "R900561278",
                    SpecialCode1 = "HDR-VLF",
                    SpecialCode2 = "ALMANYA",
                    ImageUrl = "https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 0,
                    CriticalStockLevel = 4,
                    Price = 7850.00m,
                    CategoryId = catPnomatik.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "YAG-MOBL-001",
                    Name = "Mobil DTE 25 Ultra Yüksek Performanslı Hidrolik Sistem Yağı 20L",
                    Description = "ISO VG 46 viskozite sınıfında, üstün aşınma önleyici ve oksidasyon dirençli hidrolik yağ.",
                    Brand = "Mobil",
                    ManufacturerCode = "MOB-DTE25-20L",
                    SpecialCode1 = "YAG-HDR",
                    SpecialCode2 = "ITHAL",
                    ImageUrl = "https://images.unsplash.com/photo-1615811361523-6bd03d7748e7?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 35,
                    CriticalStockLevel = 10,
                    Price = 3200.00m,
                    CategoryId = catYag.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "YAG-CST-002",
                    Name = "Castrol Spheerol EPL 2 Aşırı Basınç Lityum Bazlı Endüstriyel Gres 18KG",
                    Description = "Ağır sanayi rulmanları ve dişli bağlantıları için yüksek basınca ve suya dayanıklı gres.",
                    Brand = "Castrol",
                    ManufacturerCode = "CST-EPL2-18",
                    SpecialCode1 = "YAG-GRS",
                    SpecialCode2 = "ITHAL",
                    ImageUrl = "https://images.unsplash.com/photo-1615811361523-6bd03d7748e7?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 14,
                    CriticalStockLevel = 5,
                    Price = 2950.00m,
                    CategoryId = catYag.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "ELK-SIE-004",
                    Name = "Siemens SIMATIC S7-1200 CPU 1214C DC/DC/DC PLC",
                    Description = "14 DI 24V DC, 10 DO 24V DC, 2 AI 0-10V DC dahili giriş/çıkışlı kompakt otomasyon kontrolörü.",
                    Brand = "Siemens",
                    ManufacturerCode = "6ES7214-1AG40-0XB0",
                    SpecialCode1 = "ELK-PLC",
                    SpecialCode2 = "ALMANYA",
                    ImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 8,
                    CriticalStockLevel = 3,
                    Price = 16500.00m,
                    CategoryId = catElektrik.Id,
                    IsActive = true
                },
                new()
                {
                    ProductCode = "HRD-KIP-004",
                    Name = "Knipex 87 01 250 Cobra Su Pompası Pensesi 250mm",
                    Description = "Doğrudan iş parçası üzerinde tek elle hızlı ayar, boru ve somunlar için kendinden kitlemeli çene.",
                    Brand = "Knipex",
                    ManufacturerCode = "8701250",
                    SpecialCode1 = "EL-ALETI",
                    SpecialCode2 = "ALMANYA",
                    ImageUrl = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=400&q=80",
                    StockQuantity = 1,
                    CriticalStockLevel = 5,
                    Price = 1950.00m,
                    CategoryId = catHirdavat.Id,
                    IsActive = true
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            var customer = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Customer);
            if (customer != null)
            {
                var sampleOrder = new Order
                {
                    OrderNumber = "ORD-20260901-1001",
                    UserId = customer.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-2),
                    OrderStatus = OrderStatus.Approved,
                    TotalAmount = 4350.00m,
                    Notes = "Depo teslimatı sabah 09:00 öncesi yapılmalıdır."
                };

                sampleOrder.Items.Add(new OrderItem
                {
                    ProductId = products[0].Id,
                    ProductCode = products[0].ProductCode,
                    ProductName = products[0].Name,
                    Quantity = 2,
                    UnitPrice = products[0].Price,
                    TotalPrice = products[0].Price * 2
                });

                sampleOrder.Items.Add(new OrderItem
                {
                    ProductId = products[3].Id,
                    ProductCode = products[3].ProductCode,
                    ProductName = products[3].Name,
                    Quantity = 5,
                    UnitPrice = 290.00m,
                    TotalPrice = 290.00m * 5
                });

                await context.Orders.AddAsync(sampleOrder);
                await context.SaveChangesAsync();
            }
        }
    }
}
