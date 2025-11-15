using MusicShop.Common.DTOs;
using MusicShop.Common.DTOs.Cart;

namespace MusicShop.Server.Core.Services
{
    public interface ICartService
    {
        Task<CartOutDto> GetCartAsync(Guid? userId, string? guestId, CancellationToken ct = default);
        Task<AuthResultOutDTO> AddToCartAsync(CartItemInDto dto, CancellationToken ct = default);
        Task<AuthResultOutDTO> RemoveFromCartAsync(Guid cartId, Guid itemId, CancellationToken ct = default);
        Task<AuthResultOutDTO> UpdateQtyAsync(Guid cartId, Guid itemId, int qty, CancellationToken ct = default);
    }
}
