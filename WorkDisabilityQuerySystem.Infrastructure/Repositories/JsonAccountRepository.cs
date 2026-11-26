using System.Text.Json;
using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using WorkDisabilityQuerySystem.Application.Dtos;
using WorkDisabilityQuerySystem.Domain.Accounts;

namespace WorkDisabilityQuerySystem.Infrastructure.Repositories;

public sealed class JsonAccountRepository(string path, string seedPath) : IAccountRepository
{
    private readonly SemaphoreSlim _gate = new(1);

    public Task<Account?> FindByUsernameAsync(string username, CancellationToken cancellationToken) =>
        WithAccountsAsync(rows => rows.Find(row => SameUsername(row.Username, username))?.ToAccount(), cancellationToken);

    public Task<AccountPresentation?> FindPresentationAsync(Guid accountId, CancellationToken cancellationToken) =>
        WithAccountsAsync(rows =>
        {
            var row = rows.Find(item => item.AccountId == accountId);
            return row is null ? null : new AccountPresentation(row.Level, row.DisplayName);
        }, cancellationToken);

    public Task<bool> RecordSuccessfulLoginAsync(Account account, CancellationToken cancellationToken) =>
        ChangeAsync(rows =>
        {
            int index = rows.FindIndex(row => row.ToAccount() == account);
            if (index < 0)
            {
                return false;
            }

            rows[index] = rows[index] with { LastLoginAt = DateTime.UtcNow };
            return true;
        }, cancellationToken);

    public Task<bool> CreateAsync(Account account, string? displayName, CancellationToken cancellationToken) =>
        ChangeAsync(rows =>
        {
            if (rows.Exists(row => SameUsername(row.Username, account.Username)))
            {
                return false;
            }

            rows.Add(new AccountRow(Guid.NewGuid(), account.Username, account.PasswordHash, account.Level,
                displayName, DateTime.UtcNow, null, null));
            return true;
        }, cancellationToken);

    public Task<IReadOnlyList<AccountSummary>> ListAsync(CancellationToken cancellationToken) =>
        WithAccountsAsync<IReadOnlyList<AccountSummary>>(rows => rows
            .OrderBy(row => row.Username, StringComparer.OrdinalIgnoreCase)
            .Select(row => new AccountSummary(row.AccountId, row.Username, row.DisplayName, row.Level))
            .ToArray(), cancellationToken);

    public Task<bool> UpdateProfileAsync(Guid actorId, Guid accountId, string level, string? displayName,
        CancellationToken cancellationToken) =>
        ChangeAsync(rows =>
        {
            int index = FindEditableIndex(rows, actorId, accountId);
            if (index < 0)
            {
                return false;
            }

            rows[index] = rows[index] with { Level = level, DisplayName = displayName, UpdatedAt = DateTime.UtcNow };
            return true;
        }, cancellationToken);

    public Task<bool> DeleteAsync(Guid actorId, Guid accountId, CancellationToken cancellationToken) =>
        ChangeAsync(rows =>
        {
            int index = FindEditableIndex(rows, actorId, accountId);
            if (index < 0)
            {
                return false;
            }

            rows.RemoveAt(index);
            return true;
        }, cancellationToken);

    private async Task<T> WithAccountsAsync<T>(Func<List<AccountRow>, T> action, CancellationToken token)
    {
        await _gate.WaitAsync(token);
        try
        {
            return action(await ReadAsync(token));
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<bool> ChangeAsync(Func<List<AccountRow>, bool> change, CancellationToken token)
    {
        await _gate.WaitAsync(token);
        try
        {
            var rows = await ReadAsync(token);
            if (!change(rows))
            {
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temp, JsonSerializer.Serialize(rows), token);
                File.Move(temp, path, overwrite: true);
            }
            finally
            {
                File.Delete(temp);
            }

            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<AccountRow>> ReadAsync(CancellationToken token)
    {
        var json = await File.ReadAllTextAsync(File.Exists(path) ? path : seedPath, token);
        return JsonSerializer.Deserialize<List<AccountRow>>(json)
            ?? throw new InvalidDataException("帳號 JSON 沒有資料。");
    }

    private static int FindEditableIndex(List<AccountRow> rows, Guid actorId, Guid accountId) =>
        actorId != accountId && rows.Exists(row => row.AccountId == actorId && row.Level == Account.AdministratorLevel)
            ? rows.FindIndex(row => row.AccountId == accountId)
            : -1;

    private static bool SameUsername(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private sealed record AccountRow(Guid AccountId, string Username, string PasswordHash, string Level,
        string? DisplayName, DateTime CreatedAt, DateTime? UpdatedAt, DateTime? LastLoginAt)
    {
        public Account ToAccount() => new(AccountId, Username, PasswordHash, Level);
    }
}
