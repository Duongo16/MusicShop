using Microsoft.EntityFrameworkCore;
using MusicShop.Common.DTOs;

namespace MusicShop.Server.Data.Repositories.Impl
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _db;
        public CategoryRepository(AppDbContext db) => _db = db;
        public async Task<int> CountAsync(string? q, CancellationToken ct = default)
        {
            var query = _db.Categories.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x => x.Name.Contains(q));
            }
            return await query.CountAsync(ct);
        }

        public async Task<CategoryDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Categories.AsNoTracking()
                .Select(x => new CategoryDetailOutDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Parent = x.Parent,
                    ParentId = x.ParentId,
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<CategoryDetailOutDto>> GetListAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Categories.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x => x.Name.Contains(q));
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            var skip = (page - 1) * pageSize;

            return await query
                .Select(x => new CategoryDetailOutDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Parent = x.Parent,
                    ParentId = x.ParentId,
                })
                .Skip(skip).Take(pageSize)
                .ToListAsync(ct);
        }
    }
}
