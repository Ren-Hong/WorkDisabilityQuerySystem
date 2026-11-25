using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkDisabilityQuerySystem.Web.Areas.Frontend.Models;

namespace WorkDisabilityQuerySystem.Web.Areas.Frontend.Controllers;

[Area("Frontend")]
[Authorize]
public sealed class PermanentDisabilityController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new RatingInput());
}
