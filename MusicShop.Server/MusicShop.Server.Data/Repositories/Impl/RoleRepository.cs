using Microsoft.AspNetCore.Identity;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories.Impl
{
    public class RoleRepository : IRoleRepository
    {
        public Task<IdentityResult> CreateRoleAsync(string roleName, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteRoleAsync(string roleName, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationRole?> FindByNameAsync(string roleName, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RoleExistsAsync(string roleName, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
