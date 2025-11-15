using Microsoft.EntityFrameworkCore;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories.Impl
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _db;
        public CartRepository(AppDbContext db) => _db = db;

        public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return _db.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Item)
                .Where(c => c.UserId == userId && !c.IsCheckout)
                .FirstOrDefaultAsync(ct);
        }

        public Task<Cart?> GetByGuestIdAsync(string guestId, CancellationToken ct = default)
        {
            return _db.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Item)
                .Where(c => c.GuestId == guestId && !c.IsCheckout)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Cart> CreateForUserAsync(Guid userId, CancellationToken ct = default)
        {
            var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, IsCheckout = false };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct);
            return cart;
        }

        public async Task<Cart> CreateForGuestAsync(string guestId, CancellationToken ct = default)
        {
            var cart = new Cart { Id = Guid.NewGuid(), GuestId = guestId, IsCheckout = false };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct);
            return cart;
        }

        public async Task AddOrUpdateCartItemAsync(CartItem item, CancellationToken ct = default)
        {
            var exists = await _db.Set<CartItem>()
                .FirstOrDefaultAsync(ci => ci.CartId == item.CartId && ci.ItemId == item.ItemId, ct);

            if (exists == null)
            {
                _db.Set<CartItem>().Add(item);
            }
            else
            {
                exists.Qty += item.Qty;
                exists.UnitPrice = item.UnitPrice; // update price to latest
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task RemoveCartItemAsync(Guid cartId, Guid itemId, CancellationToken ct = default)
        {
            var item = await _db.Set<CartItem>().FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ItemId == itemId, ct);
            if (item != null)
            {
                _db.Set<CartItem>().Remove(item);
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task UpdateCartItemQtyAsync(Guid cartId, Guid itemId, int qty, CancellationToken ct = default)
        {
            var item = await _db.Set<CartItem>().FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ItemId == itemId, ct);
            if (item != null)
            {
                if (qty <= 0)
                    _db.Set<CartItem>().Remove(item);
                else
                    item.Qty = qty;

                await _db.SaveChangesAsync(ct);
            }
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
