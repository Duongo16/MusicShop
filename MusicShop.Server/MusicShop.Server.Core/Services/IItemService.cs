using MusicShop.Common.Models;
using MusicShop.Common.DTOs;

namespace MusicShop.Server.Core.Services
{
    public interface IItemService
    {
        Task<PagedResult<ItemDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default);
        Task<ItemDetailOutDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(ItemDetailOutDto data, CancellationToken ct = default);
        Task<bool> UpdateAsync(ItemDetailOutDto data, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
