using Microsoft.EntityFrameworkCore;
using Velora.Data;
using Velora.Interfaces;
using Velora.Models;

namespace Velora.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public virtual void Update(T entity) => _dbSet.Update(entity);
        public virtual void Delete(T entity) => _dbSet.Remove(entity);
        public virtual async Task<bool> ExistsAsync(int id) => await _dbSet.FindAsync(id) != null;
    }

    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8) =>
            await _context.Products.Include(p => p.Category).Where(p => p.IsFeatured && p.IsActive).Take(count).ToListAsync();

        public async Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 8) =>
            await _context.Products.Include(p => p.Category).Where(p => p.IsNewArrival && p.IsActive).OrderByDescending(p => p.CreatedAt).Take(count).ToListAsync();

        public async Task<IEnumerable<Product>> GetBestSellersAsync(int count = 8) =>
            await _context.Products.Include(p => p.Category).Where(p => p.IsBestSeller && p.IsActive).Take(count).ToListAsync();

        public async Task<IEnumerable<Product>> GetTrendingProductsAsync(int count = 8) =>
            await _context.Products.Include(p => p.Category).Where(p => p.IsTrending && p.IsActive).Take(count).ToListAsync();

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId) =>
            await _context.Products.Include(p => p.Category).Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync();

        public async Task<IEnumerable<Product>> SearchProductsAsync(string query) =>
            await _context.Products.Include(p => p.Category)
                .Where(p => p.IsActive && (p.Name.Contains(query) || (p.Brand != null && p.Brand.Contains(query)) || (p.Description != null && p.Description.Contains(query))))
                .ToListAsync();

        public async Task<Product?> GetWithDetailsAsync(int id) =>
            await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4) =>
            await _context.Products.Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.Id != productId && p.IsActive)
                .Take(count).ToListAsync();

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredAsync(
            string? search, int? categoryId, string? gender, string? brand,
            decimal? minPrice, decimal? maxPrice, string? sort, int page, int pageSize)
        {
            var query = _context.Products.Include(p => p.Category).Where(p => p.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(p => p.Gender == gender || p.Gender == "Unisex");

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(p => p.Brand == brand);

            if (minPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price) >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price) <= maxPrice.Value);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price),
                "price_desc" => query.OrderByDescending(p => p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "name_asc" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var total = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (products, total);
        }
    }

    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId) =>
            await _context.Orders.Include(o => o.OrderItems).Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToListAsync();

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId) =>
            await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).Include(o => o.User).FirstOrDefaultAsync(o => o.Id == orderId);

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber) =>
            await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10) =>
            await _context.Orders.Include(o => o.User).OrderByDescending(o => o.CreatedAt).Take(count).ToListAsync();

        public async Task<decimal> GetTotalRevenueAsync() =>
            await _context.Orders.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => o.GrandTotal);

        public async Task<IEnumerable<(int Month, decimal Revenue)>> GetMonthlyRevenueAsync(int year)
        {
            var data = await _context.Orders
                .Where(o => o.CreatedAt.Year == year && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(o => o.GrandTotal) })
                .ToListAsync();
            return data.Select(d => (d.Month, d.Revenue));
        }
    }

    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Cart?> GetCartByUserIdAsync(string userId) =>
            await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task<Cart?> GetCartWithItemsAsync(string userId) =>
            await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Product).ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Wishlist?> GetWishlistByUserIdAsync(string userId) =>
            await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId);

        public async Task<Wishlist?> GetWishlistWithItemsAsync(string userId) =>
            await _context.Wishlists.Include(w => w.WishlistItems).ThenInclude(wi => wi.Product).ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Category?> GetBySlugAsync(string slug) =>
            await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug);

        public async Task<IEnumerable<Category>> GetActiveAsync() =>
            await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }
        public ICartRepository Carts { get; }
        public IWishlistRepository Wishlists { get; }
        public ICategoryRepository Categories { get; }
        public IGenericRepository<Review> Reviews { get; }
        public IGenericRepository<Banner> Banners { get; }
        public IGenericRepository<NewsletterSubscriber> NewsletterSubscribers { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Products = new ProductRepository(context);
            Orders = new OrderRepository(context);
            Carts = new CartRepository(context);
            Wishlists = new WishlistRepository(context);
            Categories = new CategoryRepository(context);
            Reviews = new GenericRepository<Review>(context);
            Banners = new GenericRepository<Banner>(context);
            NewsletterSubscribers = new GenericRepository<NewsletterSubscriber>(context);
        }

        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
