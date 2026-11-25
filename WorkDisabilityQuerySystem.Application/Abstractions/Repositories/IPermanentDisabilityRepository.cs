using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Application.Abstractions.Repositories;

public interface IPermanentDisabilityRepository
{
    Task<IReadOnlyList<LookupOption>> SearchImpairmentsAsync(string term, CancellationToken token);
    Task<IReadOnlyList<LookupOption>> SearchOccupationsAsync(string term, CancellationToken token);
    Task<LookupOption?> FindImpairmentAsync(string code, CancellationToken token);
    Task<LookupOption?> FindOccupationAsync(Guid id, CancellationToken token);
    Task<RatingResult?> FindRatingAsync(RatingRequest request, string variantCode, CancellationToken token);
}
