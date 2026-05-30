using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velora.Data;
using Velora.Interfaces;
using Velora.Models;
using Velora.Services;
using Velora.ViewModels;

namespace Velora.Controllers
{
    // ════════════════════════════════════════════════════════════════
    //  ACCOUNT CONTROLLER
    // ════════════════════════════════════════════════════════════════
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<ApplicationUser> um,
            SignInManager<ApplicationUser> sm, IEmailService email)
        { _userManager = um; _signInManager = sm; _emailService = email; }

        // ── Register ─────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() =>
            User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email, Email = model.Email,
                FullName = model.FullName, EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);
                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName); }
                    catch { /* non-critical */ }
                });
                TempData["Success"] = $"Welcome to Velora, {user.FullName}!";
                return RedirectToAction("Index", "Home");
            }
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }

        // ── Login ─────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // ── Logout ────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ── ForgotPassword ────────────────────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            // Always show confirmation to prevent email enumeration
            TempData["Success"] = "If that email exists, a reset link has been sent.";
            return View("ForgotPasswordConfirmation");
        }

        public IActionResult AccessDenied() => View();
    }

    // ════════════════════════════════════════════════════════════════
    //  USER CONTROLLER
    // ════════════════════════════════════════════════════════════════
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public UserController(UserManager<ApplicationUser> um, IOrderService orders,
            ApplicationDbContext ctx, IImageService img)
        { _userManager = um; _orderService = orders; _context = ctx; _imageService = img; }

        // ── Dashboard ─────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var user   = await _userManager.GetUserAsync(User);
            var orders = await _orderService.GetUserOrdersAsync(user!.Id);
            ViewBag.User         = user;
            ViewBag.OrderCount   = orders.Count();
            ViewBag.RecentOrders = orders.Take(3);
            return View();
        }

        // ── Profile ───────────────────────────────────────────────────
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(new EditProfileViewModel
            {
                FullName             = user!.FullName,
                PhoneNumber          = user.PhoneNumber,
                Address              = user.Address,
                City                 = user.City,
                PostalCode           = user.PostalCode,
                ExistingProfileImage = user.ProfileImage
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);

            user!.FullName    = model.FullName;
            user.PhoneNumber  = model.PhoneNumber;
            user.Address      = model.Address;
            user.City         = model.City;
            user.PostalCode   = model.PostalCode;

            if (model.ProfileImageFile != null)
            {
                var url = await _imageService.UploadImageAsync(model.ProfileImageFile, "profiles");
                if (url != null) user.ProfileImage = url;
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        // ── Orders ────────────────────────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            var user   = await _userManager.GetUserAsync(User);
            var orders = await _orderService.GetUserOrdersAsync(user!.Id);
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var user  = await _userManager.GetUserAsync(User);
            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null || order.UserId != user!.Id) return NotFound();
            return View(order);
        }

        // ── Wishlist ──────────────────────────────────────────────────
        public async Task<IActionResult> Wishlist()
        {
            var user = await _userManager.GetUserAsync(User);
            var wish = await _context.Wishlists
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(w => w.UserId == user!.Id);
            return View(wish);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var wish = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == user!.Id);

            if (wish == null)
            {
                wish = new Wishlist { UserId = user!.Id };
                _context.Wishlists.Add(wish);
                await _context.SaveChangesAsync();
            }

            var existing = wish.WishlistItems.FirstOrDefault(wi => wi.ProductId == productId);
            if (existing != null)
            {
                _context.WishlistItems.Remove(existing);
                await _context.SaveChangesAsync();
                return Json(new { success = true, added = false, message = "Removed from wishlist" });
            }

            _context.WishlistItems.Add(new WishlistItem { WishlistId = wish.Id, ProductId = productId });
            await _context.SaveChangesAsync();
            return Json(new { success = true, added = true, message = "Added to wishlist!" });
        }

        // ── Change Password ───────────────────────────────────────────
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user   = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user!, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction(nameof(Profile));
            }
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  ERROR CONTROLLER
    // ════════════════════════════════════════════════════════════════
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.StatusCode = statusCode;
            return statusCode switch
            {
                404 => View("~/Views/Error/NotFound.cshtml"),
                403 => View("~/Views/Error/Forbidden.cshtml"),
                _   => View("~/Views/Error/Error.cshtml")
            };
        }
    }
}
