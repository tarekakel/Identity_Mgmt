namespace IdentityManagement.Application.DTOs.Roles;

public class RoleDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<string> PermissionCodes { get; set; } = Array.Empty<string>();
}
