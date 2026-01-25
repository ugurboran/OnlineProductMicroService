namespace Shared.Events.Common
{
    /// <summary>
    /// Tüm event'ler için temel sınıf
    /// 
    /// NEDEN BASE CLASS?
    /// - Her event'te ortak alanlar var (EventId, OccurredAt, SagaId)
    /// - DRY prensibi (Don't Repeat Yourself) - Kod tekrarını önler
    /// - Event tracking ve debugging için gerekli bilgileri merkezi tutar
    /// 
    /// SAGA PATTERN İÇİN ÖNEMİ:
    /// - SagaId: Hangi SAGA akışına ait? (Bir sipariş süreci = 1 SAGA)
    /// - EventId: Her event benzersiz (idempotency için kritik)
    /// - OccurredAt: Event ne zaman oluştu? (sıralama, debugging)
    /// </summary>
    public abstract class BaseEvent
    {
        /// <summary>
        /// Event'in benzersiz kimliği
        /// 
        /// KULLANIM AMACI:
        /// 1. Idempotency kontrolü
        ///    - Aynı event 2 kez işlenmemeli
        ///    - Örnek: RabbitMQ retry → Aynı mesaj 2 kez gelirse
        ///    - ProcessedEvents tablosunda bu ID var mı kontrol et
        /// 
        /// 2. Event store'da kayıt
        ///    - Her event'i veritabanında saklarsan (event sourcing)
        ///    - Bu ID primary key olur
        /// 
        /// 3. Distributed tracing
        ///    - Hangi event hangi servisi tetikledi?
        ///    - Log'larda EventId ile takip et
        /// 
        /// ÖRNEK KULLANIM:
        /// var processedEvents = await _repo.GetProcessedEventIdsAsync();
        /// if (processedEvents.Contains(@event.EventId))
        /// {
        ///     return; // Daha önce işlendi, tekrar işleme
        /// }
        /// </summary>
        public Guid EventId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// SAGA benzersiz kimliği
        /// 
        /// SAGA NEDİR?
        /// - Bir iş akışı (business transaction)
        /// - Örnek: Sipariş süreci (başlangıç → bitiş)
        /// - Birden fazla event aynı SAGA'ya ait olabilir
        /// 
        /// NEDEN GEREKLİ?
        /// - Event'leri gruplamak için
        /// - "Bu sipariş hangi aşamada?" sorusunu cevaplamak için
        /// - Compensation (geri alma) sırasında ilgili tüm event'leri bulmak için
        /// 
        /// ÖRNEK SENARYO:
        /// SagaId: "saga-123" (Bir sipariş akışı)
        ///   ├─ OrderCreatedEvent (EventId: evt-1, SagaId: saga-123)
        ///   ├─ StockReservedEvent (EventId: evt-2, SagaId: saga-123)
        ///   ├─ PaymentFailedEvent (EventId: evt-3, SagaId: saga-123) ← Hata!
        ///   ├─ StockReleasedEvent (EventId: evt-4, SagaId: saga-123) ← Compensation
        ///   └─ OrderCancelledEvent (EventId: evt-5, SagaId: saga-123) ← Compensation
        /// 
        /// Tüm event'ler saga-123'e ait → İlişkili
        /// 
        /// MONITORING İÇİN:
        /// SELECT * FROM Events WHERE SagaId = 'saga-123' ORDER BY OccurredAt
        /// → Bu SAGA'nın tüm akışını görürsün
        /// </summary>
        public Guid SagaId { get; set; }

        /// <summary>
        /// Event oluşma zamanı (UTC)
        /// 
        /// NEDEN UTC?
        /// - Timezone sorunlarını önler
        ///   Örnek: OrderService İstanbul'da (UTC+3)
        ///           PaymentService Londra'da (UTC+0)
        ///           → Farklı saat dilimleri karışıklık yaratır
        /// - UTC = Standart, tüm servisler aynı zaman dilimini kullanır
        /// 
        /// NEDEN ÖNEMLİ?
        /// 1. Event ordering (sıralama)
        ///    - Event'ler hangi sırayla oluştu?
        ///    - SAGA akışında doğru sıra kritik
        /// 
        /// 2. Debugging
        ///    - "14:30'da ne oldu?" sorusunu cevaplamak
        ///    - Log'larda timestamp ile korelasyon
        /// 
        /// 3. Business analytics
        ///    - "Sipariş oluşturma ile ödeme arasında ortalama ne kadar süre geçiyor?"
        ///    - Performance metrics
        /// 
        /// 4. Timeout kontrolü
        ///    - "Event 5 dakikadan eski mi? → Timeout, compensation başlat"
        /// 
        /// DİKKAT:
        /// - DateTime.Now değil, DateTime.UtcNow kullan!
        /// - Client tarafından gönderilen timestamp'e güvenme (manipüle edilebilir)
        /// - Server tarafında set et
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Event versiyonu (schema versiyonu)
        /// 
        /// KULLANIM AMACI:
        /// Event şeması değişirse (örn: yeni alan eklendi) backward compatibility için
        /// 
        /// ÖRNEK SENARYO:
        /// V1 (2024-01-01):
        /// {
        ///   "eventId": "...",
        ///   "sagaId": "...",
        ///   "productId": "123",
        ///   "quantity": 5
        /// }
        /// 
        /// V2 (2024-06-01): warehouseId alanı eklendi
        /// {
        ///   "eventId": "...",
        ///   "sagaId": "...",
        ///   "productId": "123",
        ///   "quantity": 5,
        ///   "warehouseId": "wh-1"  ← YENİ ALAN
        /// }
        /// 
        /// CONSUMER TARAFINDA KONTROL:
        /// if (@event.Version == 1)
        /// {
        ///     // warehouseId yok, default kullan
        ///     warehouseId = "default-warehouse";
        /// }
        /// else if (@event.Version == 2)
        /// {
        ///     // warehouseId var, onu kullan
        ///     warehouseId = @event.WarehouseId;
        /// }
        /// 
        /// MIGRATION:
        /// - Eski event'ler (V1) hala kuyrukta olabilir
        /// - Yeni consumer her iki versiyonu da handle etmeli
        /// - Bu alan ile hangi versiyon olduğunu anlarız
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Event hangi servisten geldi?
        /// 
        /// KULLANIM AMACI:
        /// 1. Debugging
        ///    - "Bu event hangi servis yayınladı?"
        ///    - Log'larda korelasyon
        /// 
        /// 2. Monitoring
        ///    - "OrderService kaç tane event yayınladı?"
        ///    - Metrics, dashboards
        /// 
        /// 3. Security / Authorization
        ///    - "Bu servisten gelen event'leri kabul et, diğerlerini reddet"
        ///    - Event kaynağı doğrulama
        /// 
        /// 4. Multi-tenant sistemlerde
        ///    - "Bu event hangi tenant'tan geldi?"
        /// 
        /// ÖRNEK DEĞERLER:
        /// - "OrderService"
        /// - "ProductService"
        /// - "PaymentService"
        /// 
        /// SET ETME:
        /// Publisher tarafında:
        /// var @event = new OrderCreatedEvent
        /// {
        ///     SourceService = "OrderService",  // veya appsettings'den al
        ///     // ...
        /// };
        /// </summary>
        public string? SourceService { get; set; }

        /// <summary>
        /// Event metadata (opsiyonel, genişletilebilir)
        /// 
        /// KULLANIM AMACI:
        /// Event'e ekstra bilgi eklemek için key-value store
        /// 
        /// ÖRNEK KULLANIM:
        /// @event.Metadata = new Dictionary<string, string>
        /// {
        ///     ["UserId"] = "user-123",
        ///     ["IpAddress"] = "192.168.1.1",
        ///     ["UserAgent"] = "Mozilla/5.0...",
        ///     ["CorrelationId"] = "trace-456"  // Distributed tracing
        /// };
        /// 
        /// NEDEN DICTIONARY?
        /// - Esnek: İleride yeni alanlar ekleyebiliriz
        /// - Event şemasını bozmadan ek bilgi taşır
        /// - Her event farklı metadata içerebilir
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}