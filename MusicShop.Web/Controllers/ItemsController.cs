using Microsoft.AspNetCore.Mvc;
using MusicShop.Web.Services;

namespace MusicShop.Web.Controllers
{
    public class ItemsController : Controller
    {
        private readonly IItemClientService _items;
        public ItemsController(IItemClientService items) => _items = items;

        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int pageSize = 12;
            var paged = await _items.GetListAsync(q, page, pageSize);
            ViewData["q"] = q;
            return View(paged);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty) return NotFound();
            var item = await _items.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}
