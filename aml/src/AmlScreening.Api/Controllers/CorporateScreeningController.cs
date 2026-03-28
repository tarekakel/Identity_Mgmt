using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateScreening;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/corporate-screening")]
[AllowAnonymous]
public class CorporateScreeningController : ControllerBase
{
    private readonly ICorporateScreeningService _service;

    public CorporateScreeningController(ICorporateScreeningService service)
    {
        _service = service;
    }

    [HttpGet("{customerId:guid}/requests/{requestId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CorporateScreeningRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid customerId, Guid requestId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(customerId, requestId, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("{customerId:guid}/requests")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CorporateScreeningRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRequests(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(customerId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{customerId:guid}/requests/{requestId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRequest(Guid customerId, Guid requestId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(customerId, requestId, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}/requests/{requestId:guid}/run")]
    [ProducesResponseType(typeof(ApiResponse<RunSanctionsScreeningResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunForRequest(Guid customerId, Guid requestId, CancellationToken cancellationToken)
    {
        var result = await _service.RunForRequestAsync(customerId, requestId, cancellationToken);
        if (!result.Success)
            return result.Message == "Corporate screening request not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    /// <summary>Most recently updated corporate request for this customer (legacy).</summary>
    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CorporateScreeningRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetLatestAsync(customerId, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CorporateScreeningRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upsert(Guid customerId, [FromBody] UpsertCorporateScreeningRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpsertAsync(customerId, dto, cancellationToken);
        if (!result.Success)
            return result.Message == "Customer not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    /// <summary>Runs screening for the most recently updated corporate request (legacy).</summary>
    [HttpPost("{customerId:guid}/run")]
    [ProducesResponseType(typeof(ApiResponse<RunSanctionsScreeningResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunLatest(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.RunAsync(customerId, cancellationToken);
        if (!result.Success)
            return result.Message == "Corporate screening request not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{customerId:guid}/results")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResults(Guid customerId, [FromQuery] Guid? corporateScreeningRequestId, CancellationToken cancellationToken)
    {
        var result = await _service.GetResultsAsync(customerId, corporateScreeningRequestId, cancellationToken);
        return Ok(result);
    }
}
