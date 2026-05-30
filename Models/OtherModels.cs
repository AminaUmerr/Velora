using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Velora.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }

    public class WishlistItem
    {
        public int Id { get; set; }

        public int WishlistId { get; set; }
        public virtual Wishlist Wishlist { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }

    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; }

        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Banner
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? ButtonText { get; set; }

        // hero / promo / category
        [MaxLength(30)]
        public string BannerType { get; set; } = "hero";

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class NewsletterSubscriber
    {
        public int Id { get; set; }

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
