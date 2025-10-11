using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Server.Data.Repositories;

namespace MusicShop.Server.Core.Services.Impl
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        public CategoryService(ICategoryRepository repo) => _repo = repo;
        public async Task<CategoryDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id, ct);
        }

        public async Task<PagedResult<CategoryDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 60) pageSize = 12;
            var items = await _repo.GetListAsync(q, page, pageSize, ct);
            var total = await _repo.CountAsync(q, ct);
            return new PagedResult<CategoryDetailOutDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}
