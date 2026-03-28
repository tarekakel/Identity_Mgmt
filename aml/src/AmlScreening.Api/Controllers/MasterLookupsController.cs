using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Lookups;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class MasterLookupsController : ControllerBase
{
    private readonly IMasterLookupCrudService _service;

    public MasterLookupsController(IMasterLookupCrudService service)
    {
        _service = service;
    }

    [HttpGet("countries")]
    public Task<IActionResult> ListCountries(CancellationToken cancellationToken) => ToAction(_service.ListCountriesAsync(cancellationToken));

    [HttpGet("countries/{id:guid}")]
    public Task<IActionResult> GetCountry(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetCountryAsync(id, cancellationToken));

    [HttpPost("countries")]
    public Task<IActionResult> CreateCountry([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateCountryAsync(request, cancellationToken));

    [HttpPut("countries/{id:guid}")]
    public Task<IActionResult> UpdateCountry(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateCountryAsync(id, request, cancellationToken));

    [HttpDelete("countries/{id:guid}")]
    public Task<IActionResult> DeleteCountry(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteCountryAsync(id, cancellationToken));

    [HttpGet("nationalities")]
    public Task<IActionResult> ListNationalities(CancellationToken cancellationToken) => ToAction(_service.ListNationalitiesAsync(cancellationToken));

    [HttpGet("nationalities/{id:guid}")]
    public Task<IActionResult> GetNationality(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetNationalityAsync(id, cancellationToken));

    [HttpPost("nationalities")]
    public Task<IActionResult> CreateNationality([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateNationalityAsync(request, cancellationToken));

    [HttpPut("nationalities/{id:guid}")]
    public Task<IActionResult> UpdateNationality(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateNationalityAsync(id, request, cancellationToken));

    [HttpDelete("nationalities/{id:guid}")]
    public Task<IActionResult> DeleteNationality(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteNationalityAsync(id, cancellationToken));

    [HttpGet("genders")]
    public Task<IActionResult> ListGenders(CancellationToken cancellationToken) => ToAction(_service.ListGendersAsync(cancellationToken));

    [HttpGet("genders/{id:guid}")]
    public Task<IActionResult> GetGender(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetGenderAsync(id, cancellationToken));

    [HttpPost("genders")]
    public Task<IActionResult> CreateGender([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateGenderAsync(request, cancellationToken));

    [HttpPut("genders/{id:guid}")]
    public Task<IActionResult> UpdateGender(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateGenderAsync(id, request, cancellationToken));

    [HttpDelete("genders/{id:guid}")]
    public Task<IActionResult> DeleteGender(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteGenderAsync(id, cancellationToken));

    [HttpGet("customer-types")]
    public Task<IActionResult> ListCustomerTypes(CancellationToken cancellationToken) => ToAction(_service.ListCustomerTypesAsync(cancellationToken));

    [HttpGet("customer-types/{id:guid}")]
    public Task<IActionResult> GetCustomerType(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetCustomerTypeAsync(id, cancellationToken));

    [HttpPost("customer-types")]
    public Task<IActionResult> CreateCustomerType([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateCustomerTypeAsync(request, cancellationToken));

    [HttpPut("customer-types/{id:guid}")]
    public Task<IActionResult> UpdateCustomerType(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateCustomerTypeAsync(id, request, cancellationToken));

    [HttpDelete("customer-types/{id:guid}")]
    public Task<IActionResult> DeleteCustomerType(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteCustomerTypeAsync(id, cancellationToken));

    [HttpGet("customer-statuses")]
    public Task<IActionResult> ListCustomerStatuses(CancellationToken cancellationToken) => ToAction(_service.ListCustomerStatusesAsync(cancellationToken));

    [HttpGet("customer-statuses/{id:guid}")]
    public Task<IActionResult> GetCustomerStatus(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetCustomerStatusAsync(id, cancellationToken));

    [HttpPost("customer-statuses")]
    public Task<IActionResult> CreateCustomerStatus([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateCustomerStatusAsync(request, cancellationToken));

    [HttpPut("customer-statuses/{id:guid}")]
    public Task<IActionResult> UpdateCustomerStatus(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateCustomerStatusAsync(id, request, cancellationToken));

    [HttpDelete("customer-statuses/{id:guid}")]
    public Task<IActionResult> DeleteCustomerStatus(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteCustomerStatusAsync(id, cancellationToken));

    [HttpGet("document-types")]
    public Task<IActionResult> ListDocumentTypes(CancellationToken cancellationToken) => ToAction(_service.ListDocumentTypesAsync(cancellationToken));

    [HttpGet("document-types/{id:guid}")]
    public Task<IActionResult> GetDocumentType(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetDocumentTypeAsync(id, cancellationToken));

    [HttpPost("document-types")]
    public Task<IActionResult> CreateDocumentType([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateDocumentTypeAsync(request, cancellationToken));

    [HttpPut("document-types/{id:guid}")]
    public Task<IActionResult> UpdateDocumentType(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateDocumentTypeAsync(id, request, cancellationToken));

    [HttpDelete("document-types/{id:guid}")]
    public Task<IActionResult> DeleteDocumentType(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteDocumentTypeAsync(id, cancellationToken));

    [HttpGet("occupations")]
    public Task<IActionResult> ListOccupations(CancellationToken cancellationToken) => ToAction(_service.ListOccupationsAsync(cancellationToken));

    [HttpGet("occupations/{id:guid}")]
    public Task<IActionResult> GetOccupation(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetOccupationAsync(id, cancellationToken));

    [HttpPost("occupations")]
    public Task<IActionResult> CreateOccupation([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateOccupationAsync(request, cancellationToken));

    [HttpPut("occupations/{id:guid}")]
    public Task<IActionResult> UpdateOccupation(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateOccupationAsync(id, request, cancellationToken));

    [HttpDelete("occupations/{id:guid}")]
    public Task<IActionResult> DeleteOccupation(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteOccupationAsync(id, cancellationToken));

    [HttpGet("source-of-funds")]
    public Task<IActionResult> ListSourceOfFunds(CancellationToken cancellationToken) => ToAction(_service.ListSourceOfFundsAsync(cancellationToken));

    [HttpGet("source-of-funds/{id:guid}")]
    public Task<IActionResult> GetSourceOfFunds(Guid id, CancellationToken cancellationToken) => ToAction(_service.GetSourceOfFundsAsync(id, cancellationToken));

    [HttpPost("source-of-funds")]
    public Task<IActionResult> CreateSourceOfFunds([FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.CreateSourceOfFundsAsync(request, cancellationToken));

    [HttpPut("source-of-funds/{id:guid}")]
    public Task<IActionResult> UpdateSourceOfFunds(Guid id, [FromBody] UpsertMasterLookupRequest request, CancellationToken cancellationToken) =>
        ToAction(_service.UpdateSourceOfFundsAsync(id, request, cancellationToken));

    [HttpDelete("source-of-funds/{id:guid}")]
    public Task<IActionResult> DeleteSourceOfFunds(Guid id, CancellationToken cancellationToken) => ToAction(_service.DeleteSourceOfFundsAsync(id, cancellationToken));

    private static async Task<IActionResult> ToAction<T>(Task<ApiResponse<T>> task)
    {
        var result = await task;
        if (!result.Success)
            return new BadRequestObjectResult(result);
        return new OkObjectResult(result);
    }

    private static async Task<IActionResult> ToAction(Task<ApiResponse> task)
    {
        var result = await task;
        if (!result.Success)
            return new BadRequestObjectResult(result);
        return new OkObjectResult(result);
    }
}
