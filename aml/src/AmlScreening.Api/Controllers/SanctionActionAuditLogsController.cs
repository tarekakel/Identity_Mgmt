using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SanctionActionAuditLogsController : ControllerBase
{
    private readonly ISanctionActionAuditLogService _service;

    public SanctionActionAuditLogsController(ISanctionActionAuditLogService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SanctionActionAuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? sanctionsScreeningId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAuditLogsAsync(customerId, sanctionsScreeningId, fromDate, toDate, cancellationToken);
        return Ok(result);
    }
}
