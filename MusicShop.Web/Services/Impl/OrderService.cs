using MusicShop.Common.DTOs.Order;
using MusicShop.Common.Transport;
using System.Text.Json;

namespace MusicShop.Web.Services.Impl
{
    public class OrderService : IOrderService
    {
        private readonly TcpClientHelper _tcp;
        public OrderService(TcpClientHelper tcp) => _tcp = tcp;

        public async Task<(bool Succeeded, string? Error, OrderOutDTO? Order)> CheckoutAsync(OrderCheckoutRequestDTO req)
        {
            var resp = await _tcp.SendAsync<JsonElement>("Order.Checkout", req); 
            if (resp.ValueKind == JsonValueKind.Undefined) return (false, "No response", null);

            bool ok = false;
            string? err = null;
            OrderOutDTO? order = null;

            if (resp.TryGetProperty("ok", out var okEl) && okEl.ValueKind != JsonValueKind.Null)
                ok = okEl.GetBoolean();

            if (resp.TryGetProperty("error", out var errEl) && errEl.ValueKind != JsonValueKind.Null)
                err = errEl.GetString();

            if (resp.TryGetProperty("order", out var orderEl) && orderEl.ValueKind != JsonValueKind.Null)
            {
                order = JsonSerializer.Deserialize<OrderOutDTO>(orderEl.GetRawText(), TcpFraming.Json);
            }

            return (ok, err, order);
        }
    }
}
