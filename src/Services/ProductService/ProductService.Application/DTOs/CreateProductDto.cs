using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs
{
    /// <summary>
    /// Yeni ürün oluşturma veri transfer objesi
    /// Kullanıcı yeni ürün eklerken bu model kullanılır
    /// 
    /// NEDEN AYRI DTO?
    /// - Ürün oluştururken Id gerekmiyor (otomatik oluşacak)
    /// - Validasyon kuralları farklı (örn: Name zorunlu)
    /// - CreatedAt, UpdatedAt otomatik set edilecek
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Ürün adı (zorunlu, max 200 karakter)
        /// 
        /// VALIDASYON:
        /// - [Required]: Boş olamaz
        /// - [MaxLength]: 200 karakterden uzun olamaz
        /// </summary>
        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [MaxLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ürün açıklaması (opsiyonel, max 500 karakter)
        /// </summary>
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        /// <summary>
        /// Ürün fiyatı (zorunlu, pozitif olmalı)
        /// 
        /// VALIDASYON:
        /// - [Required]: Boş olamaz
        /// - [Range]: 0.01 ile 1.000.000 arasında olmalı
        /// </summary>
        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, 1000000, ErrorMessage = "Fiyat 0.01 ile 1.000.000 arasında olmalıdır")]
        public decimal Price { get; set; }

        /// <summary>
        /// Başlangıç stok miktarı (zorunlu, pozitif veya 0 olabilir)
        /// 
        /// NOT: Ürün oluşturulurken ProductStock tablosuna da kayıt eklenir
        /// </summary>
        [Required(ErrorMessage = "Stok miktarı zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int InitialStock { get; set; }

        /// <summary>
        /// Ürün aktif mi? (varsayılan: true)
        /// Genelde yeni ürünler aktif oluşturulur
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}