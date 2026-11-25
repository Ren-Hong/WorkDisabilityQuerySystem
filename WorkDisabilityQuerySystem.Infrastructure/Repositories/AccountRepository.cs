using Dapper;
using Microsoft.Data.SqlClient;
using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using WorkDisabilityQuerySystem.Application.Dtos;
using WorkDisabilityQuerySystem.Domain.Accounts;

namespace WorkDisabilityQuerySystem.Infrastructure.Repositories;

public sealed class AccountRepository(string? connectionString) : IAccountRepository
{
    public async Task<Account?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT UIDACCTID AS AccountId, NVCHUSERNAME AS Username,
                   VCHPASSWORDHASH AS PasswordHash, NVCHLEVEL AS [Level]
            FROM dbo.WDQSACCTBL
            WHERE NVCHUSERNAME = @Username;
            """;
        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Account>(
            new CommandDefinition(sql, new { Username = username }, cancellationToken: cancellationToken));
    }

    public async Task<AccountPresentation?> FindPresentationAsync(Guid accountId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT NVCHLEVEL AS [Level], NVCHDISPLAYNAME AS DisplayName
            FROM dbo.WDQSACCTBL WHERE UIDACCTID = @AccountId;
            """;
        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<AccountPresentation>(
            new CommandDefinition(sql, new { AccountId = accountId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> RecordSuccessfulLoginAsync(Account account, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.WDQSACCTBL SET DTMLASTLOGINAT = SYSUTCDATETIME()
            WHERE UIDACCTID = @AccountId AND VCHPASSWORDHASH = @PasswordHash AND NVCHLEVEL = @Level;
            """;
        await using var connection = CreateConnection();
        var parameters = new { account.AccountId, account.Level,
            PasswordHash = new DbString { Value = account.PasswordHash, IsAnsi = true, Length = 255 } };
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<bool> CreateAsync(Account account, string? displayName, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT dbo.WDQSACCTBL (NVCHUSERNAME, VCHPASSWORDHASH, NVCHLEVEL, NVCHDISPLAYNAME)
            VALUES (@Username, @PasswordHash, @Level, @DisplayName);
            """;
        await using var connection = CreateConnection();
        var parameters = new { account.Username, account.PasswordHash, account.Level, DisplayName = displayName };
        try
        {
            return await connection.ExecuteAsync(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)) == 1;
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<AccountSummary>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        var command = new CommandDefinition(
            """
            SELECT UIDACCTID AS AccountId, NVCHUSERNAME AS Username,
                   NVCHDISPLAYNAME AS DisplayName, NVCHLEVEL AS [Level]
            FROM dbo.WDQSACCTBL
            ORDER BY NVCHUSERNAME;
            """,
            cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<AccountSummary>(command);
        return results.AsList();
    }

    public async Task<bool> UpdateProfileAsync(Guid actorId, Guid accountId, string level, string? displayName, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        var command = new CommandDefinition(
            """
            UPDATE target
            SET NVCHLEVEL = @Level, NVCHDISPLAYNAME = @DisplayName, DTMUPDATEDAT = SYSUTCDATETIME()
            FROM dbo.WDQSACCTBL AS target
            WHERE target.UIDACCTID = @AccountId
              AND target.UIDACCTID <> @ActorId
              AND EXISTS (
                  SELECT 1 FROM dbo.WDQSACCTBL AS actor WITH (UPDLOCK, HOLDLOCK)
                  WHERE actor.UIDACCTID = @ActorId AND actor.NVCHLEVEL = @AdminLevel
              );
            """,
            new { ActorId = actorId, AccountId = accountId, Level = level, DisplayName = displayName, AdminLevel = Account.AdministratorLevel },
            cancellationToken: cancellationToken);
        return await connection.ExecuteAsync(command) == 1;
    }

    public async Task<bool> DeleteAsync(Guid actorId, Guid accountId, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        var command = new CommandDefinition(
            """
            DELETE target
            FROM dbo.WDQSACCTBL AS target
            WHERE target.UIDACCTID = @AccountId
              AND target.UIDACCTID <> @ActorId
              AND EXISTS (
                  SELECT 1 FROM dbo.WDQSACCTBL AS actor WITH (UPDLOCK, HOLDLOCK)
                  WHERE actor.UIDACCTID = @ActorId AND actor.NVCHLEVEL = @AdminLevel
              );
            """,
            new { ActorId = actorId, AccountId = accountId, AdminLevel = Account.AdministratorLevel },
            cancellationToken: cancellationToken);
        return await connection.ExecuteAsync(command) == 1;
    }

    private SqlConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("請設定 ConnectionStrings:WdqsDatabase。");
        }

        return new SqlConnection(connectionString);
    }
}
