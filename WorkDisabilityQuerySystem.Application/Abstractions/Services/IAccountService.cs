using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Application.Abstractions.Services;

/// <summary>處理帳號驗證與建立。</summary>
public interface IAccountService
{
    /// <summary>驗證帳號密碼並記錄登入成功時間，驗證失敗時回傳 null。</summary>
    Task<AuthenticatedAccount?> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AccountSummary>> ListAsync(CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid actorId, Guid accountId, CancellationToken cancellationToken);

    Task<bool> UpdateProfileAsync(Guid actorId, Guid accountId, string level, string? displayName, CancellationToken cancellationToken);

    /// <summary>建立帳號，帳號名稱已存在時回傳 false。</summary>
    Task<bool> CreateAsync(string username, string password, string level, string? displayName, CancellationToken cancellationToken);
}
