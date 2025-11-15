namespace MusicShop.Common.DTOs.Cart
{
    public class CartItemInDto
    {
        public Guid? UserId { get; set; }      
        public string? GuestId { get; set; }   
        public Guid ItemId { get; set; }
        public int Qty { get; set; }
    }
}
