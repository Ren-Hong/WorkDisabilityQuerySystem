using System.Globalization;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;
using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Application.Services;

public sealed class CombinedRatingsService : ICombinedRatingsService
{
    public IReadOnlyList<CombinedRatingStep>? Calculate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var entries = input.Split(',', '，');
        if (entries.Length < 2)
            return null;

        var ratings = new int[entries.Length];
        for (var index = 0; index < entries.Length; index++)
        {
            if (!int.TryParse(entries[index].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out ratings[index]) ||
                ratings[index] is < 0 or > 100)
                return null;
        }

        Array.Sort(ratings);
        var steps = new List<CombinedRatingStep>(ratings.Length - 1);
        var current = ratings[^1];
        for (var index = ratings.Length - 2; index >= 0; index--)
        {
            var next = ratings[index];
            var combined = (int)Math.Round(current + (100 - current) * next / 100m, MidpointRounding.AwayFromZero);
            steps.Add(new(current, next, combined));
            current = combined;
        }

        return steps;
    }
}
