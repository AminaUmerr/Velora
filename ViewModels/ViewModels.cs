using System.ComponentModel.DataAnnotations;
using Velora.Models;

namespace Velora.ViewModels
{
    // ─── Auth ViewModels ─────────────────────────────────────────────────────────
    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(6), DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword"), DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    // ─── Profile ViewModel ───────────────────────────────────────────────────────
    public class EditProfileViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Phone, Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        public IFormFile? ProfileImageFile { get; set; }
        public string? ExistingProfileImage { get; set; }
    }

    // ─── Shop ViewModels ─────────────────────────────────────────────────────────
    public class ShopViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? Gender { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Sort { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public IEnumerable<Product> RelatedProducts { get; set; } = new List<Product>();
        public bool IsInWishlist { get; set; }
        public ReviewViewModel NewReview { get; set; } = new();
    }

    public class ReviewViewModel
    {
        public int ProductId { get; set; }
        [Range(1, 5)] public int Rating { get; set; } = 5;
        [MaxLength(100)] public string? Title { get; set; }
        public string? Comment { get; set; }
    }

    // ─── Cart ViewModels ─────────────────────────────────────────────────────────
    public class CartViewModel
    {
        public Cart? Cart { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
        public bool FreeShipping { get; set; }
    }

    // ─── NEW: Credit Card ViewModel ──────────────────────────────────────────────
    public class CardDetailsViewModel
    {
        [Required(ErrorMessage = "Card number is required.")]
        [RegularExpression(@"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$",
            ErrorMessage = "Enter a valid 16-digit card number.")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cardholder name is required.")]
        [MaxLength(60)]
        [Display(Name = "Name on Card")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$",
            ErrorMessage = "Enter expiry as MM/YY.")]
        [Display(Name = "Expiry Date (MM/YY)")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        [Display(Name = "CVV")]
        public string CVV { get; set; } = string.Empty;
    }

    // ─── Checkout ViewModel ──────────────────────────────────────────────────────
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required.")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        public string? Notes { get; set; }

        // Card details — only required when PaymentMethod == CreditDebitCard
        public CardDetailsViewModel? CardDetails { get; set; }

        // For display only (not posted)
        public CartViewModel? CartSummary { get; set; }
    }

    // ─── Home ViewModel ──────────────────────────────────────────────────────────
    public class HomeViewModel
    {
        public IEnumerable<Banner> Banners { get; set; } = new List<Banner>();
        public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
        public IEnumerable<Product> NewArrivals { get; set; } = new List<Product>();
        public IEnumerable<Product> BestSellers { get; set; } = new List<Product>();
        public IEnumerable<Product> TrendingProducts { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    }

    // ─── Admin ViewModels ────────────────────────────────────────────────────────
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required."), MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, 9999999, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, 9999999)]
        [Display(Name = "Discount Price")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [Range(0, 10000)]
        public int Stock { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        public string? Gender { get; set; }
        public string? Sizes { get; set; }
        public string? Colors { get; set; }
        public string? Tags { get; set; }
        public string? SKU { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsTrending { get; set; }
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }
        public IFormFile? ImageFile2 { get; set; }
        public IFormFile? ImageFile3 { get; set; }
        public string? ExistingImage { get; set; }
        public string? ExistingImage2 { get; set; }
        public string? ExistingImage3 { get; set; }

        public IEnumerable<Category>? Categories { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public IEnumerable<Order> RecentOrders { get; set; } = new List<Order>();
        public IEnumerable<MonthlyRevenue> MonthlyRevenues { get; set; } = new List<MonthlyRevenue>();
    }

    public class MonthlyRevenue
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}
