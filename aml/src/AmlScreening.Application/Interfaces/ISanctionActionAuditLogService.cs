using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;

namespace AmlScreening.Application.Interfaces;

public interface ISanctionActionAuditLogService
{
    /// <summary>
    /// Get sanction action audit log entries with optional filters.
    /// </summary>
    Task<ApiResponse<IReadOnlyList<SanctionActionAuditLogDto>>> GetAuditLogsAsync(
        Guid? customerId = null,
        Guid? sanctionsScreeningId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a checker action (Approve/Reject) on a screening result and insert audit log.
    /// </summary>
    Task<ApiResponse<SanctionsScreeningResultItemDto>> RecordCheckerActionAsync(
        Guid customerId,
        Guid screeningId,
        RecordSanctionScreeningActionRequest request,
        CancellationToken cancellationToken = default);
}
