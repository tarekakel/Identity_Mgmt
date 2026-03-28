using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.InstantSanctionScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class InstantSanctionScreeningService : IInstantSanctionScreeningService
{
    private const int MaxResults = 200;

    private readonly ApplicationDbContext _context;

    public InstantSanctionScreeningService(ApplicationDbContext context)
    {
        _context = context;
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

        nationalityName = nationalityName?.Trim() ?? string.Empty;

        var (searchFirst, searchLast) = SplitFullName(fullName);

        var entries = await _context.SanctionListEntries
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rows = new List<(double Score, InstantSanctionScreeningResultItemDto Dto)>();

        foreach (var entry in entries)
        {
            var (score, _) = SanctionListMatchScoring.ComputeMatchScore(
                fullName,
                searchFirst,
                searchLast,
                nationalityName,
                request.DateOfBirth,
                entry.FullName?.Trim() ?? string.Empty,
                entry.FirstName?.Trim() ?? string.Empty,
                entry.SecondName?.Trim() ?? string.Empty,
                entry.Nationality?.Trim() ?? string.Empty,
                entry.DateOfBirth);

            if (score < SanctionListMatchScoring.ThresholdPossibleMatch)
                continue;

            var uidKey = !string.IsNullOrWhiteSpace(entry.DataId)
                ? entry.DataId!
                : !string.IsNullOrWhiteSpace(entry.ReferenceNumber)
                    ? entry.ReferenceNumber!
                    : entry.Id.ToString("N")[..8];

            var idNumber = !string.IsNullOrWhiteSpace(entry.DocumentNumber)
                ? entry.DocumentNumber
                : entry.ReferenceNumber;

            var remarks = !string.IsNullOrWhiteSpace(entry.Comments)
                ? entry.Comments
                : entry.OtherInformation;

            var dto = new InstantSanctionScreeningResultItemDto
            {
                MatchScore = (decimal)Math.Round(score, 2),
                CustomerId = null,
                Uid = $"{entry.ListSource}-{uidKey}",
                EntryType = entry.EntryType ?? entry.TypeDetail,
                Name = entry.FullName,
                NationalityOrCountry = entry.Nationality,
                DateOfBirth = entry.DateOfBirth,
                IdNumber = idNumber,
                Source = entry.ListSource,
                CreatedOn = entry.ListedOn ?? entry.LastDayUpdated,
                Remarks = remarks
            };

            rows.Add((score, dto));
        }

        var ordered = rows
            .OrderByDescending(r => r.Score)
            .Take(MaxResults)
            .Select(r => r.Dto)
            .ToList();

        return ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>.Ok(ordered);
    }

    private static (string First, string Last) SplitFullName(string fullName)
    {
        var trimmed = fullName.Trim();
        var i = trimmed.IndexOf(' ');
        if (i <= 0)
            return (trimmed, string.Empty);
        return (trimmed[..i].Trim(), trimmed[(i + 1)..].Trim());
    }
}
