using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs.Order
{
    public class OrderListItemOutDTO
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }
    }
}
