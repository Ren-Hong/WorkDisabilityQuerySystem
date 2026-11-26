using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkDisabilityQuerySystem.Web.Areas.Frontend.Controllers;

[Area("Frontend")]
[Authorize]
public sealed class CombinedRatingsController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
