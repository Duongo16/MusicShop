using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;
        public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct = default)
        {
            _db.Users.Add(user);
            return _db.SaveChangesAsync(ct)
                .ContinueWith(t => t.IsCompletedSuccessfully
                    ? IdentityResult.Success
                    : IdentityResult.Failed(new IdentityError { Description = "Failed to create user." }), ct);

        }

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return _db.Users
                 .Where(u => u.Email == email)
                 .FirstOrDefaultAsync(ct);
        }

        public Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
