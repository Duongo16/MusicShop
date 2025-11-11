using MusicShop.Common.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MusicShop.Common.DTOs;
using MusicShop.Web.Services;


namespace MusicShop.Web.Controllers
{

    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authClient;
        private readonly IAccountService _accountClient;

        public AccountController(IAuthService authClient, IAccountService accountClient)
        {
            _authClient = authClient;
            _accountClient = accountClient;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var req = new RegisterInDTO(model.Email, model.Password, model.UserName);
            var resp = await _authClient.RegisterAsync(req);

            if (!resp.Succeeded)
            {
                foreach (var e in resp.Errors ?? Array.Empty<string>())
                    ModelState.AddModelError(string.Empty, e);
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var req = new LoginInDTO(model.Email, model.Password, model.RememberMe);
            var resp = await _authClient.LoginAsync(req);

            if (!resp.Succeeded)
            {
                foreach (var err in resp.Errors)
                    ModelState.AddModelError(string.Empty, err);
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, resp.UserId.ToString()),
                new Claim(ClaimTypes.Name, resp.Email)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return RedirectToAction("Login", "Account");

            var profile = await _accountClient.GetProfileAsync((Guid)userId);
            if (profile == null) return NotFound();

            var vm = new ProfileViewModel
            {
                Id = profile.Id,
                Email = profile.Email,
                UserName = profile.UserName,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Dob = profile.Dob,
                Address = profile.Address
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetUserIdFromClaims();
            if (userId == null) return RedirectToAction("Login", "Account");

            var dto = new UpdateProfileInDto(
                userId.Value,
                model.UserName,
                model.FullName,
                model.Phone,
                model.Dob,
                model.Address
            );

            var (succeeded, errors) = await _accountClient.UpdateProfileAsync(dto);
            if (!succeeded)
            {
                foreach (var e in errors) ModelState.AddModelError(string.Empty, e);
                return View(model);
            }

            TempData["ProfileSuccess"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        private Guid? GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(idClaim, out var id)) return id;
            return null;
        }

    }


}
