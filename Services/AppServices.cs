using Velora.Interfaces;
using Velora.Models;
using Velora.ViewModels;
using Microsoft.EntityFrameworkCore;
using Velora.Data;

namespace Velora.Services
{
    // ─── Cart Service ────────────────────────────────────────────────────────────
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int quantity, string? size, string? color);
        Task RemoveFromCartAsync(string userId, int cartItemId);
        Task UpdateQuantityAsync(string userId, int cartItemId, int quantity);
        Task ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        public CartService(ApplicationDbContext context) => _context = context;

        public async Task<Cart?> GetCartAsync(string userId) =>
            await _context.Carts
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task AddToCartAsync(string userId, int productId, int qty, string? size, string? color)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existing = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == productId &&
                ci.SelectedSize == size &&
                ci.SelectedColor == color);

            if (existing != null)
                existing.Quantity += qty;
            else
                cart.CartItems.Add(new CartItem
                {
                    CartId       = cart.Id,
                    ProductId    = productId,
                    Quantity     = qty,
                    SelectedSize = size,
                    SelectedColor= color
                });

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string userId, int cartItemId)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantityAsync(string userId, int cartItemId, int qty)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (item == null) return;

            if (qty <= 0) _context.CartItems.Remove(item);
            else          item.Quantity = qty;

            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            var items = await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCountAsync(string userId) =>
            await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;
    }

    // ─── Order Service ───────────────────────────────────────────────────────────
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string userId, CheckoutViewModel model);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderDetailsAsync(int orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
    }

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;

        public OrderService(IUnitOfWork uow, IEmailService email,
            ICartService cart, ApplicationDbContext ctx)
        {
            _uow = uow; _emailService = email;
            _cartService = cart; _context = ctx;
        }

        public async Task<Order> CreateOrderAsync(string userId, CheckoutViewModel model)
        {
            var cart = await _cartService.GetCartAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            decimal subTotal = cart.CartItems.Sum(ci => ci.TotalPrice);
            decimal shipping = subTotal >= 5000 ? 0 : 350m;
            decimal tax      = Math.Round(subTotal * 0.05m, 2);
            decimal grand    = subTotal + shipping + tax;

            // ── Build masked card info if paid by card ───────────────
            string? maskedCard  = null;
            string? cardHolder  = null;
            bool    isPaid      = false;

            if (model.PaymentMethod == PaymentMethod.CreditDebitCard
                && model.CardDetails != null)
            {
                // Strip spaces, keep last 4 digits only — NEVER store the full number
                var digits = model.CardDetails.CardNumber.Replace(" ", "");
                var last4  = digits.Length >= 4 ? digits[^4..] : digits;
                maskedCard = $"**** **** **** {last4}";
                cardHolder = model.CardDetails.CardHolderName;
                isPaid     = true;   // card payment is immediate
            }

            var order = new Order
            {
                OrderNumber     = $"VLR-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                UserId          = userId,
                FullName        = model.FullName,
                ShippingAddress = model.ShippingAddress,
                City            = model.City,
                PostalCode      = model.PostalCode,
                PhoneNumber     = model.PhoneNumber,
                PaymentMethod   = model.PaymentMethod,
                Notes           = model.Notes,
                SubTotal        = subTotal,
                ShippingCharges = shipping,
                Tax             = tax,
                GrandTotal      = grand,
                Status          = OrderStatus.Pending,
                IsPaid          = isPaid,
                MaskedCardNumber= maskedCard,
                CardHolderName  = cardHolder,
                OrderItems      = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId     = ci.ProductId,
                    ProductName   = ci.Product?.Name ?? "",
                    ProductImage  = ci.Product?.ImageUrl,
                    UnitPrice     = ci.UnitPrice,
                    Quantity      = ci.Quantity,
                    TotalPrice    = ci.TotalPrice,
                    SelectedSize  = ci.SelectedSize,
                    SelectedColor = ci.SelectedColor
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await _cartService.ClearCartAsync(userId);
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            var prev    = order.Status;
            order.Status = status;
            if (status == OrderStatus.Shipped)   order.ShippedAt   = DateTime.Now;
            if (status == OrderStatus.Delivered) order.DeliveredAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (status == OrderStatus.Shipped && prev != OrderStatus.Shipped
                && order.User != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendShippingNotificationAsync(
                            order.User.Email!, order.User.FullName, order.OrderNumber);
                    }
                    catch { /* logged inside EmailService */ }
                });
            }

            return true;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId) =>
            await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

        public async Task<Order?> GetOrderDetailsAsync(int orderId) =>
            await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber) =>
            await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    // ─── Image Service ───────────────────────────────────────────────────────────
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder);
        void DeleteImage(string? imageUrl);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageService> _logger;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger)
        { _env = env; _logger = logger; }

        public async Task<string?> UploadImageAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0) return null;
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext)) return null;
                if (file.Length > 5 * 1024 * 1024)   return null;  // 5 MB max

                var dir = Path.Combine(_env.WebRootPath, "images", folder);
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(dir, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
                return $"/images/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image upload failed");
                return null;
            }
        }

        public void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http")) return;
            try
            {
                var full = Path.Combine(_env.WebRootPath,
                    imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(full)) File.Delete(full);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image delete failed for {Url}", imageUrl);
            }
        }
    }
}
