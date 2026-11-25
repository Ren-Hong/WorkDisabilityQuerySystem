using WorkDisabilityQuerySystem.Application.Dtos;
using WorkDisabilityQuerySystem.Domain.Accounts;

namespace WorkDisabilityQuerySystem.Application.Abstractions.Repositories;

/// <summary>查詢及更新登入帳號資料。</summary>
public interface IAccountRepository
{
    Task<IReadOnlyList<AccountSummary>> ListAsync(CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid actorId, Guid accountId, CancellationToken cancellationToken);

    Task<bool> UpdateProfileAsync(Guid actorId, Guid accountId, string level, string? displayName, CancellationToken cancellationToken);

    Task<bool> CreateAsync(Account account, string? displayName, CancellationToken cancellationToken);
    /// <summary>依登入名稱查詢帳號資料，查無帳號時回傳 null。</summary>
    Task<Account?> FindByUsernameAsync(string username, CancellationToken cancellationToken);


    /// <summary>依帳號識別碼查詢目前權限等級，查無帳號時回傳 null。</summary>
    Task<AccountPresentation?> FindPresentationAsync(Guid accountId, CancellationToken cancellationToken);


    /// <summary>記錄登入成功時間，回傳是否成功更新帳號資料。</summary>
    Task<bool> RecordSuccessfulLoginAsync(Account account, CancellationToken cancellationToken);
}

public sealed record AccountPresentation(string Level, string? DisplayName);

