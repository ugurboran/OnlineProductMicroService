using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Domain.Entities
{
    /// <summary>
    /// Ürün stok entity'si - ProductStock tablosunu temsil eder
    /// SQL: CREATE TABLE ProductStock (...)
    /// RabbitMQ ile stok güncellemeleri bu entity üzerinden yapılacak
    /// </summary>
    public class ProductStock
    {
        /// <summary>
        /// Ürün ID (Primary Key + Foreign Key)
        /// SQL: ProductId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
        /// </summary>
        [Key]
        [ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Stok miktarı
        /// SQL: Quantity INT NOT NULL
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Son güncelleme zamanı
        /// SQL: UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        /// <summary>
        /// İlişkili ürün
        /// CONSTRAINT FK_ProductStock_Product FOREIGN KEY (ProductId) REFERENCES Products(Id)
        /// </summary>
        public Product Product { get; set; } = null!;
    }
}
