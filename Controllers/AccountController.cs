using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models.ViewModels;
using RaceDay.MVC.Services;
using System.Security.Claims;
using System.Text.Json;

namespace RaceDay.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IRaceDayApiClient _api;

    public AccountController(IRaceDayApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await _api.PostAsync("api/auth/login", new { model.Email, model.Password });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthResultDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Store JWT in Session
        HttpContext.Session.SetString("JWToken", authResult.Token);
        HttpContext.Session.SetString("UserRole", authResult.User.Role);
        HttpContext.Session.SetString("UserName", $"{authResult.User.FirstName} {authResult.User.LastName}");

        // Create Cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, authResult.User.Email),
            new Claim(ClaimTypes.Role, authResult.User.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return authResult.User.Role == "Organiser"
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Participant");
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}