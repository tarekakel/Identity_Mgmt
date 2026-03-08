using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Cases;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class CaseService : ICaseService
{
    private readonly ApplicationDbContext _context;

    public CaseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<CaseDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Cases.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                (c.Status != null && c.Status.ToLower().Contains(term)));
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
        var paged = new PagedResult<CaseDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<CaseDto>>.Ok(paged);
    }

    public async Task<ApiResponse<CaseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Cases.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<CaseDto>.Fail("Case not found.");
        return ApiResponse<CaseDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<CaseDto>> CreateAsync(CreateCaseDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Case
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            AlertId = dto.AlertId,
            Status = dto.Status,
            AssignedToId = dto.AssignedToId,
            CreatedByRole = dto.CreatedByRole
        };
        _context.Cases.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<CaseDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<CaseDto>> UpdateAsync(Guid id, UpdateCaseDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Cases.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<CaseDto>.Fail("Case not found.");
        entity.CustomerId = dto.CustomerId;
        entity.AlertId = dto.AlertId;
        entity.Status = dto.Status;
        entity.AssignedToId = dto.AssignedToId;
        entity.CreatedByRole = dto.CreatedByRole;
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<CaseDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Cases.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse.Fail("Case not found.");
        if (entity is ISoftDelete softDelete)
            softDelete.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok("Case deleted.");
    }

    private static IQueryable<Case> ApplySort(IQueryable<Case> query, string? sortBy, bool sortDescending)
    {
        var isDesc = sortDescending;
        return sortBy?.ToLowerInvariant() switch
        {
            "status" => isDesc ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "createdat" => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static CaseDto MapToDto(Case c) => new()
    {
        Id = c.Id,
        CustomerId = c.CustomerId,
        AlertId = c.AlertId,
        CaseNumber = "CASE-" + c.Id.ToString("N")[..8].ToUpperInvariant(),
        Status = c.Status,
        AssignedToId = c.AssignedToId,
        CreatedByRole = c.CreatedByRole,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        CreatedBy = c.CreatedBy,
        UpdatedBy = c.UpdatedBy,
        IsActive = c.IsActive,
        IsDeleted = c.IsDeleted
    };
}
