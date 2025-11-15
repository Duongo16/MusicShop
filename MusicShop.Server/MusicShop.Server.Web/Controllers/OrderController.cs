using Microsoft.AspNetCore.Mvc;
using MusicShop.Server.Core.Services;
using System.Security.Claims;

namespace MusicShop.Server.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderClient;
        private readonly IUserService _userService;

        public OrderController(IOrderService orderClient, IUserService userService)
        {
            _orderClient = orderClient;
            _userService = userService;
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
            var orders = await _orderClient.GetOrdersAsync(userId, guestId);
            return View(orders);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = GetUserId();
            var guestId = GetGuestId();
            var order = await _orderClient.GetOrderDetailAsync(id, userId, guestId);
            if (order == null) return NotFound();

            return View(order);
        }
    }

}
