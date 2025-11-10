using System.Text.Json.Serialization;

namespace MusicShop.Common.DTOs
{
    public class LoginInDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;

        public LoginInDTO() { } 

        [JsonConstructor]
        public LoginInDTO(string email, string password, bool rememberMe)
        {
            Email = email;
            Password = password;
            RememberMe = rememberMe;
        }
    }
}
