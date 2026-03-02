using Microsoft.EntityFrameworkCore;
using Prompteer.Infrastructure.Data;

namespace Prompteer.Web.Middleware;

/// <summary>
/// Redirects to /Setup when no admin user exists yet (first run).
/// Skips static files, Account routes and Setup itself.
/// </summary>
public class SetupRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public SetupRedirectMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Always pass through: setup, auth callbacks, static assets
        if (path.StartsWith("/Setup", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/MicrosoftIdentity", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/signin-oidc", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/signout-callback-oidc", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        try
        {
            // Setup is needed when there are no users at all
            var anyUser = await db.ApplicationUsers
                .IgnoreQueryFilters()
                .AnyAsync();

            if (!anyUser)
            {
                context.Response.Redirect("/Setup");
                return;
            }
        }
        catch
        {
            // DB not available — let the request through so EF error surfaces normally
        }

        await _next(context);
    }
}
