using Microsoft.AspNetCore.Mvc;
using Velora.Interfaces;
using Velora.Services;
using Velora.ViewModels;

namespace Velora.Controllers
{
    public class ShopController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ICartService _cartService;

        public ShopController(IUnitOfWork uow, ICartService cartService)
        {
            _uow = uow; _cartService = cartService;
        }

        public async Task<IActionResult> Index(ShopViewModel filter)
        {
            filter.PageSize = 12;
            if (filter.Page < 1) filter.Page = 1;

            var (products, total) = await _uow.Products.GetFilteredAsync(
                filter.Search, filter.CategoryId, filter.Gender, filter.Brand,
                filter.MinPrice, filter.MaxPrice, filter.Sort, filter.Page, filter.PageSize);

            filter.Products = products;
            filter.TotalCount = total;
            filter.Categories = await _uow.Categories.GetActiveAsync();
            return View(filter);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _uow.Products.GetWithDetailsAsync(id);
            if (product == null) return NotFound();

            var related = await _uow.Products.GetRelatedProductsAsync(id, product.CategoryId);

            bool inWishlist = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetUserId();
                var wishlist = await _uow.Wishlists.GetWishlistWithItemsAsync(userId);
                inWishlist = wishlist?.WishlistItems.Any(wi => wi.ProductId == id) ?? false;
            }

            var vm = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = related,
                IsInWishlist = inWishlist,
                NewReview = new ReviewViewModel { ProductId = id }
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(ReviewViewModel model)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                await _uow.Reviews.AddAsync(new Models.Review
                {
                    ProductId = model.ProductId,
                    UserId = GetUserId(),
                    Rating = model.Rating,
                    Title = model.Title,
                    Comment = model.Comment
                });
                await _uow.SaveAsync();
                TempData["Success"] = "Review submitted successfully!";
            }
            return RedirectToAction("Details", new { id = model.ProductId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
                return Json(new List<object>());

            var products = await _uow.Products.SearchProductsAsync(q);
            var suggestions = products.Take(6).Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.FinalPrice,
                image = p.ImageUrl,
                category = p.Category?.Name
            });
            return Json(suggestions);
        }

        private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}
