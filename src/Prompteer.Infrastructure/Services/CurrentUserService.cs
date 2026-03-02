using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;
using Prompteer.Domain.Interfaces;
using Prompteer.Infrastructure.Data;

namespace Prompteer.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _db;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, AppDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? EntraObjectId =>
        Principal?.FindFirst("oid")?.Value ??
        Principal?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ??
        Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? DisplayName =>
        Principal?.FindFirst("name")?.Value ??
        Principal?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email =>
        Principal?.FindFirst("preferred_username")?.Value ??
        Principal?.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Reads App Roles directly from JWT claims — source of truth is Entra, not the database.
    /// Roles are defined in the App Registration manifest and assigned in Enterprise Applications.
    /// Mapping: "Admin" → Admin | "Editor" → Editor | anything else → Viewer.
    /// </summary>
    public UserRole GetRoleFromClaims()
    {
        if (!IsAuthenticated) return UserRole.Viewer;

        // Microsoft.Identity.Web maps "roles" claim to ClaimTypes.Role automatically.
        // IsInRole checks both "roles" claim type and ClaimTypes.Role.
        if (Principal!.IsInRole("Admin"))  return UserRole.Admin;
        if (Principal!.IsInRole("Editor")) return UserRole.Editor;
        return UserRole.Viewer;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || EntraObjectId is null) return null;
        return await _db.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.EntraObjectId == EntraObjectId, cancellationToken);
    }
}
