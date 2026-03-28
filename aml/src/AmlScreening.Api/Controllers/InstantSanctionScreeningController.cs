using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.InstantSanctionScreening;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/instant-sanction-screening")]
[AllowAnonymous]
public class InstantSanctionScreeningController : ControllerBase
{
    private readonly IInstantSanctionScreeningService _service;

    public InstantSanctionScreeningController(IInstantSanctionScreeningService service)
    {
        _service = service;
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromBody] InstantSanctionScreeningSearchRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>.Fail("Request body is required."));

        var result = await _service.SearchAsync(request, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
