namespace WorkDisabilityQuerySystem.Application.Dtos;

public sealed record AccountSummary(Guid AccountId, string Username, string? DisplayName, string Level);
