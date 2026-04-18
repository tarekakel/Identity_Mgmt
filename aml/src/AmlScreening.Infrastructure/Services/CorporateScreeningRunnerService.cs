using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;

namespace AmlScreening.Infrastructure.Services;

public class CorporateScreeningRunnerService : ICorporateScreeningRunnerService
{
    private const string StatusConfirmedMatch = "ConfirmedMatch";
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string ReviewStatusPendingReview = "PendingReview";
    private const string MatchTypeCorporateName = "CorporateName";
    private const string MatchTypeShareholder = "Shareholder";

    private readonly ApplicationDbContext _context;
    private readonly IScreeningEngine _engine;

    public CorporateScreeningRunnerService(ApplicationDbContext context, IScreeningEngine engine)
    {
        _context = context;
        _engine = engine;
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(CorporateScreeningRequest request, CancellationToken cancellationToken = default)
    {
        var threshold = Math.Clamp(request.MatchThreshold, 0, 100);
        var listSources = ListSourceSelector.GetSelectedSources(request);

        var screenedAt = DateTime.UtcNow;
        var results = new List<SanctionsScreeningResultItemDto>();
        var hasConfirmedMatch = false;

        // Company name screening
        var companyName = request.FullName?.Trim() ?? string.Empty;
        if (companyName.Length > 0)
        {
            var companyQuery = new ScreeningQuery
            {
                FullName = companyName,
                ListSources = listSources,
                Threshold0to100 = threshold,
                Top = 50
            };
            var companyHits = await _engine.SearchAsync(companyQuery, cancellationToken);
            foreach (var c in companyHits)
            {
                if (c.Status == StatusConfirmedMatch) hasConfirmedMatch = true;
                results.Add(PersistAndMap(request, c, MatchTypeCorporateName, screenedAt));
            }
        }

        // Shareholders - run in parallel queries
        var shareholderTasks = request.Shareholders
            .Where(sh => !string.IsNullOrWhiteSpace(sh.FullName))
            .Select(async sh =>
            {
                var q = new ScreeningQuery
                {
                    FullName = sh.FullName.Trim(),
                    Nationality = sh.Nationality?.Name?.Trim(),
                    DateOfBirth = sh.DateOfBirth,
                    ListSources = listSources,
                    Threshold0to100 = threshold,
                    Top = 25
                };
                return await _engine.SearchAsync(q, cancellationToken);
            })
            .ToList();

        var shareholderResults = await Task.WhenAll(shareholderTasks);
        foreach (var hits in shareholderResults)
        {
            foreach (var c in hits)
            {
                if (c.Status == StatusConfirmedMatch) hasConfirmedMatch = true;
                results.Add(PersistAndMap(request, c, MatchTypeShareholder, screenedAt));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<RunSanctionsScreeningResultDto>.Ok(new RunSanctionsScreeningResultDto
        {
            Results = results.OrderByDescending(r => r.MatchScore).ToList(),
            HasConfirmedMatch = hasConfirmedMatch
        });
    }

    private SanctionsScreeningResultItemDto PersistAndMap(
        CorporateScreeningRequest request,
        ScreeningCandidate c,
        string matchTypeOverride,
        DateTime screenedAt)
    {
        var reviewStatus = (c.Status == StatusPossibleMatch || c.Status == StatusConfirmedMatch)
            ? ReviewStatusPendingReview
            : null;

        var entity = new SanctionsScreening
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            CorporateScreeningRequestId = request.Id,
            ScreeningList = c.ListSource,
            Result = c.Status,
            MatchedName = c.MatchedAlias ?? c.FullName,
            MatchType = matchTypeOverride,
            Score = (decimal)Math.Round(c.NormalizedScore0to100, 2),
            ScreenedAt = screenedAt,
            ReviewStatus = reviewStatus
        };

        _context.SanctionsScreenings.Add(entity);
        return MapToResultItem(entity);
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
