using Microsoft.EntityFrameworkCore;
using MusicShop.Common.DTOs.Cart;
using MusicShop.Common.DTOs.Order;
using MusicShop.Common.DTOs.Payment;
using MusicShop.Common.Models;
using MusicShop.Server.Data;
using MusicShop.Server.Data.Repositories;

namespace MusicShop.Server.Core.Services.Impl
{
    public class OrderService : IOrderService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly AppDbContext _db;

        public OrderService(ICartRepository cartRepo, IOrderRepository orderRepo, AppDbContext db)
        {
            _cartRepo = cartRepo;
            _orderRepo = orderRepo;
            _db = db;
        }

        public async Task<(bool Succeeded, string? Error, OrderOutDTO? Order)> CheckoutAsync(OrderCheckoutRequestDTO req, CancellationToken ct = default)
        {
            if (req == null) return (false, "Invalid request.", null);

            Cart? cart = null;
            if (req.UserId.HasValue) cart = await _cartRepo.GetByUserIdAsync(req.UserId.Value, ct);
            if (cart == null && !string.IsNullOrEmpty(req.GuestId)) cart = await _cartRepo.GetByGuestIdAsync(req.GuestId!, ct);
            if (cart == null || !cart.Items.Any()) return (false, "Cart is empty.", null);

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var ci in cart.Items)
                {
                    var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == ci.ItemId, ct);
                    if (item == null) return (false, $"Item {ci.ItemId} not found.", null);
                    if (item.StockQty < ci.Qty) return (false, $"Insufficient stock for {item.Name}.", null);
                }

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = GenerateOrderNumber(),
                    UserId = cart.UserId,
                    GuestId = cart.GuestId,
                    Status = OrderStatus.Pending,
                    Total = 0m,
                    Items = new List<OrderItem>()
                };

                foreach (var ci in cart.Items)
                {
                    var itemEntity = await _db.Items.FirstAsync(i => i.Id == ci.ItemId, ct);
                    var unitPrice = ci.UnitPrice;
                    var oi = new OrderItem
                    {
                        OrderId = order.Id,
                        ItemId = ci.ItemId,
                        Qty = ci.Qty,
                        UnitPrice = unitPrice
                    };
                    order.Items.Add(oi);

                    itemEntity.StockQty -= ci.Qty;
                    var ledger = new InventoryLedger
                    {
                        ItemId = itemEntity.Id,
                        ChangeQty = -ci.Qty,
                        Reason = InventoryReason.Sale,
                        RefNo = order.OrderNumber
                    };
                    _db.InventoryLedgers.Add(ledger);

                    order.Total += unitPrice * ci.Qty;
                }

                if (req.Address != null)
                {
                    order.Address = new OrderAddress
                    {
                        OrderId = order.Id,
                        Name = req.Address.Name,
                        Phone = req.Address.Phone,
                        AddressFull = req.Address.AddressFull
                    };
                }

                await _orderRepo.CreateAsync(order, ct);

                cart.IsCheckout = true;
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                var dto = new OrderOutDTO
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    GuestId = order.GuestId,
                    Status = order.Status,
                    Items = order.Items.Select(x => new CartItemOutDto
                    {
                        CartId = cart.Id,
                        ItemId = x.ItemId,
                        ItemName = _db.Items.Find(x.ItemId)?.Name ?? string.Empty,
                        Qty = x.Qty,
                        StockQty = _db.Items.Find(x.ItemId)?.StockQty ?? 0,
                        UnitPrice = x.UnitPrice
                    }).ToList(),
                    Total = order.Total,
                    Address = req.Address
                };

                return (true, null, dto);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return (false, ex.Message, null);
            }
        }

        private string GenerateOrderNumber()
        {
            return $"{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        public async Task<List<OrderListItemOutDTO>> GetOrdersAsync(Guid? userId, string? guestId, CancellationToken ct = default)
        {
            List<Order> orders = userId.HasValue
                ? await _orderRepo.GetByUserIdAsync(userId.Value, ct)
                    : new List<Order>();

            return orders.Select(o => new OrderListItemOutDTO
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                Total = o.Total
            }).ToList();
        }

        public async Task<OrderDetailOutDTO?> GetOrderDetailAsync(Guid orderId, Guid? userId, string? guestId, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null) return null;
            if (userId.HasValue && order.UserId != userId) return null;

            return new OrderDetailOutDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                Total = order.Total,
                Address = order.Address == null ? null : new OrderAddressDTO
                {
                    Name = order.Address.Name,
                    Phone = order.Address.Phone,
                    AddressFull = order.Address.AddressFull
                },
                Items = order.Items?
                    .Where(i => i.Item != null)
                    .Select(i => new OrderItemOutDTO
                    {
                        ItemId = i.ItemId,
                        ItemName = i.Item.Name,
                        Qty = i.Qty,
                        UnitPrice = i.UnitPrice
                    }).ToList() ?? new List<OrderItemOutDTO>(),
                Payments = order.Payments.Select(p => new PaymentOutDTO
                {
                    Id = p.Id,
                    Method = p.Method,
                    Amount = p.Amount,
                    Status = p.Status,
                    TransactionId = p.TransactionId
                }).ToList()
            };
        }

        public async Task<List<OrderListItemOutDTO>> GetAllOrdersAsync()
        {
            var orders = await _db.Orders
                .AsNoTracking()
                .ToListAsync();

            return orders.Select(o => new OrderListItemOutDTO
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                Total = o.Total
            }).ToList();
        }

        public async Task<(bool Succeeded, string? Error)> UpdateOrderStatusAsync(OrderUpdateStatusRequestDTO req, CancellationToken ct = default)
        {
            if (req == null)
            {
                return (false, "Invalid request.");
            }

            try
            {
                var order = await _orderRepo.GetByIdAsync(req.OrderId, ct);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                if (order.Status == req.NewStatus)
                {
                    return (true, null);
                }

                order.Status = req.NewStatus;

                await _orderRepo.UpdateAsync(order, ct);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }

}
