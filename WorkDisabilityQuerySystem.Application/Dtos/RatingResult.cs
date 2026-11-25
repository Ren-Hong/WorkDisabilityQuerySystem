namespace WorkDisabilityQuerySystem.Application.Dtos;

public sealed record RatingResult(int FecRank, int FecAdjusted, int GroupNumber, string Variant, int OccupationAdjusted, int Pd);
