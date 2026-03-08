using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Tenants;
using IdentityManagement.Application.Interfaces;
using IdentityManagement.Application.Mappings;
using IdentityManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<TenantDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { id }, cancellationToken);
        if (tenant == null)
            return ApiResponse<TenantDto>.Fail("Tenant not found.");
        return ApiResponse<TenantDto>.Ok(tenant.ToDto());
    }

    public async Task<ApiResponse<PagedResult<TenantDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(t => (t.Name != null && t.Name.ToLower().Contains(term))
                || (t.Code != null && t.Code.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "code" => request.SortDescending ? query.OrderByDescending(t => t.Code) : query.OrderBy(t => t.Code),
            "createdat" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderBy(t => t.Name)
        };

        var pageNumber = request.GetPageNumber();
        var pageSize = request.GetPageSize();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(t => t.ToDto()).ToList();
        var paged = new PagedResult<TenantDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<TenantDto>>.Ok(paged);
    }

    public async Task<ApiResponse<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _context.Tenants.AnyAsync(t => t.Code == code, cancellationToken))
            return ApiResponse<TenantDto>.Fail("A tenant with this code already exists.");

        var tenant = request.ToEntity();
        _context.Tenants.Add(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<TenantDto>.Ok(tenant.ToDto());
    }

    public async Task<ApiResponse<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { id }, cancellationToken);
        if (tenant == null)
            return ApiResponse<TenantDto>.Fail("Tenant not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (code != tenant.Code && await _context.Tenants.AnyAsync(t => t.Code == code, cancellationToken))
            return ApiResponse<TenantDto>.Fail("A tenant with this code already exists.");

        tenant.ApplyUpdate(request);
        _context.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<TenantDto>.Ok(tenant.ToDto());
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { id }, cancellationToken);
        if (tenant == null)
            return ApiResponse.Fail("Tenant not found.");

        _context.Tenants.Remove(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }
}
