using System.Text;
using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using WorkDisabilityQuerySystem.Application.Abstractions.Security;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;
using WorkDisabilityQuerySystem.Application.Dtos;
using WorkDisabilityQuerySystem.Domain.Accounts;

namespace WorkDisabilityQuerySystem.Application.Services;

public sealed class AccountService(
    IAccountRepository accounts,
    IPasswordVerifier passwordVerifier) : IAccountService
{
    public async Task<AuthenticatedAccount?> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedUsername = username.Trim();
        if (normalizedUsername.Length == 0 ||
            Encoding.UTF8.GetByteCount(password) > 72)
        {
            return null;
        }

        var account = await accounts.FindByUsernameAsync(normalizedUsername, cancellationToken);
        if (account is null || !passwordVerifier.Verify(password, account.PasswordHash))
        {
            return null;
        }

        if (!await accounts.RecordSuccessfulLoginAsync(account, cancellationToken))
        {
            return null;
        }

        return new AuthenticatedAccount(account.AccountId, account.Username, account.Level);
    }

    public Task<IReadOnlyList<AccountSummary>> ListAsync(CancellationToken cancellationToken) =>
        accounts.ListAsync(cancellationToken);

    public Task<bool> DeleteAsync(Guid actorId, Guid accountId, CancellationToken cancellationToken)
    {
        if (actorId == accountId)
            throw new ArgumentException("無法刪除目前登入的帳號。");

        return accounts.DeleteAsync(actorId, accountId, cancellationToken);
    }

    public Task<bool> UpdateProfileAsync(Guid actorId, Guid accountId, string level, string? displayName, CancellationToken cancellationToken)
    {
        if (actorId == accountId)
            throw new ArgumentException("無法編輯目前登入的帳號。");

        if (level is not (Account.AdministratorLevel or Account.UserLevel))
            throw new ArgumentException("帳號角色須為管理員或使用者。");

        return accounts.UpdateProfileAsync(actorId, accountId, level, NormalizeDisplayName(displayName), cancellationToken);
    }

    public Task<bool> CreateAsync(string username, string password, string level, string? displayName, CancellationToken cancellationToken)
    {
        username = username.Trim();
        if (username.Length is < 1 or > 50 || password.Length < 12 ||
            Encoding.UTF8.GetByteCount(password) > 72)
        {
            throw new ArgumentException("帳號須為 1–50 個字元，密碼至少 12 個字元且最多 72 個 UTF-8 位元組。");
        }

        if (level is not (Account.AdministratorLevel or Account.UserLevel))
        {
            throw new ArgumentException("帳號角色須為管理員或使用者。");
        }

        var account = new Account(Guid.Empty, username, passwordVerifier.Hash(password), level);
        return accounts.CreateAsync(account, NormalizeDisplayName(displayName), cancellationToken);
    }

    private static string? NormalizeDisplayName(string? displayName)
    {
        var name = displayName?.Trim();
        if (name is { Length: > 100 })
            throw new ArgumentException("姓名最多 100 個字元。");

        return string.IsNullOrEmpty(name) ? null : name;
    }
}