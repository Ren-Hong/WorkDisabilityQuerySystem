using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;
using WorkDisabilityQuerySystem.Web.Areas.Backend.Models;

namespace WorkDisabilityQuerySystem.Web.Areas.Backend.Controllers;

[Area("Backend")]
[Authorize(Policy = "BackendAccess")]
[Route("api/backend/accounts")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AccountsApiController(IAccountService accounts) : Controller
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken token) => Ok(await accounts.ListAsync(token));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateAccountInput input, CancellationToken token)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "請確認帳號、密碼、姓名與角色。" });

        try
        {
            return await accounts.CreateAsync(input.Username, input.Password, input.Level, input.DisplayName, token)
                ? Ok(new { message = "帳號已建立。" })
                : Conflict(new { message = "帳號名稱已存在。" });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{accountId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid accountId, CancellationToken token)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId))
            return Forbid();

        try
        {
            return await accounts.DeleteAsync(actorId, accountId, token)
                ? Ok(new { message = "帳號已刪除。" })
                : Conflict(new { message = "無法刪除帳號，請重新整理列表。" });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateAccountProfileInput input, CancellationToken token)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "請確認帳號、姓名與角色。" });

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId))
            return Forbid();

        try
        {
            return await accounts.UpdateProfileAsync(actorId, input.AccountId!.Value, input.Level, input.DisplayName, token)
                ? Ok(new { message = "帳號已更新。" })
                : Conflict(new { message = "無法更新帳號，請重新整理帳號列表。" });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
