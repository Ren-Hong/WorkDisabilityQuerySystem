namespace WorkDisabilityQuerySystem.Application.Dtos;

public sealed record LookupOption(string Id, string Label, string Detail);

public sealed record OccupationLookupOption(string Id, string Label, string Detail, int GroupNumber);
