namespace IdentityManagement.Application.DTOs.Users;

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<Guid>? RoleIds { get; set; }
}
