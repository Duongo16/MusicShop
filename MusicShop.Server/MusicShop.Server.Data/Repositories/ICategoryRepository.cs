using MusicShop.Common.DTOs;

namespace MusicShop.Server.Data.Repositories
{
    public interface ICategoryRepository
    {
        Task<IReadOnlyList<CategoryDetailOutDto>> GetListAsync(string? q, int page, int pageSize, CancellationToken ct = default);
        Task<CategoryDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CountAsync(string? q, CancellationToken ct = default);
    }
}
