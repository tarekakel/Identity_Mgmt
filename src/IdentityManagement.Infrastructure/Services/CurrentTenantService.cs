using System.Security.Claims;
using IdentityManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IdentityManagement.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenant
{
    private const string TenantIdClaim = "tenant_id";
    private const string TenantCodeClaim = "tenant_code";
    private const string TenantHeader = "X-Tenant-Id";
    private const string TenantCodeHeader = "X-Tenant-Code";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(TenantIdClaim)?.Value;
            if (Guid.TryParse(claim, out var id))
                return id;
            var header = _httpContextAccessor.HttpContext?.Request?.Headers[TenantHeader].FirstOrDefault();
            return Guid.TryParse(header, out var headerId) ? headerId : null;
        }
    }

    public string? TenantCode =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(TenantCodeClaim)?.Value
        ?? _httpContextAccessor.HttpContext?.Request?.Headers[TenantCodeHeader].FirstOrDefault();

    public bool IsSet => TenantId.HasValue;
}
