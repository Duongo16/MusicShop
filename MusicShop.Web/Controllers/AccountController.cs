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
        private readonly IAuthClientService _authClient;

        public AccountController(IAuthClientService authClient) => _authClient = authClient;

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

    }


}
