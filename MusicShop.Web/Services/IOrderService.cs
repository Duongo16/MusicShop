using MusicShop.Common.DTOs.Order;

namespace MusicShop.Web.Services
{
    public interface IOrderService
    {
        Task<(bool Succeeded, string? Error, OrderOutDTO? Order)> CheckoutAsync(OrderCheckoutRequestDTO req);
    }
}
