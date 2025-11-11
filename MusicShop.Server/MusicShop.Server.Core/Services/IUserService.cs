using MusicShop.Common.DTOs;
using MusicShop.Common.Models;

namespace MusicShop.Server.Core.Services
{
    public interface IUserService
    {
        Task<AuthResultOutDTO> UpdateProfileAsync(UpdateProfileInDto dto, CancellationToken ct = default);

        Task<AuthResultOutDTO> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);

        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<UserProfileOutDto?> GetUserProfileAsync(Guid userId, CancellationToken ct = default);
    }
}
