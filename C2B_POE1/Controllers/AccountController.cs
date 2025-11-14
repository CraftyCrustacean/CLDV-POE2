using C2B_POE1.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Claims;

namespace C2B_POE1.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AccountController> _logger;
        private const string BASE_URL = "https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table";

        public AccountController(HttpClient httpClient, IPasswordHasher<User> passwordHasher, ILogger<AccountController> logger)
        {
            _httpClient = httpClient;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var users = await _httpClient.GetFromJsonAsync<List<User>>($"{BASE_URL}/Users");
            if (users?.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)) == true)
            {
                ModelState.AddModelError("Email", "Email already registered");
                return View(model);
            }

            var user = new User
            {
                PartitionKey = "User",
                RowKey = model.Email.ToLower(),
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = "Customer",
                IsActive = true
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
            await _httpClient.PostAsJsonAsync($"{BASE_URL}/Users", user);

            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var users = await _httpClient.GetFromJsonAsync<List<User>>($"{BASE_URL}/Users");
            var user = users?.FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) && u.IsActive);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.RowKey),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties {ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) });

            _logger.LogInformation($"User {user.Email} logged in as {user.Role}");

            return user.Role == "Admin" ? RedirectToAction("Index", "Orders") : RedirectToAction("Index", "Home");
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Logged out successfully";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied() => View();

    }
}