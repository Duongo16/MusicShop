using Microsoft.AspNetCore.Identity;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories
{
    public interface IRoleRepository
    {
        Task<IdentityResult> CreateRoleAsync(string roleName, CancellationToken ct = default);
        Task<IdentityResult> DeleteRoleAsync(string roleName, CancellationToken ct = default);
        Task<bool> RoleExistsAsync(string roleName, CancellationToken ct = default);
        Task<ApplicationRole?> FindByNameAsync(string roleName, CancellationToken ct = default);
    }
}
