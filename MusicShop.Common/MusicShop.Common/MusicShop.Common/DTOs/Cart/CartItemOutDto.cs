namespace MusicShop.Common.DTOs.Cart
{
    public class CartItemOutDto
    {
        public Guid CartId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public int StockQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Qty;
    }
}
