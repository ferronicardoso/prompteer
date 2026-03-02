using Prompteer.Domain.Common;
using Prompteer.Domain.Enums;

namespace Prompteer.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    /// <summary>Object ID (oid claim) from Microsoft Entra — stable and unique per tenant.</summary>
    public string EntraObjectId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// PBKDF2 hashed password — only set for the bootstrap admin created during setup.
    /// Null for all users that authenticate exclusively via Entra.
    /// </summary>
    public string? PasswordHash { get; set; }
}
