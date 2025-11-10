using Microsoft.AspNetCore.Identity;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct = default);
        Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct = default);
        Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct = default);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken ct = default);
        Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct = default);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role, CancellationToken ct = default);
        Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role, CancellationToken ct = default);
    }
}
