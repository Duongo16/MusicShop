using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MusicShop.Common.Models
{

    public class ApplicationUser : IdentityUser<Guid>
    {
        public Profile? Profile { get; set; }
    }

    public class ApplicationRole : IdentityRole<Guid> { }


    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class Profile
    {
        public Guid UserId { get; set; }
        [MaxLength(255)] public string? FullName { get; set; }
        [MaxLength(20)] public string? Phone { get; set; }
        public DateTime? Dob { get; set; }
        [MaxLength(400)] public string? Address { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }

    public class EmailQueue : BaseEntity
    {
        public int Id { get; set; }
        [MaxLength(255)] public string Recipient { get; set; } = null!;
        [MaxLength(255)] public string Subject { get; set; } = null!;
        public string Content { get; set; } = "";
        public EmailStatus Status { get; set; } = EmailStatus.Queued;
        public int RetryCount { get; set; }
        public int MaxRetry { get; set; } = 3;
        public DateTime? SentAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
    }

    public class Brand : BaseEntity
    {
        public int Id { get; set; }
        [MaxLength(120)] public string Name { get; set; } = null!;
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }

    public class Category : BaseEntity
    {
        public int Id { get; set; }
        [MaxLength(120)] public string Name { get; set; } = null!;
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }

    public class Item : BaseEntity
    {
        public Guid Id { get; set; }
        [MaxLength(64)] public string Sku { get; set; } = null!;
        [MaxLength(255)] public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ItemType ItemType { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.Active;
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQty { get; set; }
        public int ReorderLevel { get; set; } = 2;
        [MaxLength(255)] public string? ImageUrl { get; set; }
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public Brand? Brand { get; set; }
        public Category? Category { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<InventoryLedger> Ledgers { get; set; } = new List<InventoryLedger>();
    }

    public class InventoryLedger : BaseEntity
    {
        public long Id { get; set; }
        public Guid ItemId { get; set; }
        public int ChangeQty { get; set; }
        public InventoryReason Reason { get; set; }
        [MaxLength(64)] public string? RefNo { get; set; }
        public Item Item { get; set; } = null!;
    }

    public class Cart : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        [MaxLength(64)] public string? GuestId { get; set; }
        public bool IsCheckout { get; set; }
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public Guid CartId { get; set; }
        public Guid ItemId { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public Cart Cart { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }

    public class Order : BaseEntity
    {
        public Guid Id { get; set; }
        [MaxLength(40)] public string OrderNumber { get; set; } = null!;
        public Guid? UserId { get; set; }
        [MaxLength(64)] public string? GuestId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal Total { get; set; }
        public ApplicationUser? User { get; set; }
        public OrderAddress? Address { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class OrderItem
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public Order Order { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }

    public class OrderAddress
    {
        public Guid OrderId { get; set; }
        [MaxLength(255)] public string? Name { get; set; }
        [MaxLength(20)] public string? Phone { get; set; }
        [MaxLength(400)] public string? AddressFull { get; set; }
        public Order Order { get; set; } = null!;
    }

    public class Payment : BaseEntity
    {
        public long Id { get; set; }
        public Guid OrderId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Init;
        [MaxLength(100)] public string? TransactionId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
