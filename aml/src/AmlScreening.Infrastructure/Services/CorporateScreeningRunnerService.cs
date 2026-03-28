using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class CorporateScreeningRunnerService : ICorporateScreeningRunnerService
{
    private const string StatusClear = "Clear";
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string StatusConfirmedMatch = "ConfirmedMatch";
    private const string ReviewStatusPendingReview = "PendingReview";

    private const string MatchTypeFullName = "FullName";
    private const string MatchTypeNationality = "Nationality";
    private const string MatchTypeDateOfBirth = "DateOfBirth";
    private const string MatchTypeCorporateName = "CorporateName";

    private readonly ApplicationDbContext _context;

    public CorporateScreeningRunnerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(CorporateScreeningRequest request, CancellationToken cancellationToken = default)
    {
        var possibleThreshold = Math.Clamp(request.MatchThreshold, 0, 100);
        var confirmedThreshold = Math.Clamp(possibleThreshold + 15, 0, 100);
        if (confirmedThreshold < possibleThreshold) confirmedThreshold = possibleThreshold;

        var selectedSources = GetSelectedListSources(request);
        var entriesQuery = _context.SanctionListEntries.AsNoTracking();
        if (selectedSources.Count > 0)
            entriesQuery = entriesQuery.Where(e => selectedSources.Contains(e.ListSource));

        var entries = await entriesQuery.ToListAsync(cancellationToken);

        var screenedAt = DateTime.UtcNow;
        var results = new List<SanctionsScreeningResultItemDto>();
        var hasConfirmedMatch = false;

        var byList = entries.GroupBy(e => e.ListSource);
        foreach (var group in byList)
        {
            var listName = group.Key;
            foreach (var entry in group)
            {
                var companyName = request.FullName?.Trim() ?? string.Empty;
                if (companyName.Length > 0)
                {
                    var (scoreCo, _) = ComputeMatchScore(
                        companyName,
                        string.Empty,
                        null,
                        null,
                        entry.FullName?.Trim() ?? string.Empty,
                        entry.Nationality?.Trim() ?? string.Empty,
                        entry.DateOfBirth);

                    if (scoreCo >= possibleThreshold)
                    {
                        var status = GetStatusFromScore(scoreCo, possibleThreshold, confirmedThreshold);
                        if (status == StatusConfirmedMatch)
                            hasConfirmedMatch = true;

                        var reviewStatus = (status == StatusPossibleMatch || status == StatusConfirmedMatch)
                            ? ReviewStatusPendingReview
                            : null;

                        var entity = new SanctionsScreening
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = request.CustomerId,
                            CorporateScreeningRequestId = request.Id,
                            ScreeningList = listName,
                            Result = status,
                            MatchedName = entry.FullName,
                            MatchType = MatchTypeCorporateName,
                            Score = (decimal)Math.Round(scoreCo, 2),
                            ScreenedAt = screenedAt,
                            ReviewStatus = reviewStatus
                        };

                        _context.SanctionsScreenings.Add(entity);
                        results.Add(MapToResultItem(entity));
                    }
                }

                foreach (var sh in request.Shareholders)
                {
                    var shName = sh.FullName?.Trim() ?? string.Empty;
                    if (shName.Length == 0)
                        continue;

                    var shNat = sh.Nationality?.Name?.Trim() ?? string.Empty;
                    var (scoreSh, matchTypeSh) = ComputeMatchScore(
                        shName,
                        shNat,
                        sh.DateOfBirth,
                        null,
                        entry.FullName?.Trim() ?? string.Empty,
                        entry.Nationality?.Trim() ?? string.Empty,
                        entry.DateOfBirth);

                    if (scoreSh < possibleThreshold)
                        continue;

                    var statusSh = GetStatusFromScore(scoreSh, possibleThreshold, confirmedThreshold);
                    if (statusSh == StatusConfirmedMatch)
                        hasConfirmedMatch = true;

                    var reviewSh = (statusSh == StatusPossibleMatch || statusSh == StatusConfirmedMatch)
                        ? ReviewStatusPendingReview
                        : null;

                    var row = new SanctionsScreening
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = request.CustomerId,
                        CorporateScreeningRequestId = request.Id,
                        ScreeningList = listName,
                        Result = statusSh,
                        MatchedName = entry.FullName,
                        MatchType = matchTypeSh,
                        Score = (decimal)Math.Round(scoreSh, 2),
                        ScreenedAt = screenedAt,
                        ReviewStatus = reviewSh
                    };

                    _context.SanctionsScreenings.Add(row);
                    results.Add(MapToResultItem(row));
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<RunSanctionsScreeningResultDto>.Ok(new RunSanctionsScreeningResultDto
        {
            Results = results.OrderByDescending(r => r.MatchScore).ToList(),
            HasConfirmedMatch = hasConfirmedMatch
        });
    }

    private static HashSet<string> GetSelectedListSources(CorporateScreeningRequest request)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (request.CheckSanctions)
        {
            set.Add(SanctionListUploadService.SourceUn);
            set.Add(SanctionListUploadService.SourceUae);
            set.Add(SanctionListUploadService.SourceOfac);
        }

        if (set.Count == 0)
        {
            set.Add(SanctionListUploadService.SourceUn);
            set.Add(SanctionListUploadService.SourceUae);
            set.Add(SanctionListUploadService.SourceOfac);
        }

        return set;
    }

    private static (double score, string matchType) ComputeMatchScore(
        string customerFullName,
        string customerNationality,
        DateTime? customerDob,
        int? birthYearRange,
        string entryFullName,
        string entryNationality,
        DateTime? entryDob)
    {
        double fullNameScore = 0;
        if (customerFullName.Length > 0 && entryFullName.Length > 0)
            fullNameScore = Fuzz.TokenSetRatio(customerFullName, entryFullName);

        double nationalityScore = 0;
        if (customerNationality.Length > 0 && entryNationality.Length > 0)
            nationalityScore = Fuzz.TokenSetRatio(customerNationality, entryNationality);

        double dobScore = 0;
        if (customerDob.HasValue && entryDob.HasValue)
        {
            var yRange = Math.Max(0, birthYearRange ?? 0);
            var yearDiff = Math.Abs(customerDob.Value.Year - entryDob.Value.Year);
            var dateEqual = customerDob.Value.Date == entryDob.Value.Date;

            if (dateEqual) dobScore = 100;
            else if (yRange > 0 && yearDiff <= yRange) dobScore = 80;
            else dobScore = 0;
        }

        var weights = 0.0;
        var total = 0.0;
        if (customerFullName.Length > 0 || entryFullName.Length > 0) { weights += 0.6; total += 0.6 * fullNameScore; }
        if (customerNationality.Length > 0 || entryNationality.Length > 0) { weights += 0.25; total += 0.25 * nationalityScore; }
        if (customerDob.HasValue || entryDob.HasValue) { weights += 0.15; total += 0.15 * dobScore; }

        var score = weights > 0 ? total / weights : 0;

        var matchType = MatchTypeFullName;
        if (nationalityScore >= fullNameScore && nationalityScore >= dobScore) matchType = MatchTypeNationality;
        else if (dobScore >= fullNameScore && dobScore >= nationalityScore) matchType = MatchTypeDateOfBirth;

        return (Math.Round(score, 2), matchType);
    }

    private static string GetStatusFromScore(double score, int possibleThreshold, int confirmedThreshold)
    {
        if (score >= confirmedThreshold) return StatusConfirmedMatch;
        if (score >= possibleThreshold) return StatusPossibleMatch;
        return StatusClear;
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
