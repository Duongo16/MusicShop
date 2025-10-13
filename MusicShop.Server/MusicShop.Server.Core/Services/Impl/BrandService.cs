using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Server.Data.Repositories;

namespace MusicShop.Server.Core.Services.Impl
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _repo;
        public BrandService(IBrandRepository repo) => _repo = repo;

        public Task<BrandDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default)
            => id <= 0 ? Task.FromResult<BrandDetailOutDto?>(null) : _repo.GetByIdAsync(id, ct);

        public async Task<PagedResult<BrandDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 60) pageSize = 12;

            var items = await _repo.GetListAsync(q, page, pageSize, ct);
            var total = await _repo.CountAsync(q, ct);

            return new PagedResult<BrandDetailOutDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<int> CreateAsync(string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
            name = name.Trim();
            return await _repo.CreateAsync(name, ct);
        }

        public async Task<bool> UpdateAsync(int id, string name, CancellationToken ct = default)
        {
            if (id <= 0) return false;
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
            name = name.Trim();
            return await _repo.UpdateAsync(id, name, ct);
        }

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
            => id <= 0 ? Task.FromResult(false) : _repo.DeleteAsync(id, ct);
    }
}
