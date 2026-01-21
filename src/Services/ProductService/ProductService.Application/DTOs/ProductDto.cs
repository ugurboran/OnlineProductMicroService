namespace ProductService.Application.DTOs
{
    /// <summary>
    /// Ürün veri transfer objesi
    /// API'ye dönen ürün bilgilerini içerir
    /// 
    /// NEDEN DTO?
    /// - Domain entity'sini direkt API'ye göndermek güvenli değil
    /// - Sadece gerekli alanları içerir (filtreleme)
    /// - API versiyonları değişse bile Domain entity'si sabit kalır
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Ürün benzersiz kimliği
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Ürün adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ürün açıklaması (opsiyonel)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Ürün fiyatı (TL)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Stok miktarı
        /// NOT: ProductStock entity'sinden geliyor, burada tek property olarak gösteriyoruz
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Ürün aktif mi?
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ürün oluşturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}