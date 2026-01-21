using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs
{
    /// <summary>
    /// Ürün stok bilgisi veri transfer objesi
    /// RabbitMQ ile stok güncellemeleri için kullanılır
    /// 
    /// SENARYO:
    /// 1. OrderService'de sipariş oluşturulur
    /// 2. RabbitMQ ile ProductService'e mesaj gönderilir
    /// 3. ProductService stoğu günceller
    /// 
    /// Bu DTO minimal veri içerir (sadece ProductId ve Quantity)
    /// Gereksiz veri transferi önlenir
    /// </summary>
    public class ProductStockDto
    {
        /// <summary>
        /// Ürün ID (hangi ürünün stoğu güncellenecek?)
        /// </summary>
        [Required(ErrorMessage = "Ürün ID zorunludur")]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Yeni stok miktarı
        /// 
        /// NOT: Bu mutlak değer (absolute value)
        /// Örnek: Quantity = 50 → Stok 50 olarak set edilir
        /// 
        /// ALTERNATIF YAKLAŞIM (ileride):
        /// - QuantityChange = -3 → Stoktan 3 çıkar
        /// - QuantityChange = +10 → Stoğa 10 ekle
        /// </summary>
        [Required(ErrorMessage = "Stok miktarı zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int Quantity { get; set; }

        /// <summary>
        /// Stok güncelleme zamanı (otomatik set edilir)
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}