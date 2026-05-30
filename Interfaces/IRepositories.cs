using Velora.Models;

namespace Velora.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> ExistsAsync(int id);
    }

    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 8);
        Task<IEnumerable<Product>> GetBestSellersAsync(int count = 8);
        Task<IEnumerable<Product>> GetTrendingProductsAsync(int count = 8);
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchProductsAsync(string query);
        Task<Product?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredAsync(
            string? search, int? categoryId, string? gender, string? brand,
            decimal? minPrice, decimal? maxPrice, string? sort, int page, int pageSize);
    }

    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
        Task<decimal> GetTotalRevenueAsync();
        Task<IEnumerable<(int Month, decimal Revenue)>> GetMonthlyRevenueAsync(int year);
    }

    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetCartByUserIdAsync(string userId);
        Task<Cart?> GetCartWithItemsAsync(string userId);
    }

    public interface IWishlistRepository : IGenericRepository<Wishlist>
    {
        Task<Wishlist?> GetWishlistByUserIdAsync(string userId);
        Task<Wishlist?> GetWishlistWithItemsAsync(string userId);
    }

    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetActiveAsync();
    }

    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        ICartRepository Carts { get; }
        IWishlistRepository Wishlists { get; }
        ICategoryRepository Categories { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<Banner> Banners { get; }
        IGenericRepository<NewsletterSubscriber> NewsletterSubscribers { get; }
        Task<int> SaveAsync();
    }
}
