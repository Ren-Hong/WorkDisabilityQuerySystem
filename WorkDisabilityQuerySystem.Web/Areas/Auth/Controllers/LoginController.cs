using System.Security.Claims;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;
using WorkDisabilityQuerySystem.Web.Areas.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WorkDisabilityQuerySystem.Web.Areas.Auth.Controllers;

[Area("Auth")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class LoginController(IAccountService accounts) : Controller
{
    [HttpGet("/Login")]
    [AllowAnonymous]
    public IActionResult Index(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterLogin(returnUrl);
        }

        return View(new LoginInput { ReturnUrl = returnUrl });
    }

    [HttpPost("/Login")]
    [EnableRateLimiting("Login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var account = await accounts.AuthenticateAsync(
            input.Username,
            input.Password,
            cancellationToken);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "帳號或密碼錯誤。");
            return View(input);
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Role, account.Level)
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectAfterLogin(input.ReturnUrl);
    }

    [HttpPost("/Logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home", new { area = "Frontend" });
    }
}

