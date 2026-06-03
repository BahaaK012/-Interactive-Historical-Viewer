using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BahaaBuseProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _config;
        public AdminController(IConfiguration config) { _config = config; }

        [Authorize] /* only admins allowed here */
        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Panel";
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            /* if already logged in send them to panel */
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] /* secruity to stop crsf attacks */
        public async Task<IActionResult> Login(
            string username,
            string password,
            bool rememberMe = false,
            string? returnUrl = null)
        {
            /* get user list from the settings file */
            var users = _config.GetSection("AdminUsers")
                .Get<List<AdminUser>>() ?? new List<AdminUser>();

            /* check if any user matches the info */
            var match = users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (match != null)
            {
                /* create identity claims for the user */
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, match.Username),
                    new(ClaimTypes.Role, "Admin")
                };
                var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                /* remember-me = a 30 day cookie; otherwise it will be an 8 hour session cookie */
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc   = rememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true
                };

                /* sign in the user */
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProps);

                /* save login info in seassion for the admin panel */
                HttpContext.Session.SetString("admin_login_time", DateTime.UtcNow.ToString("o"));
                HttpContext.Session.SetString("admin_username",   match.Username);

                return Redirect(returnUrl ?? "/Admin");
            }

            /* show erorr if info is wrong */
            ViewData["Error"]     = "Incorrect username or password.";
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            /* clear seassion and sign out of cookies */
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }

    public record AdminUser(string Username, string Password);
}