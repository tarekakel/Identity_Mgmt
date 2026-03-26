using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualScreening;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class IndividualScreeningService : IIndividualScreeningService
{
    private readonly ApplicationDbContext _context;
    private readonly IIndividualScreeningRunnerService _runner;

    public IndividualScreeningService(ApplicationDbContext context, IIndividualScreeningRunnerService runner)
    {
        _context = context;
        _runner = runner;
    }

    public async Task<ApiResponse<IndividualScreeningRequestDto>> GetLatestAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await GetLatestEntityAsync(customerId, cancellationToken);
        if (entity == null)
            return ApiResponse<IndividualScreeningRequestDto>.Fail("Individual screening request not found.");
        return ApiResponse<IndividualScreeningRequestDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<IndividualScreeningRequestDto>> UpsertAsync(Guid customerId, UpsertIndividualScreeningRequestDto dto, CancellationToken cancellationToken = default)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
        if (!customerExists)
            return ApiResponse<IndividualScreeningRequestDto>.Fail("Customer not found.");

        var entity = await _context.IndividualScreeningRequests
            .Include(r => r.Nationality)
            .FirstOrDefaultAsync(r => r.CustomerId == customerId && !r.IsDeleted, cancellationToken);

        if (entity == null)
        {
            entity = new IndividualScreeningRequest
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                TenantId = dto.TenantId,
                IsActive = true,
                IsDeleted = false
            };
            _context.IndividualScreeningRequests.Add(entity);
        }

        entity.TenantId = dto.TenantId;
        entity.ReferenceId = dto.ReferenceId?.Trim();
        entity.FullName = dto.FullName?.Trim() ?? string.Empty;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.NationalityId = dto.NationalityId;
        entity.PlaceOfBirthCountryId = dto.PlaceOfBirthCountryId;
        entity.IdType = dto.IdType?.Trim();
        entity.IdNumber = dto.IdNumber?.Trim();
        entity.Address = dto.Address?.Trim();
        entity.GenderId = dto.GenderId;
        entity.MatchThreshold = Math.Clamp(dto.MatchThreshold, 0, 100);
        entity.BirthYearRange = dto.BirthYearRange;

        entity.CheckPepUkOnly = dto.CheckPepUkOnly;
        entity.CheckSanctions = dto.CheckSanctions;
        entity.CheckProfileOfInterest = dto.CheckProfileOfInterest;
        entity.CheckDisqualifiedDirectorUkOnly = dto.CheckDisqualifiedDirectorUkOnly;
        entity.CheckReputationalRiskExposure = dto.CheckReputationalRiskExposure;
        entity.CheckRegulatoryEnforcementList = dto.CheckRegulatoryEnforcementList;
        entity.CheckInsolvencyUkIreland = dto.CheckInsolvencyUkIreland;

        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.IndividualScreeningRequests
            .AsNoTracking()
            .Include(r => r.Nationality)
            .FirstAsync(r => r.Id == entity.Id, cancellationToken);

        return ApiResponse<IndividualScreeningRequestDto>.Ok(MapToDto(saved));
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.IndividualScreeningRequests
            .AsNoTracking()
            .Include(r => r.Nationality)
            .FirstOrDefaultAsync(r => r.CustomerId == customerId && !r.IsDeleted, cancellationToken);

        if (entity == null)
            return ApiResponse<RunSanctionsScreeningResultDto>.Fail("Individual screening request not found.");

        return await _runner.RunAsync(entity, cancellationToken);
    }

    public async Task<ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>> GetResultsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await _context.SanctionsScreenings
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.ScreenedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(s => new SanctionsScreeningResultItemDto
        {
            Id = s.Id,
            CustomerId = s.CustomerId,
            MatchedName = s.MatchedName,
            SanctionList = s.ScreeningList,
            MatchScore = s.Score,
            MatchType = s.MatchType,
            ScreeningDate = s.ScreenedAt,
            Status = s.Result,
            ReviewStatus = s.ReviewStatus,
            ReviewedAt = s.ReviewedAt,
            ReviewedBy = s.ReviewedBy
        }).ToList();

        return ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>.Ok(dtos);
    }

    private async Task<IndividualScreeningRequest?> GetLatestEntityAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await _context.IndividualScreeningRequests
            .AsNoTracking()
            .Include(r => r.Gender)
            .Include(r => r.Nationality)
            .Include(r => r.PlaceOfBirthCountry)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IndividualScreeningRequestDto MapToDto(IndividualScreeningRequest r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        CustomerId = r.CustomerId,
        ReferenceId = r.ReferenceId,
        FullName = r.FullName,
        DateOfBirth = r.DateOfBirth,
        NationalityId = r.NationalityId,
        PlaceOfBirthCountryId = r.PlaceOfBirthCountryId,
        IdType = r.IdType,
        IdNumber = r.IdNumber,
        Address = r.Address,
        GenderId = r.GenderId,
        MatchThreshold = r.MatchThreshold,
        BirthYearRange = r.BirthYearRange,
        CheckPepUkOnly = r.CheckPepUkOnly,
        CheckSanctions = r.CheckSanctions,
        CheckProfileOfInterest = r.CheckProfileOfInterest,
        CheckDisqualifiedDirectorUkOnly = r.CheckDisqualifiedDirectorUkOnly,
        CheckReputationalRiskExposure = r.CheckReputationalRiskExposure,
        CheckRegulatoryEnforcementList = r.CheckRegulatoryEnforcementList,
        CheckInsolvencyUkIreland = r.CheckInsolvencyUkIreland,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        CreatedBy = r.CreatedBy,
        UpdatedBy = r.UpdatedBy,
        IsActive = r.IsActive,
        IsDeleted = r.IsDeleted
    };
}

