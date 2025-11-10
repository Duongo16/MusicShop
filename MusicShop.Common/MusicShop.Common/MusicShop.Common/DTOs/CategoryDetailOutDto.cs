using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs
{
    public class CategoryDetailOutDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public int ItemsCount { get; set; }
    }
}
