using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.AuditLogs;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<AuditLogDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(a =>
                (a.Action != null && a.Action.ToLower().Contains(term)) ||
                (a.EntityType != null && a.EntityType.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        query = ApplySort(query, request.SortBy, request.SortDescending);

        var pageNumber = request.GetPageNumber();
        var pageSize = request.GetPageSize();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        var paged = new PagedResult<AuditLogDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<AuditLogDto>>.Ok(paged);
    }

    public async Task<ApiResponse<AuditLogDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.AuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<AuditLogDto>.Fail("Audit log not found.");
        return ApiResponse<AuditLogDto>.Ok(MapToDto(entity));
    }

    private static IQueryable<AuditLog> ApplySort(IQueryable<AuditLog> query, string? sortBy, bool sortDescending)
    {
        var isDesc = sortDescending;
        return sortBy?.ToLowerInvariant() switch
        {
            "action" => isDesc ? query.OrderByDescending(a => a.Action) : query.OrderBy(a => a.Action),
            "entitytype" => isDesc ? query.OrderByDescending(a => a.EntityType) : query.OrderBy(a => a.EntityType),
            "timestamp" or "createdat" => isDesc ? query.OrderByDescending(a => a.Timestamp) : query.OrderBy(a => a.Timestamp),
            _ => query.OrderByDescending(a => a.Timestamp)
        };
    }

    private static AuditLogDto MapToDto(AuditLog a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        Action = a.Action,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        Timestamp = a.Timestamp,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        CreatedBy = a.CreatedBy,
        UpdatedBy = a.UpdatedBy,
        IsActive = a.IsActive,
        IsDeleted = a.IsDeleted
    };
}
