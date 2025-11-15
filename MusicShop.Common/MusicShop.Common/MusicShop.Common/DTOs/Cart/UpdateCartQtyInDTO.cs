namespace MusicShop.Common.DTOs.Cart
{
    public class UpdateCartQtyInDTO
    {
        public Guid CartId { get; set; }
        public Guid ItemId { get; set; }
        public int Qty { get; set; }
    }
}
