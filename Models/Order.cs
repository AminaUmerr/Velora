using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Velora.Models
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }

    public enum PaymentMethod
    {
        CashOnDelivery = 1,
        JazzCash = 2,
        EasyPaisa = 3,
        CreditDebitCard = 4
    }

    public class Order
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FullName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCharges { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        public string? Notes { get; set; }
        public bool IsPaid { get; set; } = false;

        // Stores masked card number e.g. "**** **** **** 4242" — never store full number
        [MaxLength(30)]
        public string? MaskedCardNumber { get; set; }

        [MaxLength(60)]
        public string? CardHolderName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public string? ProductImage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string? SelectedSize { get; set; }
        public string? SelectedColor { get; set; }
    }
}
