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
    private const string PendingScreeningCode = "PendingScreening";

    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<CustomerDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.Status)
            .Include(c => c.CustomerType)
            .Include(c => c.Gender)
            .Include(c => c.Nationality)
            .Include(c => c.CountryOfResidence)
            .Include(c => c.Occupation)
            .Include(c => c.SourceOfFunds)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                (c.FullName != null && c.FullName.ToLower().Contains(term)) ||
                (c.CustomerNumber != null && c.CustomerNumber.ToLower().Contains(term)) ||
                (c.Email != null && c.Email.ToLower().Contains(term)) ||
                (c.NationalIdOrPassport != null && c.NationalIdOrPassport.ToLower().Contains(term)) ||
                (c.RiskLevel != null && c.RiskLevel.ToLower().Contains(term)));
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
        var entity = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Status)
            .Include(c => c.CustomerType)
            .Include(c => c.Gender)
            .Include(c => c.Nationality)
            .Include(c => c.CountryOfResidence)
            .Include(c => c.Occupation)
            .Include(c => c.SourceOfFunds)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<CustomerDto>.Fail("Customer not found.");
        return ApiResponse<CustomerDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var pendingScreening = await _context.CustomerStatuses
            .FirstOrDefaultAsync(s => s.Code == PendingScreeningCode, cancellationToken);
        if (pendingScreening == null)
            return ApiResponse<CustomerDto>.Fail("Customer status 'PendingScreening' is not configured.");

        var customerTypeExists = await _context.CustomerTypes.AnyAsync(t => t.Id == dto.CustomerTypeId, cancellationToken);
        if (!customerTypeExists)
            return ApiResponse<CustomerDto>.Fail("Invalid customer type.");

        var customerNumber = await GenerateCustomerNumberAsync(cancellationToken);

        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            CustomerNumber = customerNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            GenderId = dto.GenderId,
            NationalityId = dto.NationalityId,
            PassportNumber = dto.PassportNumber,
            PassportExpiryDate = dto.PassportExpiryDate,
            NationalId = dto.NationalId,
            CountryOfResidenceId = dto.CountryOfResidenceId,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            OccupationId = dto.OccupationId,
            EmployerName = dto.EmployerName,
            SourceOfFundsId = dto.SourceOfFundsId,
            AnnualIncome = dto.AnnualIncome,
            ExpectedMonthlyTransactionVolume = dto.ExpectedMonthlyTransactionVolume,
            ExpectedMonthlyTransactionValue = dto.ExpectedMonthlyTransactionValue,
            CustomerTypeId = dto.CustomerTypeId,
            AccountPurpose = dto.AccountPurpose,
            StatusId = pendingScreening.Id
        };

        _context.Customers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Status)
            .Include(c => c.CustomerType)
            .Include(c => c.Gender)
            .Include(c => c.Nationality)
            .Include(c => c.CountryOfResidence)
            .Include(c => c.Occupation)
            .Include(c => c.SourceOfFunds)
            .FirstAsync(c => c.Id == entity.Id, cancellationToken);
        return ApiResponse<CustomerDto>.Ok(MapToDto(created));
    }

    public async Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Customers
            .Include(c => c.Status)
            .Include(c => c.CustomerType)
            .Include(c => c.Gender)
            .Include(c => c.Nationality)
            .Include(c => c.CountryOfResidence)
            .Include(c => c.Occupation)
            .Include(c => c.SourceOfFunds)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse<CustomerDto>.Fail("Customer not found.");

        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.FullName = dto.FullName;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.GenderId = dto.GenderId;
        entity.NationalityId = dto.NationalityId;
        entity.PassportNumber = dto.PassportNumber;
        entity.PassportExpiryDate = dto.PassportExpiryDate;
        entity.NationalId = dto.NationalId;
        entity.CountryOfResidenceId = dto.CountryOfResidenceId;
        entity.Email = dto.Email;
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Country = dto.Country;
        entity.OccupationId = dto.OccupationId;
        entity.EmployerName = dto.EmployerName;
        entity.SourceOfFundsId = dto.SourceOfFundsId;
        entity.AnnualIncome = dto.AnnualIncome;
        entity.ExpectedMonthlyTransactionVolume = dto.ExpectedMonthlyTransactionVolume;
        entity.ExpectedMonthlyTransactionValue = dto.ExpectedMonthlyTransactionValue;
        entity.CustomerTypeId = dto.CustomerTypeId;
        entity.AccountPurpose = dto.AccountPurpose;
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

    private async Task<string> GenerateCustomerNumberAsync(CancellationToken cancellationToken)
    {
        var existing = await _context.Customers
            .IgnoreQueryFilters()
            .Where(c => c.CustomerNumber.StartsWith("CUST-"))
            .Select(c => c.CustomerNumber)
            .ToListAsync(cancellationToken);

        var maxNumber = 0;
        foreach (var num in existing)
        {
            if (num.Length > 5 && int.TryParse(num.AsSpan(5), out var n) && n > maxNumber)
                maxNumber = n;
        }

        return $"CUST-{(maxNumber + 1):D6}";
    }

    private static IQueryable<Customer> ApplySort(IQueryable<Customer> query, string? sortBy, bool sortDescending)
    {
        var isDesc = sortDescending;
        return sortBy?.ToLowerInvariant() switch
        {
            "fullname" => isDesc ? query.OrderByDescending(c => c.FullName) : query.OrderBy(c => c.FullName),
            "customernumber" => isDesc ? query.OrderByDescending(c => c.CustomerNumber) : query.OrderBy(c => c.CustomerNumber),
            "risklevel" => isDesc ? query.OrderByDescending(c => c.RiskLevel) : query.OrderBy(c => c.RiskLevel),
            "createdat" => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        CustomerNumber = c.CustomerNumber,
        FirstName = c.FirstName,
        LastName = c.LastName,
        FullName = c.FullName,
        DateOfBirth = c.DateOfBirth,
        GenderId = c.GenderId,
        GenderName = c.Gender?.Name,
        NationalityId = c.NationalityId,
        NationalityName = c.Nationality?.Name,
        PassportNumber = c.PassportNumber,
        PassportExpiryDate = c.PassportExpiryDate,
        NationalId = c.NationalId,
        CountryOfResidenceId = c.CountryOfResidenceId,
        CountryOfResidenceName = c.CountryOfResidence?.Name,
        Email = c.Email,
        Phone = c.Phone,
        Address = c.Address,
        City = c.City,
        Country = c.Country,
        OccupationId = c.OccupationId,
        OccupationName = c.Occupation?.Name,
        EmployerName = c.EmployerName,
        SourceOfFundsId = c.SourceOfFundsId,
        SourceOfFundsName = c.SourceOfFunds?.Name,
        AnnualIncome = c.AnnualIncome,
        ExpectedMonthlyTransactionVolume = c.ExpectedMonthlyTransactionVolume,
        ExpectedMonthlyTransactionValue = c.ExpectedMonthlyTransactionValue,
        CustomerTypeId = c.CustomerTypeId,
        CustomerTypeCode = c.CustomerType?.Code,
        CustomerTypeName = c.CustomerType?.Name,
        AccountPurpose = c.AccountPurpose,
        RiskScore = c.RiskScore,
        RiskLevel = c.RiskLevel,
        StatusId = c.StatusId,
        StatusCode = c.Status?.Code,
        StatusName = c.Status?.Name,
        IsPep = c.IsPep,
        BusinessActivity = c.BusinessActivity,
        NationalIdOrPassport = c.NationalIdOrPassport,
        RiskClassification = c.RiskClassification,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        CreatedBy = c.CreatedBy,
        UpdatedBy = c.UpdatedBy,
        IsActive = c.IsActive,
        IsDeleted = c.IsDeleted
    };
}
