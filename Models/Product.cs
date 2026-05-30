using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Velora.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        public int Stock { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public string? ImageUrl4 { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        // Men / Women / Unisex / Kids
        [MaxLength(20)]
        public string? Gender { get; set; }

        // Comma-separated: "XS,S,M,L,XL,XXL" or "38,39,40,41,42"
        public string? Sizes { get; set; }

        // Comma-separated: "Black,White,Red"
        public string? Colors { get; set; }

        public string? SKU { get; set; }
        public string? Tags { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsNewArrival { get; set; } = false;
        public bool IsBestSeller { get; set; } = false;
        public bool IsTrending { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Computed
        [NotMapped]
        public decimal FinalPrice => DiscountPrice.HasValue && DiscountPrice > 0 ? DiscountPrice.Value : Price;

        [NotMapped]
        public int DiscountPercentage => (DiscountPrice.HasValue && DiscountPrice > 0 && Price > 0)
            ? (int)Math.Round((1 - DiscountPrice.Value / Price) * 100)
            : 0;

        [NotMapped]
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;

        [NotMapped]
        public int ReviewCount => Reviews.Count;
    }
}
