using Microsoft.AspNetCore.Mvc;

namespace WorkDisabilityQuerySystem.Web.Areas.Auth.Controllers;

[Area("Auth")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AccessDeniedController : Controller
{
    [HttpGet("/AccessDenied")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Index()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
