using MusicShop.Common.DTOs;
using MusicShop.Common.Transport;

namespace MusicShop.Web.Services.Impl
{
    public class AuthClientService : IAuthClientService
    {
        private readonly TcpClientHelper _tcp;
        public AuthClientService(TcpClientHelper tcp) => _tcp = tcp;

        public async Task<AuthResultOutDTO> RegisterAsync(RegisterInDTO model)
        {
            var resp = await _tcp.SendAsync<AuthResultOutDTO>("Auth.Register", model);
            return resp ?? new AuthResultOutDTO(false, new[] { "No response from auth server." });
        }

        public async Task<AuthResultOutDTO> LoginAsync(LoginInDTO model)
        {
            var resp = await _tcp.SendAsync<AuthResultOutDTO>("Auth.Login", model);
            return resp ?? new AuthResultOutDTO(false, new[] { "No response from auth server." });
        }

        public async Task LogoutAsync(Guid userId)
        {
            await _tcp.SendAsync<object?>("Auth.Logout", new { UserId = userId });
        }
    }
}
