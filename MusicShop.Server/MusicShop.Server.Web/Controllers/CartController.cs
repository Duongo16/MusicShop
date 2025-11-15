using Microsoft.AspNetCore.Mvc;
using MusicShop.Common.DTOs.Cart;
using MusicShop.Server.Core.Services;
using System.Security.Claims;

[Route("[controller]/[action]")]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IItemService _itemService;

    public CartController(ICartService cartService, IItemService itemService)
    {
        _cartService = cartService;
        _itemService = itemService;
    }

    private Guid? GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(id, out var g)) return g;
        return null;
    }

    private string? GetGuestId()
    {
        if (Request.Cookies.TryGetValue("GuestId", out var guest)) return guest;
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var guestId = GetGuestId();
        var cart = await _cartService.GetCartAsync(userId, guestId, HttpContext.RequestAborted);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid itemId, int qty = 1)
    {
        var userId = GetUserId();
        var guestId = GetGuestId();
        if (userId == null && string.IsNullOrEmpty(guestId))
        {
            guestId = Guid.NewGuid().ToString();
            Response.Cookies.Append("GuestId", guestId, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30) });
        }

        var item = await _itemService.GetByIdAsync(itemId, HttpContext.RequestAborted);

        var dto = new CartItemInDto { UserId = userId, GuestId = guestId, ItemId = itemId, Qty = qty };
        var resp = await _cartService.AddToCartAsync(dto, HttpContext.RequestAborted);

        if (!resp.Succeeded)
            TempData["CartError"] = string.Join(", ", resp.Errors ?? Array.Empty<string>());

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid cartId, Guid itemId)
    {
        var resp = await _cartService.RemoveFromCartAsync(cartId, itemId, HttpContext.RequestAborted);
        if (!resp.Succeeded) TempData["CartError"] = string.Join(", ", resp.Errors ?? Array.Empty<string>());
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQty(Guid cartId, Guid itemId, int qty)
    {
        var item = await _itemService.GetByIdAsync(itemId, HttpContext.RequestAborted);
        if (item != null && qty > item.StockQty)
        {
            return Json(new
            {
                succeeded = false,
                errors = new[] { $"Số lượng tồn kho không đủ (chỉ còn {item.StockQty})." },
                maxAllowed = item.StockQty
            });
        }

        var userId = GetUserId();
        var guestId = GetGuestId();

        var resp = await _cartService.UpdateQtyAsync(cartId, itemId, qty, HttpContext.RequestAborted);

        if (!resp.Succeeded)
            return Json(new
            {
                succeeded = false,
                errors = resp.Errors ?? new[] { "Lỗi không xác định khi cập nhật." }
            });

        var cart = await _cartService.GetCartAsync(userId, guestId, HttpContext.RequestAborted);
        var updatedItem = cart.Items.FirstOrDefault(i => i.ItemId == itemId);

        decimal lineTotal = updatedItem?.LineTotal ?? 0;
        int currentQty = updatedItem?.Qty ?? 0;
        bool cartIsEmpty = !cart.Items.Any();

        return Json(new
        {
            succeeded = true,
            lineTotalInCents = (long)(lineTotal * 100),  
            cartTotalInCents = (long)(cart.Total * 100),    
            newQty = currentQty,                            
            cartIsEmpty = cartIsEmpty,                     
            itemRemoved = updatedItem == null
        });
    }
}
