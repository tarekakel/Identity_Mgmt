using IdentityManagement.Domain.Entities;

namespace IdentityManagement.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IReadOnlyList<string> roleNames, IReadOnlyList<string> permissionCodes);
    string GenerateRefreshToken();
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
}
