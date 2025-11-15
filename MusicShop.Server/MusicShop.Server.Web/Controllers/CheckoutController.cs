using Microsoft.AspNetCore.Mvc;
using MusicShop.Common.DTOs.Order;
using MusicShop.Common.ViewModels;
using MusicShop.Server.Core.Services;
using System.Security.Claims;

[Route("[controller]/[action]")]
public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CheckoutController(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
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
        var vm = new CheckoutViewModel { Cart = cart };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var userId = GetUserId();
        var guestId = GetGuestId();

        var req = new OrderCheckoutRequestDTO
        {
            UserId = userId,
            GuestId = guestId,
            Address = new OrderAddressDTO
            {
                Name = model.Address.Name,
                Phone = model.Address.Phone,
                AddressFull = model.Address.AddressFull
            },
            PaymentMethod = model.PaymentMethod
        };

        var (succeeded, error, order) = await _orderService.CheckoutAsync(req, HttpContext.RequestAborted);
        if (!succeeded)
        {
            ModelState.AddModelError(string.Empty, error ?? "Checkout failed");
            return View(model);
        }

        return RedirectToAction("Details", "Order", new { id = order.Id });
    }
}
