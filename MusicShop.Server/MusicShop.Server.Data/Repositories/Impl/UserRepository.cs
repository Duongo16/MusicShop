using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicShop.Common.DTOs;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public UserRepository(AppDbContext db)
        {
            _db = db;
            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));

            // tìm role
            var roleEntity = await _db.Roles.FirstOrDefaultAsync(r => r.Name == role, ct);
            if (roleEntity == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Role '{role}' not found." });
            }

            var userRole = new IdentityUserRole<Guid>
            {
                RoleId = roleEntity.Id,
                UserId = user.Id
            };

            var exists = await _db.Set<IdentityUserRole<Guid>>()
                                  .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == roleEntity.Id, ct);

            if (exists) return IdentityResult.Success;

            try
            {
                _db.Set<IdentityUserRole<Guid>>().Add(userRole);
                await _db.SaveChangesAsync(ct);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to add role: {ex.Message}" });
            }
        }

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (password == null) throw new ArgumentNullException(nameof(password));

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return Task.FromResult(verification != PasswordVerificationResult.Failed);
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            // ensure security stamp
            if (string.IsNullOrEmpty(user.SecurityStamp))
                user.SecurityStamp = Guid.NewGuid().ToString();

            _db.Users.Add(user);
            return _db.SaveChangesAsync(ct)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully && t.Result > 0)
                        return IdentityResult.Success;

                    return IdentityResult.Failed(new IdentityError { Description = "Failed to create user." });
                }, ct);
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            try
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync(ct);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to delete user: {ex.Message}" });
            }
        }

        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return Task.FromResult<ApplicationUser?>(null);

            return _db.Users
                 .Include(u => u.Profile)
                 .Where(u => u.Email == email)
                 .FirstOrDefaultAsync(ct);
        }

        public Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Users
                .Include(u => u.Profile)
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var roleIds = await _db.Set<IdentityUserRole<Guid>>()
                                  .Where(ur => ur.UserId == user.Id)
                                  .Select(ur => ur.RoleId)
                                  .ToListAsync(ct);

            if (!roleIds.Any()) return new List<string>();

            var roleNames = await _db.Roles
                                   .Where(r => roleIds.Contains(r.Id))
                                   .Select(r => r.Name!)
                                   .ToListAsync(ct);

            return roleNames;
        }

        public Task<UserProfileOutDto?> GetUserProfileAsync(Guid userId, CancellationToken ct)
        {
            var res =  _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Profile != null
                    ? new UserProfileOutDto
                    {
                        FullName = u.Profile.FullName,
                        Phone = u.Profile.Phone,
                        Dob = u.Profile.Dob,
                        Address = u.Profile.Address,
                        Email = u.Email,
                        UserName = u.UserName
                    }
                    : null)
                .FirstOrDefaultAsync(ct);
            return res;
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));

            var roleEntity = await _db.Roles.FirstOrDefaultAsync(r => r.Name == role, ct);
            if (roleEntity == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Role '{role}' not found." });
            }

            var userRole = await _db.Set<IdentityUserRole<Guid>>()
                                   .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == roleEntity.Id, ct);

            if (userRole == null) return IdentityResult.Success; // nothing to remove

            try
            {
                _db.Set<IdentityUserRole<Guid>>().Remove(userRole);
                await _db.SaveChangesAsync(ct);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to remove role: {ex.Message}" });
            }
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            try
            {
                user.NormalizedEmail = user.Email?.ToUpperInvariant();
                user.NormalizedUserName = user.UserName?.ToUpperInvariant();

                _db.Users.Update(user);
                await _db.SaveChangesAsync(ct);
                return IdentityResult.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Concurrency error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to update user: {ex.Message}" });
            }
        }
    }
}
