using ProductService.Domain.Entities;

namespace ProductService.Domain.Interfaces
{
    /// <summary>
    /// Product repository interface
    /// Domain katmanı bu interface'i tanımlar
    /// Infrastructure katmanı EF Core ile implement edecek
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Tüm aktif ürünleri getirir (stok bilgisi ile)
        /// </summary>
        Task<IEnumerable<Product>> GetAllAsync();

        /// <summary>
        /// ID'ye göre ürün getirir (stok bilgisi ile)
        /// </summary>
        /// <param name="id">Ürün ID (UNIQUEIDENTIFIER)</param>
        /// <returns>Ürün veya null</returns>
        Task<Product?> GetByIdAsync(Guid id);

        /// <summary>
        /// Yeni ürün ekler
        /// </summary>
        /// <param name="product">Eklenecek ürün</param>
        /// <returns>Eklenen ürün (ID ile birlikte)</returns>
        Task<Product> AddAsync(Product product);

        /// <summary>
        /// Ürünü günceller
        /// </summary>
        /// <param name="product">Güncellenecek ürün</param>
        Task UpdateAsync(Product product);

        /// <summary>
        /// Ürünü siler (soft delete - IsActive = false)
        /// </summary>
        /// <param name="id">Silinecek ürün ID</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Ürün var mı kontrol eder
        /// </summary>
        /// <param name="id">Kontrol edilecek ID</param>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Sadece stok miktarını günceller (RabbitMQ için)
        /// </summary>
        /// <param name="productId">Ürün ID</param>
        /// <param name="quantity">Yeni stok miktarı</param>
        Task UpdateStockAsync(Guid productId, int quantity);

        /// <summary>
        /// Ürünün stok bilgisini getirir
        /// </summary>
        /// <param name="productId">Ürün ID</param>
        Task<ProductStock?> GetStockAsync(Guid productId);
    }
}