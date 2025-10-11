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

        public async Task<ItemDetailOutDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) return null;
            return await _repo.GetByIdAsync(id, ct);
        }
    }
}
