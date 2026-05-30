using Microsoft.AspNetCore.Mvc;
using Velora.Interfaces;
using Velora.ViewModels;

namespace Velora.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _uow;
        public HomeController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                Banners = await _uow.Banners.GetAllAsync(),
                FeaturedProducts = await _uow.Products.GetFeaturedProductsAsync(8),
                NewArrivals = await _uow.Products.GetNewArrivalsAsync(8),
                BestSellers = await _uow.Products.GetBestSellersAsync(8),
                TrendingProducts = await _uow.Products.GetTrendingProductsAsync(4),
                Categories = await _uow.Categories.GetActiveAsync()
            };
            return View(vm);
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
        public IActionResult FAQ() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    var existing = (await _uow.NewsletterSubscribers.GetAllAsync())
                        .FirstOrDefault(n => n.Email == email);
                    if (existing == null)
                    {
                        await _uow.NewsletterSubscribers.AddAsync(new Models.NewsletterSubscriber { Email = email });
                        await _uow.SaveAsync();
                        TempData["SubscribeMsg"] = "Thank you for subscribing!";
                    }
                    else TempData["SubscribeMsg"] = "You're already subscribed!";
                }
                catch { TempData["SubscribeMsg"] = "Subscription failed. Please try again."; }
            }
            return RedirectToAction("Index");
        }
    }
}
