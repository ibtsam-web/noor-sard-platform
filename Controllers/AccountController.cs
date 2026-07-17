using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoorSardPlatform.ViewModels;

namespace NoorSardPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectUserByRole();
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model
        )
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string adminUsername =
                _configuration["LoginAccounts:AdminUsername"]
                ?? string.Empty;

            string adminPassword =
                _configuration["LoginAccounts:AdminPassword"]
                ?? string.Empty;

            string supervisorUsername =
                _configuration["LoginAccounts:SupervisorUsername"]
                ?? string.Empty;

            string supervisorPassword =
                _configuration["LoginAccounts:SupervisorPassword"]
                ?? string.Empty;

            string? role = null;
            string? displayName = null;

            bool isAdmin =
                model.Username.Trim() == adminUsername &&
                model.Password == adminPassword;

            bool isSupervisor =
                model.Username.Trim() == supervisorUsername &&
                model.Password == supervisorPassword;

            if (isAdmin)
            {
                role = "Admin";
                displayName = "الإدارة";
            }
            else if (isSupervisor)
            {
                role = "Supervisor";
                displayName = "المشرفات";
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "اسم المستخدم أو كلمة المرور غير صحيحة."
                );

                model.Password = string.Empty;

                return View(model);
            }

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.Name,
                    model.Username.Trim()
                ),
                new(
                    ClaimTypes.Role,
                    role
                ),
                new(
                    "DisplayName",
                    displayName
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var authenticationProperties =
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authenticationProperties
            );

            if (
                !string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl)
            )
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectUserByRole();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                nameof(Login),
                "Account"
            );
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectUserByRole()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "Admin"
                );
            }

            if (User.IsInRole("Supervisor"))
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            return RedirectToAction(
                "Index",
                "Home"
            );
        }
    }
}