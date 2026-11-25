using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkDisabilityQuerySystem.Web.Areas.Backend.Controllers;

[Area("Backend")]
[Authorize(Policy = "BackendAccess")]
public sealed class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Accounts");
}
