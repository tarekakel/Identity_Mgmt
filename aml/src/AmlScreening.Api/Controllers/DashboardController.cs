using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Dashboard;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[AllowAnonymous]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("customer-kpis")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDashboardKpisDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerKpis(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetCustomerKpisAsync(cancellationToken);
        return Ok(result);
    }
}
