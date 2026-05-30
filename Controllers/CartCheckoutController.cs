using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Velora.Models;
using Velora.Services;
using Velora.ViewModels;

namespace Velora.Controllers
{
    // ═══════════════════════════════════════════════════════════════
    //  CART CONTROLLER
    // ═══════════════════════════════════════════════════════════════
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return View(new CartViewModel());

            var userId   = _userManager.GetUserId(User)!;
            var cart     = await _cartService.GetCartAsync(userId);
            var subTotal = cart?.CartItems.Sum(ci => ci.TotalPrice) ?? 0;
            var shipping = subTotal >= 5000 ? 0 : (subTotal > 0 ? 350m : 0);
            var tax      = Math.Round(subTotal * 0.05m, 2);

            return View(new CartViewModel
            {
                Cart            = cart,
                SubTotal        = subTotal,
                ShippingCharges = shipping,
                Tax             = tax,
                GrandTotal      = subTotal + shipping + tax,
                FreeShipping    = subTotal >= 5000
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1,
            string? size = null, string? color = null)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Json(new
                {
                    success  = false,
                    message  = "Please login to add items to cart.",
                    redirect = "/Account/Login"
                });

            var userId = _userManager.GetUserId(User)!;
            await _cartService.AddToCartAsync(userId, productId, quantity, size, color);
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Json(new { success = true, message = "Item added to cart!", cartCount = count });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();
            await _cartService.RemoveFromCartAsync(_userManager.GetUserId(User)!, cartItemId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var userId = _userManager.GetUserId(User)!;
            await _cartService.UpdateQuantityAsync(userId, cartItemId, quantity);

            var cart     = await _cartService.GetCartAsync(userId);
            var subTotal = cart?.CartItems.Sum(ci => ci.TotalPrice) ?? 0;
            var shipping = subTotal >= 5000 ? 0 : (subTotal > 0 ? 350m : 0);
            var tax      = Math.Round(subTotal * 0.05m, 2);

            return Json(new
            {
                success    = true,
                subTotal   = subTotal.ToString("N0"),
                shipping   = shipping == 0 ? "FREE" : shipping.ToString("N0"),
                tax        = tax.ToString("N0"),
                grandTotal = (subTotal + shipping + tax).ToString("N0"),
                cartCount  = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            if (User.Identity?.IsAuthenticated != true) return Json(new { count = 0 });
            var count = await _cartService.GetCartItemCountAsync(_userManager.GetUserId(User)!);
            return Json(new { count });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CHECKOUT CONTROLLER
    // ═══════════════════════════════════════════════════════════════
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService   _cartService;
        private readonly IOrderService  _orderService;
        private readonly IEmailService  _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ICartService cart, IOrderService order,
            IEmailService email, UserManager<ApplicationUser> um)
        {
            _cartService  = cart;
            _orderService = order;
            _emailService = email;
            _userManager  = um;
        }

        // ── GET /Checkout ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetCartAsync(user.Id);
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var subTotal = cart.CartItems.Sum(ci => ci.TotalPrice);
            var shipping = subTotal >= 5000 ? 0 : 350m;
            var tax      = Math.Round(subTotal * 0.05m, 2);

            return View(new CheckoutViewModel
            {
                FullName        = user.FullName,
                PhoneNumber     = user.PhoneNumber ?? "",
                ShippingAddress = user.Address ?? "",
                City            = user.City ?? "",
                PostalCode      = user.PostalCode ?? "",
                CardDetails     = new CardDetailsViewModel(),   // pre-create so form binds
                CartSummary     = new CartViewModel
                {
                    Cart            = cart,
                    SubTotal        = subTotal,
                    ShippingCharges = shipping,
                    Tax             = tax,
                    GrandTotal      = subTotal + shipping + tax,
                    FreeShipping    = subTotal >= 5000
                }
            });
        }

        // ── POST /Checkout/PlaceOrder ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // ── Validate card fields only when card payment is chosen ──
            if (model.PaymentMethod == PaymentMethod.CreditDebitCard)
            {
                if (model.CardDetails == null)
                {
                    ModelState.AddModelError("", "Card details are required.");
                }
                else
                {
                    // Manually trigger validation on the nested object
                    if (string.IsNullOrWhiteSpace(model.CardDetails.CardNumber))
                        ModelState.AddModelError("CardDetails.CardNumber", "Card number is required.");

                    if (string.IsNullOrWhiteSpace(model.CardDetails.CardHolderName))
                        ModelState.AddModelError("CardDetails.CardHolderName", "Name on card is required.");

                    if (string.IsNullOrWhiteSpace(model.CardDetails.ExpiryDate))
                        ModelState.AddModelError("CardDetails.ExpiryDate", "Expiry date is required.");

                    if (string.IsNullOrWhiteSpace(model.CardDetails.CVV))
                        ModelState.AddModelError("CardDetails.CVV", "CVV is required.");

                    // Validate expiry date is not in the past
                    if (!string.IsNullOrWhiteSpace(model.CardDetails.ExpiryDate))
                    {
                        var parts = model.CardDetails.ExpiryDate.Split('/');
                        if (parts.Length == 2
                            && int.TryParse(parts[0], out int month)
                            && int.TryParse(parts[1], out int year))
                        {
                            var expiry = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
                            if (expiry < DateTime.Today)
                                ModelState.AddModelError("CardDetails.ExpiryDate",
                                    "This card has expired.");
                        }
                        else
                        {
                            ModelState.AddModelError("CardDetails.ExpiryDate",
                                "Enter expiry as MM/YY.");
                        }
                    }
                }
            }
            else
            {
                // Remove card validation errors for non-card payments
                foreach (var key in ModelState.Keys
                    .Where(k => k.StartsWith("CardDetails")).ToList())
                    ModelState.Remove(key);
            }

            // ── If model is invalid rebuild cart summary and return ────
            if (!ModelState.IsValid)
            {
                var cart     = await _cartService.GetCartAsync(user.Id);
                var subTotal = cart?.CartItems.Sum(ci => ci.TotalPrice) ?? 0;
                var shipping = subTotal >= 5000 ? 0 : 350m;
                var tax      = Math.Round(subTotal * 0.05m, 2);
                model.CartSummary   = new CartViewModel
                {
                    Cart            = cart,
                    SubTotal        = subTotal,
                    ShippingCharges = shipping,
                    Tax             = tax,
                    GrandTotal      = subTotal + shipping + tax,
                    FreeShipping    = subTotal >= 5000
                };
                model.CardDetails ??= new CardDetailsViewModel();
                return View("Index", model);
            }

            // ── Create order ──────────────────────────────────────────
            var order = await _orderService.CreateOrderAsync(user.Id, model);

            // Fire-and-forget confirmation email
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderConfirmationAsync(
                        user.Email!, user.FullName, order.OrderNumber, order.GrandTotal);
                }
                catch { /* non-critical */ }
            });

            TempData["Success"] = $"Order #{order.OrderNumber} placed successfully!";
            return RedirectToAction("Confirmation",
                new { orderNumber = order.OrderNumber });
        }

        // ── GET /Checkout/Confirmation ────────────────────────────────
        public IActionResult Confirmation(string orderNumber)
        {
            ViewBag.OrderNumber = orderNumber;
            return View();
        }
    }
}
