using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Common.Transport;
using System.Text.Json;

namespace MusicShop.Web.Services.Impl
{
    public class ItemClientService : IItemClientService
    {
        private readonly TcpClientHelper _tcp;

        public ItemClientService(TcpClientHelper tcp) => _tcp = tcp;

        public async Task<PagedResult<ItemDetailOutDto>> GetListAsync(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default)
        {
            var resp = await _tcp.SendAsync<PagedResult<ItemDetailOutDto>>(
                "Item.GetList", new GetListPayload(q, page, pageSize),ct);

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

            var total = items.Count;
            var pageItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<ItemDetailOutDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = pageItems
            };
        }

        public async Task<ItemDetailOutDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            //var resp = await _tcp.SendRequestAsync("Item.Get", new { id }, ct);
            //var status = resp.GetProperty("status").GetString();
            //if (status != "OK") return null;

            //var data = resp.GetProperty("data");
            //return DeserializeItem(data);
            return null;
        }

        private static ItemDetailOutDto DeserializeItem(JsonElement el)
        {
            return new ItemDetailOutDto
            {
                Id = el.TryGetProperty("id", out var pid) && pid.ValueKind == JsonValueKind.String ? pid.GetGuid() : Guid.Empty,
                Sku = el.TryGetProperty("sku", out var psku) ? psku.GetString() ?? "" : "",
                Name = el.TryGetProperty("name", out var pname) ? pname.GetString() ?? "" : "",
                Price = el.TryGetProperty("price", out var pprice) && pprice.ValueKind == JsonValueKind.Number ? pprice.GetDecimal() : 0,
                SalePrice = el.TryGetProperty("salePrice", out var psale) && psale.ValueKind == JsonValueKind.Number ? psale.GetDecimal() : null,
                Status = el.TryGetProperty("status", out var pstat) ? (ItemStatus)pstat.GetByte() : ItemStatus.Draft,
            };
        }
    }
}
