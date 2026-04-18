using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Cases;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class OnboardingSubmissionService : IOnboardingSubmissionService
{
    private const string CaseStatusPendingReview = "PendingReview";
    private const string CaseStatusOpen = "Open";

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public OnboardingSubmissionService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<OnboardingSubmissionResultDto>> SubmitAsync(
        Guid customerId,
        OnboardingSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        if (customer == null)
            return ApiResponse<OnboardingSubmissionResultDto>.Fail("Customer not found.");

        var screeningRows = await _context.SanctionsScreenings
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId)
            .Select(s => new { s.Result, s.ReviewStatus })
            .ToListAsync(cancellationToken);

        var hasConfirmedMatch = screeningRows.Any(r => r.Result == "ConfirmedMatch");
        var anyPossibleMatch = screeningRows.Any(r => r.Result == "PossibleMatch");
        var hasUnresolvedConfirmed = screeningRows.Any(r =>
            r.Result == "ConfirmedMatch" && r.ReviewStatus != "Approved");

        if (hasUnresolvedConfirmed)
        {
            return ApiResponse<OnboardingSubmissionResultDto>.Fail(
                "Cannot submit: a confirmed sanctions match still requires checker approval.");
        }

        var worstStatus = hasConfirmedMatch
            ? "ConfirmedMatch"
            : (anyPossibleMatch ? "PossibleMatch" : "Clear");

        var caseStatus = (hasConfirmedMatch || anyPossibleMatch)
            ? CaseStatusPendingReview
            : CaseStatusOpen;

        var createdBy = _currentUser.GetCurrentUserDisplayName();
        var caseEntity = new Case
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = caseStatus,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            IsActive = true
        };
        _context.Cases.Add(caseEntity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new OnboardingSubmissionResultDto
        {
            CaseId = caseEntity.Id,
            CaseStatus = caseEntity.Status,
            ScreeningResultsCount = screeningRows.Count,
            WorstScreeningStatus = worstStatus,
            HasConfirmedMatch = hasConfirmedMatch,
            RequiresCheckerApproval = false,
            Case = new CaseDto
            {
                Id = caseEntity.Id,
                CustomerId = caseEntity.CustomerId,
                Status = caseEntity.Status,
                CreatedAt = caseEntity.CreatedAt,
                CreatedBy = caseEntity.CreatedBy,
                IsActive = caseEntity.IsActive
            }
        };

        return ApiResponse<OnboardingSubmissionResultDto>.Ok(dto);
    }
}
