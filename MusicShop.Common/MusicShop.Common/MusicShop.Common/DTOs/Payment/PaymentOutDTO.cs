using MusicShop.Common.Models;

namespace MusicShop.Common.DTOs.Payment
{
    public class PaymentOutDTO
    {
        public long Id { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
    }
}
