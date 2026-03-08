using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Auth;
using IdentityManagement.Application.Interfaces;
using IdentityManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        Infrastructure.Persistence.ApplicationDbContext context,
        IJwtService jwtService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        Tenant? tenant = null;
        if (!string.IsNullOrWhiteSpace(request.TenantCode))
        {
            tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Code == request.TenantCode.Trim().ToUpperInvariant() && t.IsActive, cancellationToken);
            if (tenant == null)
                return ApiResponse<LoginResponse>.Fail("Invalid tenant.");
        }
        else
        {
            var firstTenant = await _context.Tenants.Where(t => t.IsActive).FirstOrDefaultAsync(cancellationToken);
            if (firstTenant != null)
                tenant = firstTenant;
        }

        if (tenant == null)
            return ApiResponse<LoginResponse>.Fail("No tenant available.");

        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.NormalizedEmail == normalizedEmail && u.IsActive, cancellationToken);
        if (user == null)
            return ApiResponse<LoginResponse>.Fail("Invalid email or password.");

        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            return ApiResponse<LoginResponse>.Fail("Invalid email or password.");

        var roleNames = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(cancellationToken);
        var permissionCodes = await _context.RolePermissions
            .Where(rp => _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).Contains(rp.RoleId))
            .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (_, p) => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user, roleNames, permissionCodes);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(refreshEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        });
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (accessToken, refreshToken, expiresAt) = await _jwtService.RefreshTokensAsync(request.RefreshToken, cancellationToken);
            return ApiResponse<LoginResponse>.Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            });
        }
        catch (UnauthorizedAccessException)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid or expired refresh token.");
        }
    }

    public async Task<ApiResponse> RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        await _jwtService.RevokeRefreshTokenAsync(refreshToken, ipAddress, cancellationToken);
        return ApiResponse.Ok("Token revoked.");
    }
}
