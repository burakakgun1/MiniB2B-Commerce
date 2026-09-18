-- =============================================================
-- Mini B2B E-Ticaret - Veritabanı Şeması ve Başlangıç Verileri
-- Microsoft SQL Server / LocalDB uyumludur.
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

-- 2. Kullanıcılar Tablosu
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

-- 6. Dinamik Grid Konfigürasyon Tablosu
IF OBJECT_ID('dbo.GridColumnConfigs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GridColumnConfigs (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GridName NVARCHAR(50) NOT NULL,
        PropertyName NVARCHAR(50) NOT NULL,
        HeaderTitle NVARCHAR(100) NOT NULL,
        DisplayOrder INT NOT NULL,
        IsVisible BIT NOT NULL DEFAULT 1,
        RenderType NVARCHAR(30) NOT NULL DEFAULT 'Text',
        Width NVARCHAR(20) NULL,
        Alignment NVARCHAR(20) NOT NULL DEFAULT 'Left',
        VisibleOnMobile BIT NOT NULL DEFAULT 1,
        VisibleOnTablet BIT NOT NULL DEFAULT 1,
        VisibleOnDesktop BIT NOT NULL DEFAULT 1,
        CONSTRAINT UQ_GridColumn_Name_Prop UNIQUE (GridName, PropertyName)
    );
END
GO

-- 7. Slider / Banner Tablosu
IF OBJECT_ID('dbo.SliderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SliderItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL,
        Subtitle NVARCHAR(250) NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        TargetUrl NVARCHAR(250) NULL,
        DisplayOrder INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

-- =============================================================
-- ÖRNEK BAŞLANGIÇ VERİLERİ (SEED DATA)
-- =============================================================

-- Kategoriler
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (Name, Description, DisplayOrder, IsActive) VALUES 
    (N'Şalt ve Otomasyon', N'Endüstriyel şalt malzemeleri, kontaktör ve röleler', 1, 1),
    (N'Pnömatik ve Valfler', N'Pnömatik silindirler ve yön valfleri', 2, 1),
    (N'Rulman ve Güç Aktarım', N'Sanayi tipi bilyalı rulmanlar ve kayışlar', 3, 1);
END
GO

-- Kullanıcılar (PBKDF2 Hash)
IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (FirstName, LastName, Email, Username, PasswordHash, PasswordSalt, PhoneNumber, Role, IsActive) VALUES
    (N'Sistem', N'Yöneticisi', N'admin@b2b.com', N'admin', N'n5lRz3bB72F4H1nfl/56FzB9Zt2nL8NfR2oV3F1+XpQ=', N'k2jF9h3X1v8N2b5V7c1Z9A==', N'0555 111 22 33', 1, 1),
    (N'Ahmet', N'Yılmaz', N'musteri@b2b.com', N'b2bmusteri', N'm8vK2x9Z1l7N3b5C8v2Z9X1+L3nF5pQ8r2V4B1nF3Xo=', N'z9vB2c5X1n8M3l7K4j1F9A==', N'0532 444 55 66', 2, 1);
END
GO

-- Örnek Ürünler (Var / Kritik / Yok stok durumlu)
IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (ProductCode, Name, Description, Brand, ManufacturerCode, SpecialCode1, SpecialCode2, ImageUrl, StockQuantity, CriticalStockLevel, Price, CategoryId, IsActive) VALUES
    (N'ELK-SIE-001', N'Siemens Sirius 3RT2026-1BB40 Güç Kontaktörü 24V DC 25A', N'24V DC bobin gerilimli 3 kutuplu kontaktör', N'Siemens', N'3RT2026-1BB40', N'SLT-01', N'PANO-A', N'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=400&q=80', 50, 10, 1450.00, 1, 1),
    (N'PNM-FES-001', N'Festo DNC-50-100-PPV-A Standart Pnömatik Silindir', N'ISO 15552 standartlarında 50mm çap 100mm strok', N'Festo', N'163366', N'PNM-01', N'HAT-1', N'https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?auto=format&fit=crop&w=400&q=80', 4, 8, 3850.00, 2, 1),
    (N'RLM-SKF-001', N'SKF 6205-2RSH Sabit Bilyalı Rulman', N'Çift tarafı kauçuk kapaklı derin kanallı rulman', N'SKF', N'6205-2RSH', N'RLM-01', N'MOTOR-X', N'https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=400&q=80', 0, 15, 280.00, 3, 1);
END
GO

-- Dinamik B2B Grid Kolon Ayarları
IF NOT EXISTS (SELECT 1 FROM dbo.GridColumnConfigs)
BEGIN
    INSERT INTO dbo.GridColumnConfigs (GridName, PropertyName, HeaderTitle, DisplayOrder, IsVisible, RenderType, Width, Alignment, VisibleOnMobile, VisibleOnTablet, VisibleOnDesktop) VALUES
    (N'ProductCatalog', N'ImageUrl', N'Görsel', 1, 1, N'Image', N'80px', N'Center', 0, 1, 1),
    (N'ProductCatalog', N'ProductCode', N'Ürün Kodu', 2, 1, N'DetailLink', N'140px', N'Left', 1, 1, 1),
    (N'ProductCatalog', N'Name', N'Ürün Adı', 3, 1, N'Text', N'auto', N'Left', 1, 1, 1),
    (N'ProductCatalog', N'Brand', N'Marka', 4, 1, N'Text', N'120px', N'Left', 0, 1, 1),
    (N'ProductCatalog', N'ManufacturerCode', N'Üretici Kodu', 5, 1, N'Text', N'130px', N'Left', 0, 0, 1),
    (N'ProductCatalog', N'StockStatus', N'Stok Durumu', 6, 1, N'StockBadge', N'110px', N'Center', 1, 1, 1),
    (N'ProductCatalog', N'Price', N'Fiyat', 7, 1, N'Currency', N'130px', N'Right', 1, 1, 1),
    (N'ProductCatalog', N'Action', N'Hızlı Sipariş', 8, 1, N'AddToCartAction', N'160px', N'Center', 1, 1, 1);
END
GO
