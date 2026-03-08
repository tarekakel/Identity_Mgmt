namespace AmlScreening.Application.Interfaces;

public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's display name (email or username) for audit fields.
    /// </summary>
    string? GetCurrentUserDisplayName();
}
