namespace MusicShop.Common.Transport
{
    public record TcpRequest(string RequestId, string Op, object? Payload);
    public record TcpResponse(string RequestId, bool Ok, object? Data, string? Error);

    public record GetListPayload(string? Q, int Page, int PageSize);
    public record GetByIdPayload(Guid Id);
    public record UpsertPayload(int? Id, string Name, string? Description);
    public record DeletePayload(int Id);
}
