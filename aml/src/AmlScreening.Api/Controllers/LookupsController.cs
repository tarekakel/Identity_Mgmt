using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Lookups;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("customer-types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CustomerTypeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerTypes(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetCustomerTypesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CustomerTypeDto>>.Ok(list));
    }

    [HttpGet("customer-statuses")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CustomerStatusDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerStatuses(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetCustomerStatusesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CustomerStatusDto>>.Ok(list));
    }

    [HttpGet("genders")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GenderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGenders(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetGendersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GenderDto>>.Ok(list));
    }

    [HttpGet("nationalities")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NationalityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNationalities(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetNationalitiesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NationalityDto>>.Ok(list));
    }

    [HttpGet("countries")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CountryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetCountriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CountryDto>>.Ok(list));
    }

    [HttpGet("document-types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DocumentTypeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocumentTypes(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetDocumentTypesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DocumentTypeDto>>.Ok(list));
    }

    [HttpGet("occupations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OccupationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOccupations(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetOccupationsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OccupationDto>>.Ok(list));
    }

    [HttpGet("source-of-funds")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SourceOfFundsDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSourceOfFunds(CancellationToken cancellationToken)
    {
        var list = await _lookupService.GetSourceOfFundsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SourceOfFundsDto>>.Ok(list));
    }
}
