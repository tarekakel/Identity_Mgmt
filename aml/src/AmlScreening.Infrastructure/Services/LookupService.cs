using AmlScreening.Application.DTOs.Lookups;
using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class LookupService : ILookupService
{
    private readonly ApplicationDbContext _context;

    public LookupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CustomerTypeDto>> GetCustomerTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CustomerTypes
            .AsNoTracking()
            .OrderBy(t => t.Code)
            .Select(t => new CustomerTypeDto { Id = t.Id, Code = t.Code, Name = t.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerStatusDto>> GetCustomerStatusesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CustomerStatuses
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new CustomerStatusDto { Id = s.Id, Code = s.Code, Name = s.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GenderDto>> GetGendersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Genders
            .AsNoTracking()
            .OrderBy(g => g.Code)
            .Select(g => new GenderDto { Id = g.Id, Code = g.Code, Name = g.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NationalityDto>> GetNationalitiesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Nationalities
            .AsNoTracking()
            .OrderBy(n => n.Code)
            .Select(n => new NationalityDto { Id = n.Id, Code = n.Code, Name = n.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Countries
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CountryDto { Id = c.Id, Code = c.Code, Name = c.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTypes
            .AsNoTracking()
            .OrderBy(t => t.Code)
            .Select(t => new DocumentTypeDto { Id = t.Id, Code = t.Code, Name = t.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OccupationDto>> GetOccupationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Occupations
            .AsNoTracking()
            .OrderBy(o => o.Code)
            .Select(o => new OccupationDto { Id = o.Id, Code = o.Code, Name = o.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SourceOfFundsDto>> GetSourceOfFundsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SourceOfFunds
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new SourceOfFundsDto { Id = s.Id, Code = s.Code, Name = s.Name })
            .ToListAsync(cancellationToken);
    }
}
