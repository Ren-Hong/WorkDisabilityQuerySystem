using System.Text.Json;
using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Infrastructure.Repositories;

public sealed class JsonPermanentDisabilityRepository(string path) : IPermanentDisabilityRepository
{
    private readonly LookupData _data = JsonSerializer.Deserialize<LookupData>(File.ReadAllText(path))
        ?? throw new InvalidDataException("查表 JSON 沒有資料。");

    public Task<IReadOnlyList<LookupOption>> SearchImpairmentsAsync(string term, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<LookupOption>>(_data.Impairments
            .Where(row => Contains(row.Code, term) || Contains(row.English, term) || Contains(row.Chinese, term))
            .OrderByDescending(row => Equal(row.Code, term))
            .Take(20).Select(ToOption).ToArray());
    }

    public Task<IReadOnlyList<LookupOption>> SearchOccupationsAsync(string term, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<LookupOption>>(_data.Occupations
            .Where(row => Contains(row.English, term) || Contains(row.Chinese, term) ||
                          Contains(row.Industry, term) || row.GroupNumber.ToString() == term)
            .OrderByDescending(row => Equal(row.English, term) || Equal(row.Chinese, term))
            .Take(20).Select(ToOption).ToArray());
    }

    public Task<LookupOption?> FindImpairmentAsync(string code, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var row = _data.Impairments.SingleOrDefault(item => Equal(item.Code, code));
        return Task.FromResult(row is null ? null : ToOption(row));
    }

    public Task<LookupOption?> FindOccupationAsync(Guid id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var row = _data.Occupations.SingleOrDefault(item => item.Id == id);
        return Task.FromResult(row is null ? null : ToOption(row));
    }

    public Task<RatingResult?> FindRatingAsync(RatingRequest request, string variantCode, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var result =
            from injury in _data.Impairments
            where Equal(injury.Code, request.ImpairmentCode)
            from occupation in _data.Occupations
            where occupation.Id == request.OccupationId
            join fec in _data.Fec on (injury.Rank, request.Wpi) equals (fec.Rank, fec.Wpi)
            join variant in _data.Variants
                on (variantCode.ToUpperInvariant(), occupation.GroupNumber) equals
                   (variant.Code.ToUpperInvariant(), variant.GroupNumber)
            join adjustment in _data.OccupationAdjustments
                on (fec.Adjusted, variant.Variant) equals (adjustment.FecAdjusted, adjustment.Variant)
            from band in _data.AgeBands
            where (band.Minimum is null || request.Age >= band.Minimum) &&
                  (band.Maximum is null || request.Age <= band.Maximum)
            join age in _data.AgeAdjustments
                on (adjustment.Adjusted, band.Number) equals (age.OccupationAdjusted, age.BandNumber)
            select new RatingResult(injury.Rank, fec.Adjusted, occupation.GroupNumber,
                variant.Variant, adjustment.Adjusted, age.Pd);
        return Task.FromResult(result.SingleOrDefault());
    }

    private static bool Contains(string? value, string term) =>
        value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;

    private static bool Equal(string? value, string term) =>
        string.Equals(value, term, StringComparison.OrdinalIgnoreCase);

    private static LookupOption ToOption(Impairment row) =>
        new(row.Code, string.IsNullOrEmpty(row.Chinese) ? row.English : row.Chinese, $"{row.Code} · FEC {row.Rank}");

    private static LookupOption ToOption(Occupation row) =>
        new(row.Id.ToString(), string.IsNullOrEmpty(row.Chinese) ? row.English : row.Chinese,
            $"{row.Industry} · 群組 {row.GroupNumber}");

    private sealed record LookupData(Impairment[] Impairments, Occupation[] Occupations, FecRow[] Fec,
        VariantRow[] Variants, OccupationAdjustment[] OccupationAdjustments, AgeBand[] AgeBands,
        AgeAdjustment[] AgeAdjustments);
    private sealed record Impairment(string Code, string English, string? Chinese, int Rank);
    private sealed record Occupation(Guid Id, string English, string? Chinese, string Industry, int GroupNumber);
    private sealed record FecRow(int Wpi, int Rank, int Adjusted);
    private sealed record VariantRow(string Code, int GroupNumber, string Variant);
    private sealed record OccupationAdjustment(int FecAdjusted, string Variant, int Adjusted);
    private sealed record AgeBand(int Number, int? Minimum, int? Maximum);
    private sealed record AgeAdjustment(int OccupationAdjusted, int BandNumber, int Pd);
}
