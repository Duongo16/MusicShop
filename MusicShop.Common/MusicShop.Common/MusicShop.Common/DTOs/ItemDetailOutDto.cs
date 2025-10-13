using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs
{
    public record RefInfoDto(int Id, string Name);

    public class ItemDetailOutDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public ItemType ItemType { get; set; }
        public ItemStatus Status { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQty { get; set; }
        public int ReorderLevel { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public RefInfoDto? Brand { get; set; }
        public RefInfoDto? Category { get; set; }
    }
}
