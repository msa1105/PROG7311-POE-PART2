using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PROG7311_POE.Models;

namespace PROG7311_POE.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public AccountController(IHttpClientFactory clientFactory, IConfiguration config)
    {
        _client = clientFactory.CreateClient("GlmsApi");
        _config = config;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (Request.Cookies.ContainsKey("jwt_token"))
        {
            return Redirect(returnUrl ?? "/");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        email = email?.Trim() ?? string.Empty;
        password = password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email and password are required.");
            return View();
        }

        var loginRequest = new LoginRequest { Email = email, Password = password };
        
        try
        {
            var response = await _client.PostAsJsonAsync("api/Account/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    // Set the JWT token as an HTTP-only secure cookie
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"] ?? "60"))
                    };

                    Response.Cookies.Append("jwt_token", result.Token, cookieOptions);

                    TempData["Success"] = "Logged in successfully!";
                    return Redirect(returnUrl ?? "/");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials. Please try again.");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Authentication service is currently unavailable. Please try again later.");
        }

        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt_token");
        TempData["Success"] = "Logged out successfully.";
        return RedirectToAction(nameof(Login));
    }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
}
