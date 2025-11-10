using Microsoft.AspNetCore.Identity;
using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Server.Data.Repositories;

namespace MusicShop.Server.Core.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            SignInManager<ApplicationUser> signInManager)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _signInManager = signInManager;
        }

        public async Task<AuthResultOutDTO> RegisterAsync(RegisterInDTO req, CancellationToken ct = default)
        {
            var existing = await _userRepo.GetByEmailAsync(req.Email, ct);
            if (existing != null)
                return new AuthResultOutDTO(false, new[] { "Email is already registered." });

            var hasher = new PasswordHasher<ApplicationUser>();

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = req.Email,
                UserName = string.IsNullOrWhiteSpace(req.UserName) ? req.Email : req.UserName,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user.PasswordHash = hasher.HashPassword(user, req.Password);


            var result = await _userRepo.CreateAsync(user, ct);
            if (!result.Succeeded) return new AuthResultOutDTO(false, result.Errors.Select(e => e.Description));

            return new AuthResultOutDTO(true, Array.Empty<string>());
        }

        public async Task<AuthResultOutDTO> LoginAsync(LoginInDTO req, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByEmailAsync(req.Email, ct);
            if (user == null) return new AuthResultOutDTO(false, new[] { "Invalid credentials." });

            var hasher = new PasswordHasher<ApplicationUser>();

            var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (verification == PasswordVerificationResult.Failed)
                return new AuthResultOutDTO(false, new[] { "Invalid credentials." });

            return new AuthResultOutDTO(true, Array.Empty<string>())
            {
                UserId = user.Id,
                Email = user.Email
            };
        }

        public Task SignOutAsync(CancellationToken ct = default)
        {
            return _signInManager.SignOutAsync();
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null) return false;
            var roles = await _userRepo.GetRolesAsync(user, ct);
            return roles.Contains(role);
        }

        public async Task<AuthResultOutDTO> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null) return new AuthResultOutDTO(false, new[] { "User not found." });

            if (!await _roleRepo.RoleExistsAsync(role, ct))
            {
                var createRoleResult = await _roleRepo.CreateRoleAsync(role, ct);
                if (!createRoleResult.Succeeded)
                    return new AuthResultOutDTO(false, createRoleResult.Errors.Select(e => e.Description));
            }

            var addResult = await _userRepo.AddToRoleAsync(user, role, ct);
            if (!addResult.Succeeded) return new AuthResultOutDTO(false, addResult.Errors.Select(e => e.Description));

            return new AuthResultOutDTO(true, Array.Empty<string>());
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null) return Array.Empty<string>();
            return await _userRepo.GetRolesAsync(user, ct);
        }
    }
}
