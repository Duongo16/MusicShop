using Microsoft.AspNetCore.Mvc;
using MusicShop.Server.Core.Services;

public class ItemsController : Controller
{
    private readonly IItemService _items;

    public ItemsController(IItemService items) => _items = items;

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        const int pageSize = 12;
        var paged = await _items.GetListAsync(q, page, pageSize, HttpContext.RequestAborted);
        ViewData["q"] = q;
        return View(paged);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        if (id == Guid.Empty) return NotFound();
        var item = await _items.GetByIdAsync(id, HttpContext.RequestAborted);
        if (item == null) return NotFound();
        return View(item);
    }
}
