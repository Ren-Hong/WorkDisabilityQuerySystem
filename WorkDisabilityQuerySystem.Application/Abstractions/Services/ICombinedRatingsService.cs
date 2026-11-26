using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Application.Abstractions.Services;

public interface ICombinedRatingsService
{
    IReadOnlyList<CombinedRatingStep>? Calculate(string? input);
}
