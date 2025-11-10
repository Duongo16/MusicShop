namespace MusicShop.Common.DTOs
{
    public class RegisterInDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? UserName { get; set; }

        public RegisterInDTO() { } 

        public RegisterInDTO(string email, string password, string? userName = null)
        {
            Email = email;
            Password = password;
            UserName = userName;
        }
    }
}
