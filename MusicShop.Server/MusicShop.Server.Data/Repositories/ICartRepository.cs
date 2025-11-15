using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Cart?> GetByGuestIdAsync(string guestId, CancellationToken ct = default);
        Task<Cart> CreateForUserAsync(Guid userId, CancellationToken ct = default);
        Task<Cart> CreateForGuestAsync(string guestId, CancellationToken ct = default);
        Task AddOrUpdateCartItemAsync(CartItem item, CancellationToken ct = default);
        Task RemoveCartItemAsync(Guid cartId, Guid itemId, CancellationToken ct = default);
        Task UpdateCartItemQtyAsync(Guid cartId, Guid itemId, int qty, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
