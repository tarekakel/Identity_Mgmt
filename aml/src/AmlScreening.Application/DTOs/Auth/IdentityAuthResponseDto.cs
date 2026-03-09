namespace AmlScreening.Application.DTOs.Auth;

/// <summary>
/// Mirrors Identity API response shape for login/refresh (ApiResponse&lt;LoginResponse&gt;) for HTTP deserialization.
/// </summary>
public class IdentityAuthResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public LoginResponse? Data { get; set; }
}
