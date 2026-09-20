# Mini B2B E-Ticaret Projesi

## 1. Kullanılan Teknolojiler

* **Backend:** .NET 8, C# 12
* **Web Çatısı:** ASP.NET Core MVC (Areas destekli Admin Paneli)
* **Veritabanı & ORM:** Microsoft SQL Server, Entity Framework Core 8
* **Güvenlik:** Cookie Authentication (`[Authorize]` ve Rol bazlı yetkilendirme), PBKDF2 (HMAC-SHA256, 100.000 iterasyon + 16 byte Salt)
* **Frontend:** Bootstrap 5, Bootstrap Icons, Vanilla JavaScript (AJAX ile hızlı sipariş ve modal ürün detayı)

---

## 2. Gereksinimler

* .NET 8 SDK
* Microsoft SQL Server
* Git
* Güncel bir web tarayıcısı

---

## 3. Kurulum ve Çalıştırma

Projeyi bilgisayarınızda ayağa kaldırmak için terminalden aşağıdaki adımları çalıştırabilirsiniz:

1. **Projeyi Derleyin:**
   ```bash
   dotnet build
   ```

2. **Uygulamayı Başlatın:**
   ```bash
   dotnet run --project src/MiniB2B.Web
   ```

3. **Tarayıcıdan Açın:**
   Uygulama başladığında terminalde belirtilen localhost adresi üzerinden erişilebilir:
   * **Müşteri / Katalog Portalı:** `http://localhost:5016`
   * **Yönetici Giriş Paneli:** `http://localhost:5016/Admin`

---

## 4. Veritabanı Yapılandırması

Bağlantı dizesi `appsettings.json` dosyasında yerel varsayılan SQL Server örneğine (`Server=.`) ayarlanmıştır:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=MiniB2BCommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```
*(Farklı bir SQL Server instance'ı kullanıyorsanız — örneğin `Server=.\\SQLEXPRESS;` — yalnızca buradaki sunucu adını güncellemeniz yeterlidir).*

Veritabanını iki şekilde ayağa kaldırabilirsiniz:

* **Otomatik Kurulum:** Uygulamayı çalıştırdığınızda `EnsureCreatedAsync` ve `DbInitializer` devreye girer; `MiniB2BCommerceDb` veritabanını, tabloları, ilişkileri, örnek B2B ürünlerini, kategorileri, slider ve test kullanıcılarını SQL Server üzerinde otomatik oluşturur. Ekstra bir SQL komutu çalıştırmanıza gerek yoktur.
* **Manuel SQL Scripti:** Tabloları ve verileri SQL Server Management Studio (SSMS) üzerinden kendiniz çalıştırmak isterseniz proje kök dizinindeki `sql/schema.sql` dosyasını kullanabilirsiniz. Bu script, projedeki EF Core modelleri ve başlangıç verileriyle birebir aynı şemayı oluşturur.

---

## 5. Test Kullanıcı Bilgileri

Sistemde yönetici ve müşteri hesapları rol bazlı olarak ayrılmıştır. Kullanıcılar hem kullanıcı adı hem de e-posta ile giriş yapabilir:

| Rol | Giriş Adresi | Kullanıcı Adı / E-posta | Şifre | Yetki ve Kapsam |
|---|---|---|---|---|
| **Yönetici (Admin)** | `/Admin/Account/Login` | `admin` veya `admin@b2b.com` | `Admin123*` | Ürün, kullanıcı, sipariş, slider ve dinamik grid kolon yönetimi |
| **Müşteri (Customer)** | `/Account/Login` | `b2bmusteri` veya `musteri@b2b.com` | `Musteri123*` | Katalog arama/filtreleme, satırdan hızlı sipariş, sepet ve siparişlerim |
| **Müşteri 2 (Customer)** | `/Account/Login` | `alfasanayi` veya `alfa@sanayi.com` | `Alfa123*` | Alternatif bayi hesabı |

> **Not:** Bu kullanıcılar yalnızca geliştirme ve test amaçlı oluşturulmuştur. Müşteri hesaplarının Admin paneline erişimi güvenlik gereği engellenmiştir; yetkisiz erişim denendiğinde yönetici giriş ekranına yönlendirilir.

---

## 6. Mimari Tercihler ve Nedenleri

Projeyi Core, DataAccess, Business ve Web olmak üzere dört katmana ayırdım.:

```
MiniB2B.Web  ──▶  MiniB2B.Business  ──▶  MiniB2B.DataAccess  ──▶  MiniB2B.Core
```

* **Core:** Varlık sınıfları (Product, User, Order vb.), enum'lar, DTO'lar ve şifreleme servisi (`PasswordHasher`). Başka hiçbir katmana bağımlılığı yoktur.
* **DataAccess:** EF Core `DbContext`, Fluent API tablo eşlemeleri ve `DbInitializer` seed verileri.
* **Business:** İş kuralları, stok doğrulamaları, sepet hesaplamaları, transaction yönetimi ve servis sınıfları.
* **Web:** Controller sınıfları (Customer ve Admin Areas), Razor View'lar ve arayüz varlıkları.

### Neden Bu Yaklaşımı Tercih Ettim?

* **İnce Controller (Thin Controllers):** Controller sınıflarını sadece HTTP isteklerini karşılayıp servisleri çağıracak şekilde sade tuttum. Veritabanı işlemlerini ve iş mantığını tamamen Business katmanına taşıdım. Bu yapı sayesinde ileride mobil API veya ERP entegrasyonu eklenmesi durumunda iş kurallarının tekrar yazılmasının önüne geçilmesi amaçlandı.
* **Veritabanından Yönetilebilir B2B Grid:** Ürün tablosundaki kolonları kod içinde sabit tutmak yerine `GridColumnConfigs` tablosuna bağladım. Hangi alanın gösterileceği, sırası, render stratejisi (resim, link, metin, stok rozeti, fiyat, buton) ve mobil/tablet görünürlüğü kod değiştirmeden Admin panelindeki **"Grid Yapılandırması"** ekranından yönetilebiliyor.
* **Backend Seviyesinde Stok Doğrulaması:** Stok kontrolünü yalnızca arayüzde bırakmadım. Sipariş oluşturma sırasında veritabanı transaction'ı (`BeginTransactionAsync`) içinde ürünlerin anlık stokları kontrol ediliyor. Sepetteki herhangi bir ürünün stoğu yetersizse işlem geri alınıp kullanıcıya anlaşılır bir hata mesajı veriliyor (`"{Ürün Adı} için yeterli stok bulunmamaktadır. Mevcut stok: {Adet}."`). Stoklar yeterliyse ürün stokları anlık düşülüp sipariş tamamlanıyor.
* **Fiyat Snapshot Koruması:** Sipariş verildiği andaki ürün fiyatı ve ürün kodu `OrderItems` tablosuna bağımsız olarak kopyalanır. Ürünün güncel fiyatı sonradan değişse bile geçmiş siparişlerdeki satın alınan fiyat korunur.
* **Sunucu Taraflı SQL Sorguları:** Arama (kod, ad, marka, üretici kodu, özel kodlar, açıklama) ve sayfalama işlemlerini tüm veriyi belleğe çekmeden; `EF.Functions.Like`, `Skip` ve `Take` ile doğrudan SQL Server üzerinde çalıştırdım.
* **Ürün Bazlı Kritik Stok Göstergesi:** Ham stok adedi yerine B2B kullanımına uygun yeşil (Var), sarı (Kritik) ve kırmızı (Yok) rozetler kullandım. Kritik seviyeyi sabit bir sayı yerine her ürünün kendi `CriticalStockLevel` eşiğine göre dinamik hesaplattım.
