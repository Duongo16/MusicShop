namespace MusicShop.Common.DTOs
{
    public class UserProfileOutDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public DateTime? Dob { get; set; }
        public string? Address { get; set; }
    }
}
