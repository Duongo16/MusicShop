using MusicShop.Common.DTOs;

namespace MusicShop.Web.Services
{
    public interface IAccountService
    {
        Task<UserProfileOutDto?> GetProfileAsync(Guid userId);
        Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateProfileAsync(UpdateProfileInDto dto);
    }
}
