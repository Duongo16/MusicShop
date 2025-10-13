using MusicShop.Common.DTOs;

namespace MusicShop.Server.Data.Repositories
{
    public interface IBrandRepository
    {
        Task<BrandDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<BrandDetailOutDto>> GetListAsync(string? q, int page, int pageSize, CancellationToken ct = default);
        Task<int> CountAsync(string? q, CancellationToken ct = default);

        Task<int> CreateAsync(string name, CancellationToken ct = default);         
        Task<bool> UpdateAsync(int id, string name, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
