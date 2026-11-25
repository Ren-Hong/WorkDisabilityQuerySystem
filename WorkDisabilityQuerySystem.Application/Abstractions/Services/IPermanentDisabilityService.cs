using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Application.Abstractions.Services;

public interface IPermanentDisabilityService
{
    Task<IReadOnlyList<LookupOption>> SearchImpairmentsAsync(string? term, CancellationToken token);
    Task<IReadOnlyList<LookupOption>> SearchOccupationsAsync(string? term, CancellationToken token);
    Task<RatingCalculation> CalculateAsync(RatingRequest request, CancellationToken token);
}
