using Microsoft.EntityFrameworkCore;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories.Impl
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        public OrderRepository(AppDbContext db) => _db = db;

        public Task<Order> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Orders
                .Include(o => o.Items).ThenInclude(oi => oi.Item)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.Id == id, ct)!;
        }

        public async Task<Order> CreateAsync(Order order, CancellationToken ct = default)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);
            return order;
        }

        public Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .Include(o => o.Address)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct = default)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync(ct);
        }
    }
}
