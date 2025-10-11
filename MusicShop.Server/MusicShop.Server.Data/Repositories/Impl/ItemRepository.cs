using Microsoft.EntityFrameworkCore;
using MusicShop.Common.DTOs;

namespace MusicShop.Server.Data.Repositories.Impl
{

    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _db;
        public ItemRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<ItemDetailOutDto>> GetListAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Items.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x => x.Name.Contains(q) || x.Sku.Contains(q));
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            var skip = (page - 1) * pageSize;

            return await query
                .Select(x => new ItemDetailOutDto
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    Price = x.Price,
                    StockQty = x.StockQty,
                    BrandName = x.Brand != null ? x.Brand.Name : null,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    ImageUrl = x.ImageUrl
                })
                .Skip(skip).Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> CountAsync(string? q, CancellationToken ct = default)
        {
            var query = _db.Items.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x => x.Name.Contains(q) || x.Sku.Contains(q));
            }
            return await query.CountAsync(ct);
        }

        public async Task<ItemDetailOutDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Items.AsNoTracking()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Where(x => x.Id == id)
                .Select(x => new ItemDetailOutDto
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    Price = x.Price,
                    StockQty = x.StockQty,
                    BrandName = x.Brand != null ? x.Brand.Name : null,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    ImageUrl = x.ImageUrl,
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
