using Microsoft.EntityFrameworkCore;
using MusicShop.Common.DTOs;
using MusicShop.Common.DTOs.Cart;
using MusicShop.Common.Models;
using MusicShop.Server.Data;
using MusicShop.Server.Data.Repositories;

namespace MusicShop.Server.Core.Services.Impl
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly AppDbContext _db; 

        public CartService(ICartRepository cartRepo, AppDbContext db)
        {
            _cartRepo = cartRepo;
            _db = db;
        }

        public async Task<CartOutDto> GetCartAsync(Guid? userId, string? guestId, CancellationToken ct = default)
        {
            Cart? cart = null;
            if (userId.HasValue) cart = await _cartRepo.GetByUserIdAsync(userId.Value, ct);
            if (cart == null && !string.IsNullOrEmpty(guestId)) cart = await _cartRepo.GetByGuestIdAsync(guestId, ct);

            if (cart == null) return new CartOutDto { Items = new List<CartItemOutDto>(), CartId = Guid.NewGuid(), UserId = userId, GuestId = guestId };

            var items = cart.Items.Select(ci => new CartItemOutDto
            {
                CartId = ci.CartId,
                ItemId = ci.ItemId,
                ItemName = ci.Item?.Name ?? string.Empty,
                Qty = ci.Qty,
                UnitPrice = ci.UnitPrice,
                StockQty = ci.Item?.StockQty ?? 0,
            }).ToList();

            return new CartOutDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                GuestId = cart.GuestId,
                IsCheckout = cart.IsCheckout,
                Items = items
            };
        }

        public async Task<AuthResultOutDTO> AddToCartAsync(CartItemInDto dto, CancellationToken ct = default)
        {
            if (dto == null) return new AuthResultOutDTO(false, new[] { "Invalid request." });
            if (dto.Qty <= 0) dto.Qty = 1;

            var itemEntity = await _db.Items.FirstOrDefaultAsync(i => i.Id == dto.ItemId, ct);
            if (itemEntity == null) return new AuthResultOutDTO(false, new[] { "Item not found." });

            Cart? cart = null;
            if (dto.UserId.HasValue)
            {
                cart = await _cartRepo.GetByUserIdAsync(dto.UserId.Value, ct);
                if (cart == null) cart = await _cartRepo.CreateForUserAsync(dto.UserId.Value, ct);
            }
            else if (!string.IsNullOrEmpty(dto.GuestId))
            {
                cart = await _cartRepo.GetByGuestIdAsync(dto.GuestId, ct);
                if (cart == null) cart = await _cartRepo.CreateForGuestAsync(dto.GuestId!, ct);
            }
            else
            {
                return new AuthResultOutDTO(false, new[] { "UserId or GuestId is required." });
            }

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ItemId = itemEntity.Id,
                Qty = dto.Qty,
                UnitPrice = itemEntity.SalePrice ?? itemEntity.Price
            };

            await _cartRepo.AddOrUpdateCartItemAsync(cartItem, ct);
            return new AuthResultOutDTO(true, Array.Empty<string>());
        }

        public async Task<AuthResultOutDTO> RemoveFromCartAsync(Guid cartId, Guid itemId, CancellationToken ct = default)
        {
            await _cartRepo.RemoveCartItemAsync(cartId, itemId, ct);
            return new AuthResultOutDTO(true, Array.Empty<string>());
        }

        public async Task<AuthResultOutDTO> UpdateQtyAsync(Guid cartId, Guid itemId, int qty, CancellationToken ct = default)
        {
            if (qty < 0) return new AuthResultOutDTO(false, new[] { "Quantity must be >= 0." });
            await _cartRepo.UpdateCartItemQtyAsync(cartId, itemId, qty, ct);
            return new AuthResultOutDTO(true, Array.Empty<string>());
        }
    }
}
