using MusicShop.Common.DTOs;
using MusicShop.Common.DTOs.Cart;
using MusicShop.Common.Transport;

namespace MusicShop.Web.Services.Impl
{
    public class CartService : ICartService
    {
        private readonly TcpClientHelper _tcp;
        public CartService(TcpClientHelper tcp) => _tcp = tcp;

        public async Task<CartOutDto> GetCartAsync(Guid? userId, string? guestId)
        {
            var resp = await _tcp.SendAsync<CartOutDto>("Cart.GetByUserOrGuest", new { UserId = userId, GuestId = guestId });
            return resp ?? new CartOutDto();
        }

        public Task<AuthResultOutDTO> AddToCartAsync(CartItemInDto dto)
        {
            return _tcp.SendAsync<AuthResultOutDTO>("Cart.AddToCart", dto);
        }

        public Task<AuthResultOutDTO> RemoveFromCartAsync(Guid cartId, Guid itemId)
        {
            return _tcp.SendAsync<AuthResultOutDTO>("Cart.RemoveFromCart", new { CartId = cartId, ItemId = itemId });
        }

        public Task<AuthResultOutDTO> UpdateQtyAsync(Guid cartId, Guid itemId, int qty)
        {
            return _tcp.SendAsync<AuthResultOutDTO>("Cart.UpdateQty", new { CartId = cartId, ItemId = itemId, Qty = qty });
        }
    }
}
