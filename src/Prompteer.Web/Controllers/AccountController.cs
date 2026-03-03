using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Web;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;
using Prompteer.Domain.Interfaces;
using Prompteer.Infrastructure.Data;
using Prompteer.Web.Helpers;
using Prompteer.Web.Models;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Prompteer.Web.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AccountController(
        AppDbContext db,
        ICurrentUserService currentUser,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration config,
        IStringLocalizer<SharedResource> localizer)
    {
        _db = db;
        _currentUser = currentUser;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _config = config;
        _localizer = localizer;
    }

    // ── Login page ────────────────────────────────────────────────────────────

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null, string? error = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        if (TempData["SetupSuccess"] is string adminName)
            ViewBag.SetupSuccess = adminName;

        ViewBag.ReturnUrl       = returnUrl;
        ViewBag.Error           = error;
        ViewBag.EntraConfigured = IsEntraConfigured();

        return View(new LocalLoginViewModel { ReturnUrl = returnUrl });
    }

    // ── Local login (bootstrap admin) ─────────────────────────────────────────

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> LocalLogin(LocalLoginViewModel model)
    {
        ViewBag.EntraConfigured = IsEntraConfigured();
        ViewBag.ReturnUrl       = model.ReturnUrl;

        if (!ModelState.IsValid)
            return View("Login", model);

        var user = await _db.ApplicationUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == model.Email.ToLowerInvariant() &&
                                      u.PasswordHash != null);

        if (user is null || !PasswordHasher.Verify(model.Password, user.PasswordHash!))
        {
            ModelState.AddModelError(string.Empty, _localizer["Login.InvalidCredentials"].Value);
            return View("Login", model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, _localizer["Login.AccountDisabled"].Value);
            return View("Login", model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.DisplayName),
            new(ClaimTypes.Email,          user.Email),
            new("oid",                     user.EntraObjectId),
            new("sub",                     user.Id.ToString()),
            new(ClaimTypes.Role,           user.Role.ToString()),
            new("roles",                   user.Role.ToString()),
            new("local_admin",             "true"),
        };

        var identity  = new ClaimsIdentity(claims, "LocalAdmin");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            "LocalAdmin",
            principal,
            new AuthenticationProperties { IsPersistent = true });

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var redirect = !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
            ? model.ReturnUrl
            : Url.Action("Index", "Dashboard")!;
        return LocalRedirect(redirect);
    }

    // ── Entra sign-in ─────────────────────────────────────────────────────────

    [AllowAnonymous]
    public IActionResult SignIn(string? returnUrl = null)
    {
        var redirectUrl = Url.Action("SignInCallback", "Account", new { returnUrl });
        return Challenge(
            new AuthenticationProperties { RedirectUri = redirectUrl ?? "/" },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Authorize]
    public async Task<IActionResult> SignInCallback(string? returnUrl = null)
    {
        await SyncCurrentUserAsync();
        return LocalRedirect(returnUrl ?? Url.Action("Index", "Dashboard")!);
    }

    // ── Sign out ──────────────────────────────────────────────────────────────

    [Authorize]
    public new async Task<IActionResult> SignOut()
    {
        var oid        = _currentUser.EntraObjectId;
        if (oid is not null) _cache.Remove($"photo:{oid}");

        var isLocalAdmin = User.FindFirst("local_admin")?.Value == "true";

        await HttpContext.SignOutAsync("LocalAdmin");

        if (!isLocalAdmin && IsEntraConfigured())
        {
            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                "EntraCookies",
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        return Redirect("/Account/Login");
    }

    // ── Access denied ─────────────────────────────────────────────────────────

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ── Photo proxy ───────────────────────────────────────────────────────────

    [Authorize]
    public async Task<IActionResult> Photo(string? oid = null)
    {
        var targetOid = oid is not null && User.IsInRole("Admin")
            ? oid
            : _currentUser.EntraObjectId;

        if (targetOid is null) return NotFound();

        var cacheKey = $"photo:{targetOid}";
        if (_cache.TryGetValue(cacheKey, out byte[]? cached) && cached?.Length > 0)
            return File(cached, "image/jpeg");

        try
        {
            var tokenAcquisition = HttpContext.RequestServices.GetService<ITokenAcquisition>();
            if (tokenAcquisition is null) return NotFound();

            var graphUrl = targetOid == _currentUser.EntraObjectId
                ? "https://graph.microsoft.com/v1.0/me/photo/$value"
                : $"https://graph.microsoft.com/v1.0/users/{targetOid}/photo/$value";

            var token = await tokenAcquisition.GetAccessTokenForUserAsync(
                new[] { "User.Read", "User.ReadBasic.All" });

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(graphUrl);
            if (!response.IsSuccessStatusCode) return NotFound();

            var bytes       = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
            _cache.Set(cacheKey, bytes, TimeSpan.FromMinutes(30));
            return File(bytes, contentType);
        }
        catch { return NotFound(); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsEntraConfigured() =>
        !string.IsNullOrWhiteSpace(_config["AzureAd:TenantId"]) &&
        !string.IsNullOrWhiteSpace(_config["AzureAd:ClientId"]);

    private async Task SyncCurrentUserAsync()
    {
        var oid = _currentUser.EntraObjectId;
        if (oid is null) return;

        var roleFromEntra = _currentUser.GetRoleFromClaims();
        var now           = DateTime.UtcNow;

        // Try find by OID, then by email (to link bootstrap admin account)
        var user = await _db.ApplicationUsers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.EntraObjectId == oid);

        var email = _currentUser.Email?.ToLowerInvariant();
        if (user is null && email is not null)
        {
            user = await _db.ApplicationUsers.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user is not null)
                user.EntraObjectId = oid; // Link bootstrap admin to Entra
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                EntraObjectId = oid,
                DisplayName   = _currentUser.DisplayName ?? "Unknown",
                Email         = email ?? string.Empty,
                Role          = roleFromEntra,
                IsActive      = true,
                LastLoginAt   = now,
                CreatedAt     = now,
                UpdatedAt     = now
            };
            _db.ApplicationUsers.Add(user);
        }
        else
        {
            user.DisplayName = _currentUser.DisplayName ?? user.DisplayName;
            user.Email       = email ?? user.Email;
            // Bootstrap admins (with PasswordHash) keep their role unless Entra promotes them.
            // This prevents Entra from downgrading a local admin that has no App Role assigned.
            if (user.PasswordHash == null || roleFromEntra > user.Role)
                user.Role = roleFromEntra;
            user.LastLoginAt = now;
            user.UpdatedAt   = now;
        }

        await _db.SaveChangesAsync();
    }
}
