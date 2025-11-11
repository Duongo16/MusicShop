namespace MusicShop.Common.DTOs
{
    public class UpdateProfileInDto
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public DateTime? Dob { get; set; }
        public string? Address { get; set; }

        public UpdateProfileInDto() { }

        public UpdateProfileInDto(Guid userId, string? userName, string? fullName, string? phone, DateTime? dob, string? address)
        {
            UserId = userId;
            UserName = userName;
            FullName = fullName;
            Phone = phone;
            Dob = dob;
            Address = address;
        }
    }
}
