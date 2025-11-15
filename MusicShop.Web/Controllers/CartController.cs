using Microsoft.AspNetCore.Mvc;
using MusicShop.Common.DTOs.Cart;
using MusicShop.Web.Services;
using System.Security.Claims;

[Route("[controller]/[action]")]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IItemService _itemService;
    public CartController(ICartService cartClient, IItemService itemService)
    {
        _cartService = cartClient;
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
        var cart = await _cartService.GetCartAsync(userId, guestId);
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
        var item = await _itemService.GetByIdAsync(itemId);

        var dto = new CartItemInDto { UserId = userId, GuestId = guestId, ItemId = itemId, Qty = qty };
        var resp = await _cartService.AddToCartAsync(dto);
        if (!resp.Succeeded)
        {
            TempData["CartError"] = string.Join(", ", resp.Errors);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid cartId, Guid itemId)
    {
        var resp = await _cartService.RemoveFromCartAsync(cartId, itemId);
        if (!resp.Succeeded) TempData["CartError"] = string.Join(", ", resp.Errors);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQty(Guid cartId, Guid itemId, int qty)
    {

        var item = await _itemService.GetByIdAsync(itemId);
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

        var resp = await _cartService.UpdateQtyAsync(cartId, itemId, qty);

        if (!resp.Succeeded)
        {
            return Json(new
            {
                succeeded = false,
                errors = resp.Errors ?? new[] { "Lỗi không xác định khi cập nhật." }
            });
        }

        var cart = await _cartService.GetCartAsync(userId, guestId);

        var updatedItem = cart.Items.FirstOrDefault(i => i.ItemId == itemId);
        decimal lineTotal = updatedItem?.LineTotal ?? 0;

        return Json(new
        {
            succeeded = true,
            lineTotal = lineTotal.ToString("C"),
            cartTotal = cart.Total.ToString("C"),
            itemRemoved = (updatedItem == null)
        });
    }
}
