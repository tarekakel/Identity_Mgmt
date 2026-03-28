using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class RunSanctionsScreeningService : IRunSanctionsScreeningService
{
    private const string StatusConfirmedMatch = "ConfirmedMatch";
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string ReviewStatusPendingReview = "PendingReview";

    private readonly ApplicationDbContext _context;

    public RunSanctionsScreeningService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunScreeningForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Nationality)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer == null)
            return ApiResponse<RunSanctionsScreeningResultDto>.Fail("Customer not found.");

        var entries = await _context.SanctionListEntries
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var customerFullName = customer.FullName?.Trim() ?? string.Empty;
        var customerFirstName = customer.FirstName?.Trim() ?? string.Empty;
        var customerLastName = customer.LastName?.Trim() ?? string.Empty;
        var customerNationality = customer.Nationality?.Name?.Trim() ?? string.Empty;
        var customerDob = customer.DateOfBirth;

        var screenedAt = DateTime.UtcNow;
        var results = new List<SanctionsScreeningResultItemDto>();
        var hasConfirmedMatch = false;

        var byList = entries.GroupBy(e => e.ListSource);
        foreach (var group in byList)
        {
            var listName = group.Key;

            foreach (var entry in group)
            {
                var (score, matchType) = SanctionListMatchScoring.ComputeMatchScore(
                    customerFullName,
                    customerFirstName,
                    customerLastName,
                    customerNationality,
                    customerDob,
                    entry.FullName?.Trim() ?? string.Empty,
                    entry.FirstName?.Trim() ?? string.Empty,
                    entry.SecondName?.Trim() ?? string.Empty,
                    entry.Nationality?.Trim() ?? string.Empty,
                    entry.DateOfBirth);

                if (score < SanctionListMatchScoring.ThresholdPossibleMatch)
                    continue;

                var status = SanctionListMatchScoring.GetStatusFromScore(score);
                if (status == StatusConfirmedMatch)
                    hasConfirmedMatch = true;

                var reviewStatus = (status == StatusPossibleMatch || status == StatusConfirmedMatch)
                    ? ReviewStatusPendingReview
                    : null;

                var entity = new SanctionsScreening
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    ScreeningList = listName,
                    Result = status,
                    MatchedName = entry.FullName,
                    MatchType = matchType,
                    Score = (decimal)Math.Round(score, 2),
                    ScreenedAt = screenedAt,
                    ReviewStatus = reviewStatus
                };
                _context.SanctionsScreenings.Add(entity);
                results.Add(MapToResultItem(entity));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var sortedResults = results.OrderByDescending(r => r.MatchScore).ToList();

        return ApiResponse<RunSanctionsScreeningResultDto>.Ok(new RunSanctionsScreeningResultDto
        {
            Results = sortedResults,
            HasConfirmedMatch = hasConfirmedMatch
        });
    }

    public async Task<ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>> GetResultsForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await _context.SanctionsScreenings
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.ScreenedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(s => new SanctionsScreeningResultItemDto
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
        }).ToList();

        return ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>.Ok(dtos);
    }

    private static SanctionsScreeningResultItemDto MapToResultItem(SanctionsScreening s) => new()
    {
        Id = s.Id,
        CustomerId = s.CustomerId,
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
