using MusicShop.Common.DTOs;

namespace MusicShop.Web.Services
{
    public interface IAuthClientService
    {
        Task<AuthResultOutDTO> RegisterAsync(RegisterInDTO model);
        Task<AuthResultOutDTO> LoginAsync(LoginInDTO model);
        Task LogoutAsync(Guid userId);
    }
}
