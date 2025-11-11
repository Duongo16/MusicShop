using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Common.Transport;
using System.Text.Json;

namespace MusicShop.Web.Services.Impl
{
    public class ItemService : IItemService
    {
        private readonly TcpClientHelper _tcp;

        public ItemService(TcpClientHelper tcp) => _tcp = tcp;

        public async Task<PagedResult<ItemDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12)
        {
            var resp = await _tcp.SendAsync<PagedResult<ItemDetailOutDto>>(
                "Item.GetList", new GetListPayload(q, page, pageSize));

            var data = resp?.Items;
            var items = new List<ItemDetailOutDto>();

            foreach (var el in data)
            {
                items.Add(el);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qLower = q.Trim().ToLowerInvariant();
                items = items.Where(x => x.Name.ToLowerInvariant().Contains(qLower) || x.Sku.ToLowerInvariant().Contains(qLower)).ToList();
            }

            var total = resp.TotalCount;

            return new PagedResult<ItemDetailOutDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = items
            };
        }

        public async Task<ItemDetailOutDto?> GetByIdAsync(Guid id)
        {
            var resp = await _tcp.SendAsync<ItemDetailOutDto>("Item.GetById", new GetByIdPayload(id));
            return resp;
        }



    }
}
