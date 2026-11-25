namespace WorkDisabilityQuerySystem.Application.Dtos;

public sealed record RatingRequest(string ImpairmentCode, Guid OccupationId, int Wpi, int Age, string? AffectedFunction);
