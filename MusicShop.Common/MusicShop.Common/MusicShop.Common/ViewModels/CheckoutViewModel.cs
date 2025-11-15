using MusicShop.Common.DTOs.Cart;
using MusicShop.Common.DTOs.Order;
using MusicShop.Common.Models;

namespace MusicShop.Common.ViewModels
{
    public class CheckoutViewModel
    {
        public Guid? CartId { get; set; } 
        public OrderAddressDTO Address { get; set; } = new();
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cod;
        public CartOutDto Cart { get; set; } = new();
    }

}
