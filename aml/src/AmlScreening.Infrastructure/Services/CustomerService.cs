using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Customers;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<CustomerDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                (c.FullName != null && c.FullName.ToLower().Contains(term)) ||
                (c.NationalIdOrPassport != null && c.NationalIdOrPassport.ToLower().Contains(term)) ||
                (c.RiskClassification != null && c.RiskClassification.ToLower().Contains(term)));
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
        var paged = new PagedResult<CustomerDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<CustomerDto>>.Ok(paged);
    }

    public async Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<CustomerDto>.Fail("Customer not found.");
        return ApiResponse<CustomerDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            FullName = dto.FullName,
            NationalIdOrPassport = dto.NationalIdOrPassport,
            DateOfBirth = dto.DateOfBirth,
            Nationality = dto.Nationality,
            Address = dto.Address,
            Occupation = dto.Occupation,
            SourceOfFunds = dto.SourceOfFunds,
            IsPep = dto.IsPep,
            BusinessActivity = dto.BusinessActivity,
            RiskClassification = dto.RiskClassification
        };
        _context.Customers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<CustomerDto>.Fail("Customer not found.");
        entity.FullName = dto.FullName;
        entity.NationalIdOrPassport = dto.NationalIdOrPassport;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Nationality = dto.Nationality;
        entity.Address = dto.Address;
        entity.Occupation = dto.Occupation;
        entity.SourceOfFunds = dto.SourceOfFunds;
        entity.IsPep = dto.IsPep;
        entity.BusinessActivity = dto.BusinessActivity;
        entity.RiskClassification = dto.RiskClassification;
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse.Fail("Customer not found.");
        if (entity is ISoftDelete softDelete)
            softDelete.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok("Customer deleted.");
    }

    private static IQueryable<Customer> ApplySort(IQueryable<Customer> query, string? sortBy, bool sortDescending)
    {
        var isDesc = sortDescending;
        return sortBy?.ToLowerInvariant() switch
        {
            "fullname" => isDesc ? query.OrderByDescending(c => c.FullName) : query.OrderBy(c => c.FullName),
            "riskclassification" => isDesc ? query.OrderByDescending(c => c.RiskClassification) : query.OrderBy(c => c.RiskClassification),
            "createdat" => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        FullName = c.FullName,
        NationalIdOrPassport = c.NationalIdOrPassport,
        DateOfBirth = c.DateOfBirth,
        Nationality = c.Nationality,
        Address = c.Address,
        Occupation = c.Occupation,
        SourceOfFunds = c.SourceOfFunds,
        IsPep = c.IsPep,
        BusinessActivity = c.BusinessActivity,
        RiskClassification = c.RiskClassification,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        CreatedBy = c.CreatedBy,
        UpdatedBy = c.UpdatedBy,
        IsActive = c.IsActive,
        IsDeleted = c.IsDeleted
    };
}
