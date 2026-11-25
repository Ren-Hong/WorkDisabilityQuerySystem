using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkDisabilityQuerySystem.Web.Models;

namespace WorkDisabilityQuerySystem.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Home", new { area = "Frontend" });

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}