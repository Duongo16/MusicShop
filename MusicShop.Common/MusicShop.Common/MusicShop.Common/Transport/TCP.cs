using MusicShop.Common.Models;
using System.Text.Json;

namespace MusicShop.Common.Transport
{
    public record TcpRequest(string RequestId, string Op, object? Payload);
    public record TcpResponse(string RequestId, bool Ok, JsonElement? Data, string? Error);

    public record GetListPayload(string? Q, int Page, int PageSize);
    public record GetByIdPayload(Guid Id);
    public record GetByIdIntPayload(int Id);
    public record DeletePayload(int Id);
    public record DeleteGuidPayload(Guid Id);
    public record CategoryUpsertPayload(int? Id, string Name);
    public record BrandUpsertPayload(int? Id, string Name);
    public record ItemUpsertPayload(
       Guid? Id,
       string Sku,
       string Name,
       string? Description,
       ItemType ItemType,
       ItemStatus Status,
       decimal Price,
       decimal? SalePrice,
       int StockQty,
       int ReorderLevel,
       string? ImageUrl,
       int? BrandId,
       int? CategoryId
   );



}
