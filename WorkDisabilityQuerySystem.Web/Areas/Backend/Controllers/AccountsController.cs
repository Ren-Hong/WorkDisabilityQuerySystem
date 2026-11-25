using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkDisabilityQuerySystem.Web.Areas.Backend.Controllers;

[Area("Backend")]
[Authorize(Policy = "BackendAccess")]
public sealed class AccountsController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
