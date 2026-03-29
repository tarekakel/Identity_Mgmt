using System.Text.Json;
using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateKyc;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class CorporateKycService : ICorporateKycService
{
    private readonly ApplicationDbContext _context;

    public CorporateKycService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CorporateKycDto>> GetActiveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CorporateKyc
            .AsNoTracking()
            .Where(k => k.CustomerId == customerId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return new ApiResponse<CorporateKycDto> { Success = true, Data = null };

        return ApiResponse<CorporateKycDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<CorporateKycDto>> CreateActiveAsync(
        Guid customerId,
        UpsertCorporateKycRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
        if (!customerExists)
            return ApiResponse<CorporateKycDto>.Fail("Customer not found.");

        var previousActive = await _context.CorporateKyc
            .Where(k => k.CustomerId == customerId && k.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var prev in previousActive)
            prev.IsActive = false;

        var json = "{}";
        if (dto.FormPayload.HasValue)
            json = dto.FormPayload.Value.GetRawText();

        var entity = new CorporateKyc
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            CustomerId = customerId,
            FormPayload = string.IsNullOrWhiteSpace(json) ? "{}" : json,
            IsActive = true
        };

        _context.CorporateKyc.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CorporateKycDto>.Ok(MapToDto(entity));
    }

    private static CorporateKycDto MapToDto(CorporateKyc k)
    {
        JsonElement? payload = null;
        if (!string.IsNullOrWhiteSpace(k.FormPayload))
        {
            using var doc = JsonDocument.Parse(k.FormPayload);
            payload = doc.RootElement.Clone();
        }

        return new CorporateKycDto
        {
            Id = k.Id,
            TenantId = k.TenantId,
            CustomerId = k.CustomerId,
            IsActive = k.IsActive,
            IsDeleted = k.IsDeleted,
            FormPayload = payload
        };
    }
}
