using Microsoft.EntityFrameworkCore;
using MusicShop.Common.DTOs;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly AppDbContext _db;
        public BrandRepository(AppDbContext db) => _db = db;

        public async Task<BrandDetailOutDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) return null;

            return await _db.Brands.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new BrandDetailOutDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ItemsCount = x.Items.Count
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<BrandDetailOutDto>> GetListAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 60) pageSize = 12;

            var query = _db.Brands.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                var like = $"%{q}%";
                query = query.Where(x => EF.Functions.Like(x.Name, like));
            }

            query = query.OrderBy(x => x.Name);

            int skip = (page - 1) * pageSize;

            return await query
                .Select(x => new BrandDetailOutDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ItemsCount = x.Items.Count
                })
                .Skip(skip).Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> CountAsync(string? q, CancellationToken ct = default)
        {
            var query = _db.Brands.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                var like = $"%{q}%";
                query = query.Where(x => EF.Functions.Like(x.Name, like));
            }
            return await query.CountAsync(ct);
        }

        public async Task<int> CreateAsync(string name, CancellationToken ct = default)
        {
            var entity = new Brand { Name = name };
            _db.Brands.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, string name, CancellationToken ct = default)
        {
            var entity = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return false;

            entity.Name = name;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Brands.Include(b => b.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return false;

            // Nếu muốn ngăn xóa khi còn item tham chiếu:
            if (entity.Items.Any()) return false;

            _db.Brands.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
