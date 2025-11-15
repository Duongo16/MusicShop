using MusicShop.Common.DTOs.Order;

namespace MusicShop.Server.Core.Services
{
    public interface IOrderService
    {
        Task<(bool Succeeded, string? Error, OrderOutDTO? Order)> CheckoutAsync(OrderCheckoutRequestDTO req, CancellationToken ct = default);
        Task<List<OrderListItemOutDTO>> GetOrdersAsync(Guid? userId, string? guestId, CancellationToken ct = default);
        Task<List<OrderListItemOutDTO>> GetAllOrdersAsync();
        Task<OrderDetailOutDTO?> GetOrderDetailAsync(Guid orderId, Guid? userId, string? guestId, CancellationToken ct = default);

        Task<(bool Succeeded, string? Error)> UpdateOrderStatusAsync(OrderUpdateStatusRequestDTO req, CancellationToken ct = default);
    }

}
