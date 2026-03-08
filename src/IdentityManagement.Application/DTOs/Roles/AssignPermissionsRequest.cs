namespace IdentityManagement.Application.DTOs.Roles;

public class AssignPermissionsRequest
{
    public IReadOnlyList<Guid> PermissionIds { get; set; } = Array.Empty<Guid>();
}
