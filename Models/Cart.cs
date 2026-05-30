using System.ComponentModel.DataAnnotations.Schema;

namespace Velora.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        [NotMapped]
        public decimal SubTotal => CartItems.Sum(i => i.TotalPrice);

        [NotMapped]
        public int TotalItems => CartItems.Sum(i => i.Quantity);
    }

    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public virtual Cart Cart { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; } = 1;

        public string? SelectedSize { get; set; }
        public string? SelectedColor { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public decimal UnitPrice => Product?.FinalPrice ?? 0;

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
