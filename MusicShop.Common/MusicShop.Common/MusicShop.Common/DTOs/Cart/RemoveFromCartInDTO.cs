namespace MusicShop.Common.DTOs.Cart
{
    public class RemoveFromCartInDTO
    {
        public Guid CartId { get; set; }
        public Guid ItemId { get; set; }
    }
}
