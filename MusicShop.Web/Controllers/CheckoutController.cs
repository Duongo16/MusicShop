using Microsoft.AspNetCore.Mvc;
using MusicShop.Common.DTOs.Order;
using MusicShop.Common.ViewModels;
using MusicShop.Web.Services;
using System.Security.Claims;

namespace MusicShop.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartClient;
        private readonly IOrderService _orderClient;
        private readonly IAccountService _accountService; 

        public CheckoutController(ICartService cartClient, IOrderService orderClient, IAccountService userService)
        {
            _cartClient = cartClient;
            _orderClient = orderClient;
            _accountService = userService;
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

            var cart = await _cartClient.GetCartAsync(userId, guestId);
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

            var (succeeded, error, order) = await _orderClient.CheckoutAsync(req);
            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, error ?? "Checkout failed");
                return View(model);
            }

            return RedirectToAction("Details", "Order", new { id = order.Id });
        }
    }

}
