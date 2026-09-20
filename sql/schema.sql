-- =============================================================
-- Mini B2B E-Ticaret - Veritabanı Şeması ve Başlangıç Verileri (Seed Data)
-- Microsoft SQL Server (net8.0 & EF Core 8) ile %100 uyumludur.
-- =============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MiniB2BCommerceDb')
BEGIN
    CREATE DATABASE MiniB2BCommerceDb;
END
GO

USE MiniB2BCommerceDb;
GO

-- 1. Kategoriler Tablosu
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(250) NULL,
        DisplayOrder INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

-- 2. Kullanıcılar Tablosu (PBKDF2 HMAC-SHA256, 100.000 iterasyon + Salt)
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(150) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
        Username NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        PasswordHash NVARCHAR(250) NOT NULL,
        PasswordSalt NVARCHAR(250) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Role INT NOT NULL DEFAULT 2, -- 1: Admin, 2: Customer
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

-- 3. Ürünler Tablosu
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductCode NVARCHAR(50) NOT NULL CONSTRAINT UQ_Products_ProductCode UNIQUE,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Brand NVARCHAR(100) NOT NULL,
        ManufacturerCode NVARCHAR(50) NULL,
        SpecialCode1 NVARCHAR(50) NULL,
        SpecialCode2 NVARCHAR(50) NULL,
        ImageUrl NVARCHAR(500) NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        CriticalStockLevel INT NOT NULL DEFAULT 5,
        Price DECIMAL(18,2) NOT NULL,
        CategoryId INT NOT NULL CONSTRAINT FK_Products_Categories FOREIGN KEY REFERENCES dbo.Categories(Id),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );

    CREATE INDEX IX_Products_Brand ON dbo.Products(Brand);
    CREATE INDEX IX_Products_ManufacturerCode ON dbo.Products(ManufacturerCode);
    CREATE INDEX IX_Products_SpecialCode1 ON dbo.Products(SpecialCode1);
    CREATE INDEX IX_Products_SpecialCode2 ON dbo.Products(SpecialCode2);
END
GO

-- 4. Sepet Tabloları
IF OBJECT_ID('dbo.Carts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL CONSTRAINT UQ_Carts_UserId UNIQUE CONSTRAINT FK_Carts_Users FOREIGN KEY REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CartId INT NOT NULL CONSTRAINT FK_CartItems_Carts FOREIGN KEY REFERENCES dbo.Carts(Id) ON DELETE CASCADE,
        ProductId INT NOT NULL CONSTRAINT FK_CartItems_Products FOREIGN KEY REFERENCES dbo.Products(Id),
        Quantity INT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

-- 5. Sipariş Tabloları
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        OrderNumber NVARCHAR(50) NOT NULL CONSTRAINT UQ_Orders_OrderNumber UNIQUE,
        UserId INT NOT NULL CONSTRAINT FK_Orders_Users FOREIGN KEY REFERENCES dbo.Users(Id),
        OrderDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        OrderStatus INT NOT NULL DEFAULT 1, -- 1: Pending, 2: Approved, 3: Rejected, 4: Shipped, 5: Completed
        TotalAmount DECIMAL(18,2) NOT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        OrderId INT NOT NULL CONSTRAINT FK_OrderItems_Orders FOREIGN KEY REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        ProductId INT NOT NULL CONSTRAINT FK_OrderItems_Products FOREIGN KEY REFERENCES dbo.Products(Id),
        ProductCode NVARCHAR(50) NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

-- 6. Dinamik Grid Konfigürasyon Tablosu (Veritabanından Yönetilebilir B2B Grid)
IF OBJECT_ID('dbo.GridColumnConfigs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GridColumnConfigs (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GridName NVARCHAR(50) NOT NULL,
        PropertyName NVARCHAR(100) NOT NULL,
        HeaderTitle NVARCHAR(100) NOT NULL,
        DisplayOrder INT NOT NULL,
        IsVisible BIT NOT NULL DEFAULT 1,
        RenderType INT NOT NULL DEFAULT 1, -- 1: Text, 2: Image, 3: StockBadge, 4: Currency, 5: AddToCartAction, 6: DetailLink
        Width NVARCHAR(50) NULL,
        Alignment NVARCHAR(20) NOT NULL DEFAULT 'left',
        VisibleOnMobile BIT NOT NULL DEFAULT 1,
        VisibleOnTablet BIT NOT NULL DEFAULT 1,
        VisibleOnDesktop BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT UQ_GridColumn_Name_Prop UNIQUE (GridName, PropertyName)
    );
END
GO

-- 7. Slider / Banner Tablosu
IF OBJECT_ID('dbo.SliderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SliderItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title NVARCHAR(150) NOT NULL,
        Subtitle NVARCHAR(250) NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        TargetUrl NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

-- =============================================================
-- ÖRNEK BAŞLANGIÇ VERİLERİ (SEED DATA)
-- (DbInitializer.cs ile %100 birebir aynı veriler)
-- =============================================================

-- A. Kullanıcılar (PBKDF2 HMAC-SHA256, 100.000 iterasyon)
-- Admin Şifresi: Admin123*
-- Musteri 1 Şifresi: Musteri123*
-- Musteri 2 Şifresi: Alfa123*
IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (FirstName, LastName, Email, Username, PasswordHash, PasswordSalt, PhoneNumber, Role, IsActive) VALUES
    (N'Sistem', N'Yöneticisi', N'admin@b2b.com', N'admin', N'XMUVXhbBuNreU0/u08xz7nG4ij2/kBkp3zztL86fTxE=', N'4DZWtyZrl7W7zAe8ohdInw==', N'0555 111 22 33', 1, 1),
    (N'Ahmet', N'Yılmaz', N'musteri@b2b.com', N'b2bmusteri', N'42KflDnJDnPQ3tV5C83yrsiVTmO8fga3VhMLcxWPlIg=', N'gWDZd2ryisS2RiGj3A555A==', N'0532 444 55 66', 2, 1),
    (N'Mehmet', N'Demir', N'alfa@sanayi.com', N'alfasanayi', N'HFMrnS11jEPbidZ7SOet3DCmLB/5FC6cz4mRSejyFsA=', N'2NhzvELp2Uks0kBwvKnYYg==', N'0544 777 88 99', 2, 1);
END
GO

-- B. Dinamik B2B Grid Kolon Yapılandırması (ProductB2BGrid)
IF NOT EXISTS (SELECT 1 FROM dbo.GridColumnConfigs)
BEGIN
    INSERT INTO dbo.GridColumnConfigs (GridName, PropertyName, HeaderTitle, DisplayOrder, IsVisible, RenderType, Width, Alignment, VisibleOnMobile, VisibleOnTablet, VisibleOnDesktop) VALUES
    (N'ProductB2BGrid', N'ImageUrl', N'Görsel', 1, 1, 2, N'70px', N'center', 0, 1, 1),
    (N'ProductB2BGrid', N'ProductCode', N'Ürün Kodu', 2, 1, 6, N'140px', N'left', 1, 1, 1),
    (N'ProductB2BGrid', N'Name', N'Ürün Adı', 3, 1, 6, N'auto', N'left', 1, 1, 1),
    (N'ProductB2BGrid', N'Brand', N'Marka', 4, 1, 1, N'130px', N'left', 0, 1, 1),
    (N'ProductB2BGrid', N'ManufacturerCode', N'Üretici Kodu', 5, 1, 1, N'130px', N'left', 0, 0, 1),
    (N'ProductB2BGrid', N'StockStatus', N'Stok Durumu', 6, 1, 3, N'120px', N'center', 1, 1, 1),
    (N'ProductB2BGrid', N'Price', N'Birim Fiyat', 7, 1, 4, N'130px', N'right', 1, 1, 1),
    (N'ProductB2BGrid', N'Action', N'Hızlı Sipariş', 8, 1, 5, N'170px', N'center', 1, 1, 1);
END
GO

-- C. Slider / Banner İçerikleri
IF NOT EXISTS (SELECT 1 FROM dbo.SliderItems)
BEGIN
    INSERT INTO dbo.SliderItems (Title, Subtitle, ImageUrl, TargetUrl, DisplayOrder, IsActive) VALUES
    (N'Endüstriyel Otomasyon Çözümlerinde %20''ye Varan Özel B2B İndirimi', N'Siemens ve Schneider otomasyon şalt malzemelerinde toptan sipariş avantajları.', N'https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=1200&q=80', N'/Product?brand=Siemens', 1, 1),
    (N'SKF ve FAG Ağır Hizmet Rulmanlarında Stoktan Hemen Teslim', N'Binlerce rulman çeşidi ve güç aktarım ekipmanı avantajlı B2B fiyatlarıyla tek tıkla kapınızda.', N'https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?auto=format&fit=crop&w=1200&q=80', N'/Product?brand=SKF', 2, 1),
    (N'Festo & SMC Pnömatik Valf ve Silindir Grubu Yeni Sezon Kataloğu', N'Üretim hatlarınız için yüksek hassasiyetli pnömatik bileşenleri inceleyin.', N'https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=1200&q=80', N'/Product?brand=Festo', 3, 1);
END
GO

-- D. Kategoriler
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (Name, Description, DisplayOrder, IsActive) VALUES 
    (N'Elektrik & Otomasyon', N'Kontaktörler, röleler, invertör ve şalt ürünleri', 1, 1),
    (N'Rulman & Güç Aktarım', N'Sabit bilyalı, makaralı ve konik rulman grupları', 2, 1),
    (N'Hırdavat & Bağlantı Elemanları', N'Civata, somun, pul ve endüstriyel bağlantı elemanları', 3, 1),
    (N'Pnömatik & Hidrolik Sistemler', N'Valf adaları, silindirler, regülatörler ve hortumlar', 4, 1),
    (N'Endüstriyel Yağlar & Kimyasallar', N'Gresler, hidrolik sistem yağları ve temizleyiciler', 5, 1);
END
GO

-- E. Örnek B2B Ürünleri (14 Ürün, Var / Kritik / Yok Stok Durumları)
IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    DECLARE @CatElektrik INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name = N'Elektrik & Otomasyon');
    DECLARE @CatRulman INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name = N'Rulman & Güç Aktarım');
    DECLARE @CatHirdavat INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name = N'Hırdavat & Bağlantı Elemanları');
    DECLARE @CatPnomatik INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name = N'Pnömatik & Hidrolik Sistemler');
    DECLARE @CatYag INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name = N'Endüstriyel Yağlar & Kimyasallar');

    INSERT INTO dbo.Products (ProductCode, Name, Description, Brand, ManufacturerCode, SpecialCode1, SpecialCode2, ImageUrl, StockQuantity, CriticalStockLevel, Price, CategoryId, IsActive) VALUES
    (N'ELK-SIE-001', N'Siemens Sirius 3RT2026-1BB40 Güç Kontaktörü 24V DC 25A', N'3 kutuplu, AC-3 11 kW / 400 V, 1 NO + 1 NC yardımcı kontaklı modern kontaktör.', N'Siemens', N'3RT2026-1BB40', N'ELK-A', N'ALMANYA', N'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=400&q=80', 45, 10, 1450.00, @CatElektrik, 1),
    (N'ELK-SCH-002', N'Schneider Electric TeSys D LC1D32M7 220V AC 32A Kontaktör', N'Yüksek dayanımlı motor kontrol ve kompanzasyon uygulamaları için kompakt tasarım.', N'Schneider', N'LC1D32M7', N'ELK-B', N'FRANSA', N'https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?auto=format&fit=crop&w=400&q=80', 4, 10, 1890.00, @CatElektrik, 1),
    (N'ELK-ABB-003', N'ABB AF16-30-10-13 100-250V AC/DC Bobin Kontaktör 16A', N'Geniş kontrol gerilim aralığına sahip, dahili darbe bastırıcılı endüstriyel kontaktör.', N'ABB', N'1SBL177001R1310', N'ELK-A', N'ISVEC', N'https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=400&q=80', 0, 5, 1320.00, @CatElektrik, 1),
    (N'RLM-SKF-001', N'SKF 6205-2RSH Derin Kanal Sabit Bilyalı Rulman 25x52x15', N'Çift tarafı kauçuk kapaklı, yüksek devir ve toza karşı tam korumalı endüstriyel rulman.', N'SKF', N'6205-2RSH', N'RLM-STD', N'ITHAL', N'https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?auto=format&fit=crop&w=400&q=80', 120, 25, 285.50, @CatRulman, 1),
    (N'RLM-FAG-002', N'FAG 22212-E1-K Oynak Makaralı Rulman 60x110x28', N'Ağır radyal ve eksenel yüklere uygun, konik delikli yüksek mukavemetli rulman.', N'FAG', N'22212-E1-K', N'RLM-AGR', N'ALMANYA', N'https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?auto=format&fit=crop&w=400&q=80', 3, 8, 2450.00, @CatRulman, 1),
    (N'RLM-NSK-003', N'NSK 6308-DDU CM Yüksek Devirli Bilyalı Rulman 40x90x23', N'Elektrik motorları için düşük gürültü seviyeli ve optimize edilmiş iç boşluklu rulman.', N'NSK', N'6308-DDU', N'RLM-STD', N'JAPONYA', N'https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=400&q=80', 0, 10, 520.00, @CatRulman, 1),
    (N'HRD-WRT-001', N'Würth DIN 933 M10x40 8.8 Kalite Tam Diş Çelik Civata (100 Adet)', N'Galvaniz kaplamalı, yüksek çekme dayanımına sahip standart makine montaj civatası.', N'Würth', N'0057-10-40', N'CVT-88', N'YERLI', N'https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=400&q=80', 85, 15, 420.00, @CatHirdavat, 1),
    (N'HRD-NORM-002', N'Norm Civata DIN 912 M8x30 İmbus Civata 12.9 Yüksek Mukavemet', N'Siyah oksit kaplamalı, kalıp ve ağır sanayi makineleri için 12.9 kalite imbus civata.', N'Norm', N'NC-912-830', N'CVT-129', N'YERLI', N'https://images.unsplash.com/photo-1581092334651-ddf26d9a09d0?auto=format&fit=crop&w=400&q=80', 6, 20, 310.00, @CatHirdavat, 1),
    (N'HRD-BOSCH-003', N'Bosch Professional GWS 750-115 Avuç Taşlama 750W', N'İnce gövde tasarımlı, uzun ömürlü kömür fırçalarına sahip ergonomik avuç taşlama.', N'Bosch', N'0601394000', N'EL-ALETI', N'ALMANYA', N'https://images.unsplash.com/photo-1581092334651-ddf26d9a09d0?auto=format&fit=crop&w=400&q=80', 18, 5, 2750.00, @CatHirdavat, 1),
    (N'PNM-FST-001', N'Festo DSNU-25-100-PPV-A Yuvarlak Pnömatik Silindir', N'ISO 6432 standartlarında, her iki uçta ayarlanabilir pnömatik yastıklamalı silindir.', N'Festo', N'19238', N'PNM-SIL', N'ALMANYA', N'https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=400&q=80', 22, 5, 3650.00, @CatPnomatik, 1),
    (N'PNM-SMC-002', N'SMC SY5120-5LZD-01 5/2 Tek Bobin Pnömatik Valf 24V DC', N'Düşük güç tüketimli, hızlı tepki süreli, LED göstergeli yüksek performanslı selenoid valf.', N'SMC', N'SY5120-5LZD-01', N'PNM-VLF', N'JAPONYA', N'https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=400&q=80', 2, 6, 1680.00, @CatPnomatik, 1),
    (N'PNM-REX-003', N'Bosch Rexroth 4WE6D6X/EG24N9K4 Yön Kontrol Valfi 24V DC', N'Endüstriyel hidrolik güç üniteleri için doğrudan kumandalı sürgülü yön kontrol valfi.', N'Bosch Rexroth', N'R900561278', N'HDR-VLF', N'ALMANYA', N'https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=400&q=80', 0, 4, 7850.00, @CatPnomatik, 1),
    (N'YAG-MOBL-001', N'Mobil DTE 25 Ultra Yüksek Performanslı Hidrolik Sistem Yağı 20L', N'ISO VG 46 viskozite sınıfında, üstün aşınma önleyici ve oksidasyon dirençli hidrolik yağ.', N'Mobil', N'MOB-DTE25-20L', N'YAG-HDR', N'ITHAL', N'https://images.unsplash.com/photo-1615811361523-6bd03d7748e7?auto=format&fit=crop&w=400&q=80', 35, 10, 3200.00, @CatYag, 1),
    (N'YAG-CST-002', N'Castrol Spheerol EPL 2 Aşırı Basınç Lityum Bazlı Endüstriyel Gres 18KG', N'Ağır sanayi rulmanları ve dişli bağlantıları için yüksek basınca ve suya dayanıklı gres.', N'Castrol', N'CST-EPL2-18', N'YAG-GRS', N'ITHAL', N'https://images.unsplash.com/photo-1615811361523-6bd03d7748e7?auto=format&fit=crop&w=400&q=80', 14, 5, 2950.00, @CatYag, 1),
    (N'ELK-SIE-004', N'Siemens SIMATIC S7-1200 CPU 1214C DC/DC/DC PLC', N'14 DI 24V DC, 10 DO 24V DC, 2 AI 0-10V DC dahili giriş/çıkışlı kompakt otomasyon kontrolörü.', N'Siemens', N'6ES7214-1AG40-0XB0', N'ELK-PLC', N'ALMANYA', N'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=400&q=80', 8, 3, 16500.00, @CatElektrik, 1),
    (N'HRD-KIP-004', N'Knipex 87 01 250 Cobra Su Pompası Pensesi 250mm', N'Doğrudan iş parçası üzerinde tek elle hızlı ayar, boru ve somunlar için kendinden kitlemeli çene.', N'Knipex', N'8701250', N'EL-ALETI', N'ALMANYA', N'https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=400&q=80', 1, 5, 1950.00, @CatHirdavat, 1);
END
GO

-- F. Örnek Sipariş (DbInitializer ile uyumlu)
IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    DECLARE @CustomerId INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Role = 2);
    DECLARE @Prod1Id INT = (SELECT TOP 1 Id FROM dbo.Products WHERE ProductCode = N'ELK-SIE-001');
    DECLARE @Prod2Id INT = (SELECT TOP 1 Id FROM dbo.Products WHERE ProductCode = N'RLM-SKF-001');

    IF (@CustomerId IS NOT NULL AND @Prod1Id IS NOT NULL AND @Prod2Id IS NOT NULL)
    BEGIN
        INSERT INTO dbo.Orders (OrderNumber, UserId, OrderDate, OrderStatus, TotalAmount, Notes)
        VALUES (N'ORD-20260901-1001', @CustomerId, DATEADD(DAY, -2, SYSUTCDATETIME()), 2, 4350.00, N'Depo teslimatı sabah 09:00 öncesi yapılmalıdır.');

        DECLARE @OrderId INT = SCOPE_IDENTITY();

        INSERT INTO dbo.OrderItems (OrderId, ProductId, ProductCode, ProductName, Quantity, UnitPrice, TotalPrice)
        VALUES 
        (@OrderId, @Prod1Id, N'ELK-SIE-001', N'Siemens Sirius 3RT2026-1BB40 Güç Kontaktörü 24V DC 25A', 2, 1450.00, 2900.00),
        (@OrderId, @Prod2Id, N'RLM-SKF-001', N'SKF 6205-2RSH Derin Kanal Sabit Bilyalı Rulman 25x52x15', 5, 290.00, 1450.00);
    END
END
GO
