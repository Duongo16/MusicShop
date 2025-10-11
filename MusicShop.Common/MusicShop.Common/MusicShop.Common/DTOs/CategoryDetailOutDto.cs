using MusicShop.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace MusicShop.Common.DTOs
{
    public class CategoryDetailOutDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
    }
}
