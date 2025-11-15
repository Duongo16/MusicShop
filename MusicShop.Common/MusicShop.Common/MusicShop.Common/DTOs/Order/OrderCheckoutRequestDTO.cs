using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs.Order
{
    public class OrderCheckoutRequestDTO
    {
        public Guid? UserId { get; set; }     
        public string? GuestId { get; set; }  
        public OrderAddressDTO? Address { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cod; 

    }
}
