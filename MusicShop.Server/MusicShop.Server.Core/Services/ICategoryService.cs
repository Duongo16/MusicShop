using MusicShop.Common.DTOs;
using MusicShop.Common.Models;

namespace MusicShop.Server.Core.Services
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default);

        Task<CategoryDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
