using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.AuditLogs;

namespace AmlScreening.Application.Interfaces;

public interface IAuditLogService
{
    Task<ApiResponse<PagedResult<AuditLogDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuditLogDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
