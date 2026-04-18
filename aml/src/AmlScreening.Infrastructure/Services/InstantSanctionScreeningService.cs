using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.InstantSanctionScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class InstantSanctionScreeningService : IInstantSanctionScreeningService
{
    private const int MaxResults = 50;
    private const int DefaultThreshold = 70;

    private readonly ApplicationDbContext _context;
    private readonly IScreeningEngine _engine;

    public InstantSanctionScreeningService(ApplicationDbContext context, IScreeningEngine engine)
    {
        _context = context;
        _engine = engine;
    }

    public async Task<ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>> SearchAsync(
        InstantSanctionScreeningSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        if (fullName.Length < 2)
            return ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>.Fail("Full name must be at least 2 characters.");

        string? nationalityName = null;
        if (request.NationalityId is { } nid && nid != Guid.Empty)
        {
            nationalityName = await _context.Nationalities
                .AsNoTracking()
                .Where(n => n.Id == nid)
                .Select(n => n.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var (firstName, lastName) = SplitFullName(fullName);

        var query = new ScreeningQuery
        {
            FullName = fullName,
            FirstName = firstName,
            LastName = lastName,
            Nationality = nationalityName?.Trim(),
            DateOfBirth = request.DateOfBirth,
            BirthYearRange = request.DateOfBirth.HasValue ? 2 : (int?)null,
            Threshold0to100 = DefaultThreshold,
            Top = MaxResults
        };

        var candidates = await _engine.SearchAsync(query, cancellationToken);
        if (candidates.Count == 0)
            return ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>.Ok(Array.Empty<InstantSanctionScreeningResultItemDto>());

        // Hydrate display fields from the DB by entry IDs (one round-trip)
        var ids = candidates.Where(c => c.EntryId != Guid.Empty).Select(c => c.EntryId).ToList();
        var entries = await _context.SanctionListEntries
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var ordered = new List<InstantSanctionScreeningResultItemDto>();
        foreach (var c in candidates)
        {
            entries.TryGetValue(c.EntryId, out var entry);

            var uidKey = entry?.DataId is { Length: > 0 } d
                ? d
                : entry?.ReferenceNumber is { Length: > 0 } r
                    ? r
                    : c.EntryId.ToString("N")[..8];

            ordered.Add(new InstantSanctionScreeningResultItemDto
            {
                MatchScore = (decimal)Math.Round(c.NormalizedScore0to100, 2),
                CustomerId = null,
                Uid = $"{c.ListSource}-{uidKey}",
                EntryType = entry?.EntryType ?? entry?.TypeDetail,
                Name = c.MatchedAlias ?? c.FullName,
                NationalityOrCountry = c.Nationality,
                DateOfBirth = c.DateOfBirth,
                IdNumber = entry?.DocumentNumber ?? entry?.ReferenceNumber,
                Source = c.ListSource,
                CreatedOn = entry?.ListedOn ?? entry?.LastDayUpdated,
                Remarks = entry?.Comments ?? entry?.OtherInformation
            });
        }

        return ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>.Ok(ordered);
    }

    private static (string? First, string? Last) SplitFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return (null, null);
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            0 => (null, null),
            1 => (parts[0], null),
            _ => (parts[0], parts[1])
        };
    }
}
