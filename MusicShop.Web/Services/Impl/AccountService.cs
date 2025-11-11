using MusicShop.Common.DTOs;
using MusicShop.Common.Transport;

namespace MusicShop.Web.Services.Impl
{
    public class AccountService : IAccountService
    {
        private readonly TcpClientHelper _tcp;
        public AccountService(TcpClientHelper tcp) => _tcp = tcp;

        public async Task<UserProfileOutDto?> GetProfileAsync(Guid userId)
        {
            var resp = await _tcp.SendAsync<UserProfileOutDto>("Account.GetProfile", new
            {
                Id = userId
            });
            return resp;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateProfileAsync(UpdateProfileInDto dto)
        {
            var resp = await _tcp.SendAsync<AuthResultOutDTO>("Account.UpdateProfile", dto);
            if (resp == null) return (false, new[] { "No response from server." });
            return (resp.Succeeded, resp.Errors ?? Enumerable.Empty<string>());
        }
    }
}
