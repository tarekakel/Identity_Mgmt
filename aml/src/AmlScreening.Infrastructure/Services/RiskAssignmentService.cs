using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.RiskAssignment;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class RiskAssignmentService : IRiskAssignmentService
{
    private readonly ApplicationDbContext _context;

    public RiskAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<RiskAssignmentDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.RiskAssignments.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(r =>
                (r.RiskLevel != null && r.RiskLevel.ToLower().Contains(term)));
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
        var paged = new PagedResult<RiskAssignmentDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<RiskAssignmentDto>>.Ok(paged);
    }

    public async Task<ApiResponse<RiskAssignmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RiskAssignments.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<RiskAssignmentDto>.Fail("Risk assignment not found.");
        return ApiResponse<RiskAssignmentDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<RiskAssignmentDto>> CreateAsync(CreateRiskAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new RiskAssignment
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            CountryRisk = dto.CountryRisk,
            CustomerTypeRisk = dto.CustomerTypeRisk,
            PepRisk = dto.PepRisk,
            TransactionRisk = dto.TransactionRisk,
            IndustryRisk = dto.IndustryRisk,
            TotalScore = dto.TotalScore,
            RiskLevel = dto.RiskLevel
        };
        _context.RiskAssignments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<RiskAssignmentDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<RiskAssignmentDto>> UpdateAsync(Guid id, UpdateRiskAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RiskAssignments.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<RiskAssignmentDto>.Fail("Risk assignment not found.");
        entity.CountryRisk = dto.CountryRisk;
        entity.CustomerTypeRisk = dto.CustomerTypeRisk;
        entity.PepRisk = dto.PepRisk;
        entity.TransactionRisk = dto.TransactionRisk;
        entity.IndustryRisk = dto.IndustryRisk;
        entity.TotalScore = dto.TotalScore;
        entity.RiskLevel = dto.RiskLevel;
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<RiskAssignmentDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RiskAssignments.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse.Fail("Risk assignment not found.");
        if (entity is ISoftDelete softDelete)
            softDelete.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok("Risk assignment deleted.");
    }

    private static IQueryable<RiskAssignment> ApplySort(IQueryable<RiskAssignment> query, string? sortBy, bool sortDescending)
    {
        var isDesc = sortDescending;
        return sortBy?.ToLowerInvariant() switch
        {
            "risklevel" => isDesc ? query.OrderByDescending(r => r.RiskLevel) : query.OrderBy(r => r.RiskLevel),
            "createdat" or "assignedat" => isDesc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };
    }

    private static RiskAssignmentDto MapToDto(RiskAssignment r) => new()
    {
        Id = r.Id,
        CustomerId = r.CustomerId,
        CountryRisk = r.CountryRisk,
        CustomerTypeRisk = r.CustomerTypeRisk,
        PepRisk = r.PepRisk,
        TransactionRisk = r.TransactionRisk,
        IndustryRisk = r.IndustryRisk,
        TotalScore = r.TotalScore,
        RiskLevel = r.RiskLevel,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        CreatedBy = r.CreatedBy,
        UpdatedBy = r.UpdatedBy,
        IsActive = r.IsActive,
        IsDeleted = r.IsDeleted
    };
}
