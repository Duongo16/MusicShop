using MusicShop.Common.DTOs;
using MusicShop.Common.Models;

namespace MusicShop.Server.Core.Services
{
    public interface IBrandService
    {
        Task<BrandDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<PagedResult<BrandDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default);

        Task<int> CreateAsync(string name, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, string name, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
