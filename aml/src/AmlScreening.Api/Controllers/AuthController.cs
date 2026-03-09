using AmlScreening.Application.DTOs.Auth;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("revoke")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest? request, CancellationToken cancellationToken)
    {
        var token = request?.RefreshToken ?? string.Empty;
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var accessToken = Request.Headers.Authorization.FirstOrDefault();
        await _authService.RevokeTokenAsync(token, ip, accessToken, cancellationToken);
        return Ok(new { message = "Token revoked." });
    }
}
