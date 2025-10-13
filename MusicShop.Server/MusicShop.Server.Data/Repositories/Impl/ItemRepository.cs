using Microsoft.EntityFrameworkCore;
using MusicShop.Common.DTOs;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _db;
        public ItemRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<ItemDetailOutDto>> GetListAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 60) pageSize = 12;

            var query = _db.Items.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                var like = $"%{q}%";
                query = query.Where(x =>
                    EF.Functions.Like(x.Name, like) ||
                    EF.Functions.Like(x.Sku, like) ||
                    (x.Brand != null && EF.Functions.Like(x.Brand.Name, like)) ||
                    (x.Category != null && EF.Functions.Like(x.Category.Name, like))
                );
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            int skip = (page - 1) * pageSize;

            return await query
                .Select(x => new ItemDetailOutDto
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    Description = x.Description,
                    ItemType = x.ItemType,
                    Status = x.Status,
                    Price = x.Price,
                    SalePrice = x.SalePrice,
                    StockQty = x.StockQty,
                    ReorderLevel = x.ReorderLevel,
                    ImageUrl = x.ImageUrl,
                    BrandId = x.BrandId,
                    CategoryId = x.CategoryId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Brand = x.Brand == null ? null : new RefInfoDto(x.Brand.Id, x.Brand.Name),
                    Category = x.Category == null ? null : new RefInfoDto(x.Category.Id, x.Category.Name),
                })
                .Skip(skip).Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<ItemDetailOutDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) return null;

            return await _db.Items.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ItemDetailOutDto
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    Description = x.Description,
                    ItemType = x.ItemType,
                    Status = x.Status,
                    Price = x.Price,
                    SalePrice = x.SalePrice,
                    StockQty = x.StockQty,
                    ReorderLevel = x.ReorderLevel,
                    ImageUrl = x.ImageUrl,
                    BrandId = x.BrandId,
                    CategoryId = x.CategoryId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Brand = x.Brand == null ? null : new RefInfoDto(x.Brand.Id, x.Brand.Name),
                    Category = x.Category == null ? null : new RefInfoDto(x.Category.Id, x.Category.Name),
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<int> CountAsync(string? q, CancellationToken ct = default)
        {
            var query = _db.Items.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                var like = $"%{q}%";
                query = query.Where(x =>
                    EF.Functions.Like(x.Name, like) ||
                    EF.Functions.Like(x.Sku, like) ||
                    (x.Brand != null && EF.Functions.Like(x.Brand.Name, like)) ||
                    (x.Category != null && EF.Functions.Like(x.Category.Name, like))
                );
            }
            return await query.CountAsync(ct);
        }

        public async Task<Guid> CreateAsync(ItemDetailOutDto data, CancellationToken ct = default)
        {
            var entity = new Item
            {
                Id = data.Id != Guid.Empty ? data.Id : Guid.NewGuid(),
                Sku = data.Sku,
                Name = data.Name,
                Description = data.Description,
                ItemType = data.ItemType,
                Status = data.Status,
                Price = data.Price,
                SalePrice = data.SalePrice,
                StockQty = data.StockQty,
                ReorderLevel = data.ReorderLevel,
                ImageUrl = data.ImageUrl,
                BrandId = data.BrandId,
                CategoryId = data.CategoryId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Items.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(ItemDetailOutDto data, CancellationToken ct = default)
        {
            if (data.Id == Guid.Empty) return false;

            var entity = await _db.Items.FirstOrDefaultAsync(x => x.Id == data.Id, ct);
            if (entity is null) return false;

            entity.Sku = data.Sku;
            entity.Name = data.Name;
            entity.Description = data.Description;
            entity.ItemType = data.ItemType;
            entity.Status = data.Status;
            entity.Price = data.Price;
            entity.SalePrice = data.SalePrice;
            entity.StockQty = data.StockQty;
            entity.ReorderLevel = data.ReorderLevel;
            entity.ImageUrl = data.ImageUrl;
            entity.BrandId = data.BrandId;
            entity.CategoryId = data.CategoryId;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Items
                .Include(i => i.OrderItems)
                .Include(i => i.CartItems)
                .Include(i => i.Ledgers)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null) return false;

            // Tuỳ chính sách: chặn xoá nếu có phát sinh
            if (entity.OrderItems.Any() || entity.CartItems.Any() || entity.Ledgers.Any())
                return false;

            _db.Items.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
