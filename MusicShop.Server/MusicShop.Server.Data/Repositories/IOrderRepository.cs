using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Order> CreateAsync(Order order, CancellationToken ct = default);
        Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task UpdateAsync(Order order, CancellationToken ct = default);
    }
}
