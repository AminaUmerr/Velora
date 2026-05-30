using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velora.Data;
using Velora.Interfaces;
using Velora.Models;
using Velora.Services;
using Velora.ViewModels;

namespace Velora.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationDbContext _context;

        public DashboardController(IUnitOfWork uow, ApplicationDbContext ctx)
        { _uow = uow; _context = ctx; }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.User).Include(o => o.OrderItems)
                             .OrderByDescending(o => o.CreatedAt).Take(50).ToListAsync();

            // Monthly revenue
            var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                         "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var rawMonthly = await _context.Orders
                .Where(o => o.CreatedAt.Year == DateTime.Now.Year && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(o => o.GrandTotal) })
                .ToListAsync();

            var monthly = Enumerable.Range(1, 12).Select(m =>
            {
                var r = rawMonthly.FirstOrDefault(x => x.Month == m);
                return new MonthlyRevenue { Month = monthNames[m], Revenue = r?.Revenue ?? 0 };
            }).ToList();

            var vm = new AdminDashboardViewModel
            {
                TotalProducts  = await _context.Products.CountAsync(p => !p.IsDeleted),
                TotalOrders    = await _context.Orders.CountAsync(),
                TotalUsers     = await _context.Users.CountAsync(),
                TotalRevenue   = await _context.Orders.Where(o => o.Status != OrderStatus.Cancelled)
                                     .SumAsync(o => (decimal?)o.GrandTotal) ?? 0,
                PendingOrders  = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                RecentOrders   = orders.Take(8),
                MonthlyRevenues = monthly
            };
            return View(vm);
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _imageService;
        private readonly ApplicationDbContext _context;

        public ProductsController(IUnitOfWork uow, IImageService img, ApplicationDbContext ctx)
        { _uow = uow; _imageService = img; _context = ctx; }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Products.Include(p => p.Category)
                            .Where(p => !p.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) ||
                                         (p.Brand != null && p.Brand.Contains(search)));

            ViewBag.Search = search;
            return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            return View(new ProductViewModel
            {
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                IsActive   = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            var product = new Product();
            MapToProduct(model, product);

            if (model.ImageFile  != null) product.ImageUrl  = await _imageService.UploadImageAsync(model.ImageFile,  "products");
            if (model.ImageFile2 != null) product.ImageUrl2 = await _imageService.UploadImageAsync(model.ImageFile2, "products");
            if (model.ImageFile3 != null) product.ImageUrl3 = await _imageService.UploadImageAsync(model.ImageFile3, "products");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var vm = MapToViewModel(product);
            vm.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            MapToProduct(model, product);
            product.UpdatedAt = DateTime.Now;

            if (model.ImageFile  != null) product.ImageUrl  = await _imageService.UploadImageAsync(model.ImageFile,  "products");
            if (model.ImageFile2 != null) product.ImageUrl2 = await _imageService.UploadImageAsync(model.ImageFile2, "products");
            if (model.ImageFile3 != null) product.ImageUrl3 = await _imageService.UploadImageAsync(model.ImageFile3, "products");

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.IsDeleted = true;   // soft delete
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }

        private static void MapToProduct(ProductViewModel vm, Product p)
        {
            p.Name          = vm.Name;
            p.Description   = vm.Description;
            p.Price         = vm.Price;
            p.DiscountPrice = vm.DiscountPrice;
            p.Stock         = vm.Stock;
            p.Brand         = vm.Brand;
            p.Gender        = vm.Gender;
            p.Sizes         = vm.Sizes;
            p.Colors        = vm.Colors;
            p.Tags          = vm.Tags;
            p.SKU           = vm.SKU;
            p.CategoryId    = vm.CategoryId;
            p.IsFeatured    = vm.IsFeatured;
            p.IsNewArrival  = vm.IsNewArrival;
            p.IsBestSeller  = vm.IsBestSeller;
            p.IsTrending    = vm.IsTrending;
            p.IsActive      = vm.IsActive;
        }

        private static ProductViewModel MapToViewModel(Product p) => new()
        {
            Id            = p.Id,
            Name          = p.Name,
            Description   = p.Description,
            Price         = p.Price,
            DiscountPrice = p.DiscountPrice,
            Stock         = p.Stock,
            Brand         = p.Brand,
            Gender        = p.Gender,
            Sizes         = p.Sizes,
            Colors        = p.Colors,
            Tags          = p.Tags,
            SKU           = p.SKU,
            CategoryId    = p.CategoryId,
            IsFeatured    = p.IsFeatured,
            IsNewArrival  = p.IsNewArrival,
            IsBestSeller  = p.IsBestSeller,
            IsTrending    = p.IsTrending,
            IsActive      = p.IsActive,
            ExistingImage = p.ImageUrl,
            ExistingImage2= p.ImageUrl2,
            ExistingImage3= p.ImageUrl3
        };
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;

        public OrdersController(IOrderService orders, ApplicationDbContext ctx)
        { _orderService = orders; _context = ctx; }

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Orders.Include(o => o.User).Include(o => o.OrderItems)
                            .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var s))
                query = query.Where(o => o.Status == s);

            ViewBag.CurrentStatus = status ?? "";
            return View(await query.OrderByDescending(o => o.CreatedAt).Take(200).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            await _orderService.UpdateOrderStatusAsync(id, status);
            TempData["Success"] = "Order status updated!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext ctx) => _context = ctx;

        public async Task<IActionResult> Index() =>
            View(await _context.Categories.IgnoreQueryFilters().OrderBy(c => c.DisplayOrder).ToListAsync());

        public IActionResult Create() => View(new Category { IsActive = true });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);
            model.Slug = model.Name.ToLower().Replace(" ", "-").Replace("'", "");
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category created!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (!ModelState.IsValid) return View(model);
            var cat = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            cat.Name = model.Name; cat.Description = model.Description;
            cat.IsActive = model.IsActive; cat.DisplayOrder = model.DisplayOrder;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category updated!";
            return RedirectToAction(nameof(Index));
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersController(UserManager<ApplicationUser> um) => _userManager = um;

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
    }
}
