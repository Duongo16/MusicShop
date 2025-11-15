namespace MusicShop.Common.DTOs.Order
{
    public class OrderItemOutDTO
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Qty;
    }
}
