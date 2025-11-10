using System.Text.Json.Serialization;

namespace MusicShop.Common.DTOs
{
    public class AuthResultOutDTO
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        public Guid? UserId { get; set; }
        public string? Email { get; set; }

        public AuthResultOutDTO() { }

        [JsonConstructor]
        public AuthResultOutDTO(bool succeeded, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors ?? Enumerable.Empty<string>();
        }
    }
}
