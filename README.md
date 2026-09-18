## 1. Kullanılan Teknolojiler

* **Backend:** .NET 8, C# 12
* **Web Çatısı:** ASP.NET Core MVC (Areas destekli, Razor View Engine)
* **Veritabanı & ORM:** Microsoft SQL Server / LocalDB, Entity Framework Core 8
* **Güvenlik:** Cookie Authentication (`[Authorize]` ve Role bazlı erişim), PBKDF2 (HMAC-SHA256, 100.000 iterasyon + Salt)
* **Frontend:** Bootstrap 5, Bootstrap Icons, Vanilla JS (AJAX hızlı sipariş ve modal pencereler)

---

## 2. Veritabanı ve Kurulum Scripti

Proje varsayılan olarak **SQL Server LocalDB** üzerinde çalışacak şekilde yapılandırılmıştır.

* **Otomatik Kurulum :** Uygulamayı ilk kez çalıştırdığınızda EF Core (`EnsureCreatedAsync` ve `DbInitializer`) veritabanını, tabloları, ilişkileri, örnek B2B ürünlerini, kategorileri ve dinamik grid ayarlarını otomatik olarak oluşturur. Ekstra bir SQL komutu çalıştırmanıza gerek yoktur.
* **Manuel SQL Scripti:** Eğer tabloları ve örnek kayıtları doğrudan SQL Server Management Studio (SSMS) üzerinden kendiniz çalıştırmak isterseniz, proje kök dizinindeki hazır SQL dosyası kullanılabilir:
  * **Dosya Yolu:** `sql/schema.sql`
  * Bu dosya tüm tabloları (`Users`, `Products`, `Categories`, `Carts`, `CartItems`, `Orders`, `OrderItems`, `GridColumnConfigs`, `SliderItems`), Primary/Foreign Key kısıtlarını, Unique indeksleri ve örnek başlangıç kayıtlarını içerir.

> **Bağlantı Ayarı (`appsettings.json`):**
> ```json
> "ConnectionStrings": {
>   "DefaultConnection": "Server=(localdb)\\MiniB2BDb;Database=MiniB2BCommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
> }
> ```
> *(Farklı bir SQL Server örneği kullanmak isterseniz sadece bu connection string'i güncellemeniz yeterlidir).*

---

## 3. Projenin Çalıştırılması

Projeyi bilgisayarınızda ayağa kaldırmak için aşağıdaki adımları terminal veya komut satırından izleyebilirsiniz:

1. **Bağımlılıkları ve Projeyi Derleyin:**
   ```bash
   dotnet build
   ```

2. **Uygulamayı Başlatın:**
   ```bash
   dotnet run --project src/MiniB2B.Web
   ```

3. **Tarayıcıdan Açın:**
   * **Müşteri / Katalog Portalı:** `http://localhost:5016`
   * **Yönetici Giriş Paneli:** `http://localhost:5016/Admin`

---

## 4. Test Kullanıcı Bilgileri

Sistemde müşteri ve yönetici girişleri güvenlik gereği ayrı tutulmuştur:

| Rol | Giriş Adresi | Kullanıcı Adı / E-posta | Şifre | Yetki ve Kapsam |
|---|---|---|---|---|
| **Yönetici (Admin)** | `/Admin/Account/Login` | `admin` veya `admin@b2b.com` | `Admin123*` | Ürün, sipariş, kullanıcı, slider ve grid kolon yönetimi |
| **Müşteri (Customer)** | `/Account/Login` | `b2bmusteri` veya `musteri@b2b.com` | `Musteri123*` | Katalog arama, satırdan hızlı sipariş, sepet ve siparişlerim |
| **Müşteri 2 (Customer)** | `/Account/Login` | `alfasanayi` veya `alfa@sanayi.com` | `Alfa123*` | Alternatif müşteri hesabı |

---

## 5. Mimari Tercihleri ve Nedenleri

* **Çok Katmanlı Mimari (N-Tier):**
  * `Core`, `DataAccess`, `Business` ve `Web` katmanlarına bölünmüştür.
  * **Nedeni:** İş kurallarını (stok doğrulaması, sipariş oluşturma transaction'ı vb.) Controller'lardan ayırarak yarın bir mobil uygulama veya ERP entegrasyonu (Logo, SAP) bağlandığında kod tekrarı yapmadan aynı servisleri doğrudan kullanabilmek.
* **İnce Controller (Thin Controllers):**
  * Controller sınıfları yalnızca HTTP isteklerini karşılar, validasyon yapar ve servisi çağırır. Veritabanı sorguları veya karmaşık hesaplamalar doğrudan servis katmanında yürütülür.
* **Veritabanından Yapılandırılabilir B2B Grid:**
  * Ürün listeleme ekranındaki kolonlar kodda sabit tutulmamış, `GridColumnConfigs` tablosundan dinamik render edilmiştir.
  * **Nedeni:** B2B projelerinde her bayinin veya projenin ihtiyaç duyduğu kolon sırası, genişliği ve mobil görünürlüğü kod değiştirmeden yönetim panelinden anında değiştirilebilir.
* **Sunucu Taraflı SQL Filtreleme ve Sayfalama:**
  * Ürün arama (kod, ad, marka, üretici kodu, özel kodlar, açıklama) ve sayfalama işlemleri `EF.Functions.Like` ve `Skip/Take` ile doğrudan SQL Server üzerinde çalıştırılır.
  * **Nedeni:** Binlerce ürün içeren B2B kataloglarında tüm verinin belleğe çekilmesini önleyip sunucu performansını korumak.
* **Fiyat Snapshot Koruması:**
  * Sipariş verildiği andaki ürün fiyatı ve ürün kodu `OrderItems` tablosuna bağımsız olarak kopyalanır. Ürün fiyatı sonradan değişse bile geçmiş siparişin satın alınan fiyatı değişmez.
