using MusicShop.Common.DTOs;

namespace MusicShop.Server.Core.Services
{
    public interface IAuthService
    {
        Task<AuthResultOutDTO> RegisterAsync(RegisterInDTO req, CancellationToken ct = default);
        Task<AuthResultOutDTO> LoginAsync(LoginInDTO req, CancellationToken ct = default);
        Task SignOutAsync(CancellationToken ct = default);
        Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default);
        Task<AuthResultOutDTO> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default);
        Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
    }
}
