using System.Security.Claims;
using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace WorkDisabilityQuerySystem.Web.Security;

public sealed class AccountCookieEvents(IAccountRepository? accounts = null) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return;
        }

        if (accounts is null ||
            context.Principal?.Identity is not ClaimsIdentity identity ||
            !Guid.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var accountId))
        {
            await RejectAsync(context);
            return;
        }

        var account = await accounts.FindPresentationAsync(accountId, context.HttpContext.RequestAborted);
        if (account is null)
        {
            await RejectAsync(context);
            return;
        }

        var displayName = account.DisplayName ?? "未設定姓名";
        if (!context.Principal.IsInRole(account.Level) ||
            identity.FindFirst(ClaimTypes.GivenName)?.Value != displayName)
        {
            var updatedIdentity = new ClaimsIdentity(identity);
            foreach (var role in updatedIdentity.FindAll(updatedIdentity.RoleClaimType).ToArray())
            {
                updatedIdentity.RemoveClaim(role);
            }

            updatedIdentity.AddClaim(new Claim(updatedIdentity.RoleClaimType, account.Level));
            foreach (var claim in updatedIdentity.FindAll(ClaimTypes.GivenName).ToArray())
            {
                updatedIdentity.RemoveClaim(claim);
            }

            updatedIdentity.AddClaim(new Claim(ClaimTypes.GivenName, displayName));
            context.ReplacePrincipal(new ClaimsPrincipal(updatedIdentity));
            context.ShouldRenew = true;
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(context.Scheme.Name);
    }
}
