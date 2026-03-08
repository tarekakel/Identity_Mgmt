namespace IdentityManagement.Application.DTOs.Users;

public class AssignRolesRequest
{
    public IReadOnlyList<Guid> RoleIds { get; set; } = Array.Empty<Guid>();
}
