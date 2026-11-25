using WorkDisabilityQuerySystem.Application.Abstractions.Security;

namespace WorkDisabilityQuerySystem.Infrastructure.Security;

public sealed class BCryptPasswordVerifier : IPasswordVerifier
{
    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}