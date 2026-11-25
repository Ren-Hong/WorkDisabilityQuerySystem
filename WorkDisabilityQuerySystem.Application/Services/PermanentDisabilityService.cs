using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;
using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Application.Services;

public sealed class PermanentDisabilityService(IPermanentDisabilityRepository repository) : IPermanentDisabilityService
{
    private const string SpinalAccessoryCode = "13.07.08.00";

    public async Task<IReadOnlyList<LookupOption>> SearchImpairmentsAsync(string? term, CancellationToken token) =>
        term is { Length: > 100 }
            ? [] : await repository.SearchImpairmentsAsync(term?.Trim() ?? string.Empty, token);

    public async Task<IReadOnlyList<LookupOption>> SearchOccupationsAsync(string? term, CancellationToken token) =>
        term is { Length: > 100 }
            ? [] : await repository.SearchOccupationsAsync(term?.Trim() ?? string.Empty, token);

    public async Task<RatingCalculation> CalculateAsync(RatingRequest request, CancellationToken token)
    {
        var errors = new List<RatingValidationError>();
        var impairment = await repository.FindImpairmentAsync(request.ImpairmentCode, token);
        if (impairment is null)
            errors.Add(new(nameof(request.ImpairmentCode), "找不到此損傷項目。"));

        var occupation = await repository.FindOccupationAsync(request.OccupationId, token);
        if (occupation is null)
            errors.Add(new(nameof(request.OccupationId), "找不到此職業。"));

        string? occupationalVariantImpairmentCode = request.ImpairmentCode;

        // 這項損傷可能影響吞嚥與說話、頭部轉動，或肩部活動。請依實際受影響的功能，從 11.03.04.00、`15.01.XX.XX` 或 16.02.01.00 查出適用的職業調整字母。
        if (request.ImpairmentCode == SpinalAccessoryCode)
        {
            occupationalVariantImpairmentCode = request.AffectedFunction switch
            {
                "speech" => "11.03.04.00",
                "neck" => "15.01.02.02",
                "shoulder" => "16.02.01.00",
                _ => null
            };
        }

        if (occupationalVariantImpairmentCode is null)
            errors.Add(new(nameof(request.AffectedFunction), "請選擇受影響的功能。"));

        if (errors.Count > 0)
            return new(null, errors);

        var result = await repository.FindRatingAsync(
            request with { ImpairmentCode = impairment!.Id }, occupationalVariantImpairmentCode!, token);

        return result is null
            ? new(null, [new(string.Empty, "這組條件沒有完整的查表資料，請確認選項。")])
            : new(result, []);
    }
}



