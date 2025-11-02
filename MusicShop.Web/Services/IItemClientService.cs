using MusicShop.Common.DTOs;
using MusicShop.Common.Models;

namespace MusicShop.Web.Services
{
    public interface IItemClientService
    {
        Task<PagedResult<ItemDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12);
        Task<ItemDetailOutDto?> GetByIdAsync(Guid id);
    }
}
