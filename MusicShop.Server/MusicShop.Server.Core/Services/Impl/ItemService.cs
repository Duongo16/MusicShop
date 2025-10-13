using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Server.Data.Repositories;

namespace MusicShop.Server.Core.Services.Impl
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repo;
        public ItemService(IItemRepository repo) => _repo = repo;

        public async Task<PagedResult<ItemDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 60) pageSize = 12;

            var items = await _repo.GetListAsync(q, page, pageSize, ct);
            var total = await _repo.CountAsync(q, ct);

            return new PagedResult<ItemDetailOutDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public Task<ItemDetailOutDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => id == Guid.Empty ? Task.FromResult<ItemDetailOutDto?>(null) : _repo.GetByIdAsync(id, ct);

        public async Task<Guid> CreateAsync(ItemDetailOutDto data, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(data.Sku)) throw new ArgumentException("SKU is required");
            if (string.IsNullOrWhiteSpace(data.Name)) throw new ArgumentException("Name is required");
            if (data.Price < 0) throw new ArgumentException("Price must be >= 0");

            return await _repo.CreateAsync(data, ct);
        }

        public Task<bool> UpdateAsync(ItemDetailOutDto data, CancellationToken ct = default)
            => data.Id == Guid.Empty ? Task.FromResult(false) : _repo.UpdateAsync(data, ct);

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
            => id == Guid.Empty ? Task.FromResult(false) : _repo.DeleteAsync(id, ct);
    }
}
