namespace IdentityManagement.Application.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<string> RoleNames { get; set; } = Array.Empty<string>();
}
