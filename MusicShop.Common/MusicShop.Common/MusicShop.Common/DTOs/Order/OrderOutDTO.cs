using MusicShop.Common.DTOs.Cart;
using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs.Order
{
    public class OrderOutDTO
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string? GuestId { get; set; }
        public OrderStatus Status { get; set; }
        public List<CartItemOutDto> Items { get; set; } = new();
        public decimal Total { get; set; }
        public OrderAddressDTO? Address { get; set; }
    }
}
