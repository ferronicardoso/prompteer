using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;

namespace Prompteer.Domain.Interfaces;

public interface ICurrentUserService
{
    /// <summary>Object ID (oid claim) from Entra — stable and unique per tenant.</summary>
    string? EntraObjectId { get; }
    string? DisplayName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// Role resolved directly from the JWT <c>roles</c> claim (App Roles assigned in Entra).
    /// Source of truth for authorization — never reads from the database.
    /// </summary>
    UserRole GetRoleFromClaims();

    /// <summary>Resolved ApplicationUser from the database (display/audit only).</summary>
    Task<ApplicationUser?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
