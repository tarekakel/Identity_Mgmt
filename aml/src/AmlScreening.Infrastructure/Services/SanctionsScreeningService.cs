using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class SanctionsScreeningService : ISanctionsScreeningService
{
    private readonly ApplicationDbContext _context;

    public SanctionsScreeningService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<SanctionsScreeningDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.SanctionsScreenings.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(s =>
                (s.Result != null && s.Result.ToLower().Contains(term)) ||
                (s.ScreeningList != null && s.ScreeningList.ToLower().Contains(term)) ||
                (s.MatchedName != null && s.MatchedName.ToLower().Contains(term)));
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
        var paged = new PagedResult<SanctionsScreeningDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<SanctionsScreeningDto>>.Ok(paged);
    }

    public async Task<ApiResponse<SanctionsScreeningDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SanctionsScreenings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<SanctionsScreeningDto>.Fail("Sanctions screening not found.");
        return ApiResponse<SanctionsScreeningDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<SanctionsScreeningDto>> CreateAsync(CreateSanctionsScreeningDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new SanctionsScreening
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            ScreeningList = dto.ScreeningList,
            Result = dto.Result,
            MatchedName = dto.MatchedName,
            Score = dto.Score,
            ScreenedAt = dto.ScreenedAt
        };
        _context.SanctionsScreenings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<SanctionsScreeningDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<SanctionsScreeningDto>> UpdateAsync(Guid id, UpdateSanctionsScreeningDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SanctionsScreenings.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<SanctionsScreeningDto>.Fail("Sanctions screening not found.");
        entity.ScreeningList = dto.ScreeningList;
        entity.Result = dto.Result;
        entity.MatchedName = dto.MatchedName;
        entity.Score = dto.Score;
        entity.ScreenedAt = dto.ScreenedAt;
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<SanctionsScreeningDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SanctionsScreenings.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse.Fail("Sanctions screening not found.");
        if (entity is ISoftDelete softDelete)
            softDelete.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok("Sanctions screening deleted.");
    }

    private static IQueryable<SanctionsScreening> ApplySort(IQueryable<SanctionsScreening> query, string? sortBy, bool sortDescending)
    {
        var isDesc = sortDescending;
        return sortBy?.ToLowerInvariant() switch
        {
            "result" or "status" => isDesc ? query.OrderByDescending(s => s.Result) : query.OrderBy(s => s.Result),
            "screenedat" => isDesc ? query.OrderByDescending(s => s.ScreenedAt) : query.OrderBy(s => s.ScreenedAt),
            _ => query.OrderByDescending(s => s.ScreenedAt)
        };
    }

    private static SanctionsScreeningDto MapToDto(SanctionsScreening s) => new()
    {
        Id = s.Id,
        CustomerId = s.CustomerId,
        ScreeningList = s.ScreeningList,
        Result = s.Result,
        MatchedName = s.MatchedName,
        Score = s.Score,
        ScreenedAt = s.ScreenedAt,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        CreatedBy = s.CreatedBy,
        UpdatedBy = s.UpdatedBy,
        IsActive = s.IsActive,
        IsDeleted = s.IsDeleted
    };
}
