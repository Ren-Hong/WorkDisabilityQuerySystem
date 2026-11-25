namespace WorkDisabilityQuerySystem.Domain.Accounts;

public sealed record Account(
    Guid AccountId,
    string Username,
    string PasswordHash,
    string Level)
{
    public const string UserLevel = "使用者";
    public const string AdministratorLevel = "管理員";
}
