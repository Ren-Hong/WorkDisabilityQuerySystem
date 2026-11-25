namespace WorkDisabilityQuerySystem.Application.Abstractions.Security;

public interface IPasswordVerifier
{
    bool Verify(string password, string passwordHash);
    string Hash(string password);
}