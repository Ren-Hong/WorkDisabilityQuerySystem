using Dapper;
using Microsoft.Data.SqlClient;
using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using WorkDisabilityQuerySystem.Application.Dtos;

namespace WorkDisabilityQuerySystem.Infrastructure.Repositories;

public sealed class PermanentDisabilityRepository(string? connectionString) : IPermanentDisabilityRepository
{
    private readonly string _connectionString = connectionString ??
        throw new InvalidOperationException("請設定 ConnectionStrings:WdqsDatabase。");

    public async Task<IReadOnlyList<LookupOption>> SearchImpairmentsAsync(string term, CancellationToken token)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            """
            SELECT VCHIMPAIRMENTCODE AS Id,
                CASE WHEN NULLIF(NVCHDESCRIPTIONZH, N'') IS NULL THEN NVCHDESCRIPTIONEN
                     ELSE CONCAT(NVCHDESCRIPTIONZH, N' · ', NVCHDESCRIPTIONEN) END AS Label,
                CONCAT(VCHIMPAIRMENTCODE, N' · FEC ', INTFECRANK) AS Detail
            FROM dbo.WDQSIMPAIRMENTTBL
            WHERE @Term = N'' OR CHARINDEX(@Term, VCHIMPAIRMENTCODE) > 0 OR CHARINDEX(@Term, NVCHDESCRIPTIONEN) > 0
               OR CHARINDEX(@Term, NVCHDESCRIPTIONZH) > 0
            ORDER BY CASE WHEN VCHIMPAIRMENTCODE = @Term THEN 0 ELSE 1 END, VCHIMPAIRMENTCODE;
            """,
            new { Term = term },
            cancellationToken: token);
        var results = await connection.QueryAsync<LookupOption>(command);
        return results.AsList();
    }

    public async Task<IReadOnlyList<OccupationLookupOption>> SearchOccupationsAsync(string term, CancellationToken token)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            """
            SELECT CONVERT(varchar(36), UIDOCCUPATIONID) AS Id,
                CASE WHEN NULLIF(NVCHOCCUPATIONNAMEZH, N'') IS NULL THEN NVCHOCCUPATIONNAMEEN
                     ELSE CONCAT(NVCHOCCUPATIONNAMEZH, N' · ', NVCHOCCUPATIONNAMEEN) END AS Label,
                COALESCE(NVCHINDUSTRYEN, N'') AS Detail, INTOCCGROUPNO AS GroupNumber
            FROM dbo.WDQSOCCUPATIONTBL
            WHERE @Term = N'' OR CHARINDEX(@Term, NVCHOCCUPATIONNAMEEN) > 0 OR CHARINDEX(@Term, NVCHOCCUPATIONNAMEZH) > 0
               OR CHARINDEX(@Term, NVCHINDUSTRYEN) > 0 OR CONVERT(varchar(12), INTOCCGROUPNO) = @Term
            ORDER BY CASE WHEN NVCHOCCUPATIONNAMEEN = @Term OR NVCHOCCUPATIONNAMEZH = @Term THEN 0 ELSE 1 END,
                     NVCHOCCUPATIONNAMEEN, NVCHINDUSTRYEN, INTOCCGROUPNO, UIDOCCUPATIONID;
            """,
            new { Term = term },
            cancellationToken: token);
        var results = await connection.QueryAsync<OccupationLookupOption>(command);
        return results.AsList();
    }

    public async Task<LookupOption?> FindImpairmentAsync(string code, CancellationToken token)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            """
            SELECT TOP (20) VCHIMPAIRMENTCODE AS Id,
                COALESCE(NULLIF(NVCHDESCRIPTIONZH, N''), NVCHDESCRIPTIONEN) AS Label,
                CONCAT(VCHIMPAIRMENTCODE, N' · FEC ', INTFECRANK) AS Detail
            FROM dbo.WDQSIMPAIRMENTTBL
            WHERE VCHIMPAIRMENTCODE = @Id;
            """,
            new { Id = code },
            cancellationToken: token);
        return await connection.QuerySingleOrDefaultAsync<LookupOption>(command);
    }

    public async Task<LookupOption?> FindOccupationAsync(Guid id, CancellationToken token)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            """
            SELECT TOP (20) CONVERT(varchar(36), UIDOCCUPATIONID) AS Id,
                COALESCE(NULLIF(NVCHOCCUPATIONNAMEZH, N''), NVCHOCCUPATIONNAMEEN) AS Label,
                CONCAT(NVCHINDUSTRYEN, N' · 群組 ', INTOCCGROUPNO) AS Detail
            FROM dbo.WDQSOCCUPATIONTBL
            WHERE UIDOCCUPATIONID = @Id;
            """,
            new { Id = id },
            cancellationToken: token);
        return await connection.QuerySingleOrDefaultAsync<LookupOption>(command);
    }

    public async Task<RatingResult?> FindRatingAsync(RatingRequest request, string variantCode, CancellationToken token)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            """
            SELECT i.INTFECRANK AS FecRank, f.INTFECADJPCT AS FecAdjusted,
                   o.INTOCCGROUPNO AS GroupNumber, v.CHROCCVARIANT AS Variant,
                   a.INTOCCADJPCT AS OccupationAdjusted, d.INTPDPCT AS Pd
            FROM dbo.WDQSIMPAIRMENTTBL i
            JOIN dbo.WDQSFECADJTBL f ON f.INTFECRANK = i.INTFECRANK AND f.INTWPIPCT = @Wpi
            JOIN dbo.WDQSOCCUPATIONTBL o ON o.UIDOCCUPATIONID = @OccupationId
            JOIN dbo.WDQSOCCVARIANTTBL v ON v.VCHIMPAIRMENTCODE = @VariantCode AND v.INTOCCGROUPNO = o.INTOCCGROUPNO
            JOIN dbo.WDQSOCCADJTBL a ON a.INTFECADJPCT = f.INTFECADJPCT AND a.CHROCCVARIANT = v.CHROCCVARIANT
            JOIN dbo.WDQSAGEBANDTBL b ON (b.INTMINAGE IS NULL OR @Age >= b.INTMINAGE)
                                      AND (b.INTMAXAGE IS NULL OR @Age <= b.INTMAXAGE)
            JOIN dbo.WDQSAGEADJTBL d ON d.INTOCCADJPCT = a.INTOCCADJPCT AND d.INTAGEBANDNO = b.INTAGEBANDNO
            WHERE i.VCHIMPAIRMENTCODE = @ImpairmentCode;
            """,
            new
            {
                request.Wpi,
                request.OccupationId,
                request.Age,
                request.ImpairmentCode,
                VariantCode = variantCode
            },
            cancellationToken: token);
        return await connection.QuerySingleOrDefaultAsync<RatingResult>(command);
    }
}
