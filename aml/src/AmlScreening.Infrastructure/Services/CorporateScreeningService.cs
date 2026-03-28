using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateScreening;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class CorporateScreeningService : ICorporateScreeningService
{
    private readonly ApplicationDbContext _context;
    private readonly ICorporateScreeningRunnerService _runner;

    public CorporateScreeningService(ApplicationDbContext context, ICorporateScreeningRunnerService runner)
    {
        _context = context;
        _runner = runner;
    }

    public async Task<ApiResponse<CorporateScreeningRequestDto>> GetLatestAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await GetLatestEntityAsync(customerId, cancellationToken);
        if (entity == null)
            return ApiResponse<CorporateScreeningRequestDto>.Fail("Corporate screening request not found.");
        return ApiResponse<CorporateScreeningRequestDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<IReadOnlyList<CorporateScreeningRequestDto>>> ListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var list = await _context.CorporateScreeningRequests
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId && !r.IsDeleted)
            .Include(r => r.Country)
            .Include(r => r.CompanyDocuments)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Nationality)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Documents)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<CorporateScreeningRequestDto>>.Ok(list.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<CorporateScreeningRequestDto>> GetByIdAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CorporateScreeningRequests
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId && r.Id == requestId && !r.IsDeleted)
            .Include(r => r.Country)
            .Include(r => r.CompanyDocuments)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Nationality)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Documents)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return ApiResponse<CorporateScreeningRequestDto>.Fail("Corporate screening request not found.");

        return ApiResponse<CorporateScreeningRequestDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<CorporateScreeningRequestDto>> UpsertAsync(Guid customerId, UpsertCorporateScreeningRequestDto dto, CancellationToken cancellationToken = default)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
        if (!customerExists)
            return ApiResponse<CorporateScreeningRequestDto>.Fail("Customer not found.");

        CorporateScreeningRequest entity;

        if (dto.Id.HasValue)
        {
            var existing = await _context.CorporateScreeningRequests
                .Include(r => r.CompanyDocuments)
                .Include(r => r.Shareholders)
                    .ThenInclude(s => s.Documents)
                .FirstOrDefaultAsync(r => r.Id == dto.Id.Value && r.CustomerId == customerId && !r.IsDeleted, cancellationToken);

            if (existing == null)
                return ApiResponse<CorporateScreeningRequestDto>.Fail("Corporate screening request not found.");

            entity = existing;
            _context.CorporateScreeningShareholders.RemoveRange(entity.Shareholders);
            _context.CorporateScreeningCompanyDocuments.RemoveRange(entity.CompanyDocuments);
        }
        else
        {
            entity = new CorporateScreeningRequest
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                TenantId = dto.TenantId,
                IsActive = true,
                IsDeleted = false
            };
            _context.CorporateScreeningRequests.Add(entity);
        }

        ApplyUpsertFields(entity, dto);

        foreach (var d in dto.CompanyDocuments)
        {
            entity.CompanyDocuments.Add(new CorporateScreeningCompanyDocument
            {
                Id = Guid.NewGuid(),
                DocumentNo = d.DocumentNo?.Trim(),
                IssuedDate = d.IssuedDate,
                ExpiryDate = d.ExpiryDate,
                Details = d.Details?.Trim(),
                Remarks = d.Remarks?.Trim()
            });
        }

        foreach (var shDto in dto.Shareholders)
        {
            var sh = new CorporateScreeningShareholder
            {
                Id = Guid.NewGuid(),
                FullName = shDto.FullName?.Trim() ?? string.Empty,
                NationalityId = shDto.NationalityId,
                DateOfBirth = shDto.DateOfBirth,
                SharePercent = shDto.SharePercent
            };
            foreach (var docDto in shDto.Documents)
            {
                sh.Documents.Add(new CorporateScreeningShareholderDocument
                {
                    Id = Guid.NewGuid(),
                    DocumentNo = docDto.DocumentNo?.Trim(),
                    IssuedDate = docDto.IssuedDate,
                    ExpiryDate = docDto.ExpiryDate,
                    Details = docDto.Details?.Trim(),
                    Remarks = docDto.Remarks?.Trim()
                });
            }
            entity.Shareholders.Add(sh);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var saved = await LoadRequestTrackedAsync(entity.Id, cancellationToken);
        return ApiResponse<CorporateScreeningRequestDto>.Ok(MapToDto(saved!));
    }

    private static void ApplyUpsertFields(CorporateScreeningRequest entity, UpsertCorporateScreeningRequestDto dto)
    {
        entity.TenantId = dto.TenantId;
        entity.CompanyCode = dto.CompanyCode?.Trim();
        entity.FullName = dto.FullName?.Trim() ?? string.Empty;
        entity.CountryId = dto.CountryId;
        entity.DateOfRegistration = dto.DateOfRegistration;
        entity.TradeLicenceNo = dto.TradeLicenceNo?.Trim();
        entity.Address = dto.Address?.Trim();
        entity.MatchThreshold = Math.Clamp(dto.MatchThreshold, 0, 100);

        entity.CheckPepUkOnly = dto.CheckPepUkOnly;
        entity.CheckSanctions = dto.CheckSanctions;
        entity.CheckProfileOfInterest = dto.CheckProfileOfInterest;
        entity.CheckDisqualifiedDirectorUkOnly = dto.CheckDisqualifiedDirectorUkOnly;
        entity.CheckReputationalRiskExposure = dto.CheckReputationalRiskExposure;
        entity.CheckRegulatoryEnforcementList = dto.CheckRegulatoryEnforcementList;
        entity.CheckInsolvencyUkIreland = dto.CheckInsolvencyUkIreland;
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CorporateScreeningRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.CustomerId == customerId && !r.IsDeleted, cancellationToken);

        if (entity == null)
            return ApiResponse<bool>.Fail("Corporate screening request not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CorporateScreeningRequests
            .Include(r => r.Country)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Nationality)
            .AsSplitQuery()
            .Where(r => r.CustomerId == customerId && !r.IsDeleted)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return ApiResponse<RunSanctionsScreeningResultDto>.Fail("Corporate screening request not found.");

        return await _runner.RunAsync(entity, cancellationToken);
    }

    public async Task<ApiResponse<RunSanctionsScreeningResultDto>> RunForRequestAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CorporateScreeningRequests
            .Include(r => r.Country)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Nationality)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == requestId && r.CustomerId == customerId && !r.IsDeleted, cancellationToken);

        if (entity == null)
            return ApiResponse<RunSanctionsScreeningResultDto>.Fail("Corporate screening request not found.");

        return await _runner.RunAsync(entity, cancellationToken);
    }

    public async Task<ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>> GetResultsAsync(Guid customerId, Guid? corporateScreeningRequestId, CancellationToken cancellationToken = default)
    {
        var query = _context.SanctionsScreenings
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId && !s.IsDeleted);

        if (corporateScreeningRequestId.HasValue)
            query = query.Where(s => s.CorporateScreeningRequestId == corporateScreeningRequestId.Value);

        var items = await query
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.ScreenedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToResultItem).ToList();
        return ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>.Ok(dtos);
    }

    private async Task<CorporateScreeningRequest?> LoadRequestTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.CorporateScreeningRequests
            .AsNoTracking()
            .Include(r => r.Country)
            .Include(r => r.CompanyDocuments)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Nationality)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Documents)
            .FirstAsync(r => r.Id == id, cancellationToken);
    }

    private async Task<CorporateScreeningRequest?> GetLatestEntityAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await _context.CorporateScreeningRequests
            .AsNoTracking()
            .Include(r => r.Country)
            .Include(r => r.CompanyDocuments)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Nationality)
            .Include(r => r.Shareholders)
                .ThenInclude(s => s.Documents)
            .Where(r => r.CustomerId == customerId && !r.IsDeleted)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static CorporateScreeningRequestDto MapToDto(CorporateScreeningRequest r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        CustomerId = r.CustomerId,
        CompanyCode = r.CompanyCode,
        FullName = r.FullName,
        CountryId = r.CountryId,
        DateOfRegistration = r.DateOfRegistration,
        TradeLicenceNo = r.TradeLicenceNo,
        Address = r.Address,
        MatchThreshold = r.MatchThreshold,
        CheckPepUkOnly = r.CheckPepUkOnly,
        CheckSanctions = r.CheckSanctions,
        CheckProfileOfInterest = r.CheckProfileOfInterest,
        CheckDisqualifiedDirectorUkOnly = r.CheckDisqualifiedDirectorUkOnly,
        CheckReputationalRiskExposure = r.CheckReputationalRiskExposure,
        CheckRegulatoryEnforcementList = r.CheckRegulatoryEnforcementList,
        CheckInsolvencyUkIreland = r.CheckInsolvencyUkIreland,
        CompanyDocuments = r.CompanyDocuments.Select(d => new CorporateScreeningCompanyDocumentDto
        {
            Id = d.Id,
            DocumentNo = d.DocumentNo,
            IssuedDate = d.IssuedDate,
            ExpiryDate = d.ExpiryDate,
            Details = d.Details,
            Remarks = d.Remarks
        }).ToList(),
        Shareholders = r.Shareholders.Select(s => new CorporateScreeningShareholderDto
        {
            Id = s.Id,
            FullName = s.FullName,
            NationalityId = s.NationalityId,
            DateOfBirth = s.DateOfBirth,
            SharePercent = s.SharePercent,
            Documents = s.Documents.Select(d => new CorporateScreeningShareholderDocumentDto
            {
                Id = d.Id,
                DocumentNo = d.DocumentNo,
                IssuedDate = d.IssuedDate,
                ExpiryDate = d.ExpiryDate,
                Details = d.Details,
                Remarks = d.Remarks
            }).ToList()
        }).ToList(),
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    private static SanctionsScreeningResultItemDto MapToResultItem(SanctionsScreening s) => new()
    {
        Id = s.Id,
        CustomerId = s.CustomerId,
        CorporateScreeningRequestId = s.CorporateScreeningRequestId,
        MatchedName = s.MatchedName,
        SanctionList = s.ScreeningList,
        MatchScore = s.Score,
        MatchType = s.MatchType,
        ScreeningDate = s.ScreenedAt,
        Status = s.Result,
        ReviewStatus = s.ReviewStatus,
        ReviewedAt = s.ReviewedAt,
        ReviewedBy = s.ReviewedBy
    };
}
