using Shared.Events.Common;

namespace Shared.Events.Orders
{
    /// <summary>
    /// Sipariş oluşturuldu event'i
    /// 
    /// SAGA AKIŞINDAKİ YERİ:
    /// Bu event SAGA'nın başlangıç noktasıdır
    /// 
    /// AKIŞ:
    /// 1. Kullanıcı → OrderService (POST /api/orders)
    /// 2. OrderService → Sipariş oluştur (DB'ye kaydet, Status: Pending)
    /// 3. OrderService → OrderCreatedEvent yayınla (RabbitMQ'ya)
    /// 4. ProductService → Bu event'i dinle → Stok kontrolü yap
    /// 
    /// KİM DİNLER?
    /// - ProductService: Stok azaltmak için
    /// - NotificationService: Kullanıcıya "Sipariş alındı" bildirimi için (opsiyonel)
    /// - AnalyticsService: Sipariş istatistikleri için (opsiyonel)
    /// 
    /// BAŞARISIZ SENARYO:
    /// Eğer ProductService "stok yetersiz" derse:
    /// → StockReservationFailedEvent yayınlar
    /// → OrderService dinler
    /// → COMPENSATION: Siparişi iptal et (Status: Cancelled)
    /// </summary>
    public class OrderCreatedEvent : BaseEvent
    {
        /// <summary>
        /// Sipariş benzersiz kimliği
        /// 
        /// NE ZAMAN SET EDİLİR?
        /// - OrderService sipariş oluştururken (DB insert)
        /// - Order entity'sinin Id'si buraya kopyalanır
        /// 
        /// KİM KULLANIR?
        /// - ProductService: "Hangi sipariş için stok azaltacağım?" → Bu ID
        /// - PaymentService: "Hangi sipariş için ödeme alacağım?" → Bu ID
        /// - NotificationService: "Hangi siparişi bildireceğim?" → Bu ID
        /// 
        /// COMPENSATION'DA:
        /// - OrderCancelledEvent.OrderId = Bu ID (aynı sipariş iptal ediliyor)
        /// 
        /// VERİTABANI İLİŞKİSİ:
        /// Orders tablosu → Id kolonu (Primary Key)
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Siparişi oluşturan kullanıcı ID
        /// 
        /// KİM KULLANIR?
        /// - PaymentService: Bu kullanıcının ödeme bilgilerini al
        /// - NotificationService: Bu kullanıcıya e-posta/SMS gönder
        /// - IdentityService: Kullanıcı bilgilerini getir (ad, e-posta)
        /// 
        /// GÜVENLİK:
        /// - Bu ID JWT token'dan alınır (authenticated user)
        /// - Client'tan gelen UserId'ye güvenme, token'dan al
        /// 
        /// ÖRNEK KULLANIM (OrderService):
        /// var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        /// var @event = new OrderCreatedEvent { UserId = Guid.Parse(userId) };
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Sipariş kalemleri (hangi ürünlerden kaç adet?)
        /// 
        /// NEDEN LİSTE?
        /// - Bir sipariş birden fazla ürün içerebilir
        /// - Örnek: Laptop (1 adet) + Mouse (2 adet) + Klavye (1 adet)
        /// 
        /// PRODUCTSERVICE KULLANIMI:
        /// foreach (var item in @event.Items)
        /// {
        ///     // Her ürün için stok kontrolü
        ///     var product = await GetProductAsync(item.ProductId);
        ///     if (product.Stock < item.Quantity)
        ///     {
        ///         // Stok yetersiz → SAGA başarısız
        ///         await PublishStockReservationFailedEvent(...);
        ///         return;
        ///     }
        ///     
        ///     // Stok azalt
        ///     product.Stock -= item.Quantity;
        ///     await UpdateProductAsync(product);
        /// }
        /// 
        /// VERİTABANI İLİŞKİSİ:
        /// OrderItems tablosu (her kalem bir satır)
        /// </summary>
        public List<OrderItemDto> Items { get; set; } = new();

        /// <summary>
        /// Sipariş toplam tutarı (TL)
        /// 
        /// NASIL HESAPLANIR?
        /// TotalAmount = Σ (Item.Quantity × Item.UnitPrice)
        /// 
        /// ÖRNEK:
        /// Item 1: Laptop × 1 = 15.000 TL
        /// Item 2: Mouse × 2 = 500 TL
        /// Item 3: Klavye × 1 = 800 TL
        /// ─────────────────────────────
        /// TotalAmount = 16.300 TL
        /// 
        /// KİM KULLANIR?
        /// - PaymentService: Bu tutarı tahsil et
        /// - NotificationService: "16.300 TL tutarında siparişiniz alındı"
        /// 
        /// GÜVENLİK:
        /// - Client'tan gelen TotalAmount'a güvenme!
        /// - Server tarafında tekrar hesapla ve doğrula
        /// 
        /// ÖRNEK DOĞRULAMA (OrderService):
        /// var calculatedTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
        /// if (Math.Abs(calculatedTotal - dto.TotalAmount) > 0.01m)
        /// {
        ///     throw new ValidationException("Total amount mismatch");
        /// }
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Sipariş durumu (başlangıç: Pending)
        /// 
        /// DURUM GEÇİŞLERİ (BAŞARILI SENARYO):
        /// Pending → StockReserved → PaymentProcessed → Confirmed → Shipped → Delivered
        /// 
        /// DURUM GEÇİŞLERİ (BAŞARISIZ SENARYO - Stok Yetersiz):
        /// Pending → Cancelled
        /// 
        /// DURUM GEÇİŞLERİ (BAŞARISIZ SENARYO - Ödeme Başarısız):
        /// Pending → StockReserved → PaymentFailed → Cancelled
        /// (Stok geri eklenir - compensation)
        /// 
        /// NEDEN STRING?
        /// - Esnek: İleride yeni durumlar eklenebilir
        /// - Okunabilir: "Pending" vs 1, 2, 3 (enum index)
        /// 
        /// ALTERNATİF: ENUM KULLANIMI
        /// public OrderStatus Status { get; set; } = OrderStatus.Pending;
        /// 
        /// BİZİM TERCİHİMİZ: String (basit, esnek)
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Teslimat adresi (opsiyonel, şimdilik)
        /// 
        /// İLERİDE EKLENEBİLİR:
        /// - Teslimat adresi bilgileri
        /// - Kargo tercihi
        /// - Teslimat zamanı tercihi
        /// 
        /// ŞİMDİLİK:
        /// - Basit tutmak için null bırakıyoruz
        /// - SAGA pattern'i öğrenmeye odaklanıyoruz
        /// </summary>
        public string? ShippingAddress { get; set; }
    }

    /// <summary>
    /// Sipariş kalemi veri transfer objesi
    /// 
    /// NEDEN AYRI CLASS?
    /// - OrderCreatedEvent içinde liste olarak kullanılır
    /// - Temiz, organize kod
    /// - Yeniden kullanılabilir (başka event'lerde de kullanılabilir)
    /// 
    /// VERİTABANI KARŞILIĞI:
    /// OrderItems tablosu
    /// </summary>
    public class OrderItemDto
    {
        /// <summary>
        /// Ürün benzersiz kimliği
        /// 
        /// REFERANS:
        /// Products tablosu → Id kolonu
        /// 
        /// PRODUCTSERVICE KULLANIMI:
        /// var product = await _productRepository.GetByIdAsync(item.ProductId);
        /// if (product == null)
        /// {
        ///     throw new NotFoundException($"Product {item.ProductId} not found");
        /// }
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Sipariş edilen miktar (kaç adet?)
        /// 
        /// VALİDASYON:
        /// - Quantity > 0 olmalı
        /// - Quantity <= Product.Stock olmalı (ProductService kontrol eder)
        /// 
        /// ÖRNEK:
        /// Kullanıcı "Laptop" ürününden 2 adet sipariş etti
        /// → Quantity = 2
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Birim fiyat (sipariş anındaki fiyat)
        /// 
        /// ÖNEMLİ NOKTA:
        /// - Bu fiyat SİPARİŞ ANINDAK İ fiyattır
        /// - Ürün fiyatı yarın değişebilir
        /// - Ama sipariş fiyatı değişmez (tarihi kayıt)
        /// 
        /// ÖRNEK SENARYO:
        /// t=0: Ürün fiyatı 10.000 TL
        /// t=1: Kullanıcı sipariş oluşturur → UnitPrice = 10.000 TL kaydedilir
        /// t=2: Ürün fiyatı 12.000 TL'ye çıkar (kampanya bitti)
        /// t=3: Kullanıcı siparişe bakıyor → Hala 10.000 TL görür ✅
        /// 
        /// NASIL SET EDİLİR? (OrderService)
        /// var product = await _productRepository.GetByIdAsync(dto.ProductId);
        /// var orderItem = new OrderItem
        /// {
        ///     ProductId = dto.ProductId,
        ///     Quantity = dto.Quantity,
        ///     UnitPrice = product.Price  // ← O anki fiyat
        /// };
        /// 
        /// GÜVENLİK:
        /// - Client'tan gelen UnitPrice'a güvenme!
        /// - Server tarafında Product tablosundan al
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Ürün adı (snapshot - o anki ad)
        /// 
        /// NEDEN GEREKLİ?
        /// - Ürün adı ileride değişebilir
        /// - Ama sipariş kaydında eski ad kalmalı
        /// 
        /// ÖRNEK SENARYO:
        /// t=0: Ürün adı "MacBook Pro 14 inch"
        /// t=1: Kullanıcı sipariş oluşturur → ProductName = "MacBook Pro 14 inch"
        /// t=2: Ürün adı "MacBook Pro 14 inch M3" olarak değişir
        /// t=3: Kullanıcı eski siparişe bakıyor → "MacBook Pro 14 inch" görür ✅
        /// 
        /// ALTERNATİF YAKLAŞIM:
        /// - ProductName kaydetmeyiz
        /// - Her seferinde Product tablosundan çekeriz
        /// - Ama ürün silinirse? → OrderItem'da ProductName null olur ❌
        /// 
        /// BİZİM YAKLAŞIM:
        /// - Snapshot al (o anki bilgiyi kaydet)
        /// - Tarihsel veri bütünlüğü
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
    }
}