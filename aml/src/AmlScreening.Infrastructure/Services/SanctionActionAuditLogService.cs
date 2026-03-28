using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class SanctionActionAuditLogService : ISanctionActionAuditLogService
{
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string StatusConfirmedMatch = "ConfirmedMatch";
    private const string ReviewStatusApproved = "Approved";
    private const string ReviewStatusRejected = "Rejected";

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SanctionActionAuditLogService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<IReadOnlyList<SanctionActionAuditLogDto>>> GetAuditLogsAsync(
        Guid? customerId = null,
        Guid? sanctionsScreeningId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SanctionActionAuditLogs
            .AsNoTracking()
            .Include(a => a.SanctionsScreening)
            .ThenInclude(s => s.Customer)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(a => a.SanctionsScreening.CustomerId == customerId.Value);
        if (sanctionsScreeningId.HasValue)
            query = query.Where(a => a.SanctionsScreeningId == sanctionsScreeningId.Value);
        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            query = query.Where(a => a.CreatedAt < end);
        }

        var list = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        var dtos = list.Select(a => new SanctionActionAuditLogDto
        {
            Id = a.Id,
            SanctionsScreeningId = a.SanctionsScreeningId,
            CustomerId = a.SanctionsScreening.CustomerId,
            CustomerName = a.SanctionsScreening.Customer?.FullName,
            MatchedName = a.SanctionsScreening.MatchedName,
            SanctionList = a.SanctionsScreening.ScreeningList,
            Action = a.Action,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt,
            CreatedBy = a.CreatedBy,
            UpdatedAt = a.UpdatedAt,
            UpdatedBy = a.UpdatedBy
        }).ToList();

        return ApiResponse<IReadOnlyList<SanctionActionAuditLogDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<SanctionsScreeningResultItemDto>> RecordCheckerActionAsync(
        Guid customerId,
        Guid screeningId,
        RecordSanctionScreeningActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var action = request.Action?.Trim() ?? string.Empty;
        if (action != "Approve" && action != "Reject")
            return ApiResponse<SanctionsScreeningResultItemDto>.Fail("Action must be Approve or Reject.");

        var screening = await _context.SanctionsScreenings
            .FirstOrDefaultAsync(s => s.Id == screeningId && s.CustomerId == customerId, cancellationToken);

        if (screening == null)
            return ApiResponse<SanctionsScreeningResultItemDto>.Fail("Screening result not found.");

        if (screening.Result != StatusPossibleMatch && screening.Result != StatusConfirmedMatch)
            return ApiResponse<SanctionsScreeningResultItemDto>.Fail("Only PossibleMatch or ConfirmedMatch results can be approved or rejected.");

        var reviewStatus = action == "Approve" ? ReviewStatusApproved : ReviewStatusRejected;
        var now = DateTime.UtcNow;
        var user = _currentUserService.GetCurrentUserDisplayName();

        screening.ReviewStatus = reviewStatus;
        screening.ReviewedAt = now;
        screening.ReviewedBy = user;

        var auditLog = new SanctionActionAuditLog
        {
            Id = Guid.NewGuid(),
            SanctionsScreeningId = screeningId,
            Action = action,
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            CreatedBy = user,
            UpdatedAt = now,
            UpdatedBy = user
        };
        _context.SanctionActionAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new SanctionsScreeningResultItemDto
        {
            Id = screening.Id,
            CustomerId = screening.CustomerId,
            CorporateScreeningRequestId = screening.CorporateScreeningRequestId,
            MatchedName = screening.MatchedName,
            SanctionList = screening.ScreeningList,
            MatchScore = screening.Score,
            MatchType = screening.MatchType,
            ScreeningDate = screening.ScreenedAt,
            Status = screening.Result,
            ReviewStatus = screening.ReviewStatus,
            ReviewedAt = screening.ReviewedAt,
            ReviewedBy = screening.ReviewedBy
        };

        return ApiResponse<SanctionsScreeningResultItemDto>.Ok(dto);
    }
}
