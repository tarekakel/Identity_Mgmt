namespace IdentityManagement.Application.Interfaces;

public interface ICurrentTenant
{
    Guid? TenantId { get; }
    string? TenantCode { get; }
    bool IsSet { get; }
}
