using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class RunSanctionsScreeningService : IRunSanctionsScreeningService
{
    private const int DefaultThreshold = 70;
    private const string StatusConfirmedMatch = "ConfirmedMatch";
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string ReviewStatusPendingReview = "PendingReview";

    private readonly ApplicationDbContext _context;
    private readonly IScreeningEngine _engine;

    public RunSanctionsScreeningService(ApplicationDbContext context, IScreeningEngine engine)
    {
        _context = context;
        _engine = engine;
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunScreeningForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Nationality)
            .Include(c => c.Gender)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer == null)
            return ApiResponse<RunSanctionsScreeningResultDto>.Fail("Customer not found.");

        var fullName = customer.FullName?.Trim() ?? string.Empty;
        var firstName = customer.FirstName?.Trim();
        var lastName = customer.LastName?.Trim();
        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            (firstName, lastName) = SplitFullName(fullName);

        var query = new ScreeningQuery
        {
            FullName = fullName,
            FirstName = firstName,
            LastName = lastName,
            Nationality = customer.Nationality?.Name?.Trim(),
            Gender = customer.Gender?.Name?.Trim(),
            DateOfBirth = customer.DateOfBirth,
            BirthYearRange = customer.DateOfBirth.HasValue ? 2 : (int?)null,
            Threshold0to100 = DefaultThreshold,
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
                CustomerId = customerId,
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
