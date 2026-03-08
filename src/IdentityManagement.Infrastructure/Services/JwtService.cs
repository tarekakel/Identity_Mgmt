using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityManagement.Application.Common;
using IdentityManagement.Application.Interfaces;
using IdentityManagement.Domain.Entities;
using IdentityManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace IdentityManagement.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _settings;
    private readonly byte[] _key;

    public JwtService(ApplicationDbContext context, IOptions<JwtSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
        _key = Encoding.UTF8.GetBytes(_settings.Secret);
        if (_key.Length < 32)
            throw new InvalidOperationException("JWT Secret must be at least 32 characters.");
    }

    public string GenerateAccessToken(User user, IReadOnlyList<string> roleNames, IReadOnlyList<string> permissionCodes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", user.TenantId.ToString()),
            new("tenant_code", user.Tenant?.Code ?? string.Empty)
        };
        foreach (var role in roleNames)
            claims.Add(new Claim(ClaimTypes.Role, role));
        foreach (var perm in permissionCodes)
            claims.Add(new Claim("permission", perm));

        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(_key),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u!.Tenant)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);
        if (stored?.User == null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = GenerateRefreshToken();
        var newRefreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = stored.UserId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(newRefreshEntity);

        var roleNames = await _context.UserRoles
            .Where(ur => ur.UserId == stored.UserId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(cancellationToken);
        var permissionCodes = await _context.RolePermissions
            .Where(rp => _context.UserRoles.Where(ur => ur.UserId == stored.UserId).Select(ur => ur.RoleId).Contains(rp.RoleId))
            .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (_, p) => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var accessToken = GenerateAccessToken(stored.User, roleNames, permissionCodes);
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);
        await _context.SaveChangesAsync(cancellationToken);

        return (accessToken, newRefreshToken, expiresAt);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        if (stored == null)
            return;
        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
