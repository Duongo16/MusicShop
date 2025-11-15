using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs.Order
{
    public class OrderUpdateStatusRequestDTO
    {
        public Guid OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}
