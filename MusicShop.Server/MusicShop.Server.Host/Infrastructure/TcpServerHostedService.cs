using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicShop.Common.DTOs;
using MusicShop.Common.DTOs.Cart;
using MusicShop.Common.DTOs.Order;
using MusicShop.Common.Models;
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
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var accountService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            using var stream = client.GetStream();

            try
            {
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    var json = await TcpFraming.ReadJsonAsync(stream);
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
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Item.GetById":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await itemService.GetByIdAsync(p.Id, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Item.Create":
                                {
                                    var p = JsonSerializer.Deserialize<ItemUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;

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

                                    var data = await itemService.CreateAsync(dto, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
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

                                    var data = await itemService.UpdateAsync(dto, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Item.Delete":
                                {
                                    var p = JsonSerializer.Deserialize<DeleteGuidPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await itemService.DeleteAsync(p.Id, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }

                            // =================== CATEGORY ===================
                            case "Category.GetList":
                                {
                                    var p = JsonSerializer.Deserialize<GetListPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)
                                            ?? new(null, 1, 12);
                                    var data = await categoryService.GetListAsync(p.Q, p.Page, p.PageSize, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Category.GetById":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdIntPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await categoryService.GetByIdAsync(p.Id, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Category.Create":
                                {
                                    var p = JsonSerializer.Deserialize<CategoryUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await categoryService.CreateAsync(p.Name, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Category.Update":
                                {
                                    var p = JsonSerializer.Deserialize<CategoryUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    if (p.Id is null || p.Id <= 0) throw new Exception("Invalid Id");
                                    var data = await categoryService.UpdateAsync(p.Id.Value, p.Name, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Category.Delete":
                                {
                                    var p = JsonSerializer.Deserialize<DeletePayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await categoryService.DeleteAsync(p.Id, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }

                            // =================== BRAND ===================

                            case "Brand.GetList":
                                {
                                    var p = JsonSerializer.Deserialize<GetListPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)
                                            ?? new(null, 1, 12);
                                    var data = await brandService.GetListAsync(p.Q, p.Page, p.PageSize, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Brand.GetById":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdIntPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await brandService.GetByIdAsync(p.Id, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Brand.Create":
                                {
                                    var p = JsonSerializer.Deserialize<BrandUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await brandService.CreateAsync(p.Name, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Brand.Update":
                                {
                                    var p = JsonSerializer.Deserialize<BrandUpsertPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    if (p.Id is null || p.Id <= 0) throw new Exception("Invalid Id");
                                    var data = await brandService.UpdateAsync(p.Id.Value, p.Name, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Brand.Delete":
                                {
                                    var p = JsonSerializer.Deserialize<DeletePayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await brandService.DeleteAsync(p.Id, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }

                            // =================== AUTH ===================
                            case "Auth.Register":
                                {
                                    var p = JsonSerializer.Deserialize<RegisterInDTO>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await authService.RegisterAsync(p, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Auth.Login":
                                {
                                    var p = JsonSerializer.Deserialize<LoginInDTO>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await authService.LoginAsync(p, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }

                            // =================== ACCOUNT ===================
                            case "Account.UpdateProfile":
                                {
                                    var p = JsonSerializer.Deserialize<UpdateProfileInDto>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await accountService.UpdateProfileAsync(p, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Account.GetProfile":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await accountService.GetUserProfileAsync(p.Id, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }


                            // =================== CART ===================
                            case "Cart.GetByUserOrGuest":
                                {
                                    var p = JsonSerializer.Deserialize<GetCartInDTO>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await cartService.GetCartAsync(p.UserId, p.GuestId, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Cart.AddToCart":
                                {
                                    var p = JsonSerializer.Deserialize<CartItemInDto>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await cartService.AddToCartAsync(p, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Cart.RemoveFromCart":
                                {
                                    var p = JsonSerializer.Deserialize<RemoveFromCartInDTO>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await cartService.RemoveFromCartAsync(p.CartId, p.ItemId, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Cart.UpdateQty":
                                {
                                    var p = JsonSerializer.Deserialize<UpdateCartQtyInDTO>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await cartService.UpdateQtyAsync(p.CartId, p.ItemId, p.Qty, ct);
                                    var dataElement = JsonSerializer.SerializeToElement(data, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }

                            // =================== CHECKOUT - ORDER ===================
                            case "Order.Checkout":
                                {
                                    var p = JsonSerializer.Deserialize<OrderCheckoutRequestDTO>(req.Payload?.ToString() ?? "{}", TcpFraming.Json);
                                    var data = await orderService.CheckoutAsync(p, ct);
                                    var envelope = new
                                    {
                                        Ok = data.Succeeded,
                                        Error = data.Error,
                                        order = data.Order
                                    };

                                    var dataElement = JsonSerializer.SerializeToElement(envelope, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;  
                                }
                            case "Order.UpdateStatus":
                                {
                                    var p = JsonSerializer.Deserialize<OrderUpdateStatusRequestDTO>(req.Payload?.ToString() ?? "{}", TcpFraming.Json);
                                    var data = await orderService.UpdateOrderStatusAsync(p, ct);
                                    var envelope = new
                                    {
                                        Ok = data.Succeeded,
                                        Error = data.Error
                                    };
                                    var dataElement = JsonSerializer.SerializeToElement(envelope, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
                                    break;
                                }
                            case "Order.GetList":
                                {
                                    var payload = JsonSerializer.Deserialize<GetListPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;

                                    var allOrders = await orderService.GetAllOrdersAsync(); 

                                    var query = payload.Q?.Trim().ToLower();
                                    if (!string.IsNullOrEmpty(query))
                                        allOrders = allOrders
                                            .Where(o => o.OrderNumber.ToLower().Contains(query))
                                            .ToList();

                                    var totalCount = allOrders.Count;

                                    var items = allOrders
                                        .Skip((payload.Page - 1) * payload.PageSize)
                                        .Take(payload.PageSize)
                                        .ToList();

                                    var pagedResult = new PagedResult<OrderListItemOutDTO>
                                    {
                                        Items = items,
                                        TotalCount = totalCount
                                    };

                                    var dataElement = JsonSerializer.SerializeToElement(pagedResult, TcpFraming.Json);
                                    resp = new(req.RequestId, true, dataElement, null);
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

                    await TcpFraming.WriteJsonAsync(stream, resp);
                }
            }
            catch (Exception)
            {
            }
            finally { client.Close(); }
        }
    }
}
