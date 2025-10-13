using MusicShop.Common.DTOs;

namespace MusicShop.Server.Data.Repositories
{
    public interface IItemRepository
    {
        Task<IReadOnlyList<ItemDetailOutDto>> GetListAsync(string? q, int page, int pageSize, CancellationToken ct = default);
        Task<ItemDetailOutDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<int> CountAsync(string? q, CancellationToken ct = default);
        Task<Guid> CreateAsync(ItemDetailOutDto data, CancellationToken ct = default);
        Task<bool> UpdateAsync(ItemDetailOutDto data, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
