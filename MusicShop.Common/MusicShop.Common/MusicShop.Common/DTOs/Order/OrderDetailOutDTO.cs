using MusicShop.Common.DTOs.Payment;
using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs.Order
{
    public class OrderDetailOutDTO
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }
        public OrderAddressDTO? Address { get; set; }
        public List<OrderItemOutDTO> Items { get; set; } = new();
        public List<PaymentOutDTO> Payments { get; set; } = new();
    }
}
