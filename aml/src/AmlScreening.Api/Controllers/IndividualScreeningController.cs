using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualScreening;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/individual-screening")]
[AllowAnonymous]
public class IndividualScreeningController : ControllerBase
{
    private readonly IIndividualScreeningService _service;

    public IndividualScreeningController(IIndividualScreeningService service)
    {
        _service = service;
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IndividualScreeningRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetLatestAsync(customerId, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IndividualScreeningRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upsert(Guid customerId, [FromBody] UpsertIndividualScreeningRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpsertAsync(customerId, dto, cancellationToken);
        if (!result.Success)
            return result.Message == "Customer not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}/run")]
    [ProducesResponseType(typeof(ApiResponse<RunSanctionsScreeningResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Run(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.RunAsync(customerId, cancellationToken);
        if (!result.Success)
            return result.Message == "Individual screening request not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{customerId:guid}/results")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResults(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetResultsAsync(customerId, cancellationToken);
        return Ok(result);
    }
}

