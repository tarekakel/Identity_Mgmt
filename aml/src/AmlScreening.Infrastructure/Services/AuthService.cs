using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Auth;
using AmlScreening.Application.Interfaces;

namespace AmlScreening.Infrastructure.Services;

/// <summary>
/// Stub implementation. Integrate with Identity project later for real login/refresh/revoke.
/// </summary>
public class AuthService : IAuthService
{
    public Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiResponse<LoginResponse>.Fail("Authentication is not configured. Integrate with Identity API."));
    }

    public Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiResponse<LoginResponse>.Fail("Authentication is not configured. Integrate with Identity API."));
    }

    public Task RevokeTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
