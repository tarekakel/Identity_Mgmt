using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;

namespace AmlScreening.Infrastructure.Services;

public class IndividualScreeningRunnerService : IIndividualScreeningRunnerService
{
    private const string StatusConfirmedMatch = "ConfirmedMatch";
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string ReviewStatusPendingReview = "PendingReview";

    private readonly ApplicationDbContext _context;
    private readonly IScreeningEngine _engine;

    public IndividualScreeningRunnerService(ApplicationDbContext context, IScreeningEngine engine)
    {
        _context = context;
        _engine = engine;
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(IndividualScreeningRequest request, CancellationToken cancellationToken = default)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var (firstName, lastName) = SplitFullName(fullName);

        var query = new ScreeningQuery
        {
            FullName = fullName,
            FirstName = firstName,
            LastName = lastName,
            Nationality = request.Nationality?.Name?.Trim(),
            PlaceOfBirthCountry = request.PlaceOfBirthCountry?.Name?.Trim(),
            Gender = request.Gender?.Name?.Trim(),
            DateOfBirth = request.DateOfBirth,
            BirthYearRange = request.BirthYearRange ?? 2,
            ListSources = ListSourceSelector.GetSelectedSources(request),
            Threshold0to100 = Math.Clamp(request.MatchThreshold, 0, 100),
            Top = 50
        };

        var candidates = await _engine.SearchAsync(query, cancellationToken);

        var screenedAt = DateTime.UtcNow;
        var results = new List<SanctionsScreeningResultItemDto>();
        var hasConfirmedMatch = false;

        foreach (var c in candidates)
        {
            if (c.Status == StatusConfirmedMatch) hasConfirmedMatch = true;

            var reviewStatus = (c.Status == StatusPossibleMatch || c.Status == StatusConfirmedMatch)
                ? ReviewStatusPendingReview
                : null;

            var entity = new SanctionsScreening
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                ScreeningList = c.ListSource,
                Result = c.Status,
                MatchedName = c.MatchedAlias ?? c.FullName,
                MatchType = c.MatchType,
                Score = (decimal)Math.Round(c.NormalizedScore0to100, 2),
                ScreenedAt = screenedAt,
                ReviewStatus = reviewStatus
            };

            _context.SanctionsScreenings.Add(entity);
            results.Add(MapToResultItem(entity));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<RunSanctionsScreeningResultDto>.Ok(new RunSanctionsScreeningResultDto
        {
            Results = results.OrderByDescending(r => r.MatchScore).ToList(),
            HasConfirmedMatch = hasConfirmedMatch
        });
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

    private static SanctionsScreeningResultItemDto MapToResultItem(SanctionsScreening s) => new()
    {
        Id = s.Id,
        CustomerId = s.CustomerId,
        CorporateScreeningRequestId = s.CorporateScreeningRequestId,
        MatchedName = s.MatchedName,
        SanctionList = s.ScreeningList,
        MatchScore = s.Score,
        MatchType = s.MatchType,
        ScreeningDate = s.ScreenedAt,
        Status = s.Result,
        ReviewStatus = s.ReviewStatus,
        ReviewedAt = s.ReviewedAt,
        ReviewedBy = s.ReviewedBy
    };
}
