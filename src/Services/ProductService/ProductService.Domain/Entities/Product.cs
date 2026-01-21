using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain.Entities
{
    /// <summary>
    /// Ürün entity'si - Products tablosunu temsil eder
    /// SQL: CREATE TABLE Products (...)
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Benzersiz ürün kimliði
        /// SQL: Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Ürün adý (zorunlu, max 200 karakter)
        /// SQL: Name NVARCHAR(200) NOT NULL
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ürün açýklamasý (max 500 karakter, opsiyonel)
        /// SQL: Description NVARCHAR(500)
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Ürün fiyatý
        /// SQL: Price DECIMAL(18,2) NOT NULL
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Ürün aktif mi? (Soft delete için)
        /// SQL: IsActive BIT NOT NULL DEFAULT 1
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ürün oluþturulma tarihi
        /// SQL: CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Son güncellenme tarihi (nullable)
        /// SQL: UpdatedAt DATETIME2 NULL
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property (1-1 iliþki)
        /// <summary>
        /// Ürün stok bilgisi (ProductStock tablosu ile iliþki)
        /// </summary>
        public ProductStock? Stock { get; set; } //(nullable) Ürün yeni oluşturulduğunda stok henüz olmayabilir ? olmazsa NullReferenceException riski
    }
}