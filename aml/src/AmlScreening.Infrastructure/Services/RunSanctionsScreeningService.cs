using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class RunSanctionsScreeningService : IRunSanctionsScreeningService
{
    private const int ThresholdConfirmedMatch = 90;
    private const int ThresholdPossibleMatch = 70;

    private const string StatusClear = "Clear";
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string StatusConfirmedMatch = "ConfirmedMatch";

    private const string MatchTypeFullName = "FullName";
    private const string MatchTypeFirstName = "FirstName";
    private const string MatchTypeLastName = "LastName";
    private const string MatchTypeNationality = "Nationality";
    private const string MatchTypeDateOfBirth = "DateOfBirth";
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

        // Group by list source; for each list add all entries with score >= 70 (all matches)
        var byList = entries.GroupBy(e => e.ListSource);
        foreach (var group in byList)
        {
            var listName = group.Key;

            foreach (var entry in group)
            {
                var (score, matchType) = ComputeMatchScore(
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

                if (score < ThresholdPossibleMatch)
                    continue;

                var status = GetStatusFromScore(score);
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

    private static (double score, string matchType) ComputeMatchScore(
        string customerFullName,
        string customerFirstName,
        string customerLastName,
        string customerNationality,
        DateTime? customerDob,
        string entryFullName,
        string entryFirstName,
        string entrySecondName,
        string entryNationality,
        DateTime? entryDob)
    {
        double fullNameScore = 0;
        if (customerFullName.Length > 0 && entryFullName.Length > 0)
            fullNameScore = Fuzz.TokenSetRatio(customerFullName, entryFullName);

        double firstNameScore = 0;
        if (customerFirstName.Length > 0 && entryFirstName.Length > 0)
            firstNameScore = Fuzz.TokenSetRatio(customerFirstName, entryFirstName);

        double lastNameScore = 0;
        if (customerLastName.Length > 0 && entrySecondName.Length > 0)
            lastNameScore = Fuzz.TokenSetRatio(customerLastName, entrySecondName);

        // Name component: max of fullName vs average of first+last when available
        var nameScore = fullNameScore;
        if (firstNameScore > 0 || lastNameScore > 0)
        {
            var firstLastAvg = (firstNameScore + lastNameScore) / 2.0;
            if (firstNameScore == 0) firstLastAvg = lastNameScore;
            else if (lastNameScore == 0) firstLastAvg = firstNameScore;
            nameScore = Math.Max(fullNameScore, firstLastAvg);
        }

        double nationalityScore = 0;
        if (customerNationality.Length > 0 && entryNationality.Length > 0)
            nationalityScore = Fuzz.TokenSetRatio(customerNationality, entryNationality);

        double dobScore = 0;
        if (customerDob.HasValue && entryDob.HasValue)
            dobScore = customerDob.Value.Date == entryDob.Value.Date ? 100 : 0;

        // Weighted combination: name 50%, nationality 30%, DOB 20%
        var weights = 0.0;
        var total = 0.0;
        if (customerFullName.Length > 0 || entryFullName.Length > 0 || customerFirstName.Length > 0 || entryFirstName.Length > 0 || customerLastName.Length > 0 || entrySecondName.Length > 0)
        { weights += 0.5; total += 0.5 * nameScore; }
        if (customerNationality.Length > 0 || entryNationality.Length > 0) { weights += 0.3; total += 0.3 * nationalityScore; }
        if (customerDob.HasValue || entryDob.HasValue) { weights += 0.2; total += 0.2 * dobScore; }

        var score = weights > 0 ? total / weights : 0;

        // Primary match type is the field with highest contribution
        var matchType = MatchTypeFullName;
        if (firstNameScore >= nameScore && firstNameScore >= lastNameScore && firstNameScore >= nationalityScore && firstNameScore >= dobScore)
            matchType = MatchTypeFirstName;
        else if (lastNameScore >= nameScore && lastNameScore >= nationalityScore && lastNameScore >= dobScore)
            matchType = MatchTypeLastName;
        else if (nationalityScore >= nameScore && nationalityScore >= dobScore)
            matchType = MatchTypeNationality;
        else if (dobScore >= nameScore && dobScore >= nationalityScore)
            matchType = MatchTypeDateOfBirth;

        return (Math.Round(score, 2), matchType);
    }

    private static string GetStatusFromScore(double score)
    {
        if (score >= ThresholdConfirmedMatch) return StatusConfirmedMatch;
        if (score >= ThresholdPossibleMatch) return StatusPossibleMatch;
        return StatusClear;
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
