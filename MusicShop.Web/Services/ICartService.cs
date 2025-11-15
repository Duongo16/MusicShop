using MusicShop.Common.DTOs;
using MusicShop.Common.DTOs.Cart;

namespace MusicShop.Web.Services
{
    public interface ICartService
    {
        Task<CartOutDto> GetCartAsync(Guid? userId, string? guestId);
        Task<AuthResultOutDTO> AddToCartAsync(CartItemInDto dto);
        Task<AuthResultOutDTO> RemoveFromCartAsync(Guid cartId, Guid itemId);
        Task<AuthResultOutDTO> UpdateQtyAsync(Guid cartId, Guid itemId, int qty);
    }
}
