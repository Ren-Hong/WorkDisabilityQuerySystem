namespace WorkDisabilityQuerySystem.Application.Dtos;

public sealed record AuthenticatedAccount(Guid AccountId, string Username, string Level);
