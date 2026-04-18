using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Cases;

namespace AmlScreening.Application.Interfaces;

public class OnboardingSubmissionRequest
{
    public string? Notes { get; set; }
}

public class OnboardingSubmissionResultDto
{
    public Guid CaseId { get; set; }
    public string CaseStatus { get; set; } = string.Empty;
    public int ScreeningResultsCount { get; set; }
    public string? WorstScreeningStatus { get; set; }
    public bool HasConfirmedMatch { get; set; }
    public bool RequiresCheckerApproval { get; set; }
    public CaseDto? Case { get; set; }
}

/// <summary>
/// Final stepper step: bundles the customer + KYC docs + screening results into a Case.
/// Blocks submission if a confirmed sanctions match still has no Approved checker action.
/// </summary>
public interface IOnboardingSubmissionService
{
    Task<ApiResponse<OnboardingSubmissionResultDto>> SubmitAsync(
        Guid customerId,
        OnboardingSubmissionRequest request,
        CancellationToken cancellationToken = default);
}
