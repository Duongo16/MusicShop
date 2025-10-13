using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicShop.Common.DTOs;
using MusicShop.Common.Transport;
using MusicShop.Server.Core.Services;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace MusicShop.Server.Host.Infrastructure
{
    public class TcpServerHostedService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<TcpServerHostedService> _log;
        private readonly int _port;

        public TcpServerHostedService(IServiceProvider sp, ILogger<TcpServerHostedService> log, IConfiguration cfg)
        {
            _sp = sp; _log = log;
            _port = cfg.GetValue("Tcp:Port", 5055);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            _log.LogInformation("TCP listening on {Port}", _port);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(stoppingToken);
                    _ = HandleClientAsync(client, stoppingToken);
                }
            }
            finally { listener.Stop(); }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var itemService = scope.ServiceProvider.GetRequiredService<IItemService>();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
            var brandService = scope.ServiceProvider.GetRequiredService<IBrandService>();
            using var stream = client.GetStream();

            try
            {
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    var json = await TcpFraming.ReadJsonAsync(stream, ct);
                    if (json is null) break;

                    TcpResponse resp;
                    try
                    {
                        var req = JsonSerializer.Deserialize<TcpRequest>(json, TcpFraming.Json)
                                  ?? throw new Exception("Invalid request");

                        switch (req.Op)
                        {
                            // ===================== ITEM =====================
                            case "Item.GetList":
                                {
                                    var p = JsonSerializer.Deserialize<GetListPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)
                                            ?? new(null, 1, 12);
                                    var data = await itemService.GetListAsync(p.Q, p.Page, p.PageSize, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }
                            case "Item.GetById":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await itemService.GetByIdAsync(p.Id, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }
                            case "Item.Create":
                                {
                                    var p = JsonSerializer.Deserialize<ItemUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;

                                    // map payload -> DTO
                                    var dto = new ItemDetailOutDto
                                    {
                                        Id = p.Id ?? Guid.Empty,
                                        Sku = p.Sku,
                                        Name = p.Name,
                                        Description = p.Description,
                                        ItemType = p.ItemType,
                                        Status = p.Status,
                                        Price = p.Price,
                                        SalePrice = p.SalePrice,
                                        StockQty = p.StockQty,
                                        ReorderLevel = p.ReorderLevel,
                                        ImageUrl = p.ImageUrl,
                                        BrandId = p.BrandId,
                                        CategoryId = p.CategoryId
                                    };

                                    var newId = await itemService.CreateAsync(dto, ct);
                                    resp = new(req.RequestId, true, newId, null);
                                    break;
                                }
                            case "Item.Update":
                                {
                                    var p = JsonSerializer.Deserialize<ItemUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;

                                    if (p.Id is null || p.Id == Guid.Empty) throw new Exception("Invalid Id");

                                    var dto = new ItemDetailOutDto
                                    {
                                        Id = p.Id.Value,
                                        Sku = p.Sku,
                                        Name = p.Name,
                                        Description = p.Description,
                                        ItemType = p.ItemType,
                                        Status = p.Status,
                                        Price = p.Price,
                                        SalePrice = p.SalePrice,
                                        StockQty = p.StockQty,
                                        ReorderLevel = p.ReorderLevel,
                                        ImageUrl = p.ImageUrl,
                                        BrandId = p.BrandId,
                                        CategoryId = p.CategoryId
                                    };

                                    var ok = await itemService.UpdateAsync(dto, ct);
                                    resp = new(req.RequestId, true, ok, null);
                                    break;
                                }
                            case "Item.Delete":
                                {
                                    var p = JsonSerializer.Deserialize<DeleteGuidPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var ok = await itemService.DeleteAsync(p.Id, ct);
                                    resp = new(req.RequestId, true, ok, null);
                                    break;
                                }

                            // =================== CATEGORY ===================
                            case "Category.GetList":
                                {
                                    var p = JsonSerializer.Deserialize<GetListPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)
                                            ?? new(null, 1, 12);
                                    var data = await categoryService.GetListAsync(p.Q, p.Page, p.PageSize, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }
                            case "Category.GetById":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdIntPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await categoryService.GetByIdAsync(p.Id, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }

                            case "Category.Create":
                                {
                                    var p = JsonSerializer.Deserialize<CategoryUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var ok = await categoryService.CreateAsync(p.Name, ct);
                                    resp = new(req.RequestId, true, ok, null);
                                    break;
                                }
                            case "Category.Update":
                                {
                                    var p = JsonSerializer.Deserialize<CategoryUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    if (p.Id is null || p.Id <= 0) throw new Exception("Invalid Id");
                                    var ok = await categoryService.UpdateAsync(p.Id.Value, p.Name, ct);
                                    resp = new(req.RequestId, true, ok, null);
                                    break;
                                }
                            case "Category.Delete":
                                {
                                    var p = JsonSerializer.Deserialize<DeletePayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var ok = await categoryService.DeleteAsync(p.Id, ct);
                                    resp = new(req.RequestId, true, ok, null);
                                    break;
                                }

                            // =================== BRAND ===================

                            case "Brand.GetList":
                                {
                                    var p = JsonSerializer.Deserialize<GetListPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)
                                            ?? new(null, 1, 12);
                                    var data = await brandService.GetListAsync(p.Q, p.Page, p.PageSize, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }
                            case "Brand.GetById":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdIntPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await brandService.GetByIdAsync(p.Id, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }
                            case "Brand.Create":
                                {
                                    var p = JsonSerializer.Deserialize<BrandUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var newId = await brandService.CreateAsync(p.Name, ct);
                                    resp = new(req.RequestId, true, newId, null);
                                    break;
                                }
                            case "Brand.Update":
                                {
                                    var p = JsonSerializer.Deserialize<BrandUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    if (p.Id is null || p.Id <= 0) throw new Exception("Invalid Id");
                                    var ok = await brandService.UpdateAsync(p.Id.Value, p.Name, ct);
                                    resp = new(req.RequestId, true, ok, null);
                                    break;
                                }
                            case "Brand.Delete":
                                {
                                    var p = JsonSerializer.Deserialize<DeletePayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var ok = await brandService.DeleteAsync(p.Id, ct);
                                    resp = new(req.RequestId, true, ok, null);
                                    break;
                                }


                            default:
                                resp = new(req.RequestId, false, null, $"Unknown Op: {req.Op}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        resp = new("?", false, null, ex.Message);
                    }

                    await TcpFraming.WriteJsonAsync(stream, resp, ct);
                }
            }
            catch (Exception)
            {
                // log nếu cần: _log.LogError(ex, "TCP client error");
            }
            finally { client.Close(); }
        }
    }
}
