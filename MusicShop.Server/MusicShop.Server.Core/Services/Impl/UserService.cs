using Microsoft.AspNetCore.Identity;
using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Server.Data.Repositories;

namespace MusicShop.Server.Core.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<AuthResultOutDTO> UpdateProfileAsync(UpdateProfileInDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var user = await _userRepo.GetByIdAsync(dto.UserId, ct);
            if (user == null) return new AuthResultOutDTO(false, new[] { "User not found." });

            if (!string.IsNullOrWhiteSpace(dto.UserName))
                user.UserName = dto.UserName;

            if (user.Profile == null)
            {
                user.Profile = new Profile
                {
                    UserId = user.Id,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Dob = dto.Dob,
                    Address = dto.Address
                };
            }
            else
            {
                user.Profile.FullName = dto.FullName;
                user.Profile.Phone = dto.Phone;
                user.Profile.Dob = dto.Dob;
                user.Profile.Address = dto.Address;
            }

            var updateRes = await _userRepo.UpdateAsync(user, ct);
            if (!updateRes.Succeeded)
            {
                var errs = updateRes.Errors?.Select(e => e.Description) ?? new[] { "Failed to update profile." };
                return new AuthResultOutDTO(false, errs);
            }

            return new AuthResultOutDTO(true, Array.Empty<string>());
        }

        public async Task<AuthResultOutDTO> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null) return new AuthResultOutDTO(false, new[] { "User not found." });

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verification == PasswordVerificationResult.Failed)
                return new AuthResultOutDTO(false, new[] { "Current password is incorrect." });

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

            var updateRes = await _userRepo.UpdateAsync(user, ct);
            if (!updateRes.Succeeded)
            {
                var errs = updateRes.Errors?.Select(e => e.Description) ?? new[] { "Failed to change password." };
                return new AuthResultOutDTO(false, errs);
            }

            return new AuthResultOutDTO(true, Array.Empty<string>());
        }


        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return _userRepo.GetByEmailAsync(email, ct);
        }

        public Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _userRepo.GetByIdAsync(id, ct);
        }

        public Task<UserProfileOutDto?> GetUserProfileAsync(Guid userId, CancellationToken ct = default)
        {
            return _userRepo.GetUserProfileAsync(userId, ct);
        }
    }
}
