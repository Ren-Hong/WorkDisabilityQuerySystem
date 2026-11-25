namespace WorkDisabilityQuerySystem.Application.Dtos;

public sealed record RatingCalculation(RatingResult? Result, IReadOnlyList<RatingValidationError> Errors);
