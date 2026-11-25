using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;
using WorkDisabilityQuerySystem.Application.Dtos;
using WorkDisabilityQuerySystem.Web.Areas.Frontend.Models;

namespace WorkDisabilityQuerySystem.Web.Areas.Frontend.Controllers;

[Area("Frontend")]
[Authorize]
[Route("api/frontend/permanent-disability")]
public sealed class PermanentDisabilityApiController(IPermanentDisabilityService ratings) : Controller
{
    [HttpPost("calculate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate([FromForm] RatingInput input, CancellationToken token)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var request = new RatingRequest(input.ImpairmentCode!, input.OccupationId!.Value,
            input.Wpi!.Value, input.Age!.Value, input.AffectedFunction);
        var calculation = await ratings.CalculateAsync(request, token);

        foreach (var error in calculation.Errors)
            ModelState.AddModelError(error.Field, error.Message);

        return ModelState.IsValid ? Ok(calculation.Result) : ValidationProblem(ModelState);
    }

    [HttpGet("impairments")]
    public async Task<IActionResult> Impairments(string? term, CancellationToken token) =>
        Json(await ratings.SearchImpairmentsAsync(term, token));

    [HttpGet("occupations")]
    public async Task<IActionResult> Occupations(string? term, CancellationToken token) =>
        Json(await ratings.SearchOccupationsAsync(term, token));
}

