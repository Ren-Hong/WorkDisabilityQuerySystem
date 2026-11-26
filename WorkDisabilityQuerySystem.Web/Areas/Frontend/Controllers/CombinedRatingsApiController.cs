using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;

namespace WorkDisabilityQuerySystem.Web.Areas.Frontend.Controllers;

[Area("Frontend")]
[Authorize]
[Route("api/frontend/combined-ratings")]
public sealed class CombinedRatingsApiController(ICombinedRatingsService ratingsService) : Controller
{
    [HttpPost("calculate")]
    [ValidateAntiForgeryToken]
    public IActionResult Calculate([FromForm] string? ratings)
    {
        var steps = ratingsService.Calculate(ratings);
        if (steps is null)
            return BadRequest(new { message = "請輸入至少兩個 0–100 的整數，以逗號分隔。" });

        return Ok(new { combined = steps[^1].Combined, steps });
    }
}
