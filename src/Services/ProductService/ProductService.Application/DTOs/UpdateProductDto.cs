using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs
{
    /// <summary>
    /// Ürün güncelleme veri transfer objesi
    /// Mevcut ürün bilgilerini güncellemek için kullanılır
    /// 
    /// NEDEN NULLABLE?
    /// - Kullanıcı sadece istediği alanı güncelleyebilmeli
    /// - Örnek: Sadece fiyat değişsin, isim aynı kalsın
    /// - Null olan alanlar güncellenmez
    /// </summary>
    public class UpdateProductDto
    {
        /// <summary>
        /// Ürün adı (opsiyonel güncelleme)
        /// Eğer null ise, mevcut isim değişmez
        /// </summary>
        [MaxLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir")]
        public string? Name { get; set; }

        /// <summary>
        /// Ürün açıklaması (opsiyonel güncelleme)
        /// Eğer null ise, mevcut açıklama değişmez
        /// </summary>
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        /// <summary>
        /// Ürün fiyatı (opsiyonel güncelleme)
        /// Eğer null ise, mevcut fiyat değişmez
        /// 
        /// NOT: decimal? = Nullable decimal
        /// </summary>
        [Range(0.01, 1000000, ErrorMessage = "Fiyat 0.01 ile 1.000.000 arasında olmalıdır")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Ürün aktif mi? (opsiyonel güncelleme)
        /// Eğer null ise, mevcut durum değişmez
        /// 
        /// KULLANIM ÖRNEĞİ:
        /// - IsActive = false → Ürünü deaktif et (soft delete)
        /// - IsActive = true → Ürünü aktif et
        /// - IsActive = null → Durumu değiştirme
        /// </summary>
        public bool? IsActive { get; set; }
    }
}