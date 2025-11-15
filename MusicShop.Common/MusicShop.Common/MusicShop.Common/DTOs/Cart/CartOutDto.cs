namespace MusicShop.Common.DTOs.Cart
{
    public class CartOutDto
    {
        public Guid CartId { get; set; }
        public Guid? UserId { get; set; }
        public string? GuestId { get; set; }
        public bool IsCheckout { get; set; }
        public List<CartItemOutDto> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.LineTotal);
    }
}
